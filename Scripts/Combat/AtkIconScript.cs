using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Character;
using ButtonGame.Core;
using ButtonGame.Attributes;
using TMPro;
using ButtonGame.UI;

namespace ButtonGame.Combat
{
    public class AtkIconScript : MonoBehaviour, IAction, IBattleState, IAtkSkill
    {
        [SerializeField] AttackType attackType;
        [SerializeField] KeyCode keyCode;
        [SerializeField] AttackDB attackDB = null;
        protected BaseAttackStats attackValues = null;
        protected PlayerController player = null;
        protected GuardController guard = null;
        protected Mana mana;
        protected Health target;
        protected BaseStats targetStats;
        protected Fighter fighter;
        protected Button thisButton;
        protected Sprite atkIcon = null;

        [SerializeField] protected TextMeshProUGUI atkCooldownText = null;
        [SerializeField] Image atkCooldownOverlay = null;
        [SerializeField] ParticleSystem clickEffect = null;
        [SerializeField] ParticleSystem ResetEffect = null;
        [SerializeField] TextMeshProUGUI atkKeyCode = null;
        [SerializeField] TextMeshProUGUI atkPriorityText = null;
        [SerializeField] CanvasGroup cdResetOverlay = null;

        protected float timeSinceAtkOnCooldown = Mathf.Infinity;
        protected float timeSinceAtkStart = Mathf.Infinity;
        protected float timeSinceLastHit = Mathf.Infinity;
        protected int currentHitCount;
        protected float cooldownDelay;
        float maxCooldown;
        protected float maxTimeToHit;
        float skillPriority;
        protected float maxHitCount;
        float manaCost;
        protected bool isCrit = false;
        bool[] isEffectOnHit;
        protected bool isBattleActive;

        [SerializeField] HitTimerSpawner hitTimerSpawner = null;
        Coroutine spawnHitTimer = null;
        protected List<HitTimer> hitTimers = new List<HitTimer>();

        protected UIShake shaker = null;
        [SerializeField] protected float shakeDuration = 0f;
        [SerializeField] protected float shakeMagnitude = 0;
        [SerializeField] protected int shakeCount = 0;

        public void SetTarget(Health _target)
        {
            target = _target;
            targetStats = target.GetComponentInParent<BaseStats>();
            shaker = target.GetComponent<UIShake>();
        }

        protected virtual void Awake()
        {
            thisButton = GetComponent<Button>();
            atkIcon = GetComponent<Image>().sprite;
            player = GetComponentInParent<PlayerController>();
            guard = player.GetComponent<GuardController>();
            mana = player.GetComponent<Mana>();
            fighter = player.GetComponent<Fighter>();
            attackValues = player.GetComponent<BaseAttackStats>();
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
            if (isBattleActive)
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
                GetComponentInParent<ActionScheduler>().StartAction(this);
                timeSinceAtkStart = 0;
                timeSinceLastHit = 0;
                currentHitCount = 0;
                float atkSpeed = fighter.GetStat(Stat.AttackSpeed);
                atkSpeed = 1 / atkSpeed * 100;
                cooldownDelay = GetStat(AttackStat.CDStart, 0) * atkSpeed;
                float cdr = fighter.GetStat(Stat.CooldownReduction) / 100;
                maxCooldown = GetStat(AttackStat.Cooldown, 0) * (1 - cdr);
                clickEffect.Stop();
                var main = clickEffect.main;
                if (clickEffect.isStopped) main.duration = Mathf.Max(0.1f, cooldownDelay);
                clickEffect.Play();

                mana.UseMana(manaCost);
                float[] skillTimes = GetSkillTimes(atkSpeed);
                player.StartAttack(skillTimes);
                CheckEffectActivation(false);

                if (GetStatBool(AttackStat.Block, 0))
                {
                    guard.StartBlock(skillTimes[2], this);
                }
                else
                {
                    guard.CancelBlock();
                }

                if (GetStat(AttackStat.InvulnTime, 0) > 0)
                {
                    fighter.StartInvulnTime(GetStat(AttackStat.InvulnTime, 0));
                }

                if (maxHitCount > 0)
                {
                    spawnHitTimer = StartCoroutine(SpawnHitTimer(atkSpeed));
                }
                fighter.activeAttack += StartCooldown;
            }
        }

