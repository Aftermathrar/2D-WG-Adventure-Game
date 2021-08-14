using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.UI.Inventories
{
    public class FollowerEquipmentUI : EquipmentUI
    {
        EquipmentSlotUI[] equipmentSlots;
        Dictionary<EquipLocation, int> equipSlotLookup = null;

        protected override void Awake()
        {
            equipmentSlots = GetComponentsInChildren<EquipmentSlotUI>();
            RegisterEquipment();
        }

        // Get follower references to set up equipment slots and assign index numbers to each slot
        private void RegisterEquipment()
        {
            Inventory playerInventory = Inventory.GetPlayerInventory();
            // Equipment followerEquipment = Equipment.GetEntityEquipment("Follower");
            
            FollowerManager followerManager = GameObject.FindWithTag("LevelManager").GetComponent<FollowerManager>();
            GameObject followerGO;
            if(!followerManager.GetActiveFollowerObject(out followerGO))
            {
                return;
            }

            Equipment followerEquipment = followerGO.GetComponent<Equipment>();
            equipmentSlots ??= GetComponentsInChildren<EquipmentSlotUI>();
            Dictionary<EquipLocation, int> equipSlotLookup = new Dictionary<EquipLocation, int>();

            foreach (var slot in equipmentSlots)
            {
                EquipLocation equipLocation = slot.GetEquipLocation();
                if (equipSlotLookup.ContainsKey(equipLocation))
                {
                    equipSlotLookup[equipLocation]++;
                }
                else
                {
                    equipSlotLookup[equipLocation] = 0;
                }

                slot.Setup(playerInventory, followerEquipment, equipSlotLookup[equipLocation]);
            }
        }

        public void OnFollowerChange()
        {
            // Get new follower reference and pass in updated equipment to Equipment Slots
            FollowerManager followerManager = GameObject.FindWithTag("LevelManager").GetComponent<FollowerManager>();
            GameObject followerGO;
            if (!followerManager.GetActiveFollowerObject(out followerGO))
            {
                return;
            }

            Equipment followerEquipment = followerGO.GetComponent<Equipment>();
            foreach (var slot in equipmentSlots)
            {
                slot.OnFollowerChange(followerEquipment);
            }
        }
    }
}
