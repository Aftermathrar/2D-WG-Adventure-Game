using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Quests;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class QuestTooltipUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] Transform objectiveContainer;
        [SerializeField] GameObject objectivePrefab;
        [SerializeField] GameObject objectiveIncompletePrefab;
        [SerializeField] TextMeshProUGUI rewardText;

        public void Setup(QuestStatus status)
        {
            Quest quest = status.GetQuest();
            title.text = quest.GetTitle();
            foreach (Transform item in objectiveContainer)
            {
                Destroy(item.gameObject);
            }

            foreach (var objective in quest.GetObjectives())
            {
                GameObject prefab = objectiveIncompletePrefab;
                if(status.IsObjectiveComplete(objective))
                {
                    prefab = objectivePrefab;
                }
                GameObject objectiveInstance = Instantiate(prefab, objectiveContainer);
                objectiveInstance.GetComponentInChildren<TextMeshProUGUI>().text = objective;
            }

            rewardText.text = GetRewardText(quest);
        }

        private string GetRewardText(Quest quest)
        {
            string rewardText = "";
            foreach (var reward in quest.GetRewards())
            {
                if(rewardText != "")
                {
                    rewardText += ", ";
                }
                if (reward.number > 1)
                {
                    rewardText += reward.number + " ";
                }
                rewardText += reward.item.GetDisplayName();
            }
            if(rewardText == "")
            {
                rewardText = "No reward";
            }
            rewardText += ".";
            return rewardText;
        }
    }
}
