using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [CreateAssetMenu(menuName = ("Inventory/Consumable"))]
    public class ConsumableItem : InventoryItem
    {
        [SerializeField] float size = 100;
        [SerializeField] float calories = 100;
        float calorieDensity;

        private void Start() 
        {
            calorieDensity = calories / size;
        }

        public float GetSize()
        {
            return size;
        }

        public float GetCalories()
        {
            return calories;
        }
    }
}
