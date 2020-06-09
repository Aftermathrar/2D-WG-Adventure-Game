using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGame.Stats;
using ButtonGame.Character;
using ButtonGame.Core;
using TMPro;
using ButtonGame.Resources;

namespace ButtonGame.Combat
{
    public class AtkBtnScript : MonoBehaviour, IAction, IBattleState
    {
        [SerializeField] AttackType attackType;
        [SerializeField] KeyCode keyCode;
        [SerializeField] AttackDB attackDB = null;
        LevelManager levelManager;
        AttackValues attackValues = null;
        PlayerController player = null;
        GuardController guard = null;
        Health target;
        Mana mana;
        BaseStats baseStats;
        Fighter fighter;

        [SerializeField] TextMeshProUGUI atkNameText = null;
        [SerializeField] TextMeshProUGUI atkPriorityText = null;
        [SerializeField] TextMeshProUGUI atkKeyCode = null;
        [SerializeField] RectTransform btnOverlay = null;
        [SerializeField] ParticleSystem clickEffect = null;

        float timeSinceAtkOnCooldown = Mathf.Infinity;
        float timeSinceAtkStart = Mathf.Infinity;
        float timeSinceLastHit = Mathf.Infinity;
        int currentHitCount;
        float cooldownDelay;
        float maxCooldown;
        float[] maxTimeToHit;
        float skillPriority;
        float maxHitCount;
        float manaCost;
        bool isCrit = false;
        bool[] isEffectOnHit;

        private Button thisButton;

        [SerializeField] HitTimer hitTimer;
        List<HitTimer> hitTimers = new List<HitTimer>();

        private bool isBattleActive;

        private void Awake() 
        {
            thisButton = GetComponent<Button>();
            player = GetComponentInParent<PlayerController>();
            guard = player.GetComponent<GuardController>();
            mana = player.GetComponent<Mana>();
            baseStats = player.GetComponent<BaseStats>();
            fighter = player.GetComponent<Fighter>();
            attackValues = player.GetComponent<AttackValues>();
            levelManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<LevelManager>();
            levelManager.StartCurrentBattle += StartBattle;
            levelManager.EndCurrentBattle += EndBattle;
        }

        private void Start() 
        {
            ResetButton();
            AssignSkillVariables();
            AssignSkillInfoText();
        }

        private void AssignSkillVariables()
        {
            manaCost = GetStat(AttackStat.Cost, 0);
            maxHitCount = GetStat(AttackStat.HitCount, 0);
            maxCooldown = GetStat(AttackStat.Cooldown, 0);
            skillPriority = GetStat(AttackStat.Priority, 0);
            int maxEffectCount = attackDB.GetAttackStat(AttackStat.EffectID, attackType).Length;
            isEffectOnHit = new bool[maxEffectCount];
            for (int i = 0; i < maxEffectCount; i++)
            {
                string[] fxApply = attackDB.GetAttackStat(AttackStat.ApplyEffect, attackType);
                isEffectOnHit[i] = fxApply[i] == "OnHit";
            }
        }

