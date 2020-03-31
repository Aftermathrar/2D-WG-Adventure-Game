using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class AttackValues : MonoBehaviour
    {
        [SerializeField] AttackDB attackDB;

        public string[] GetAttackStatBool(AttackStat stat, AttackType attackType)
        {
            string[] sAtkBool = attackDB.GetAttackStat(stat, attackType);
            string[] resultBools = new string[sAtkBool.Length];
            for (int i = 0; i < sAtkBool.Length; i++)
            {
                foreach (IAttackEffectProvider fxAtk in GetComponents<IAttackEffectProvider>())
                {
                    foreach (bool fxValue in fxAtk.GetAtkBooleanModifiers(attackType, stat))
                    {
                        resultBools[i] = (bool.Parse(sAtkBool[i]) || fxValue).ToString();
                    }
                }
            }
            return resultBools;
        }
    }
}
