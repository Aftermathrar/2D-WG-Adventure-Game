using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName="EffectDB", menuName="Stats/EffectDB", order=1)]
    public class EffectDB:ScriptableObject
    {
        [SerializeField] DBEffectName[] effectNameDB = null;

        Dictionary<EffectName, Dictionary<EffectStat, string>> effectLookup = null;

        public string GetEffectStat(EffectStat stat, EffectName effectName)
        {
            BuildLookup();
            
            return effectLookup[effectName][stat];
        }

        private void BuildLookup()
        {
            if(effectLookup != null) return;

            effectLookup = new Dictionary<EffectName, Dictionary<EffectStat, string>>();

            foreach(DBEffectName fxName in effectNameDB)
            {
                var statLookupTable = new Dictionary<EffectStat, string>();

                foreach(EffectNameStats fxStats in fxName.effectStats)
                {
                    statLookupTable[fxStats.stat] = fxStats.value;
                }
                effectLookup[fxName.effectName] = statLookupTable;
            }
        }
    }

    [System.Serializable]
    class DBEffectName
    {
        public EffectName effectName;
        public EffectNameStats[] effectStats;
    }

    [System.Serializable]
    class EffectNameStats
    {
        public EffectStat stat;
        public string value;
    }
}