        private void Update() 
        {
            if(isBattleActive)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    thisButton.targetGraphic.CrossFadeColor(thisButton.colors.pressedColor, thisButton.colors.fadeDuration, true, true);
                    thisButton.onClick.Invoke();
                }
                else if (Input.GetKeyUp(keyCode))
                {
                    thisButton.targetGraphic.CrossFadeColor(thisButton.colors.normalColor, thisButton.colors.fadeDuration, true, true);
                }
            }
        }

        public void OnClick()
        {
            if (target == null || LevelManager.isPaused) return;
            if (IsAttackReady() && player.CanAttack(skillPriority) && HasEnoughMana())
            {
                clickEffect.Play();
                timeSinceAtkStart = 0;
                timeSinceLastHit = 0;
                currentHitCount = 0;
                float atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
                atkSpeed = 1 / atkSpeed * 100;

                GetComponentInParent<ActionScheduler>().StartAction(this);
                cooldownDelay = GetStat(AttackStat.CDStart, 0) * atkSpeed;
                maxCooldown = GetStat(AttackStat.Cooldown, 0);
                mana.UseMana(manaCost);
                float[] skillTimes = GetSkillTimes();
                player.StartAttack(skillTimes);
                CheckEffectActivation(false);

                if(GetStatBool(AttackStat.Block, 0))
                {
                    guard.StartBlock(skillTimes[2], this);
                }
                else
                {
                    guard.CancelBlock();
                }

                if(GetStat(AttackStat.InvulnTime, 0) > 0)
                {
                    fighter.StartInvulnTime(GetStat(AttackStat.InvulnTime, 0));
                }

                if(maxHitCount > 0) 
                { 
                    maxTimeToHit = GetStatArray(Stats.AttackStat.TimeToHit);
                    fighter.activeAttack += UpdateTimeToHit;

                    for (int i = 0; i < maxHitCount; i++)
                    {
                        float newPosX = -175 - (i * 50);
                        HitTimer myObjectInstance = Instantiate(hitTimer, new Vector3(newPosX, 50, 0), Quaternion.identity);
                        myObjectInstance.transform.SetParent(player.transform, false);
                        hitTimers.Add(myObjectInstance);

                        maxTimeToHit[i] = maxTimeToHit[i] * atkSpeed;
                        myObjectInstance.SetFillTime(maxTimeToHit[i], i);
                    }
                }
                fighter.activeAttack += StartCooldown;
            }
        }

        public bool IsAttackReady()
        {
            if (timeSinceAtkOnCooldown >= maxCooldown)
            {
                return true;
            }
            return false;
        }

        public bool HasEnoughMana()
        {
            float curMana = mana.GetMana();
            if (curMana < manaCost)
            {
                return false;
            }
            return true;
        }

        public void ResetButton()
        {
            atkNameText.text = GetStatString(AttackStat.Name);
            thisButton.targetGraphic.CrossFadeColor(thisButton.colors.normalColor, thisButton.colors.fadeDuration, true, true);
            btnOverlay.localScale = new Vector3(0, 1, 1);
            fighter.activeAttack -= UpdateTimer;
        }

        private void AssignSkillInfoText()
        {
            if(skillPriority == 1)
            {
                atkPriorityText.text = "H";
                atkPriorityText.color = new Color32(255, 239, 177, 255);
            }
            else
            {
                atkPriorityText.text = "L";
                atkPriorityText.color = new Color32(204, 255, 255, 255);
            }

            string keyString = keyCode.ToString();
            if(keyString.Contains("Alpha"))
            {
                atkKeyCode.text = keyString.Substring(keyString.Length-1);
            }
            else
            {
                atkKeyCode.text = keyCode.ToString();
            }
        }

        private void UpdateTimeToHit()
        {
            timeSinceLastHit += Time.deltaTime;

            if(hitTimers.Count == 0)
            {
                fighter.activeAttack -= UpdateTimeToHit;
                return;
            }

            if(timeSinceLastHit >= maxTimeToHit[currentHitCount])
            {
                hitTimers.Remove(hitTimers[0]);
                timeSinceLastHit = 0;
                // On deal damage, calculate current buffs and update next buffs

                float damage = CalculateDamage();
                if(target.TakeDamage(damage, isCrit))
                {
                    player.DamageDealt(damage);
                    CheckEffectActivation(true);
                }
                currentHitCount += 1;

                if (currentHitCount >= maxHitCount)
                {
                    fighter.activeAttack -= UpdateTimeToHit;
                }
                else
                {
                    timeSinceLastHit = 0;
                    for (int i = 0; i < hitTimers.Count; i++)
                    {
                        hitTimers[i].SetMoveTime(.2f);
                        hitTimers[i].SetFillTime(maxTimeToHit[currentHitCount], i);
                    }
                }
            }
        }

        private float CalculateDamage()
        {
            float total = 0;
            float atkPower = baseStats.GetStat(Stat.AttackPower);
            atkPower *= GetStat(AttackStat.Power, currentHitCount);

            if(GetStatBool(AttackStat.Bloodlust, currentHitCount))
            {
                float hPercent = target.GetPercentage();
                float mult = 100;
                if (hPercent < 50)
                {
                    mult += 78 - Mathf.Floor(hPercent/5) * 4;
                }
                else if (hPercent < 95)
                {
                    mult += 76 - Mathf.Floor(hPercent/5) * 4;
                }
                atkPower *= (mult/100);
            }

            float enemyDef = target.GetComponentInParent<BaseStats>().GetStat(Stat.Defense);
            total = Mathf.Ceil(atkPower/enemyDef);

            if(CalculateCriticalHit())
            {
                float critPower = baseStats.GetStat(Stat.CritDamage)/100;
                total *= critPower;
            }

            return total;
        }

        private bool CalculateCriticalHit()
        {
            float baseCritChance = 0;
            isCrit = false;

            float skillCritMod = GetStat(AttackStat.CritMod, currentHitCount);
            float critFactor = baseStats.GetStat(Stat.CritFactor);
            float targetCritResist = target.GetComponentInParent<BaseStats>().GetStat(Stat.CritResist);
            // Need glyph factor bonus to be added
            baseCritChance = (1.2f * skillCritMod * critFactor)/(10f * targetCritResist);

            if (baseCritChance >= 1)
            {
                isCrit = true;
            }

            float critChance = 0;
            // Add glyph multiplier bonus later
            float glyphBonusMult = 1f;

            critChance = baseCritChance + glyphBonusMult * baseCritChance * (1-baseCritChance);
            if (critChance >= 1 || Random.Range(0f, 1f) <= critChance)
            {
                isCrit = true;
            }

            return isCrit;
        }

        public void CalculateReflectDamage()
        {
            float total = 0;
            float atkPower = baseStats.GetStat(Stat.AttackPower);
            float lastHitPower = GetStat(AttackStat.Power, (int)maxHitCount-1);
            atkPower *= lastHitPower;

            if (GetStatBool(AttackStat.Bloodlust, (int)maxHitCount-1))
            {
                float hPercent = target.GetPercentage();
                float mult = 100;
                if (hPercent < 50)
                {
                    mult += 78 - Mathf.Floor(hPercent / 5) * 4;
                }
                else if (hPercent < 95)
                {
                    mult += 76 - Mathf.Floor(hPercent / 5) * 4;
                }
                atkPower *= (mult / 100);
            }

            float enemyDef = target.GetComponentInParent<BaseStats>().GetStat(Stat.Defense);
            total = Mathf.Ceil(atkPower / enemyDef);

            // Reflect always crits
            float critPower = baseStats.GetStat(Stat.CritDamage) / 100;
            total *= critPower;

            //Reflect is 40% damage
            total *= 0.4f;

            target.TakeDamage(total, true);
        }

        private float[] GetSkillTimes()
        {
            float[] skillTimes = new float[3];
            float atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
            atkSpeed = 1/atkSpeed * 100;
            skillTimes[0] = GetStat(AttackStat.AnimLock, 0);
            skillTimes[1] = GetStat(AttackStat.SkillLock, 0);
            skillTimes[2] = GetStat(AttackStat.TotalTime, 0);
            for (int i = 0; i < skillTimes.Length; i++)
            {
                skillTimes[i] = skillTimes[i] * atkSpeed;
            }
            return skillTimes;
        }

        private void CheckEffectActivation(bool isHit)
        {
            string[] atkFXid = attackDB.GetAttackStat(AttackStat.EffectID, attackType);
            string[] atkFXTarget = attackDB.GetAttackStat(AttackStat.EffectTarget, attackType);
            for (int i = 0; i < isEffectOnHit.Length; i++)
            {
                if(isEffectOnHit[i] == isHit)
                    fighter.PassEffect(atkFXid[i], atkFXTarget[i]);
            }
        }

        public void SetTarget(GameObject _target)
        {
            target = _target.GetComponent<Health>();
        }

        public float GetStat(AttackStat stat, int index)
        {
            float value = attackValues.GetAttackStat(stat, attackType, index);
            return value;
        }

        public float[] GetStatArray(AttackStat stat)
        {
            float[] atkFloats = attackValues.GetAttackStatArray(stat, attackType);
            return atkFloats;
        }

        public bool GetStatBool(AttackStat stat, int index)
        {
            string value = attackValues.GetAttackStatBool(stat, attackType, index);
            return bool.Parse(value);
        }

        public string GetStatString(AttackStat stat)
        {
            return attackDB.GetAttackStat(stat, attackType)[0];
        }

        public void StartCooldown()
        {
            timeSinceAtkStart += Time.deltaTime;
            if (timeSinceAtkStart >= cooldownDelay)
            {
                timeSinceAtkOnCooldown = 0;
                thisButton.targetGraphic.CrossFadeColor(thisButton.colors.disabledColor, thisButton.colors.fadeDuration, true, true);
                fighter.activeAttack -= StartCooldown;
                fighter.activeAttack += UpdateTimer;
            }
        }

        public void UpdateTimer()
        {
            timeSinceAtkOnCooldown += Time.deltaTime;
            if (timeSinceAtkOnCooldown >= maxCooldown)
            {
                ResetButton();
            }
            else
            {
                atkNameText.text = string.Format("{0:0.000}", maxCooldown - timeSinceAtkOnCooldown);
                float overlayPercent = Mathf.Clamp01((maxCooldown - timeSinceAtkOnCooldown)/maxCooldown);
                btnOverlay.localScale = new Vector3(overlayPercent, 1, 1);
            }
        }

        public void Cancel()
        {
            StopAttack();
        }

        private void StopAttack()
        {
            for (int i = 0; i < hitTimers.Count; i++)
            {
                hitTimers[i].Cancel();
            }
            hitTimers.Clear();
            if (timeSinceAtkStart < cooldownDelay)
            {
                fighter.activeAttack -= StartCooldown;
            }
        }

        public void StartBattle()
        {
            isBattleActive = true;
        }

        public void EndBattle()
        {
            isBattleActive = false;
        }
    }
}