using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class PlayerBattleStats : MonoBehaviour, ISaveable
    {
        Dictionary<string, int> enemyKillCountLookup = null;

        public Dictionary<string, int> GetEnemyKillCounts()
        {
            BuildLookup();
            return enemyKillCountLookup;
        }

        public void AddEnemyKill(string enemyName)
        {
            BuildLookup();
            if(enemyKillCountLookup.ContainsKey(enemyName))
            {
                enemyKillCountLookup[enemyName]++;
            }
            else
            {
                enemyKillCountLookup[enemyName] = 1;
            }
        }

        private void BuildLookup()
        {
            if(enemyKillCountLookup != null) return;

            enemyKillCountLookup = new Dictionary<string, int>();
        }

        public object CaptureState()
        {
            BuildLookup();
            return enemyKillCountLookup;
        }

        public void RestoreState(object state)
        {
            enemyKillCountLookup = (Dictionary<string, int>)state;
        }
    }
}
