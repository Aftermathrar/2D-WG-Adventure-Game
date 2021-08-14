using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Quests
{
    public class QuestTrigger : MonoBehaviour
    {
        [SerializeField] Quest quest;
        [SerializeField] int objectiveIndex;

        public void CompleteObjective()
        {
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            string objective = quest.GetObjective(objectiveIndex);
            questList.CompleteObjective(quest, objective);
        }

        public void SetQuest(Quest newQuest)
        {
            quest = newQuest;
        }

        public void SetObjectiveIndex(int newIndex)
        {
            objectiveIndex = newIndex;
        }
    }
}
