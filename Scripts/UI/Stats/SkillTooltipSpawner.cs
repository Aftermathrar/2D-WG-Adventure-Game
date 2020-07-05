using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Tooltips;
using ButtonGame.Stats;
using ButtonGame.UI.Inventories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButtonGame.UI.Stats
{
    [RequireComponent(typeof(SkillDisplay))]
    public class SkillTooltipSpawner : TooltipSpawner, IPointerEnterHandler
    {
        SkillDisplay skill = null;
        int skillDescLength;
        List<Text> textLines = new List<Text>();

        private void OnEnable() 
        {
            skill = GetComponent<SkillDisplay>();
        }
        public override bool CanCreateTooltip()
        {
            skillDescLength = skill.GetSkillDescription();
            return skillDescLength > 0;
        }

        public override void UpdateTooltip()
        {
            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;
            
            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            Transform parent = itemTooltip.GetTooltipContainer();

            for (int i = 0; i < skillDescLength; i++)
            {
                Text skillText = Instantiate(tooltipPrefab, parent).GetComponent<Text>();
                skill.GetAttackStat(skillText, i);
                textLines.Add(skillText);
            }
            
            itemTooltip.RemoveTooltip();
            itemTooltip.Setup(skill);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (CanCreateTooltip())
            {
                UpdateTooltip();
            }
        }

        protected override void ClearTooltip()
        {
            foreach (Text text in textLines)
            {
                Destroy(text.gameObject);
            }
            textLines.Clear();

            var tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            if (!tooltipWindow) return;

            var itemTooltip = tooltipWindow.GetComponent<ItemTooltip>();
            itemTooltip.RemoveTooltip();
        }
    }
}
