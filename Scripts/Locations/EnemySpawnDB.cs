using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using UnityEngine;

namespace ButtonGame.Locations
{
    [CreateAssetMenu(fileName = "EnemySpawnDB", menuName = "Locations/EnemySpawnDB", order = 0)]
    public class EnemySpawnDB : ScriptableObject 
    {
        [SerializeField] EnemyList[] enemyLists;

        public BaseStats[] GetEnemyList(LocationList locName)
        {
            foreach (var enemyList in enemyLists)
            {
                if(enemyList.location == locName)
                {
                    return enemyList.enemyTypes;
                }
            }
            
            return null;
        }

        [System.Serializable]
        private class EnemyList
        {
            public LocationList location;
            public BaseStats[] enemyTypes;
            public int roomsToClear;
            public LocationList locationToUnlock;
        }
    }
}
