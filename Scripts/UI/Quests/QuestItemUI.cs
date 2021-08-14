using System.Collections;
using System.Collections.Generic;
using ButtonGame.Quests;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class QuestItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI progress;
        [SerializeField] RectTransform dropdownIcon;
        [SerializeField] GameObject objectivePrefab;
        [SerializeField] GameObject incompletePrefab;
        [SerializeField] Transform objectiveContainer;

        QuestStatus status;
        bool isDropdownFolded = true;

        public void Setup(QuestStatus status)
        {
            this.status = status;
            title.text = status.GetQuest().GetTitle();
            progress.text = status.GetCompletedCount() + "/" + status.GetQuest().GetObjectiveCount();
        }

        public QuestStatus GetQuestStatus()
        {
            return status;
        }

        public bool IsFolded()
        {
            return isDropdownFolded;
        }

        public void FoldoutQuestInfo()
        {
            if(isDropdownFolded)
            {
                Quest quest = status.GetQuest();
                foreach (var objective in quest.GetObjectives())
                {
                    GameObject prefab = incompletePrefab;
                    if (status.IsObjectiveComplete(objective))
                    {
                        prefab = objectivePrefab;
                    }
                    GameObject objectiveInstance = Instantiate(prefab, objectiveContainer);
                    TextMeshProUGUI objectiveText = objectiveInstance.GetComponentInChildren<TextMeshProUGUI>();
                    objectiveText.text = objective;
                    objectiveText.fontSize = 24;
                }
            }
            else
            {
                foreach (Transform item in objectiveContainer)
                {
                    Destroy(item.gameObject);
                }
            }

            RotateDropdownIcon();
        }

        private void RotateDropdownIcon()
        {
            float z = isDropdownFolded ? -90 : 0;
            dropdownIcon.localRotation = Quaternion.Euler(0, 0, z);
            isDropdownFolded = !isDropdownFolded;
        }
    }
}
