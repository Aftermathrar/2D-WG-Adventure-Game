using System;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{

    public class Fighter : MonoBehaviour
    {
        // Damage calculation handled on atkBtnScripts
        // Get target to apply the damage
        // Get effect from atk to pass to CombatEffects

        protected CombatEffects effects;
        // protected Health target;

        float invulnDuration = 0f;
        float invulnTimer = Mathf.Infinity;

        // Caching stats for Skills
        protected BaseStats baseStats;
        protected float atkPower;
        protected float atkSpeed;
        protected float damageMod;
        protected float cdr;
        protected float critFactor;
        protected float critPower;

        public event Action activeAttack;
        
        protected virtual void Awake() 
        {
            baseStats = GetComponent<BaseStats>();
            effects = GetComponent<CombatEffects>();
        }

        protected virtual void Start() 
        {
            // target = effects.GetTarget();
            RecalculateStats();
        }

        private void Update() 
        {
            activeAttack?.Invoke();
        }

        public void StartInvulnTime(float invulnTime)
        {
            effects.ToggleInvuln();
            invulnDuration = invulnTime;
            invulnTimer = 0;
            activeAttack += InvulnTimer;
        }

        public void InvulnTimer()
        {
            invulnTimer += Time.deltaTime;
            if(invulnTimer >= invulnDuration)
            {
                effects.ToggleInvuln();
                activeAttack -= InvulnTimer;
            }
        }

        public virtual void RecalculateStats()
        {
            atkPower = baseStats.GetStat(Stat.AttackPower);
            atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
            damageMod = baseStats.GetStat(Stat.Damage);
            critFactor = baseStats.GetStat(Stat.CritFactor);
            cdr = baseStats.GetStat(Stat.CooldownReduction);
            critPower = baseStats.GetStat(Stat.CritDamage);
        }

        public float GetStat(Stat stat)
        {
            float value;
            switch (stat)
            {
                case Stat.AttackPower:
                    value = atkPower;
                    break;
                case Stat.AttackSpeed:
                    value = atkSpeed;
                    break;
                case Stat.Damage:
                    value = damageMod;
                    break;
                case Stat.CooldownReduction:
                    value = cdr;
                    break;
                case Stat.CritFactor:
                    value = critFactor;
                    break;
                case Stat.CritDamage:
                    value = critPower;
                    break;
                default:
                    Debug.Log(stat.ToString() + " not found in fighter class");
                    value = 0;
                    break;
            }
            return value;
        }

        public virtual void PassEffect(string ID, string effectTarget)
        {
            if(ID != "None")
            {
                if(effectTarget == "Self")
                {
                    effects.BuffSelf(ID);
                }
                else
                {
                    effects.DebuffTarget(ID);
                }
            }
        }
    }
}
