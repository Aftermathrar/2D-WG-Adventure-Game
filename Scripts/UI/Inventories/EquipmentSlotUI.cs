using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Dragging;
using ButtonGame.Inventories;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButtonGame.UI.Inventories
{
    /// <summary>
    /// An slot for the players equipment.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG DATA

        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] EquipLocation equipLocation = EquipLocation.Weapon;

        // CACHE
        Equipment playerEquipment;
        Inventory inventory;
        int index = 0;

        // LIFECYCLE METHODS

        private void Start()
        {
            RedrawUI();
        }

        // PUBLIC

        public void Setup(Inventory inventory, Equipment equipment, int index)
        {
            this.inventory = inventory;
            playerEquipment = equipment;
            this.index = index;
            playerEquipment.equipmentUpdated += RedrawUI;
        }

        public EquipLocation GetEquipLocation()
        {
            return equipLocation;
        }

        public int MaxAcceptable(InventoryItem item)
        {
            EquipableItem equipableItem = item as EquipableItem;
            if (equipableItem == null) return 0;
            if (equipableItem.GetAllowedEquipLocation() != equipLocation) return 0;
            if (GetItem() != null) return 0;

            return 1;
        }

        public bool HasStack(InventoryItem item)
        {
            return false;
        }

        public void AddItems(InventoryItem item, int number, object state)
        {
            playerEquipment.AddItem(equipLocation, (EquipableItem)item, index, state);
        }

        public InventoryItem GetItem()
        {
            return playerEquipment.GetItemInSlot(equipLocation, index);
        }

        public int GetNumber()
        {
            if (GetItem() != null)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void RemoveItems(int number)
        {
            playerEquipment.RemoveItem(equipLocation, index);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //If right click, unequip to inventory
            if (eventData.pointerId == -2)
            {
                InventoryItem item = playerEquipment.GetItemInSlot(equipLocation, index);
                if(item != null && inventory.AddToFirstEmptySlot(item, 1))
                {
                    playerEquipment.RemoveItem(equipLocation, index);
                }
            }
        }

        // PRIVATE

        void RedrawUI()
        {
            icon.SetItem(playerEquipment.GetItemInSlot(equipLocation, index), 1);
        }

        public object GetModifiers()
        {
            return playerEquipment.GetItemInSlot(equipLocation, index).GetModifiers();
        }

        public object GetSourceModifiers()
        {
            return playerEquipment.GetItemInSlot(equipLocation, index).GetModifiers();
        }
    }
}
