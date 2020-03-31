using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Resources
{
    public class Mana : MonoBehaviour
    {
        [SerializeField] GainManaEvent gainMana;

        [System.Serializable]
        public class GainManaEvent : UnityEvent<float> {}

        LazyValue<float> manaPoints;
        float maxMana;
        
        private void Awake() 
        {
            manaPoints = new LazyValue<float>(GetInitialMana);
            maxMana = manaPoints.value;
        }

        private float GetInitialMana()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Mana);
        }

        private void Start() 
        {
            manaPoints.ForceInit();
        }

        public void UseMana(float mpCost)
        {
            manaPoints.value = Mathf.Max(manaPoints.value - mpCost, 0);
        }

        public void GainMana(float mpGain = 0f)
        {
            if(mpGain == 0)
            {
                mpGain = GetComponent<BaseStats>().GetStat(Stat.ManaRegen);
            }
            manaPoints.value = Mathf.Min(manaPoints.value + mpGain, GetMaxMana());
        }

        public float GetMana()
        {
            return manaPoints.value;
        }

        public float GetMaxMana()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Mana);
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return manaPoints.value / GetMaxMana();
        }
    }
}