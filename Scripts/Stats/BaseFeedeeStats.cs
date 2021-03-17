using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats
{
    public class BaseFeedeeStats : MonoBehaviour
    {
        [SerializeField] FeedeeStatsDB FeedeeStatsDB;
        [SerializeField] FeedeeClass feedeeClass;

        public float GetStat(FeedeeStat stat)
        {
            float[] statModifiers = GetStatModifiers(stat);
            return (GetBaseStat(stat) + statModifiers[1]) * (1 + statModifiers[0] / 100);
        }

        public FeedeeClass GetClass()
        {
            return feedeeClass;
        }

        private float GetBaseStat(FeedeeStat stat)
        {
            return FeedeeStatsDB.GetStat(feedeeClass, stat);
        }

        private float[] GetStatModifiers(FeedeeStat stat)
        {
            float[] total = new float[] { 0, 0 };
            foreach (IFeedeeStatModifier fxProvider in GetComponents<IFeedeeStatModifier>())
            {
                float[] result = fxProvider.GetFeedeeStatEffectModifiers(stat);
                total[0] += result[0];
                total[1] += result[1];
            }
            return total;
        }
    }
}