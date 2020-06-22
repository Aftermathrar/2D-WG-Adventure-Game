﻿using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using ButtonGame.Core.UI.Dragging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButtonGame.UI.Inventories
{
    public class RewardSlotUI : MonoBehaviour, IItemHolder, IPointerClickHandler, IDragContainer<InventoryItem>
    {
        [SerializeField] InventoryItemIcon icon = null;
        [SerializeField] Pickup pickup = null;

        int index;
        Inventory inventory;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (pickup.CanBePickedUp())
            {
                int number = pickup.PickupItem();
                inventory.RemoveFromSlot(index, number);
            }
        }

        public void Setup(Inventory inventory, int index)
        {
            this.inventory = inventory;
            this.index = index;
            icon.SetItem(inventory.GetItemInSlot(index), inventory.GetCountInSlot(index));
            pickup.Setup(inventory.GetItemInSlot(index), inventory.GetCountInSlot(index));
        }

        public InventoryItem GetItem()
        {
            return inventory.GetItemInSlot(index);
        }

        public int GetNumber()
        {
            return inventory.GetCountInSlot(index);
        }

        public int MaxAcceptable(InventoryItem item)
        {
            if (inventory.HasSpaceFor(item))
            {
                return int.MaxValue;
            }
            return 0;
        }

        public void AddItems(InventoryItem item, int number)
        {
            inventory.AddItemToSlot(index, item, number);
        }

        public void RemoveItems(int number)
        {
            inventory.RemoveFromSlot(index, number);
        }
    }
}
