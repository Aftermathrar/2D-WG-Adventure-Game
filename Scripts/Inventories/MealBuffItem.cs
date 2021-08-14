using System.Collections;
using System.Collections.Generic;
using ButtonGame.Combat;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Inventories
{
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "MealBuffItem", menuName = "Inventory/MealBuffItem", order = 5)]
    public class MealBuffItem : InventoryItem 
    {
        [SerializeField] EffectName effect;

        public void OnItemUse()
        {
            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<CombatEffects>().BuffItemEffect(effect);
        }
    }
}
