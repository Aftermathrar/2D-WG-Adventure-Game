using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Inventories;

namespace ButtonGame.UI.Inventories
{
    /// <summary>
    /// To be placed on the root of the inventory UI. Handles spawning all the
    /// inventory slot prefabs.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // CONFIG DATA
        [SerializeField] InventorySlotUI InventoryItemPrefab = null;

        // CACHE
        Inventory playerInventory;
        Equipment playerEquipment;
        int playerInventorySize;
        Dictionary<int, InventorySlotUI> inventorySlotCache;

        // LIFECYCLE METHODS

        private void Awake() 
        {
            playerInventory = Inventory.GetPlayerInventory();
            playerEquipment = Equipment.GetEntityEquipment("Player");
            playerInventory.inventoryUpdated += Redraw;
        }

        private void Start()
        {
            Redraw();
        }

        // PRIVATE

        private void Redraw()
        {
            // Get inventory size
            int sizeCheck = playerInventory.GetSize();

            // Check if Inv cache is null or different size
            if (inventorySlotCache == null || playerInventorySize != sizeCheck)
            {
                // Destroy all old slots
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                inventorySlotCache = new Dictionary<int, InventorySlotUI>();
                playerInventorySize = sizeCheck;
                for (int i = 0; i < playerInventorySize; i++)
                {
                    var itemUI = Instantiate(InventoryItemPrefab, transform);
                    inventorySlotCache[i] = itemUI;
                    itemUI.Setup(playerInventory, playerEquipment, i);
                }

                // Set content window to show top row
                transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            }
            else
            {
                foreach (KeyValuePair<int, InventorySlotUI> inventorySlot in inventorySlotCache)
                {
                    var itemUI = inventorySlot.Value;
                    itemUI.Setup(playerInventory, playerEquipment, inventorySlot.Key);
                }
            }
        }
    }
}