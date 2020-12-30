using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using ButtonGame.Saving;
using ButtonGame.Core;
using UnityEngine;

namespace ButtonGame.Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        List<QuestStatus> statuses = new List<QuestStatus>();
        Dictionary<string, QuestStatus> completedQuests = new Dictionary<string, QuestStatus>();

        public event Action onQuestUpdated;

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest)) return;
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);

            OnQuestUpdated();
        }

        public void AddQuest(string[] questString)
        {
            Quest quest = Quest.GetByName(questString[0]);
            if(HasQuest(quest)) return;
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);

            OnQuestUpdated();
        }

        private void OnQuestUpdated()
        {
            if (onQuestUpdated != null)
            {
                onQuestUpdated();
            }
        }

        public void CompleteObjective(Quest quest, string objective)
        {
            QuestStatus status = GetQuestStatus(quest);
            if(status != null && status.CompleteObjective(objective))
            {
                if(status.IsComplete())
                {
                    GiveReward(quest);
                    completedQuests[quest.GetTitle()] = status;
                    statuses.Remove(status);
                }
                OnQuestUpdated();
            }
        }

        public void CompleteObjective(string[] questParameters)
        {
            Quest quest = Quest.GetByName(questParameters[0]);
            CompleteObjective(quest, questParameters[1]);
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        public bool HasObjectiveCompleted(Quest quest, string objective)
        {
            if(completedQuests.ContainsKey(quest.GetTitle()))
            {
                return true;
            }
            QuestStatus status = GetQuestStatus(quest);
            return (status != null && status.IsObjectiveComplete(objective));
        }

        public bool HasQuestCompleted(string questString)
        {
            return completedQuests.ContainsKey(questString);
        }

        private QuestStatus GetQuestStatus(Quest quest)
        {
            if(completedQuests.ContainsKey(quest.GetTitle()))
            {
                return completedQuests[quest.GetTitle()];
            }
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }

            return null;
        }

        private void GiveReward(Quest quest)
        {
            foreach (var reward in quest.GetRewards())
            {
                if(!GetComponent<Inventory>().AddToFirstEmptySlot(reward.item, reward.number))
                {
                    // TODO: Refuse quest completion if no space for rewards
                }
            }
        }

        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }

        public object CaptureState()
        {
            List<object> state = new List<object>();
            foreach (QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            List<object> stateList = state as List<object>;
            if(stateList == null) return;

            statuses.Clear();
            foreach (object objectState in stateList)
            {
                statuses.Add(new QuestStatus(objectState));
            }
        }

        public bool? Evaluate(ConditionPredicate predicate, List<string> parameters)
        {
            switch(predicate)
            {
                case ConditionPredicate.HasQuest:
                    foreach (var parameter in parameters)
                    {
                        if (!HasQuest(Quest.GetByName(parameter))) return false;
                    }
                    return true;
                case ConditionPredicate.CompleteQuest:
                    return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
                case ConditionPredicate.CompleteObjective:
                    QuestStatus status = GetQuestStatus(Quest.GetByName(parameters[0]));
                    if (status != null)
                    {
                        return status.IsObjectiveComplete(parameters[1]);
                    }
                    return null;
            }

            // if(predicate == ConditionPredicate.None) return null;
            // else if(predicate == ConditionPredicate.HasQuest)
            // {
            //     foreach (var parameter in parameters)
            //     {
            //         if(!HasQuest(Quest.GetByName(parameter))) return false;
            //     }
            //     return true;
            // }
            // else if(predicate == ConditionPredicate.CompleteQuest)
            // {
            //     return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
            // }
            // else if(predicate == ConditionPredicate.CompleteObjective)
            // {
            //     QuestStatus status = GetQuestStatus(Quest.GetByName(parameters[0]));
            //     if(status != null)
            //     {
            //         return status.IsObjectiveComplete(parameters[1]);
            //     }
            // }

            return null;
        }
    }
}
