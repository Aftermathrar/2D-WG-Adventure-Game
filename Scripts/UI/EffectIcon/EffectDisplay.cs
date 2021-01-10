using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Combat;
using ButtonGame.Inventories;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.UI.EffectIcon
{
    public class EffectDisplay : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] CharacterClass healingClass;
        [SerializeField] EffectDB effectDB;
        [SerializeField] EffectDescription[] effectDescriptions;
        [SerializeField] TooltipDescriptionField[] effectTooltips;

        EffectName fxName;
        List<TooltipDescriptionField> fxStatList;

        [System.Serializable]
        private struct EffectDescription
        {
            public string prefix;
            public EffectStat effectStat;
        }

        private void OnEnable() 
        {
            GameObject follower = GameObject.FindWithTag("Follower");
            if(follower == null) return;

            healingClass = follower.GetComponent<BaseStats>().GetClass();
        }

        public void SetEffectName(string id)
        {
            fxName = (EffectName)Enum.Parse(typeof(EffectName), id);
            BuildEffectTooltips();
        }

        public int GetEffectDescriptionLength()
        {
            return effectDescriptions.Length;
        }

        public string GetEffectStat(int i)
        {
            float fxValue;
            string s = effectDB.GetEffectStat(effectDescriptions[i].effectStat, fxName);
            if(float.TryParse(s, out fxValue))
            {
                if(effectDescriptions[i].effectStat == EffectStat.Duration)
                {
                    if(fxValue > 60)
                    {
                        fxValue = Mathf.RoundToInt(fxValue/60);
                        s = effectDescriptions[i].prefix + " " + fxValue + " minutes";
                    }
                    else
                    {
                        s = effectDescriptions[i].prefix + " " + fxValue + " seconds";
                    }
                }
                else
                {
                    s = effectDescriptions[i].prefix + " " + fxValue;
                }
            }
            return s;
        }

        public string GetCategoryName()
        {
            return "Combat Buff";
        }

        public IEnumerable<TooltipDescriptionField> GetDescriptionFields()
        {
            return effectTooltips;
        }

        public string GetDisplayName()
        {
            return effectDB.GetEffectStat(EffectStat.Name, fxName);
        }

        private void BuildEffectTooltips() 
        {
            fxStatList = new List<TooltipDescriptionField>();
            string[] fxAdditiveData = effectDB.GetEffectStat(EffectStat.Additive, fxName).Split(new char[] { ',' });
            string[] fxBaseStats = effectDB.GetEffectStat(EffectStat.StatsAffected, fxName).Split(new char[] { ',' });
            string[] fxValueData = effectDB.GetEffectStat(EffectStat.EffectValues, fxName).Split(new char[] { ',' });
            string modType;

            for (int i = 0; i < fxAdditiveData.Length; i++)
            {
                TooltipDescriptionField tip = new TooltipDescriptionField();
                tip.hasIcon = false;
                if(fxAdditiveData[i] == "1")
                {
                    modType = " points";
                }
                else
                {
                    modType = "%";
                }
                tip.description = "Increases " + fxBaseStats[i] + " by " + fxValueData[i] + modType;

                fxStatList.Add(tip);
            }

            effectTooltips = fxStatList.ToArray();
        }
    }
}
