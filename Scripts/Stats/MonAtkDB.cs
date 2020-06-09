using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "MonsterAttackDB", menuName = "Stats/MonAtkDB", order = 3)]
    public class MonAtkDB : ScriptableObject
    {
        [SerializeField] DBMonsterAtkName[] monsterAtkMovesetDB = null;

        Dictionary<MonAtkName, Dictionary<MonAtkStat, string[]>> lookupTable = null;

        public string[] GetAttackStat(MonAtkStat stat, MonAtkName monAtkName)
        {
            BuildLookup();

            return lookupTable[monAtkName][stat];
        }

        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<MonAtkName, Dictionary<MonAtkStat, string[]>>();

            foreach (DBMonsterAtkName monAtkDB in monsterAtkMovesetDB)
            {
                var statLookupTable = new Dictionary<MonAtkStat, string[]>();

                foreach (MonAttackStats atkStats in monAtkDB.monAtkStats)
                {
                    statLookupTable[atkStats.stat] = atkStats.value;
                }
                lookupTable[monAtkDB.monAtkName] = statLookupTable;
            }
        }
    }

    [System.Serializable]
    class DBMonsterAtkName
    {
        public MonAtkName monAtkName;
        public MonAttackStats[] monAtkStats;
    }

    [System.Serializable]
    class MonAttackStats
    {
        public MonAtkStat stat;
        public string[] value;
    }
}