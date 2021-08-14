using ButtonGame.Quests;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.Quests
{
    public class ObjectiveTooltipUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] Transform objectiveContainer;
        [SerializeField] GameObject parameterPrefab;
        
        public void Setup(QuestStatus status, int i)
        {
            Quest quest = status.GetQuest();
            title.text = quest.GetObjective(i);
            foreach (Transform item in objectiveContainer)
            {
                Destroy(item.gameObject);
            }

            int objectiveCount = quest.GetObjectiveCount();

            foreach (string parameter in quest.GetObjectiveParameters(i))
            {
                GameObject parameterInstance = Instantiate(parameterPrefab, objectiveContainer);
                parameterInstance.GetComponent<TextMeshProUGUI>().text = parameter;
            }
        }
    }
}
