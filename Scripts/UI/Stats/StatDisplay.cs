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

        protected virtual void Start()
        {
            statTextLines = GetComponentsInChildren<StatText>();
            RedrawStatDisplay();

            characterEquipment.equipmentUpdated += RedrawStatDisplay;
        }

        protected virtual void OnEnable() 
        {
            statTextLines = GetComponentsInChildren<StatText>();
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
