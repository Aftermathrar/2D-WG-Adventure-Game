using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Tooltips;
using ButtonGame.Quests;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class ObjectiveTooltipSpawner : TooltipSpawner
    {
        public override bool CanCreateTooltip()
        {
            return true;
        }

        public override void UpdateTooltip()
        {
            
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            QuestStatus status = GetComponentInParent<QuestItemUI>().GetQuestStatus();
            int index = transform.GetSiblingIndex();
            tooltip.GetComponent<ObjectiveTooltipUI>().Setup(status, index);
        }
    }
}
