using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "AttackDB", menuName = "Stats/AttackDB", order = 1)]
    public class AttackDB : ScriptableObject
    {
        [SerializeField] DBAttackType[] attackTypeDB = null;

        Dictionary<AttackType, Dictionary<AttackStat, string[]>> lookupTable = null;

        public string[] GetAttackStat(AttackStat stat, AttackType attackType)
        {
            BuildLookup();

            return lookupTable[attackType][stat];
        }

        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<AttackType, Dictionary<AttackStat, string[]>>();
            
            foreach (DBAttackType atkDB in attackTypeDB)
            {
                var statLookupTable = new Dictionary<AttackStat, string[]>();

                foreach (AttackTypeStats atkStats in atkDB.attackStats)
                {
                    statLookupTable[atkStats.stat] = atkStats.value;
                }
                lookupTable[atkDB.attackType] = statLookupTable;
            }
        }
    }

    [System.Serializable]
    class DBAttackType
    { 
        public AttackType attackType;
        public AttackTypeStats[] attackStats;
    }

    [System.Serializable]
    class AttackTypeStats
    {
        public AttackStat stat;
        public string[] value;
    }
}

