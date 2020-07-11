using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Inventories
{
    public class DebugItems : MonoBehaviour
    {
        [SerializeField] DebugItem[] debugItems;
        Inventory inventory;

        [System.Serializable]
        private struct DebugItem
        {
            public InventoryItem item;
            public int number;
        }

        public void GetAllItems()
        {
            var player = GameObject.FindWithTag("Player");
            inventory = player.GetComponent<Inventory>();

            foreach (var debugItem in debugItems)
            {
                inventory.AddToFirstEmptySlot(debugItem.item, debugItem.number);
            }
        }
    }
}
