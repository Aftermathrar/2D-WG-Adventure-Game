using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [RequireComponent(typeof(Inventory), typeof(BaseStats))]
    public class RandomDropper : MonoBehaviour
    {
        [SerializeField] DropLibrary dropLibrary;
        [SerializeField] Inventory inventory = null;
        [SerializeField] BaseStats baseStats = null;

        private void Start() 
        {
            if(inventory == null)
                inventory = GetComponent<Inventory>();

            if(baseStats == null)
                baseStats = GetComponent<BaseStats>();
        }

        public void RandomDrop()
        {
            var itemDrops = dropLibrary.GetRandomDrops(Mathf.RoundToInt(baseStats.GetRawStat(Stat.DropTier)));
            
            foreach (var drop in itemDrops)
            {
                inventory.AddToFirstEmptySlot(drop.item, drop.number);
            }
        }
    }
}
