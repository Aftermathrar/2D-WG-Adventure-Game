using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    public class BodyManager : MonoBehaviour //, ISaveable
    {
        // Dictionary<string, FollowerSetup> characterDict = null;
        CalorieManager calorieManager;

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
        

        // public object CaptureState()
        // {
        //     return characterDict;
        // }

        // public void RestoreState(object state)
        // {
        //     characterDict = (Dictionary<string, FollowerSetup>)state;
        // }
    }

    [System.Serializable]
    public class CalorieManager
    {
        public float dailyCalories = 0f;
        public float stomachCalories = 800f;
    }

}