using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using ButtonGame.UI;
using UnityEngine;

namespace ButtonGame.Combat.Skills
{
    public class Guard : AtkIconScript
    {
        public override void CalculateReflectDamage()
        {
            // Guard has 60% chance to increase attack by 25%
            int procChance = Random.Range(0, 5);
            if(procChance <= 2)
            {
                fighter.PassEffect(EffectName.AbsorbPower.ToString(), EffectTarget.Self.ToString());
            }

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
            StartCoroutine(shaker.Shake(0.2f, 7.5f, 3));
            target.TakeDamage(total, true);
            player.DamageDealt(total);
        }
    }
}
