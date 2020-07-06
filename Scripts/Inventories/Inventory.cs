using System;
using UnityEngine;
using ButtonGame.Saving;
using System.Collections.Generic;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// Provides storage for the player inventory. A configurable number of
    /// slots are available.
    ///
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class Inventory : MonoBehaviour, ISaveable
    {
        // CONFIG DATA
        [Tooltip("Allowed size")]
        [SerializeField] int inventorySize = 16;
        [SerializeField] int money = 0;

        // STATE
        InventorySlot[] slots;

        public struct InventorySlot
        {
            public InventoryItem item;
            public int number;
        }

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public event Action inventoryUpdated;

        /// <summary>
        /// Convenience for getting the player's inventory.
        /// </summary>
        public static Inventory GetPlayerInventory()
        {
            var player = GameObject.FindWithTag("Player");
            return player.GetComponent<Inventory>();
        }

        /// <summary>
        /// Could this item fit anywhere in the inventory?
        /// </summary>
        public bool HasSpaceFor(InventoryItem item)
        {
            return FindSlot(item) >= 0;
        }

        /// <summary>
        /// How many slots are in the inventory?
        /// </summary>
        public int GetSize()
        {
            return slots.Length;
        }

        /// <summary>
        /// Attempt to add the items to the first available slot.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>Whether or not the item could be added.</returns>
        public bool AddToFirstEmptySlot(InventoryItem item, int number)
        {
            int i = FindSlot(item);

            if (i < 0)
            {
                return false;
            }

            slots[i].item = item;
            slots[i].number += number;
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
            return true;
        }

        /// <summary>
        /// Is there an instance of the item in the inventory?
        /// </summary>
        public bool HasItem(InventoryItem item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return the item type in the given slot.
        /// </summary>
        public InventoryItem GetItemInSlot(int slot)
        {
            return slots[slot].item;
        }

        /// <summary>
        /// Return the item count in the given slot.
        /// </summary>
        public int GetCountInSlot(int slot)
        {
            return slots[slot].number;
        }

        /// <summary>
        /// Remove the item from the given slot.
        /// </summary>
        public void RemoveFromSlot(int slot, int number)
        {
            slots[slot].number -= number;
            if(slots[slot].number <= 0)
            {
                slots[slot].number = 0;
                slots[slot].item = null;
            }
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }

        /// <summary>
        /// Will add an item to the given slot if possible. If there is already
        /// a stack of this type, it will add to the existing stack. Otherwise,
        /// it will be added to the first empty slot.
        /// </summary>
        /// <param name="slot">The slot to attempt to add to.</param>
        /// <param name="item">The item type to add.</param>
        /// <param name="number">The number of items to add.null</param>
        /// <returns>True if the item was added anywhere in the inventory.</returns>
        public bool AddItemToSlot(int slot, InventoryItem item, int number)
        {
            if (slots[slot].item != null)
            {
                return AddToFirstEmptySlot(item, number);
            }

            int i = FindStack(item);
            if (i >= 0)
            {
                slot = i;
            }

            slots[slot].item = item;
            slots[slot].number += number;
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
            return true;
        }

        // PRIVATE

        private void Awake()
        {
            slots = new InventorySlot[inventorySize];
            slots[0].item = InventoryItem.GetFromID("80d319c7-6b3f-4f74-8525-2635f8320532");
            slots[0].number = 1;
            slots[1].item = InventoryItem.GetFromID("0017758c-658d-4d95-aba3-1e124bfe5767");
            slots[1].number = 1;
            slots[2].item = InventoryItem.GetFromID("161a4c8d-f1c5-48ff-8322-680671f5b0c1");
            slots[2].number = 40;
            slots[4].item = InventoryItem.GetFromID("843bfc27-cea2-4119-9aac-ea2f404739d7");
            slots[4].number = 1;
            slots[5].item = InventoryItem.GetFromID("d008dd4c-14b2-4591-87ed-7c717ddf32b7");
            slots[5].number = 2;
            slots[6].item = InventoryItem.GetFromID("116ccfc9-2af7-4130-afb5-a776d6b8c16f");
            slots[6].number = 2;
            slots[7].item = InventoryItem.GetFromID("963a13ad-9170-4398-ac4e-88256afb1a5b");
            slots[7].number = 5;
            slots[8].item = InventoryItem.GetFromID("30419955-2fbb-4f89-ac80-0620e947de70");
            slots[8].number = 5;
            slots[9].item = InventoryItem.GetFromID("c942f2ed-9441-4a9e-a268-c7b6293ee1fd");
            slots[9].number = 1;
            slots[3].item = InventoryItem.GetFromID("12ff2f88-dc29-4ce6-a46a-2695265b4df1");
            slots[3].number = 1;
        }

        /// <summary>
        /// Find a slot that can accomodate the given item.
        /// </summary>
        /// <returns>-1 if no slot is found.</returns>
        private int FindSlot(InventoryItem item)
        {
            int i = FindStack(item);
            
            if(i < 0)
            {
                i = FindEmptySlot();
            }

            return i;
        }

        /// <summary>
        /// Find an empty slot.
        /// </summary>
        /// <returns>-1 if all slots are full.</returns>
        private int FindEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find an existing stack of this item type.
        /// </summary>
        /// <returns>-1 if no stack exists or if the item is not stackable</returns>
        private int FindStack(InventoryItem item)
        {
            if(item.IsStackable())
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if(object.ReferenceEquals(slots[i].item, item))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        [System.Serializable]
        private struct InventorySlotRecord
        {
            public string itemID;
            public int number;
        }

        object ISaveable.CaptureState()
        {
            var slotRecords = new InventorySlotRecord[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                if (slots[i].item != null)
                {
                    slotRecords[i].itemID = slots[i].item.GetItemID();
                    slotRecords[i].number = slots[i].number;
                }
            }
            return slotRecords;
        }

        void ISaveable.RestoreState(object state)
        {
            var slotRecords = (InventorySlotRecord[])state;
            for (int i = 0; i < inventorySize; i++)
            {
                slots[i].item = InventoryItem.GetFromID(slotRecords[i].itemID);
                slots[i].number = slotRecords[i].number;
            }

            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }
    }
}