using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class PlayerBattleStats : MonoBehaviour, ISaveable
    {
        Dictionary<string, int> enemyKillCountLookup = null;
        Dictionary<string, int> jobKillCountLookup = null;
        Dictionary<string, int> locationKillCountLookup = null;

        public Dictionary<string, int> GetEnemyKillCounts()
        {
            BuildLookup();
            return enemyKillCountLookup;
        }

        public Dictionary<string, int> GetJobKillCounts()
        {
            BuildLookup();
            return jobKillCountLookup;
        }

        public int GetLocationKillCount(string locName)
        {
            BuildLookup();
            if(locationKillCountLookup.ContainsKey(locName))
            {
                return locationKillCountLookup[locName];
            }
            return 0;
        }

        public void AddEnemyKill(string enemyName, string locName)
        {
            BuildLookup();
            if(enemyKillCountLookup.ContainsKey(enemyName))
            {
                enemyKillCountLookup[enemyName]++;
                jobKillCountLookup[enemyName]++;
            }
            else
            {
                enemyKillCountLookup[enemyName] = 1;
                jobKillCountLookup[enemyName] = 1;
            }

            if(locationKillCountLookup.ContainsKey(locName))
            {
                locationKillCountLookup[locName]++;
            }
            else
            {
                locationKillCountLookup[locName] = 1;
            }
        }

        private void BuildLookup()
        {
            if(enemyKillCountLookup != null) return;

            enemyKillCountLookup = new Dictionary<string, int>();
            jobKillCountLookup = new Dictionary<string, int>();
            locationKillCountLookup = new Dictionary<string, int>();
        }

        public object CaptureState()
        {
            BuildLookup();
            List<Dictionary<string, int>> killLookups = new List<Dictionary<string, int>>();
            killLookups.Add(enemyKillCountLookup);
            killLookups.Add(jobKillCountLookup);
            killLookups.Add(locationKillCountLookup);
            return killLookups;
        }

        public void RestoreState(object state)
        {
            List<Dictionary<string, int>> killLookups = (List<Dictionary<string, int>>)state;
            enemyKillCountLookup = killLookups[0];
            jobKillCountLookup = killLookups[1];
            locationKillCountLookup = killLookups[2];
        }
    }
}
