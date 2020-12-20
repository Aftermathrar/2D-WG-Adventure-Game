using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class FollowerFighter : Fighter
    {
        CombatEffects playerEffects;

        protected override void Awake()
        {
            baseStats = GetComponent<BaseStats>();
            effects = GetComponent<CombatEffects>();
            effects = GetComponent<CombatEffects>();
            playerEffects = GameObject.FindGameObjectWithTag("Player").GetComponent<CombatEffects>();
        }

        protected override void Start()
        {
            // target = effects.GetTarget();
            RecalculateStats();
        }

        public override void RecalculateStats()
        {
            cdr = baseStats.GetStat(Stat.CooldownReduction);
            atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
        }
        
        public bool HasEffect(string ID, bool lookAtPlayer)
        {
            if(lookAtPlayer)
            {
                return playerEffects.HasEffect(ID);
            }
            return effects.HasEffect(ID);
        }

        public float GetEffectElapsedTime(string ID, bool lookAtPlayer)
        {
            if (lookAtPlayer)
            {
                return playerEffects.GetEffectElapsedTime(ID);
            }
            return effects.GetEffectElapsedTime(ID);
        }

        public void ClearEffect(string debuffID)
        {
            playerEffects.ClearEffect(debuffID);
        }

        public void PassEffectEnemy(string ID)
        {
            effects.DebuffTarget(ID);
        }

        public void PassEffectSelf(string ID)
        {
            effects.BuffSelf(ID);
        }

        public void PassEffectPlayer(string ID)
        {
            playerEffects.BuffSelf(ID);
        }

        public void PassEffectParty(string ID)
        {
            effects.BuffSelf(ID);
            playerEffects.BuffSelf(ID);
        }
    }
}