using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core.UI.Tooltips;
using ButtonGame.Quests;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class QuestTooltipSpawner : TooltipSpawner
    {
        QuestItemUI questItemUI = null;

        private void Awake() 
        {
            questItemUI = GetComponent<QuestItemUI>();
        }

        public override bool CanCreateTooltip()
        {
            return questItemUI.IsFolded();
        }

        public override void UpdateTooltip()
        {
            // Not sure if relevant
        }

        public override void UpdateTooltip(GameObject tooltip)
        {
            QuestStatus status = questItemUI.GetQuestStatus();
            tooltip.GetComponent<QuestTooltipUI>().Setup(status);
        }
    }
}