        private IEnumerator SpawnHitTimer(float atkSpeed)
        {
            maxTimeToHit = GetStat(AttackStat.TimeToHit, 0);
            fighter.activeAttack += UpdateTimeToHit;
            float newPosX = 0;
            HitTimer hitTimerInstance = hitTimerSpawner.SpawnHitTimer(newPosX);
            hitTimers.Add(hitTimerInstance);
            hitTimerInstance.SetSprite(atkIcon);

            maxTimeToHit = maxTimeToHit * atkSpeed;
            hitTimerInstance.SetFighter(fighter);
            hitTimerInstance.SetFillTime(maxTimeToHit);

            for (int i = 1; i < maxHitCount; i++)
            {
                newPosX -= 75;
                hitTimerInstance = hitTimerSpawner.SpawnHitTimer(newPosX);
                hitTimers.Add(hitTimerInstance);
                hitTimerInstance.SetSprite(atkIcon);

                yield return null;
            }
            spawnHitTimer = null;
            yield return null;
        }

        protected virtual void UpdateTimeToHit()
        {
            timeSinceLastHit += Time.deltaTime;

            if (hitTimers.Count == 0)
            {
                fighter.activeAttack -= UpdateTimeToHit;
                return;
            }

            if (timeSinceLastHit >= maxTimeToHit)
            {
                hitTimers.Remove(hitTimers[0]);
                timeSinceLastHit = 0;
                // On deal damage, calculate current buffs and update next buffs

                float damage = CalculateDamage();
                if (target.TakeDamage(damage, isCrit))
                {
                    shaker = target.GetComponent<UIShake>();
                    StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude, shakeCount));
                    player.DamageDealt(damage);
                    CheckEffectActivation(true);
                }
                currentHitCount += 1;

