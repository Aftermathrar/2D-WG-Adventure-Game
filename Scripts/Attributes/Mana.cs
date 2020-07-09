using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Attributes
{
    public class Mana : MonoBehaviour, ISaveable, IAttribute
    {
        BaseStats baseStats;
        LazyValue<float> manaPoints;
        float maxMana;
        
        private void Awake() 
        {
            baseStats = GetComponent<BaseStats>();
            manaPoints = new LazyValue<float>(GetInitialMana);
            maxMana = manaPoints.value;
        }

        private float GetInitialMana()
        {
            return baseStats.GetStat(Stat.Mana);
        }

        private void Start() 
        {
            manaPoints.ForceInit();
        }

        public void RecalculateMaxMana()
        {
            maxMana = baseStats.GetStat(Stat.Mana);
        }

        public void UseMana(float mpCost)
        {
            manaPoints.value = Mathf.Max(manaPoints.value - mpCost, 0);
        }

        public void GainAttribute(float amount = 0f)
        {
            if(amount == 0)
            {
                amount = baseStats.GetStat(Stat.ManaRegen);
            }
            manaPoints.value = Mathf.Min(manaPoints.value + amount, GetMaxAttributeValue());
        }

        public float GetAttributeValue()
        {
            return manaPoints.value;
        }

        public float GetMaxAttributeValue()
        {
            return maxMana;
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return manaPoints.value / maxMana;
        }

        public object CaptureState()
        {
            return manaPoints.value;
        }

        public void RestoreState(object state)
        {
            manaPoints.value = (float)state;
        }
    }
}