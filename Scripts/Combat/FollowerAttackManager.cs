using System;
using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class FollowerAttackManager : MonoBehaviour 
    {
        [SerializeField] FollowerAttackDB attackDB = null;
        CharacterClass characterClass;
        Dictionary<CharacterClass, Dictionary<FollowerAttackPool, FollowerAttackName>> lookupTable;
        Dictionary<FollowerAttackName, FollowerAttackStats> statList;

        private void Awake() 
        {
            statList = new Dictionary<FollowerAttackName, FollowerAttackStats>();
            lookupTable = new Dictionary<CharacterClass, Dictionary<FollowerAttackPool, FollowerAttackName>>();
            CharacterClass[] healingClasses = new CharacterClass[2] { CharacterClass.Priest, CharacterClass.WitchDoctor };

            foreach (CharacterClass healingClass in healingClasses)
            {
                var classMoveList = new Dictionary<FollowerAttackPool, FollowerAttackName>();

                foreach (FollowerAttackName attack in Enum.GetValues(typeof(FollowerAttackName)))
                {
                    FollowerAttackStats attackStats = new FollowerAttackStats();
                    attackStats = attackDB.GetAttackStat(attack);
                    if(attackStats.HealingClass != healingClass) continue;
                    
                    statList[attack] = attackStats;
                    classMoveList[attackStats.movePool] = attack;
                }
                lookupTable[healingClass] = classMoveList;
            }

            characterClass = GetComponent<BaseStats>().GetClass();
        }

        public bool HasAttackInPool(FollowerAttackPool movePool)
        {
            return lookupTable[characterClass].ContainsKey(movePool);
        }

        public FollowerAttackName GetAttackOfType(FollowerAttackPool movePool)
        {
            return lookupTable[characterClass][movePool];
        }

        public KeyValuePair<FollowerAttackName, int> GetAttackCost(FollowerAttackPool movePool)
        {
            if(lookupTable[characterClass].ContainsKey(movePool))
            {
                FollowerAttackName attackName = lookupTable[characterClass][movePool];
                return new KeyValuePair<FollowerAttackName, int>(attackName, statList[attackName].Cost);
            }
            return new KeyValuePair<FollowerAttackName, int>(FollowerAttackName.None, int.MaxValue);
        }

        public FollowerAttackStats GetAttackStats(FollowerAttackName attackName)
        {
            return statList[attackName];
        }
    }
}