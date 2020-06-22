using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.UI.Inventories
{
    public class RewardsUI : MonoBehaviour
    {
        // CONFIG DATA
        [SerializeField] RewardSlotUI rewardItemPrefab = null;

        // CACHE
        Inventory rewardsInventory;
        int rewardsInventorySize;
        Dictionary<int, RewardSlotUI> inventorySlotCache;

        public void OnEnable() 
        {
            var enemy = GameObject.FindGameObjectWithTag("Enemy");
            rewardsInventory = enemy.GetComponent<Inventory>();
            rewardsInventory.inventoryUpdated += Redraw;
            Redraw();
        }

        // PRIVATE

        private void Redraw()
        {
            // Get inventory size
            int sizeCheck = rewardsInventory.GetSize();

            // Check if Inv cache is null or different size
            if (inventorySlotCache == null || rewardsInventorySize != sizeCheck)
            {
                // Destroy all old slots
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                inventorySlotCache = new Dictionary<int, RewardSlotUI>();
                rewardsInventorySize = sizeCheck;
                for (int i = 0; i < rewardsInventorySize; i++)
                {
                    var rewardUI = Instantiate(rewardItemPrefab, transform);
                    inventorySlotCache[i] = rewardUI;
                    rewardUI.Setup(rewardsInventory, i);
                }

                // Set content window to show top row
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            }
            else
            {
                foreach (KeyValuePair<int, RewardSlotUI> inventorySlot in inventorySlotCache)
                {
                    var rewardUI = inventorySlot.Value;
                    rewardUI.Setup(rewardsInventory, inventorySlot.Key);
                }
            }
        }
    }
}
