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
                total[i] = (float.Parse(s[i]) + GetAdditiveModifier(stat, attackType)) * (1 + GetPercentageModifier(stat, attackType)/100);
            }
            
            return total;
        }

        public float GetAttackStat(AttackStat stat, AttackType attackType, int i)
        {
            string[] s = GetBaseAttackStat(stat, attackType);
            float total = 0;

            total = (float.Parse(s[i]) + GetAdditiveModifier(stat, attackType)) * (1 + GetPercentageModifier(stat, attackType) / 100);

            return total;
        }

        private string[] GetBaseAttackStat(AttackStat stat, AttackType attackType)
        {
            return attackDB.GetAttackStat(stat, attackType);
        }

        private float GetAdditiveModifier(AttackStat stat, AttackType attackType)
        {
            float total = 0;
            foreach (IAttackEffectProvider fxProvider in GetComponents<IAttackEffectProvider>())
            {
                foreach (float modifier in fxProvider.GetAtkAddivitiveModifiers(attackType, stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetPercentageModifier(AttackStat stat, AttackType attackType)
        {
            float total = 0;
            foreach (IAttackEffectProvider fxProvider in GetComponents<IAttackEffectProvider>())
            {
                foreach (float modifier in fxProvider.GetAtkPercentageModifiers(attackType, stat))
                {
                    total += modifier;
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
