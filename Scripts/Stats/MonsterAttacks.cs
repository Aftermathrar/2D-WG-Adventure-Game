using System.Collections;
using System.Collections.Generic;
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
                total[i] = (float.Parse(s[i]));
            }
            return total;
        }

        public float GetAttackStat(MonAtkStat stat, MonAtkName atkName, int i)
        {
            string[] s = GetBaseAttackStat(stat, atkName);
            float total = 0;
            
            total = float.Parse(s[i]);
            
            return total;
        }
        
        private string[] GetBaseAttackStat(MonAtkStat stat, MonAtkName atkName)
        {
            return monAtkDB.GetAttackStat(stat, atkName);
        }
        
        // Add modifiers after implementing monster attack effect interface

    }

}