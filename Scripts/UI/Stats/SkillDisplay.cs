using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Stats;
using ButtonGame.Inventories;
using UnityEngine.UI;
using ButtonGame.Stats.Enums;

namespace ButtonGame.UI.Stats
{
    public class SkillDisplay : MonoBehaviour, ITooltipProvider, ISkillDisplay
    {
        [SerializeField] AttackType atkName;
        [SerializeField] SkillDescription[] skillDescriptions;
        [SerializeField] TooltipDescriptionField[] skillTooltips;

        // Cache
        BaseAttackStats atkStats = null;

        [System.Serializable]
        private struct SkillDescription
        {
            public string prefix;
            public AttackStat attackStat;
        }

        private void OnEnable() 
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            atkStats = player.GetComponent<BaseAttackStats>();
        }

        public int GetSkillDescription()
        {
            return skillDescriptions.Length;
        }

        public string GetAttackStat(int i)
        {
            float atkValue;
            string s = atkStats.GetAttackStatString(skillDescriptions[i].attackStat, atkName);
            if (float.TryParse(s, out atkValue))
            {
                float[] atkTotal = atkStats.GetAttackStatArray(skillDescriptions[i].attackStat, atkName);
                if(atkTotal.Length > 1)
                {
                    atkValue = 0;
                    foreach (float val in atkTotal)
                    {
                        atkValue += val;
                    }
                }
                s = skillDescriptions[i].prefix + " " + atkValue;
            }

            return s;
        }

        public string GetDisplayName()
        {
            return atkStats.GetAttackStatString(AttackStat.Name, atkName);
        }

        public string GetCategoryName()
        {
            return "Skill";
        }

        public IEnumerable<TooltipDescriptionField> GetDescriptionFields()
        {
            return skillTooltips;
        }
    }
}
