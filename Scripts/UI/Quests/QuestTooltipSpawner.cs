using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Tooltips;
using ButtonGame.Quests;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class QuestTooltipSpawner : TooltipSpawner
    {
        public override bool CanCreateTooltip()
        {
            return true;
        }

        public override void UpdateTooltip()
        {
            // Not sure if relevant
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            QuestStatus status = GetComponent<QuestItemUI>().GetQuestStatus();
            tooltip.GetComponent<QuestTooltipUI>().Setup(status);
        }
    }
}
