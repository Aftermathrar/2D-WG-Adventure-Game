using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    [CreateAssetMenu(fileName = "FollowerAttackDB", menuName = "Stats/FollowerAttackDB", order = 5)]
    public class FollowerAttackDB : ScriptableObject
    {
        [SerializeField] DBFollowerAttacks[] followerAttackDB = null;

        Dictionary<FollowerAttackName, FollowerAttackStats> lookupTable = null;

        public FollowerAttackStats GetAttackStat(FollowerAttackName attackName)
        {
            BuildLookup();

            return lookupTable[attackName];
        }

        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<FollowerAttackName, FollowerAttackStats>();

            foreach (DBFollowerAttacks atk in followerAttackDB)
            {
                lookupTable[atk.attackName] = atk.attackStats;
            }
        }
    }

    [System.Serializable]
    class DBFollowerAttacks
    {
        public FollowerAttackName attackName;
        public FollowerAttackStats attackStats;
    }
}