using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.Menus
{
    public class ItemInfoDescription : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI description;

        public void SetItemInfo(InventoryItem item)
        {
            string sTooltip = "";
            foreach (var tooltipDescription in item.GetDescriptionFields())
            {
                sTooltip = tooltipDescription.description + "\n";
            }

            PopulateDescriptionText(sTooltip);
        }

        private void PopulateDescriptionText(string newText)
        {
            description.text = newText;
        }
    }
}
