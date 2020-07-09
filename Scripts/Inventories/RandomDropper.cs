using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [RequireComponent(typeof(Inventory))]
    public class RandomDropper : MonoBehaviour
    {
        [SerializeField] InventoryItem[] dropLibrary;
        [SerializeField] Inventory inventory = null;

        private void Start() 
        {
            if(inventory == null)
                inventory = GetComponent<Inventory>();
        }

        public void RandomDrop()
        {
            int dropAmount = Random.Range(1, 6);
            for (int i = 0; i < dropAmount; i++)
            {
                int index = Random.Range(0, dropLibrary.Length);
                InventoryItem itemDrop = dropLibrary[index];
                inventory.AddToFirstEmptySlot(itemDrop, 1);
            }
        }
    }
}
