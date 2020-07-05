using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using GameDevTV.Utils;
using UnityEngine;

namespace ButtonGame.Attributes
{
    public class Fullness : MonoBehaviour, ISaveable, IAttribute
    {
        LazyValue<float> fullnessPoints;
        
        float maxCapacity;
        BaseStats baseStats;

        private void Awake() 
        {
            fullnessPoints = new LazyValue<float>(GetInitialFullness);
            maxCapacity = fullnessPoints.value;
        }

        private float GetInitialFullness()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Capacity);
        }

        private void Start() 
        {
            fullnessPoints.ForceInit();
        }

        public void RecalculateMaxCapacity()
        {
            maxCapacity = GetInitialFullness();
        }

        public float DigestFood(float value)
        {
            fullnessPoints.value = Mathf.Max(0, fullnessPoints.value - value);
            // Get calories from item
            // Compare gain vs 1:1 size:calorie ratio
            return 1;
        }

        public void EatFood(ConsumableItem food)
        {
            // Get calories from item
            // Get size of item
            // Check if follower has room
            // fullnessPoints.value += food.size
            Debug.Log("follower eats food");
        }

        public void GainAttribute(float amount)
        {
            fullnessPoints.value += amount;
        }

        public float GetAttributeValue()
        {
            return fullnessPoints.value;
        }

        public float GetMaxAttributeValue()
        {
            return maxCapacity;
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return fullnessPoints.value / maxCapacity;
        }

        public object CaptureState()
        {
            return fullnessPoints;
        }

        public void RestoreState(object state)
        {
            fullnessPoints.value = (float)state;
        }
    }
}
