using System.Collections;
using System.Collections.Generic;
using ButtonGame.Locations;
using ButtonGame.Saving;
using ButtonGame.Dialogues;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Quests
{
    public class QuestChecker : MonoBehaviour, ISaveable
    {
        [SerializeField] QuestActionCheck[] questActionChecks = null;
        private LocationManager locationManager = null;
        private QuestList questList = null;
        private AIConversant conversant = null;

        QuestActionCheck activeCheck = null;

        private void Awake() 
        {
            locationManager = GetComponent<LocationManager>();
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            questList = playerGO?.GetComponent<QuestList>();
            conversant = GetComponent<AIConversant>();
        }

        private void Start() 
        {
            // CheckQuestTriggers();
        }

        private void CheckQuestTriggers()
        {
            LocationList curLocation = locationManager.GetCurrentLocation();
            foreach (var check in questActionChecks)
            {
                if(!check.hasTriggered && check.location == curLocation && questList.HasQuest(check.quest)
                    && questList.HasObjectiveCompleted(check.quest, check.objective) == check.isOnCompletion)
                {
                    if (check.buttonTrigger != null)
                    {
                        QuestTrigger trigger = check.buttonTrigger.gameObject.AddComponent<QuestTrigger>();
                        trigger.SetQuest(check.quest);
                        trigger.SetObjectiveIndex(check.objective);
                        check.buttonTrigger.onClick.AddListener(() => trigger.CompleteObjective());
                    }
                    else
                    {
                        conversant.StartDialogue(check.dialogue);
                        activeCheck = check;
                    }
                }
            }
        }

        public void MarkAsTriggered()
        {
            if(activeCheck != null)
            {
                activeCheck.hasTriggered = true;
            }
            activeCheck = null;
        }

        public object CaptureState()
        {
            List<bool> hasTriggeredList = new List<bool>();
            foreach (var check in questActionChecks)
            {
                hasTriggeredList.Add(check.hasTriggered);
            }

            return hasTriggeredList;
        }

        public void RestoreState(object state)
        {
            List<bool> hasTriggeredList = (List<bool>)state;
            for (int i = 0; i < hasTriggeredList.Count; i++)
            {
                questActionChecks[i].hasTriggered = hasTriggeredList[i];
            }

            CheckQuestTriggers();
        }

        [System.Serializable]
        private class QuestActionCheck
        {
            public LocationList location;
            public Button buttonTrigger;
            public Quest quest;
            public int objective;
            public bool isOnCompletion;
            public bool hasTriggered;
            public Dialogue dialogue;
        }
    }
}
