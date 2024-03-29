﻿using System;
using UnityEngine;
using ButtonGame.Saving;
using System.Collections.Generic;
using ButtonGame.Core;

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
        [SerializeField] float money = 0;

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
        /// Does the player have money for this item?
        /// </summary>
        public bool HasMoneyFor(float cost)
        {
            if(money >= cost) return true;
            return true;
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
        /// Add money to the inventory.
        /// </summary>
        public void AddMoney(float value)
        {
            money += value;
        }

        public void AddItem(string[] itemParameters)
        {
            InventoryItem item = InventoryItem.GetFromID(itemParameters[0]);
            int amount;
            if(int.TryParse(itemParameters[1], out amount))
            {
                AddToFirstEmptySlot(item, amount);
            }
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
        /// Is there an instance of the item in the inventory? If so, return slot.
        /// </summary>
        public bool HasItem(InventoryItem item, out int slot)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    slot = i;
                    return true;
                }
            }
            slot = -1;
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
        /// Removes multiple items from inventory.
        /// </summary>
        public void RemoveItemFromInventory(InventoryItem item, int removeCount)
        {
            if(item.IsStackable())
            {
                RemoveFromSlot(FindSlot(item), removeCount);
            }
            else
            {
                int slot = -1;
                if(HasItem(item, out slot))
                {
                    RemoveFromSlot(slot, 1);
                }
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
        public bool AddItemToSlot(int slot, InventoryItem item, int number, object state = null)
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

            // slots[slot].item = Instantiate(item);
            slots[slot].item = item;
            slots[slot].number += number;
            if(state != null) slots[slot].item.SetModifiers(state);
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
            return true;
        }

        /// <summary>
        /// Clears inventory
        /// </summary>
        public void ClearInventory()
        {
            slots = new InventorySlot[inventorySize];
        }

        /// <summary>
        /// Returns count of an item in inventory.
        /// </summary>
        public int GetItemCount(InventoryItem item)
        {
            if(item.IsStackable())
            {
                int slot = -1;
                if(HasItem(item, out slot))
                {
                    return GetCountInSlot(slot);
                }
            }

            int itemCount = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    itemCount++;
                }
            }
            return itemCount;
        }

        // PRIVATE

        private void Awake()
        {
            slots = new InventorySlot[inventorySize];
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
            public object modifiers;
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
                    slotRecords[i].modifiers = slots[i].item.GetModifiers();
                }
            }
            return slotRecords;
        }

        void ISaveable.RestoreState(object state)
        {
            var slotRecords = (InventorySlotRecord[])state;
            for (int i = 0; i < inventorySize; i++)
            {
                InventoryItem candidate = InventoryItem.GetFromID(slotRecords[i].itemID);
                if(candidate != null)
                {
                    // slots[i].item = Instantiate(candidate);
                    slots[i].item = candidate;
                    slots[i].number = slotRecords[i].number;
                    slots[i].item.SetModifiers(slotRecords[i].modifiers);
                }
            }

            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }

        public bool? Evaluate(ConditionPredicate predicate, List<string> parameters)
        {
            if (predicate != ConditionPredicate.HasItem) return null;
            string[] parameterArray = parameters.ToArray();
            for (int i = 0; i < parameterArray.Length; i++)
            {
                int slot;
                InventoryItem item = InventoryItem.GetFromID(parameterArray[i]);
                i++;
                if (HasItem(item, out slot))
                {
                    if (GetCountInSlot(slot) < int.Parse(parameters[i]))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}