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
        Dictionary<float, EffectName> fxNameByID = null;

        public string GetEffectStat(EffectStat stat, EffectName effectName)
        {
            BuildLookup();
            
            return effectLookup[effectName][stat];
        }

        public EffectName GetEffectName(float id)
        {
            BuildLookup();

            return fxNameByID[id];
        }

        private void BuildLookup()
        {
            if(effectLookup != null) return;

            effectLookup = new Dictionary<EffectName, Dictionary<EffectStat, string>>();
            fxNameByID = new Dictionary<float, EffectName>();

            foreach(DBEffectName fxName in effectNameDB)
            {
                var statLookupTable = new Dictionary<EffectStat, string>();

                foreach(EffectNameStats fxStats in fxName.effectStats)
                {
                    if(fxStats.stat.ToString() == "ID")
                    {
                        fxNameByID[float.Parse(fxStats.value)] = fxName.effectName;
                    }
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