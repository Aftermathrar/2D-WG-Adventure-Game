using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Stats;
using ButtonGame.Inventories;
using UnityEngine.UI;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;

namespace ButtonGame.UI.Stats
{
    public class FollowerSkillDisplay : MonoBehaviour, ITooltipProvider, ISkillDisplay
    {
        [SerializeField] FollowerAttackName atkName;
        [SerializeField] FollowerAttackDB attackDB;
        [SerializeField] TooltipDescriptionField[] skillTooltips;

        // Cache
        FollowerAttackStats attackStats;
        string[] skillDescription;

        private void OnEnable() 
        {
            GameObject follower = GameObject.FindGameObjectWithTag("Follower");
            attackStats = attackDB.GetAttackStat(atkName);
            if(follower.GetComponent<BaseStats>().GetClass() != attackStats.HealingClass)
                gameObject.SetActive(false);
            
            skillDescription = new string[4];
            skillDescription[0] = attackStats.Description;
            skillDescription[1] = "Mana Cost: " + attackStats.Cost.ToString();
            skillDescription[2] = "Cast Time: " + attackStats.CastTime.ToString();
            skillDescription[3] = "Cooldown: " + attackStats.Cooldown.ToString();
        }

        public int GetSkillDescription()
        {
            return skillDescription.Length;
        }

        public string GetAttackStat(int i)
        {
            return skillDescription[i];
        }

        public string GetDisplayName()
        {
            return attackDB.GetAttackStat(atkName).Name;
        }

        public string GetCategoryName()
        {
            return attackDB.GetAttackStat(atkName).movePool.ToString();
        }

        public IEnumerable<TooltipDescriptionField> GetDescriptionFields()
        {
            return skillTooltips;
        }
    }
}
