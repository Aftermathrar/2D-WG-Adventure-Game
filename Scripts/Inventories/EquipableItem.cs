using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// An inventory item that can be equipped to the player. Weapons could be a
    /// subclass of this.
    /// </summary>
    [CreateAssetMenu(menuName = ("Inventory/Equipment"))]
    public class EquipableItem : InventoryItem
    {
        // CONFIG DATA
        [Tooltip("Where are we allowed to put this item.")]
        [SerializeField] EquipLocation allowedEquipLocation = EquipLocation.Weapon;
        [SerializeField] EquipmentStats[] equipmentStats;

        // PUBLIC

        public EquipLocation GetAllowedEquipLocation()
        {
            return allowedEquipLocation;
        }

        public Dictionary<Stat, float[]> GetStatValues()
        {
            var statDict = new Dictionary<Stat, float[]>();
            foreach (var equipStat in equipmentStats)
            {
                if(!statDict.ContainsKey(equipStat.stat))
                {
                    float[] val = new float[2] {0, 0};
                    int i = equipStat.isAdditive ? 1 : 0;
                    val[i] = equipStat.value;
                    statDict[equipStat.stat] = val;
                }
                else
                {
                    int i = equipStat.isAdditive ? 1 : 0;
                    statDict[equipStat.stat][i] = equipStat.value;
                }
            }
            return statDict;
        }

        [System.Serializable]
        private struct EquipmentStats
        {
            public Stat stat;
            public float value;
            public bool isAdditive;
        }
    }
}