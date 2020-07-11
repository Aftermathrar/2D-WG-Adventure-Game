using System;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Saving;
using ButtonGame.Stats;
using UnityEngine.Events;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// Provides a store for the items equipped to a player. Items are stored by
    /// their equip locations.
    /// </summary>
    public class Equipment : MonoBehaviour, ISaveable, IStatModifier
    {
        // CONFIG DATA
        [Tooltip("Allowed size")]
        [SerializeField] EquipmentLocationLimits[] slotLimits;

        [System.Serializable]
        public struct EquipmentLocationLimits
        {
            public EquipLocation equipType;
            public int slotsAvailable;
        }

        // STATE
        Dictionary<EquipLocation, EquipableItem[]> equippedItems;
        Dictionary<Stat, float[]> equippedStats;

        // Sets up dictionary for multiple slots per location
        private void Awake() 
        {
            equippedItems = new Dictionary<EquipLocation, EquipableItem[]>();
            equippedStats = new Dictionary<Stat, float[]>();
            foreach (var slotLimit in slotLimits)
            {
                equippedItems[slotLimit.equipType] = new EquipableItem[slotLimit.slotsAvailable];
            }
        }

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public event Action equipmentUpdated;
        public UnityEvent equipmentEffectsUpdated;

        /// <summary>
        /// Convenience for getting the player's equipment.
        /// </summary>
        public static Equipment GetEntityEquipment(string tag)
        {
            var entity = GameObject.FindWithTag(tag);
            return entity.GetComponent<Equipment>();
        }

        /// <summary>
        /// Return the items in the given equip location.
        /// </summary>
        public EquipableItem GetItemInSlot(EquipLocation equipLocation, int index)
        {
            if(equippedItems[equipLocation] == null) return null;

            return equippedItems[equipLocation][index];
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
            
            AddEquipmentStats(item);

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
                equipmentEffectsUpdated.Invoke();
            }
        }

        private void AddEquipmentStats(EquipableItem item)
        {
            if(item == null) return;
            
            Dictionary<Stat, float[]> equipDict = item.GetStatValues();
            foreach (var pair in equipDict)
            {
                if (!equippedStats.ContainsKey(pair.Key))
                {
                    equippedStats[pair.Key] = pair.Value;
                }
                else
                {
                    equippedStats[pair.Key][0] += pair.Value[0];
                    equippedStats[pair.Key][1] += pair.Value[1];
                }
            }
        }

        /// <summary>
        /// Remove the item for the given slot.
        /// </summary>
        public void RemoveItem(EquipLocation slot, int index)
        {
            // Remove stats from dictionary
            Dictionary<Stat, float[]> equipDict = equippedItems[slot][index].GetStatValues();
            foreach (var pair in equipDict)
            {
                equippedStats[pair.Key][0] -= pair.Value[0];
                equippedStats[pair.Key][1] -= pair.Value[1];
            }

            // Remove item from inventory
            equippedItems[slot][index] = null;

            if (equipmentUpdated != null)
            {
                equipmentUpdated();
                equipmentEffectsUpdated.Invoke();
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
            equippedStats = new Dictionary<Stat, float[]>();
            
            var equippedItemsForSerialization = (Dictionary<EquipLocation, string[]>)state;
            
            foreach (var pair in equippedItemsForSerialization)
            {
                var items = new EquipableItem[pair.Value.Length];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = (EquipableItem)InventoryItem.GetFromID(pair.Value[i]);
                    AddEquipmentStats(items[i]);
                }
                equippedItems[pair.Key] = items;
            }
        }

        public float[] GetStatEffectModifiers(Stat stat)
        {
            // Short circuit on null check
            if(equippedStats != null && equippedStats.ContainsKey(stat))
            {
                return equippedStats[stat];
            }
            return new float[] { 0, 0 };
        }
    }
}