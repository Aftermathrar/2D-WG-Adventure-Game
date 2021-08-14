using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ButtonGame.Attributes;
using ButtonGame.Core;
using UnityEngine;

namespace ButtonGame.Dialogues
{
    public class PlayerConversant : MonoBehaviour
    {
        public string playerName;

        [SerializeField] Dialogue currentDialogue;
        DialogueNode currentNode = null;
        AIConversant currentConversant = null;
        bool isChoosing = false;

        public event Action onConversationUpdated;

        public void StartDialogue(AIConversant newConversant, Dialogue newDialogue)
        {
            playerName = GetComponent<PlayerInfo>().GetPlayerInfo("name");
            currentConversant = newConversant;
            currentDialogue = newDialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterAction();
            onConversationUpdated?.Invoke();
        }

        public void Quit()
        {
            TriggerExitAction();
            currentConversant = null;
            currentDialogue = null;
            currentNode = null;
            isChoosing = false;
            onConversationUpdated?.Invoke();
        }

        public bool IsActive()
        {
            return currentDialogue != null;
        }

        public bool IsChoosing()
        {
            return isChoosing;
        }

        public string GetText()
        {
            if(currentNode == null)
            {
                return "";
            }

            return currentNode.GetText();
        }

        public string GetText(DialogueNode queryNode)
        {
            return queryNode.GetText();
        }

        public AIConversant GetCurrentConversant()
        {
            return currentConversant;
        }

        public string GetCurrentConversantName()
        {
            if(isChoosing)
            {
                return playerName;
            }
            else
            {
                return currentNode.GetSpeaker();
            }
        }

        public string GetCurrentConversantName(DialogueNode queryNode)
        {
            return queryNode.GetSpeaker();
        }

        public IEnumerable<DialogueNode> GetChoices()
        {
            return FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode));
        }

        public IEnumerable<DialogueNode> GetResponses()
        {
            return FilterOnCondition(currentDialogue.GetAIChildren(currentNode));
        }

        public void SelectChoice(DialogueNode chosenNode)
        {
            currentNode = chosenNode;
            TriggerEnterAction();
            Next();
        }

        public void Next()
        {
            int numPlayerResponses = FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode)).Count();
            DialogueNode[] children = FilterOnCondition(currentDialogue.GetAIChildren(currentNode)).ToArray();
            TriggerExitAction();

            if ((children.Length + numPlayerResponses) == 0)
            {
                Quit();
                return;
            }

            if(numPlayerResponses > 0)
            {
                isChoosing = true;
            }
            else
            {
                isChoosing = false;
                int randomIndex = UnityEngine.Random.Range(0, children.Length);
                currentNode = children[randomIndex];
                TriggerEnterAction();
            }
            onConversationUpdated();
        }

        public bool HasNext()
        {
            return FilterOnCondition(currentDialogue.GetAllChildren(currentNode)).Count() > 0;
        }

        private IEnumerable<DialogueNode> FilterOnCondition(IEnumerable<DialogueNode> inputNode)
        {
            foreach (var node in inputNode)
            {
                if(node.CheckCondition(GetEvaluators()))
                {
                    yield return node;
                }
            }
        }

        private IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return GetComponents<IPredicateEvaluator>();
        }

        private void TriggerEnterAction()
        {
            if(currentNode != null)
            {
                TriggerAction(currentNode.GetOnEnterAction(), currentNode.GetOnEnterActionParameters().ToArray());
            }
        }

        private void TriggerExitAction()
        {
            if (currentNode != null)
            {
                TriggerAction(currentNode.GetOnExitAction(), currentNode.GetOnExitActionParameters().ToArray());
            }
        }

        private void TriggerAction(OnDialogueAction action, string[] actionParameters)
        {
            if(action == OnDialogueAction.None) return;

            foreach (DialogueTrigger trigger in currentConversant.GetComponents<DialogueTrigger>())
            {
                trigger.Trigger(action, actionParameters);
            }
        }
    }
}
