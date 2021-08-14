using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ButtonGame.Stats;
using ButtonGame.Inventories;

namespace ButtonGame.UI.Stats
{
    public class StatDisplay : MonoBehaviour
    {
        [SerializeField] protected BaseStats characterStats = null;
        [SerializeField] protected Equipment characterEquipment = null;
        protected StatText[] statTextLines = null;

        private void Awake() 
        {
            statTextLines = GetComponentsInChildren<StatText>();
        }
        
        protected virtual void Start()
        {
            RedrawStatDisplay();

            characterEquipment.equipmentUpdated += RedrawStatDisplay;
        }

        protected virtual void OnEnable() 
        {
            RedrawStatDisplay();
        }

        public virtual void OnEffectChange()
        {
            RedrawStatDisplay();
        }

        protected void RedrawStatDisplay()
        {
            foreach (var statText in statTextLines)
            {
                statText.SetValue(characterStats.GetStat(statText.GetStat()));
            }
        }
    }
}
