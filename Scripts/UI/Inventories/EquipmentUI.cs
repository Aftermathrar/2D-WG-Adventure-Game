using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.UI.Inventories
{
    public class EquipmentUI : MonoBehaviour
    {
        
        private void Awake()
        {
            Inventory playerInventory = Inventory.GetPlayerInventory();
            Equipment playerEquipment = Equipment.GetPlayerEquipment();
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

                slot.Setup(playerInventory, playerEquipment, equipSlotLookup[key]);
            }

            playerEquipment.DefineEquipmentSlots(equipSlotLookup);
        }
    }
}
