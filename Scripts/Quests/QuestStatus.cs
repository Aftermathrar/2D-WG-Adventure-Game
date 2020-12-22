using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Quests
{
    public class QuestStatus
    {
        Quest quest;
        List<string> completedObjectives = new List<string>();

        [System.Serializable]
        class QuestStatusRecord
        {
            public string questName;
            public List<string> completedObjectives = new List<string>();
        }

        public QuestStatus(Quest quest)
        {
            this.quest = quest;
        }

        public QuestStatus(object objectState)
        {
            QuestStatusRecord state = objectState as QuestStatusRecord;
            quest = Quest.GetByName(state.questName);
            completedObjectives = state.completedObjectives;
        }

        public Quest GetQuest()
        {
            return quest;
        }

        public int GetCompletedCount()
        {
            return completedObjectives.Count;
        }
        
        public bool IsObjectiveComplete(string objective)
        {
            return completedObjectives.Contains(objective);
        }

        public bool IsComplete()
        {
            // Checking for duplicate completions, no need to iterate
            return quest.GetObjectiveCount() == completedObjectives.Count;

            // foreach (var objective in quest.GetObjectives())
            // {
            //     if(!completedObjectives.Contains(objective.reference))
            //     {
            //         return false;
            //     }
            // }
            // return true;
        }

        internal bool CompleteObjective(string objective)
        {
            if(!IsObjectiveComplete(objective))
            {
                if (quest.HasObjective(objective))
                {
                    completedObjectives.Add(objective);
                    return true;
                }
                else
                {
                    Debug.Log("Error! " + quest.GetTitle() + " does not contain objective: " + objective);
                }
            }
            return false;
        }

        public object CaptureState()
        {
            QuestStatusRecord state = new QuestStatusRecord();
            state.questName = quest.name;
            state.completedObjectives = completedObjectives;
            return state;
        }
    }
}