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
        Text[] textLines = null;
        StatText[] statTextLines = null;
        BaseStats playerStats = null;
        Equipment playerEquipment = null;

        private void Start() 
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerStats = player.GetComponent<BaseStats>();
            playerEquipment = player.GetComponent<Equipment>();
            statTextLines = GetComponentsInChildren<StatText>();
            RedrawStatDisplay();

            playerEquipment.equipmentUpdated += RedrawStatDisplay;
        }

        private void RedrawStatDisplay()
        {
            foreach (var statText in statTextLines)
            {
                statText.SetValue(playerStats.GetStat(statText.GetStat()));
            }
        }
    }
}
