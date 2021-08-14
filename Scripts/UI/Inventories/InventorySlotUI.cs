using UnityEngine;
using ButtonGame.Inventories;
using ButtonGame.Core.UI.Dragging;
using UnityEngine.EventSystems;
using System;

namespace ButtonGame.UI.Inventories
{
    public class InventorySlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>, IPointerClickHandler
    {
        // CONFIG DATA
        [SerializeField] InventoryItemIcon icon = null;

        // STATE
        int index;
        Inventory inventory = null;
        Equipment playerEquipment = null;
        Equipment followerEquipment = null;

        // PUBLIC

        public void Setup(Inventory inventory, Equipment playerEquipment, Equipment followerEquipment, int index)
        {
            this.inventory = inventory;
            this.playerEquipment = playerEquipment;
            this.followerEquipment = followerEquipment;
            this.index = index;
            icon.SetItem(inventory.GetItemInSlot(index), inventory.GetCountInSlot(index));
        }

        public void ChangeFollowerEquipment(Equipment newFollowerEquipment)
        {
            followerEquipment = newFollowerEquipment;
        }

        public int MaxAcceptable(InventoryItem item)
        {
            if (inventory.HasSpaceFor(item))
            {
                return int.MaxValue;
            }
            return 0;
        }

        public bool HasStack(InventoryItem item)
        {
            if(item.IsStackable() && inventory.HasItem(item))
            {
                return true;
            }
            return false;
        }

        public void AddItems(InventoryItem item, int number, object state)
        {
            inventory.AddItemToSlot(index, item, number, state);
        }

        public InventoryItem GetItem()
        {
            return inventory.GetItemInSlot(index);
        }

        public int GetNumber()
        {
            return inventory.GetCountInSlot(index);
        }

        public object GetModifiers()
        {
            return inventory.GetItemInSlot(index).GetModifiers();
        }

        public object GetSourceModifiers()
        {
            return inventory.GetItemInSlot(index).GetModifiers();
        }

        public void RemoveItems(int number)
        {
            inventory.RemoveFromSlot(index, number);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerId != -2) return;

            MealBuffItem mealBuffItem = inventory.GetItemInSlot(index) as MealBuffItem;
            if(mealBuffItem != null)
            {
                AddMealBuff(mealBuffItem);
            }

            EquipableItem equipableItem = inventory.GetItemInSlot(index) as EquipableItem;
            if (playerEquipment != null && equipableItem != null)
            {
                EquipNewItem(equipableItem);
            }

        }

        private void AddMealBuff(MealBuffItem mealBuffItem)
        {
            mealBuffItem.OnItemUse();
            RemoveItems(1);
        }

        private void EquipNewItem(EquipableItem equipableItem)
        {
            // Set up correct equipment reference
            Equipment equipment = null;
            if (equipableItem.IsPlayerEquipment())
                equipment = playerEquipment;
            else
            {
                if (followerEquipment == null) return;

                equipment = followerEquipment;
            }

            int equipIndex = equipment.TryAddItem(equipableItem);
            if (equipIndex >= 0)
            {
                EquipLocation equipLocation = equipableItem.GetAllowedEquipLocation();

                EquipableItem takebackItem = equipment.GetItemInSlot(equipLocation, equipIndex);

                RemoveItems(1);
                if (takebackItem != null)
                {
                    equipment.RemoveItem(equipLocation, equipIndex);
                    AddItems(takebackItem, 1, takebackItem.GetModifiers());
                }
                equipment.AddItem(equipLocation, equipableItem, equipIndex, equipableItem.GetModifiers());
            }
        }
    }
}