                if (currentHitCount >= maxHitCount || hitTimers.Count == 0)
                {
                    fighter.activeAttack -= UpdateTimeToHit;
                }
                else
                {
                    timeSinceLastHit = 0;
                    for (int i = 0; i < hitTimers.Count; i++)
                    {
                        hitTimers[i].SetFighter(fighter);
                        hitTimers[i].SetMoveTime(.2f);
                    }

                    maxTimeToHit = GetStat(AttackStat.TimeToHit, currentHitCount);
                    float atkSpeed = fighter.GetStat(Stat.AttackSpeed);
                    atkSpeed = 1 / atkSpeed * 100;
                    maxTimeToHit *= atkSpeed;

                    hitTimers[0].SetFillTime(maxTimeToHit);
                }
            }
        }

        public bool IsAttackReady()
        {
            if (timeSinceAtkOnCooldown >= maxCooldown && isBattleActive)
            {
                return true;
            }
            return false;
        }

        public bool HasEnoughMana()
        {
            float curMana = mana.GetAttributeValue();
            if (curMana < manaCost)
            {
                return false;
            }
            return true;
        }

        protected void ResetButton()
        {
            atkCooldownText.gameObject.SetActive(false);
            thisButton.targetGraphic.CrossFadeColor(thisButton.colors.normalColor, thisButton.colors.fadeDuration, true, true);
            atkCooldownOverlay.fillAmount = 0;
            fighter.activeAttack -= UpdateTimer;
        }

        protected float CalculateDamage()
        {
            float total = 0;
            float atkPower = fighter.GetStat(Stat.AttackPower);
            atkPower *= GetStat(AttackStat.Power, currentHitCount);

            if (GetStatBool(AttackStat.Bloodlust, currentHitCount))
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

            // Reduce by enemy defense
            float enemyDef = targetStats.GetStat(Stat.Defense);
            float enemyReduction = targetStats.GetStat(Stat.DamageReduction) / 100;
            total = Mathf.Ceil(atkPower / enemyDef) * (1 - enemyReduction);

            // Get player damage modifier
            float damageMod = 1 + fighter.GetStat(Stat.Damage) / 100;
            total *= damageMod;

            if (CalculateCriticalHit())
            {
                float critPower = fighter.GetStat(Stat.CritDamage) / 100;
                total *= critPower;
            }

            return total;
        }

        private bool CalculateCriticalHit()
        {
            float baseCritChance = 0;
            isCrit = false;

            float skillCritMod = GetStat(AttackStat.CritMod, currentHitCount);
            float critFactor = fighter.GetStat(Stat.CritFactor);
            float targetCritResist = targetStats.GetStat(Stat.CritResist);
            // Need glyph factor bonus to be added
            baseCritChance = (1.2f * skillCritMod * critFactor) / (10f * targetCritResist);

            if (baseCritChance >= 1)
            {
                isCrit = true;
            }

            float critChance = 0;
            // Add glyph multiplier bonus later
            float glyphBonusMult = 1f;

            critChance = baseCritChance + glyphBonusMult * baseCritChance * (1 - baseCritChance);
            if (critChance >= 1 || Random.Range(0f, 1f) <= critChance)
            {
                isCrit = true;
            }

            return isCrit;
        }

        public virtual void CalculateReflectDamage()
        {
            float total = 0;
            float atkPower = fighter.GetStat(Stat.AttackPower);
            float lastHitPower = GetStat(AttackStat.Power, (int)maxHitCount - 1);
            atkPower *= lastHitPower;

            if (GetStatBool(AttackStat.Bloodlust, (int)maxHitCount - 1))
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

            float enemyDef = targetStats.GetStat(Stat.Defense);
            float enemyReduction = targetStats.GetStat(Stat.DamageReduction) / 100;
            total = Mathf.Ceil(atkPower / enemyDef) * (1 - enemyReduction);

            // Reflect always crits
            float critPower = fighter.GetStat(Stat.CritDamage) / 100;
            total *= critPower;

            // Get player damage modifier
            float damageMod = 1 + fighter.GetStat(Stat.Damage) / 100;
            total *= damageMod;

            //Reflect is 40% damage
            total *= 0.4f;

            shaker = target.GetComponent<UIShake>();
            StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude / 2, shakeCount));
            target.TakeDamage(total, true);
            player.DamageDealt(total);
        }

        private float[] GetSkillTimes(float atkSpeed)
        {
            float[] skillTimes = new float[3];
            skillTimes[0] = GetStat(AttackStat.AnimLock, 0) * atkSpeed;
            skillTimes[1] = GetStat(AttackStat.SkillLock, 0) * atkSpeed;
            skillTimes[2] = GetStat(AttackStat.TotalTime, 0) * atkSpeed;
            return skillTimes;
        }

        protected void CheckEffectActivation(bool isHit)
        {
            string[] atkFXid = attackDB.GetAttackStat(AttackStat.EffectID, attackType);
            string[] atkFXTarget = attackDB.GetAttackStat(AttackStat.EffectTarget, attackType);
            for (int i = 0; i < isEffectOnHit.Length; i++)
            {
                if (isEffectOnHit[i] == isHit)
                    fighter.PassEffect(atkFXid[i], atkFXTarget[i]);
            }
        }

        private void AssignSkillInfoText()
        {
            if (skillPriority == 1)
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
            if (keyString.Contains("Alpha"))
            {
                atkKeyCode.text = keyString.Substring(keyString.Length - 1);
            }
            else
            {
                atkKeyCode.text = keyCode.ToString();
            }
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

        public virtual void StartCooldown()
        {
            timeSinceAtkStart += Time.deltaTime;
            if (timeSinceAtkStart >= cooldownDelay)
            {
                timeSinceAtkOnCooldown = 0;
                atkCooldownText.gameObject.SetActive(true);
                fighter.activeAttack -= StartCooldown;
                fighter.activeAttack += UpdateTimer;
            }
        }

        public void UpdateTimer()
        {
            timeSinceAtkOnCooldown += Time.deltaTime;
            if (timeSinceAtkOnCooldown >= maxCooldown)
            {
                ResetEffect.Play();
                StartCoroutine(FadeResetOverlay());
                ResetButton();
            }
            else
            {
                atkCooldownText.text = string.Format("{0:0}", Mathf.Ceil(maxCooldown - timeSinceAtkOnCooldown));
                float overlayPercent = Mathf.Clamp01((maxCooldown - timeSinceAtkOnCooldown) / maxCooldown);
                atkCooldownOverlay.fillAmount = overlayPercent;
            }
        }

        private IEnumerator FadeResetOverlay()
        {
            cdResetOverlay.alpha = 1;

            while(cdResetOverlay.alpha > 0)
            {
                cdResetOverlay.alpha -= Time.deltaTime / 0.25f;
                yield return null;
            }
            yield return null;
        }

        public void Cancel()
        {
            StopAttack();
        }

        private void StopAttack()
        {
            if (spawnHitTimer != null)
                StopCoroutine(spawnHitTimer);

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
            StopAttack();
            timeSinceAtkStart = Mathf.Infinity;
            timeSinceAtkOnCooldown = Mathf.Infinity;
            isBattleActive = true;
        }

        public void EndBattle()
        {
            Cancel();
            isBattleActive = false;
        }
    }
}