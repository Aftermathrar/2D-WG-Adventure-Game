using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.Quests
{   
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest", order = 0)]
    public class Quest : ScriptableObject 
    {
        [SerializeField]
        List<Objective> objectives = new List<Objective>();
        [SerializeField] 
        List<Reward> rewards = new List<Reward>();

        [System.Serializable]
        public class Reward
        {
            [Min(1)]
            public int number;
            public InventoryItem item;
        }

        [System.Serializable]
        public class Objective
        {
            public string description;
            public QuestAction completeObjectiveAction;
            public List<string> parameter;
        }

        public int GetObjectiveCount()
        {
            return objectives.Count;
        }

        public IEnumerable<string> GetObjectives()
        {
            foreach (var objective in objectives)
            {
                yield return objective.description;
            }
        }

        public IEnumerable<Reward> GetRewards()
        {
            return rewards;
        }

        internal string GetTitle()
        {
            return name;
        }

        internal bool HasObjective(string objectiveRef)
        {
            foreach (var objective in objectives)
            {
                if(objective.description == objectiveRef)
                {
                    return true;
                }
            }
            return false;
        }

        public static Quest GetByName(string questName)
        {
            foreach (Quest quest in Resources.LoadAll<Quest>(""))
            {
                if(quest.name == questName)
                {
                    return quest;
                }
            } 
            return null;
        }
    }
}
