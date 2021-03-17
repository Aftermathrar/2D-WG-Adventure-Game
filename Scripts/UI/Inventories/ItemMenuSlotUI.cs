using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Inventories
{
    public class ItemMenuSlotUI : MonoBehaviour
    {
        [SerializeField] Image icon;
        [SerializeField] Text nameText;
        [SerializeField] Text costText;
        [SerializeField] Text ownedText;
        [SerializeField] Text affordCountText;
        [SerializeField] TextMeshProUGUI[] actionButtonTexts;
        [SerializeField] string[] actionButtonCounts;

        public void SlotSetup(string action, Sprite sprite, string name, float cost, float owned, float goldOnHand)
        {
            icon.sprite = sprite;
            nameText.text = name;
            costText.text = "Cost: " + cost.ToString();
            ownedText.text = "Currently Owned: " + owned.ToString();
            // int affordCount = Mathf.FloorToInt(goldOnHand / cost);
            // affordCountText.text = "Can Afford: " + affordCount.ToString();
            for (int i = 0; i < actionButtonTexts.Length; i++)
            {
                string actionText = action + "\n" + actionButtonCounts[i];
                actionButtonTexts[i].text = actionText;
            }
        }
    }
}