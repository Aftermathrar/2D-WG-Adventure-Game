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
    public class AtkBtnScript : MonoBehaviour, IAction
    {
        [SerializeField] AttackType attackType;
        [SerializeField] KeyCode keyCode;
        [SerializeField] AttackDB attackDB = null;
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
        bool isCrit = false;
        bool isEffectOnHit = false;

        private Button _button;

        [SerializeField] HitTimer hitTimer;
        List<HitTimer> hitTimers = new List<HitTimer>();

        private void Awake() 
        {
            _button = GetComponent<Button>();
            player = GetComponentInParent<PlayerController>();
            guard = player.GetComponent<GuardController>();
            mana = player.GetComponent<Mana>();
            baseStats = player.GetComponent<BaseStats>();
            fighter = player.GetComponent<Fighter>();
        }

        private void Start() 
        {
            maxHitCount = GetStat(AttackStat.HitCount);
            maxCooldown = GetStat(AttackStat.Cooldown);
            skillPriority = GetStat(AttackStat.Priority);
            isEffectOnHit = GetStatString(AttackStat.ApplyEffect) == "OnHit";
            ResetButton();
            AssignSkillInfoText();
        }

        private void Update() 
        {
            if(Input.GetKeyDown(keyCode))
            {
                _button.targetGraphic.CrossFadeColor(_button.colors.pressedColor, _button.colors.fadeDuration, true, true);
                _button.onClick.Invoke();
            }
            else if (Input.GetKeyUp(keyCode))
            {
                _button.targetGraphic.CrossFadeColor(_button.colors.normalColor, _button.colors.fadeDuration, true, true);
            }
        }

        public void OnClick()
        {
            if (target == null) return;
            if (IsAttackReady() && player.CanAttack(skillPriority) && HasEnoughMana())
            {
                clickEffect.Play();
                timeSinceAtkStart = 0;
                timeSinceLastHit = 0;
                currentHitCount = 0;
                float atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
                atkSpeed = 1 / atkSpeed * 100;

                GetComponentInParent<ActionScheduler>().StartAction(this);
                cooldownDelay = GetStat(AttackStat.CDStart) * atkSpeed;
                maxCooldown = GetStat(AttackStat.Cooldown) * atkSpeed;
                mana.UseMana(GetStat(AttackStat.Cost));
                float[] skillTimes = GetSkillTimes();
                player.StartAttack(skillTimes);
                if(!isEffectOnHit)
                {
                    fighter.PassEffect(GetStat(AttackStat.EffectID), GetStatString(AttackStat.EffectTarget));
                }

                if(GetStatBool(AttackStat.Block))
                {
                    guard.StartBlock(skillTimes[2], this);
                }
                else
                {
                    guard.CancelBlock();
                }

                if(GetStat(AttackStat.InvulnTime) > 0)
                {
                    fighter.StartInvulnTime(GetStat(AttackStat.InvulnTime));
                }

                if(maxHitCount > 0) 
                { 
                    maxTimeToHit = GetStatArray(Stats.AttackStat.TimeToHit);
                    fighter.activeAttack += UpdateTimeToHit;
                }
                fighter.activeAttack += StartCooldown;

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
            if (curMana < GetStat(AttackStat.Cost))
            {
                return false;
            }
            return true;
        }

        public void ResetButton()
        {
            atkNameText.text = GetStatString(AttackStat.Name);
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

            float maskPercent = 0;
            maskPercent = Mathf.Clamp01((maxTimeToHit[currentHitCount] - timeSinceLastHit) / maxTimeToHit[currentHitCount]);

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
                target.TakeDamage(damage, isCrit);
                if(isEffectOnHit)
                {
                    fighter.PassEffect(GetStat(AttackStat.EffectID), GetStatString(AttackStat.EffectTarget));
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
            float AtkPower = baseStats.GetStat(Stat.Attack);
            AtkPower *= GetStatArray(AttackStat.Power)[currentHitCount];

            if(GetStatBool(AttackStat.Bloodlust))
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
                AtkPower *= (mult/100);
            }

            float enemyDef = target.GetComponentInParent<BaseStats>().GetStat(Stat.Defense);

            total = Mathf.Ceil(AtkPower/enemyDef);

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

            float v = GetStatArray(AttackStat.CritMod)[currentHitCount];
            float v1 = baseStats.GetStat(Stat.CritFactor);
            float v2 = target.GetComponentInParent<BaseStats>().GetStat(Stat.CritResist);
            // Need glyph factor bonus to be added
            baseCritChance = (1.2f * v * v1)/(10f * v2);

            if (baseCritChance >= 1)
            {
                isCrit = true;
            }

            float critChance = 0;
            // Add glyph multiplier bonus
            float glyphBonusMult = 1f;

            critChance = baseCritChance + glyphBonusMult * baseCritChance * (1-baseCritChance);
            if (critChance >= 1)
            {
                isCrit = true;
            }
            if(Random.Range(0f, 1f) <= critChance)
            {
                isCrit = true;
            }

            return isCrit;
        }

        public void CalculateReflectDamage()
        {
            float total = 0;
            float atkPower = baseStats.GetStat(Stat.Attack);
            float[] lastHitPower = GetStatArray(AttackStat.Power);
            atkPower *= lastHitPower[lastHitPower.Length - 1];

            if (GetStatBool(AttackStat.Bloodlust))
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
            skillTimes[0] = GetStat(AttackStat.AnimLock);
            skillTimes[1] = GetStat(AttackStat.SkillLock);
            skillTimes[2] = GetStat(AttackStat.TotalTime);
            for (int i = 0; i < skillTimes.Length; i++)
            {
                skillTimes[i] = skillTimes[i] * atkSpeed;
            }
            return skillTimes;
        }

        public void SetTarget(GameObject _target)
        {
            target = _target.GetComponent<Health>();
        }

        public float GetStat(AttackStat stat)
        {
            return float.Parse(attackDB.GetAttackStat(stat, attackType)[0]);
        }

        public float[] GetStatArray(AttackStat stat)
        {
            string[] atkStats = null;
            atkStats = attackDB.GetAttackStat(stat, attackType);
            float[] atkFloats = new float[atkStats.Length];
            for (int i = 0; i < atkStats.Length; i++)
            {
                atkFloats[i] = float.Parse(atkStats[i]);
            }
            return atkFloats;
        }

        public bool GetStatBool(AttackStat stat)
        {
            string value = GetComponentInParent<AttackValues>().GetAttackStatBool(stat, attackType)[0];
            Debug.Log("Attack " + attackType.ToString() + " stat " + stat.ToString() + ": " + value);
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
    }
}