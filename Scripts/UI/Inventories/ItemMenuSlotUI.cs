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
        [SerializeField] Button[] actionButtons;
        [SerializeField] TextMeshProUGUI[] actionButtonTexts;
        [SerializeField] int[] actionButtonCounts;
        int owned;

        public void SlotSetup(string action, Sprite sprite, string displayName, 
            float cost, int _owned, float goldOnHand)
        {
            owned = _owned;
            icon.sprite = sprite;
            nameText.text = displayName;
            costText.text = "Value: " + cost.ToString();
            ownedText.text = "Currently Owned: " + owned.ToString();
            for (int i = 0; i < actionButtonTexts.Length; i++)
            {
                string actionText = action + "\n" + actionButtonCounts[i].ToString();
                actionButtonTexts[i].text = actionText;
            }
        }

        public Button[] GetActionButtons()
        {
            return actionButtons;
        }

        public int GetActionButtonCount(int btnIndex)
        {
            return actionButtonCounts[btnIndex];
        }

        public void ChangeOwnedValue(int value)
        {
            owned += value;
            ownedText.text = "Currently Owned: " + owned.ToString();
        }
    }
}