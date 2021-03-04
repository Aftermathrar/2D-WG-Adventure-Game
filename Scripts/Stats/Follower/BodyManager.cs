using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.Stats.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Stats.Follower
{
    public class BodyManager : MonoBehaviour, IStatModifier, ISaveable
    {
        // Dictionary<string, FollowerSetup> characterDict = null;
        CalorieManager calorieManager;
        Dictionary<Stat, float[]> modifiedStats;

        public UnityEvent bodyUpdated;

        // GUID key to appearance stat record
        // [System.Serializable]
        // private struct FollowerSetup
        // {
        //     public BaseStats charClass;
        //     public CalorieManager calorieManager;
            // public AppearanceStats appearance;
            // public PersonalityStats personality;
            // public InternalStats internalStats;
            // public FollowerPositions position;
        // }
    
        private void Awake()
        {
            calorieManager = new CalorieManager();
            modifiedStats = new Dictionary<Stat, float[]>();
            modifiedStats[Stat.Capacity] = new float[] { 0, Random.Range(-100, 200) };
            modifiedStats[Stat.Greed] = new float[] { 0, Random.Range(0, 20) };
            modifiedStats[Stat.FatDesire] = new float[] { 0, Random.Range(0, 20) };
            modifiedStats[Stat.Metabolism] = new float[] { 0, Random.Range(0, 300) };
        }

        public void ModifyStat(Stat stat, float modValue, bool isAdditive)
        {
            float[] addValue = new float[2];
            int i = isAdditive? 1 : 0;
            addValue[i] = modValue;

            if(!modifiedStats.ContainsKey(stat))
            {
                modifiedStats[stat] = addValue;
            }
            else
            {
                modifiedStats[stat][i] += modValue;
            }
        }

        public float GetDailyCalories()
        {
            return calorieManager.dailyCalories;
        }

        public float GetStomachCalories()
        {
            return calorieManager.stomachCalories;
        }

        public void AddCalories(float cal)
        {
            calorieManager.stomachCalories += cal;
        }

        public float Digest(float calPercent)
        {
            float cal = calPercent * calorieManager.stomachCalories;
            calorieManager.stomachCalories -= cal;
            calorieManager.dailyCalories += cal;
            return cal;
        }

        public float[] GetStatEffectModifiers(Stat stat)
        {
            if(modifiedStats != null && modifiedStats.ContainsKey(stat))
            {
                return modifiedStats[stat];
            }
            return new float[] { 0, 0 };
        }

        public object CaptureState()
        {
            BodyRecord body = new BodyRecord();
            body.manager = calorieManager;
            body.stats = modifiedStats;
            return body;
        }

        public void RestoreState(object state)
        {
            BodyRecord body = (BodyRecord)state;
            calorieManager = body.manager;
            modifiedStats = body.stats;
            bodyUpdated.Invoke();
        }

        [System.Serializable]
        private class BodyRecord
        {
            public CalorieManager manager;
            public Dictionary<Stat, float[]> stats;
        }

        [System.Serializable]
        protected class CalorieManager
        {
            public float dailyCalories = 0f;
            public float stomachCalories = 800f;
        }
    }

}