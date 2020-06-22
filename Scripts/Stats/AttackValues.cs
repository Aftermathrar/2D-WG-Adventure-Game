using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class AttackValues : MonoBehaviour
    {
        [SerializeField] AttackDB attackDB;

        public float[] GetAttackStatArray(AttackStat stat, AttackType attackType)
        {
            string[] s = GetBaseAttackStat(stat, attackType);
            float[] total = new float[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                total[i] = (float.Parse(s[i]) + GetAtkStatModifiers(stat, attackType)[1]) * (1 + GetAtkStatModifiers(stat, attackType)[0] / 100);
            }
            
            return total;
        }

        public float GetAttackStat(AttackStat stat, AttackType attackType, int i)
        {
            string[] s = GetBaseAttackStat(stat, attackType);
            float total = 0;

            // For stat modifiers, 0 is Additive, 1 is Percentage. From .asset values
            total = (float.Parse(s[i]) + GetAtkStatModifiers(stat, attackType)[1]) *(1 + GetAtkStatModifiers(stat, attackType)[0] / 100);

            return total;
        }

        private string[] GetBaseAttackStat(AttackStat stat, AttackType attackType)
        {
            return attackDB.GetAttackStat(stat, attackType);
        }

        private float[] GetAtkStatModifiers(AttackStat stat, AttackType attackType)
        {
            float[] total = new float[] {0, 0};
            foreach (IAttackEffectProvider fxProvider in GetComponents<IAttackEffectProvider>())
            {
                foreach (float[] modifier in fxProvider.GetAtkStatModifiers(attackType, stat))
                {
                    total[0] += modifier[0];
                    total[1] += modifier[1];
                }
            }
            return total;
        }

        public string GetAttackStatBool(AttackStat stat, AttackType attackType, int index)
        {
            string sAtkBool = attackDB.GetAttackStat(stat, attackType)[index];
            string resultBools = null;
            foreach (IAttackEffectProvider fxAtk in GetComponents<IAttackEffectProvider>())
            {
                foreach (bool fxValue in fxAtk.GetAtkBooleanModifiers(attackType, stat))
                {
                    resultBools = (bool.Parse(sAtkBool) || fxValue).ToString();
                }
            }
            return resultBools;
        }
    }
}
