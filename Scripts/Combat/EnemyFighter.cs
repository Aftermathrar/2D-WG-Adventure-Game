using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class EnemyFighter : Fighter
    {
        public override void RecalculateStats()
        {
            atkPower = baseStats.GetStat(Stat.AttackPower);
            atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
            critFactor = baseStats.GetStat(Stat.CritFactor);
        }
    }
}
