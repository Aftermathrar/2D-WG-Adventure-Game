using System.Collections;
using System.Collections.Generic;
using System.Text;
using ButtonGame.Inventories;
using ButtonGame.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Inventories
{
    public class ItemMenuSlotUI : MonoBehaviour
    {
        [SerializeField] NodeMenu nodeMenu;
        [SerializeField] Image icon;
        [SerializeField] Text nameText;
        [SerializeField] Text costText;
        [SerializeField] Text ownedText;
        [SerializeField] Button[] actionButtons;
        [SerializeField] TextMeshProUGUI[] actionButtonTexts;
        [SerializeField] int[] actionButtonCounts;
        InventoryItem item;
        int owned;
        int slotIndex;

        public void SlotSetup(string action, InventoryItem _item, 
            float cost, int _owned, float goldOnHand, int slot)
        {
            item = _item;
            owned = _owned;
            icon.sprite = item.GetIcon();
            nameText.text = item.GetDisplayName();
            costText.text = "Value: " + cost.ToString();
            ownedText.text = "Currently Owned: " + owned.ToString();
            for (int i = 0; i < actionButtonTexts.Length; i++)
            {
                int btnIndex = i;
                actionButtons[i].onClick.RemoveAllListeners();
                actionButtons[i].onClick.AddListener(() => 
                {
                    switch (action)
                    {
                        case "Buy":
                            ShopBuyAction(btnIndex);
                            break;
                        case "Sell":
                            ShopSellAction(btnIndex);
                            break;
                        default:
                            ShopCraftAction(btnIndex);
                            break;
                    }
                });
                string actionText = action + "\n" + actionButtonCounts[i].ToString();
                actionButtonTexts[i].text = actionText;
            }
            slotIndex = slot;
        }

        public void ShopBuyAction(int btnIndex)
        {
            int actionCount = actionButtonCounts[btnIndex];

            owned += nodeMenu.BuyItem(item, actionCount);
            ownedText.text = "Currently Owned: " + owned.ToString();
        }

        public void ShopSellAction(int btnIndex)
        {
            int actionCount = actionButtonCounts[btnIndex];

            owned -= nodeMenu.SellItem(item, actionCount);
            ownedText.text = "Currently Owned: " + owned.ToString();
        }

        public void ShopCraftAction(int btnIndex)
        {
            Debug.Log("Craft button clicked!!!! " + btnIndex.ToString());
        }
    }
}