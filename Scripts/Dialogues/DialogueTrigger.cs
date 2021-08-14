using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Dialogues
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] ActionTriggerPair[] actionTriggerPairs;
        Dictionary<OnDialogueAction, UnityEvent<string[]>> actionLookup = null;

        private void Awake() 
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            if(actionLookup != null) return;

            actionLookup = new Dictionary<OnDialogueAction, UnityEvent<string[]>>();
            foreach (var pair in actionTriggerPairs)
            {
                actionLookup[pair.action] = pair.onTrigger;
            }
        }

        public void Trigger(OnDialogueAction actionToTrigger, string[] actionParameters)
        {
            UnityEvent<string[]> unityEvent;
            if(actionLookup.TryGetValue(actionToTrigger, out unityEvent))
            {
                unityEvent.Invoke(actionParameters);
            }
        }

        [System.Serializable]
        private class ActionTriggerPair
        {
            public OnDialogueAction action;
            public UnityEvent<string[]> onTrigger;
        }
    }
}
