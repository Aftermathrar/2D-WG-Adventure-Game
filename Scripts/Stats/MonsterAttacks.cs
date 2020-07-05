using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class MonsterAttacks : MonoBehaviour
    {
        [SerializeField] MonAtkDB monAtkDB;

        public float[] GetAttackStatArray(MonAtkStat stat, MonAtkName atkName)
        {
            string[] s = GetBaseAttackStat(stat, atkName);
            float[] total = new float[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                total[i] = (float.Parse(s[i]) + GetAtkStatModifiers(stat, atkName)[1]) * (1 + GetAtkStatModifiers(stat, atkName)[0] / 100);
            }
            return total;
        }

        public float GetAttackStat(MonAtkStat stat, MonAtkName atkName, int i)
        {
            string[] s = GetBaseAttackStat(stat, atkName);
            float total = 0;
            
            total = (float.Parse(s[i]) + GetAtkStatModifiers(stat, atkName)[1]) * (1 + GetAtkStatModifiers(stat, atkName)[0] / 100);
            // print(atkName.ToString() + " " + stat.ToString() + " has a value of " + total);

            return total;
        }
        
        private string[] GetBaseAttackStat(MonAtkStat stat, MonAtkName atkName)
        {
            return monAtkDB.GetAttackStat(stat, atkName);
        }

        private float[] GetAtkStatModifiers(MonAtkStat stat, MonAtkName atkName)
        {
            float[] total = new float[] { 0, 0 };
            foreach (IMonAtkEffectProvider fxProvider in GetComponents<IMonAtkEffectProvider>())
            {
                foreach (float[] modifier in fxProvider.GetMonAtkStatModifiers(atkName, stat))
                {
                    total[0] += modifier[0];
                    total[1] += modifier[1];
                }
            }
            return total;
        }
        
        // Add modifiers after implementing monster attack effect interface

    }

}