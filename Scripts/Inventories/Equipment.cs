using System;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Saving;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// Provides a store for the items equipped to a player. Items are stored by
    /// their equip locations.
    /// </summary>
    public class Equipment : MonoBehaviour, ISaveable
    {
        // STATE
        Dictionary<EquipLocation, EquipableItem[]> equippedItems = new Dictionary<EquipLocation, EquipableItem[]>();

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public event Action equipmentUpdated;

        /// <summary>
        /// Convenience for getting the player's equipment.
        /// </summary>
        public static Equipment GetPlayerEquipment()
        {
            var player = GameObject.FindWithTag("Player");
            return player.GetComponent<Equipment>();
        }

        /// <summary>
        /// Return the items in the given equip location.
        /// </summary>
        public EquipableItem GetItemInSlot(EquipLocation equipLocation, int index)
        {
            return equippedItems[equipLocation][index];
        }

        // Sets up equipment dictionary to store limits on number of equips of same type
        public void DefineEquipmentSlots(Dictionary<EquipLocation, int> slotLimitTable)
        {
            if(equippedItems.Keys.Count > 0) return;

            int slotLimit = 1;
            foreach (var key in slotLimitTable.Keys)
            {
                slotLimit = slotLimitTable[key] + 1;
                equippedItems[key] = new EquipableItem[slotLimit];
            }
        }

        // Handles equipping via right click from inventory
        public int TryAddItem(InventoryItem item)
        {
            // Check if it's an equipable item
            EquipableItem equipableItem = item as EquipableItem;
            if(equipableItem == null) return -1;

            // Get location and check number of slots. If 1, add or swap items
            EquipLocation equipLocation = equipableItem.GetAllowedEquipLocation();
            if(equippedItems[equipLocation].Length == 1)
            {
                return 0;
            }
            
            // Otherwise check for empty slot. Don't swap if all slots for equip type are full
            int index = FindEmptySlot(equipLocation);
            if(index >= 0)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Add an item to the given equip location. Do not attempt to equip to
        /// an incompatible slot.
        /// </summary>
        public void AddItem(EquipLocation slot, EquipableItem item, int index)
        {
            Debug.Assert(item.GetAllowedEquipLocation() == slot);

            equippedItems[slot][index] = item;

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }
        }

        /// <summary>
        /// Remove the item for the given slot.
        /// </summary>
        public void RemoveItem(EquipLocation slot, int index)
        {
            equippedItems[slot][index] = null;

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
            }
        }

        private int FindEmptySlot(EquipLocation equip)
        {
            int equipSlots = equippedItems[equip].Length;
            for(int i = 0; i < equipSlots; i++)
            {
                if(equippedItems[equip][i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public object CaptureState()
        {
            var equippedItemsForSerialization = new Dictionary<EquipLocation, string[]>();
            foreach (var pair in equippedItems)
            {
                string[] s = new string[pair.Value.Length];
                for (int i = 0; i < s.Length; i++)
                {
                    if(pair.Value[i] !=null)
                    {
                        s[i] = pair.Value[i].GetItemID();
                    }
                }
                equippedItemsForSerialization[pair.Key] = s;
            }
            return equippedItemsForSerialization;
        }

        public void RestoreState(object state)
        {
            equippedItems = new Dictionary<EquipLocation, EquipableItem[]>();
            
            var equippedItemsForSerialization = (Dictionary<EquipLocation, string[]>)state;
            
            foreach (var pair in equippedItemsForSerialization)
            {
                var items = new EquipableItem[pair.Value.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (EquipableItem)InventoryItem.GetFromID(pair.Value[i]);
                }
                equippedItems[pair.Key] = items;
            }
        }
    }
}