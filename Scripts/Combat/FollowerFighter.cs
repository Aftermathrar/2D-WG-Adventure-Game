using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class FollowerFighter : Fighter
    {
        public override void RecalculateStats()
        {
            cdr = baseStats.GetStat(Stat.CooldownReduction);
            atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
        }
        
        public bool HasEffect(string ID)
        {
            return effects.HasEffect(ID);
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
            effects.BuffPlayer(ID);
        }

        public void PassEffectParty(string ID)
        {
            effects.BuffSelf(ID);
            effects.BuffPlayer(ID);
        }
    }
}