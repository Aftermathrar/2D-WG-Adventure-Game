using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGame.Inventories;
using TMPro;

namespace ButtonGame.UI.Inventories
{
    /// <summary>
    /// To be put on the icon representing an inventory item. Allows the slot to
    /// update the icon and number.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class InventoryItemIcon : MonoBehaviour
    {
        [SerializeField] GameObject stackContainer = null;
        [SerializeField] TextMeshProUGUI stackText = null;
        // PUBLIC

        public void SetItem(InventoryItem item, int number)
        {
            var iconImage = GetComponent<Image>();
            if (item == null)
            {
                iconImage.enabled = false;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = item.GetIcon();
            }

            if(stackContainer)
            {
                stackContainer.SetActive(false);
                if (number > 1)
                {
                    stackContainer.SetActive(true);
                    stackText.text = number.ToString();
                }
            }
        }
    }
}