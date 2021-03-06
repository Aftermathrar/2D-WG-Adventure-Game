﻿using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.UI.Inventories
{
    public class FollowerEquipmentUI : EquipmentUI
    {
        // Get follower references to set up equipment slots and assign index numbers to each slot
        protected override void Awake()
        {
            Inventory playerInventory = Inventory.GetPlayerInventory();
            Equipment followerEquipment = Equipment.GetEntityEquipment("Follower");
            EquipmentSlotUI[] equipmentSlots = GetComponentsInChildren<EquipmentSlotUI>();
            Dictionary<EquipLocation, int> equipSlotLookup = new Dictionary<EquipLocation, int>();

            foreach (var slot in equipmentSlots)
            {
                EquipLocation key = slot.GetEquipLocation();
                if (equipSlotLookup.ContainsKey(key))
                {
                    equipSlotLookup[key] += 1;
                }
                else
                {
                    equipSlotLookup[key] = 0;
                }

                slot.Setup(playerInventory, followerEquipment, equipSlotLookup[key]);
            }
        }
    }
}
