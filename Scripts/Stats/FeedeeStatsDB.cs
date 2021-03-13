using UnityEngine;
using ButtonGame.Stats.Enums;
using System.Collections.Generic;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "FeedeeStatsDB", menuName = "Stats/FeedeeStatsDB")]
    public class FeedeeStatsDB : ScriptableObject 
    {
        [SerializeField] BaseFeedeeClass[] feedeeClasses;
        Dictionary<FeedeeClass, Dictionary<FeedeeStat, float>> lookupTable = null;

        public float GetStat(FeedeeClass feedeeClass, FeedeeStat stat)
        {
            BuildLookup();

            return lookupTable[feedeeClass][stat];
        }

        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<FeedeeClass, Dictionary<FeedeeStat, float>>();

            foreach (BaseFeedeeClass feedeeClass in feedeeClasses)
            {
                var statLookup = new Dictionary<FeedeeStat, float>();
                foreach (BaseFeedeeStats stat in feedeeClass.feedeeStats)
                {
                    statLookup[stat.stat] = stat.value;
                }

                lookupTable[feedeeClass.feedeeClass] = statLookup;
            }
        }

        [System.Serializable]
        private class BaseFeedeeClass
        {
            public FeedeeClass feedeeClass;
            public BaseFeedeeStats[] feedeeStats;
        }

        [System.Serializable]
        class BaseFeedeeStats
        {
            public FeedeeStat stat;
            public float value;
        }
    }
}
