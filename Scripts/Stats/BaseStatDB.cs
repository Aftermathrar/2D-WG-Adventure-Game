using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "BaseStatDB", menuName = "Stats/BaseStatDB", order = 2)]
    public class BaseStatDB : ScriptableObject
    {
        [SerializeField] DBCharacterClass[] characterClasses = null;

        Dictionary<CharacterClass, Dictionary<Stat, string>> lookupTable = null;

        public string GetStat(CharacterClass characterClass, Stat stat)
        {

            BuildLookup();

            return lookupTable[characterClass][stat];
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, string>>();

            foreach (DBCharacterClass charClass in characterClasses)
            {
                var statLookupTable = new Dictionary<Stat, string>();

                foreach (BaseStats baseStat in charClass.baseStats)
                {
                    statLookupTable[baseStat.stat] = baseStat.value.ToString();
                }

                lookupTable[charClass.characterClass] = statLookupTable;
            }
        }

        [System.Serializable]
        class DBCharacterClass
        {
            public CharacterClass characterClass;
            public BaseStats[] baseStats;
        }

        [System.Serializable]
        class BaseStats
        {
            public Stat stat;
            public string value;
        }
    }
}