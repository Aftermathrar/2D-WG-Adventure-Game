using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Quests;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Dialogue
{
    public class DialogueNode : ScriptableObject
    {
        [SerializeField]
        bool isPlayerSpeaking = false;
        [SerializeField]
        string speakerName;
        [SerializeField]
        string text;
        [SerializeField]
        List<string> children = new List<string>();
        [SerializeField]
        Rect rect = new Rect(10, 10, 250, 75);
        [SerializeField]
        string OnEnterAction;
        [SerializeField]
        string onExitAction;
        [SerializeField]
        Condition condition;

        bool hasOnEnterAction;
        bool hasOnExitAction;
        bool hasConditionSelect;
        int objectiveIndex;

        public string GetText()
        {
            return text;
        }

        public string GetSpeaker()
        {
            return speakerName;
        }

        public List<string> GetChildren()
        {
            return children;
        }

        public Rect GetRect()
        {
            return rect;
        }

        public bool IsPlayerSpeaking()
        {
            return isPlayerSpeaking;
        }

        public bool GetHasOnEnterAction()
        {
            return hasOnEnterAction;
        }

        public bool GetHasOnExitAction()
        {
            return hasOnExitAction;
        }

        public bool GetHasConditionSelect()
        {
            return hasConditionSelect;
        }

        public string GetOnEnterAction()
        {
            return OnEnterAction;
        }

        public string GetOnExitAction()
        {
            return onExitAction;
        }

        public int GetObjectiveIndex()
        {
            return objectiveIndex;
        }

        public ConditionPredicate GetCondition()
        {
            return condition.GetPredicate();
        }

        public IEnumerable<string> GetParameters()
        {
            return condition.GetParameters();
        }

        public bool GetConditionNegate()
        {
            return condition.GetNegate();
        }

        public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
        {
            return condition.Check(evaluators);
        }

#if UNITY_EDITOR
        public void SetText(string newText)
        {
            if(newText != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");
                text = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void SetSpeaker(string newSpeaker)
        {
            Undo.RecordObject(this, "Modify Speaker Name");
            speakerName = newSpeaker;
            EditorUtility.SetDirty(this);
        }

        public void SetOnEnterAction(string newEnterAction)
        {
            Undo.RecordObject(this, "Modify OnEnterAction");
            OnEnterAction = newEnterAction;
            EditorUtility.SetDirty(this);
        }

        public void SetOnExitAction(string newExitAction)
        {
            Undo.RecordObject(this, "Modify OnExitAction");
            onExitAction = newExitAction;
            EditorUtility.SetDirty(this);
        }

        public void SetCondition(ConditionPredicate newPredicate)
        {
            Undo.RecordObject(this, "Change Condition Predicate");
            condition.SetPredicate(newPredicate);
            EditorUtility.SetDirty(this);
        }

        public void SetNodeHeight(float newHeight)
        {
            rect.height = newHeight;
            EditorUtility.SetDirty(this);
        }

        public void AddChild(string childID)
        {
            Undo.RecordObject(this, "Add Dialogue Link");
            children.Add(childID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            children.Remove(childID);
            EditorUtility.SetDirty(this);
        }

        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void SetPlayerSpeaking(bool newIsPlayerSpeaking)
        {
            Undo.RecordObject(this, "Change Dialogue Speaker");
            isPlayerSpeaking = newIsPlayerSpeaking;
            EditorUtility.SetDirty(this);
        }

        // Value controlled by Dialogue Editor GUI
        public void SetHasOnEnterAction(bool toggleValue)
        {
            hasOnEnterAction = toggleValue;
            EditorUtility.SetDirty(this);
        }

        // Value controlled by Dialogue Editor GUI
        public void SetHasOnExitAction(bool toggleValue)
        {
            hasOnExitAction = toggleValue;
            EditorUtility.SetDirty(this);
        }

        // Value controlled by Dialogue Editor GUI
        public void SetHasConditionSelect(bool toggleValue)
        {
            hasConditionSelect = toggleValue;
            EditorUtility.SetDirty(this);
        }

        public void SetConditionParameters(IEnumerable<string> newParameters)
        {
            Undo.RecordObject(this, "Change Condition Parameter");
            condition.SetParameters(newParameters);
            EditorUtility.SetDirty(this);
        }

        public void SetConditionNegate(bool newNegate)
        {
            Undo.RecordObject(this, "Change Condition Negate");
            condition.SetNegate(newNegate);
            EditorUtility.SetDirty(this);
        }

        public void SetObjectiveIndex(int newIndex)
        {
            Undo.RecordObject(this, "Change ConditionObjective");
            objectiveIndex = newIndex;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
