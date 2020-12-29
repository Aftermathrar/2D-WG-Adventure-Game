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
        OnDialogueAction onEnterAction;
        [SerializeField]
        List<string> onEnterActionParameter = new List<string>();
        [SerializeField]
        OnDialogueAction onExitAction;
        [SerializeField]
        List<string> onExitActionParameter = new List<string>();
        [SerializeField]
        Condition condition;

        bool hasOnEnterAction;
        bool hasOnExitAction;
        bool hasConditionSelect;
        int enterActionIndex;
        int exitActionIndex;

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

        public IEnumerable<string> GetOnEnterActionParameters()
        {
            return onEnterActionParameter;
        }

        public IEnumerable<string> GetOnExitActionParameters()
        {
            return onExitActionParameter;
        }

        public bool GetHasConditionSelect()
        {
            return hasConditionSelect;
        }

        public OnDialogueAction GetOnEnterAction()
        {
            return onEnterAction;
        }

        public OnDialogueAction GetOnExitAction()
        {
            return onExitAction;
        }

        public int GetObjectiveIndex(int indexAnd, int indexOr)
        {
            return condition.GetObjectiveIndex(indexAnd, indexOr);
        }

        public IEnumerable<ConditionPredicate> GetConditionPredicates(int index)
        {
            return condition.GetPredicate(index);
        }

        public IEnumerable<string> GetParameters(int indexAnd, int indexOr)
        {
            return condition.GetParameters(indexAnd, indexOr);
        }

        public bool GetConditionNegate(int indexAnd, int indexOr)
        {
            return condition.GetNegate(indexAnd, indexOr);
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

        public void SetOnEnterAction(OnDialogueAction newEnterAction)
        {
            Undo.RecordObject(this, "Modify OnEnterAction");
            onEnterAction = newEnterAction;
            EditorUtility.SetDirty(this);

            int paramLength = onEnterActionParameter.Count;
            switch (onEnterAction)
            {
                case OnDialogueAction.GiveQuest:
                    if (paramLength != 1)
                    {
                        onEnterActionParameter.Clear();
                        onEnterActionParameter.Add("");
                    }
                    return;
                case OnDialogueAction.MoveWindow:
                    if(paramLength != 1)
                    {
                        onEnterActionParameter.Clear();
                        onEnterActionParameter.Add("0");
                    }
                    break;
                case OnDialogueAction.CompleteObjective:
                    if (paramLength != 2)
                    {
                        onEnterActionParameter.Clear();
                        onEnterActionParameter.Add("");
                        onEnterActionParameter.Add("");
                    }
                    return;
                case OnDialogueAction.GiveItem:
                    if (paramLength != 2)
                    {
                        onEnterActionParameter.Clear();
                        onEnterActionParameter.Add("");
                        onEnterActionParameter.Add("0");
                    }
                    return;
                default:
                    onEnterActionParameter.Clear();
                    return;
            }
        }

        public void SetOnExitAction(OnDialogueAction newExitAction)
        {
            Undo.RecordObject(this, "Modify OnExitAction");
            onExitAction = newExitAction;
            EditorUtility.SetDirty(this);

            int paramLength = onExitActionParameter.Count;
            switch (onExitAction)
            {
                case OnDialogueAction.GiveQuest:
                    if(paramLength != 1)
                    {
                        onExitActionParameter.Clear();
                        onExitActionParameter.Add("");
                    }
                    return;
                case OnDialogueAction.MoveWindow:
                    if (paramLength != 1)
                    {
                        onEnterActionParameter.Clear();
                        onEnterActionParameter.Add("0");
                    }
                    break;
                case OnDialogueAction.CompleteObjective:
                    if(paramLength != 2)
                    {
                        onExitActionParameter.Clear();
                        onExitActionParameter.Add("");
                        onExitActionParameter.Add("");
                    }
                    return;
                case OnDialogueAction.GiveItem:
                    if (paramLength != 2)
                    {
                        onExitActionParameter.Clear();
                        onExitActionParameter.Add("");
                        onExitActionParameter.Add("0");
                    }
                    return;
                default:
                    onExitActionParameter.Clear();
                    return;
            }
        }

        public void SetOnEnterActionParameters(List<string> newAction)
        {
            Undo.RecordObject(this, "Change Action Parameter");
            onEnterActionParameter = newAction;
            EditorUtility.SetDirty(this);
        }

        public void SetOnExitActionParameters(List<string> newAction)
        {
            Undo.RecordObject(this, "Change Action Parameter");
            onExitActionParameter = newAction;
            EditorUtility.SetDirty(this);
        }

        public void SetConditionPredicate(ConditionPredicate newPredicate, int indexAnd, int indexOr)
        {
            Undo.RecordObject(this, "Change Condition Predicate");
            condition.SetPredicate(newPredicate, indexAnd, indexOr);
            EditorUtility.SetDirty(this);
        }

        public void SetConditionParameters(IEnumerable<string> newParameters, int indexAnd, int indexOr)
        {
            Undo.RecordObject(this, "Change Condition Parameter");
            condition.SetParameters(newParameters, indexAnd, indexOr);
            EditorUtility.SetDirty(this);
        }

        public void SetConditionNegate(bool newNegate, int indexAnd, int indexOr)
        {
            Undo.RecordObject(this, "Change Condition Negate");
            condition.SetNegate(newNegate, indexAnd, indexOr);
            EditorUtility.SetDirty(this);
        }

        public void SetObjectiveIndex(int newIndex, int indexAnd, int indexOr)
        {
            Undo.RecordObject(this, "Change Condition Objective");
            condition.SetObjectiveIndex(newIndex, indexAnd, indexOr);
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
            Undo.RecordObject(this, "Toggle Enter Action");
            hasOnEnterAction = toggleValue;
            if (!hasOnEnterAction)
            {
                onEnterAction = OnDialogueAction.None;
                onEnterActionParameter.Clear();
            }
            EditorUtility.SetDirty(this);
        }

        // Value controlled by Dialogue Editor GUI
        public void SetHasOnExitAction(bool toggleValue)
        {
            Undo.RecordObject(this, "Toggle Exit Action");
            hasOnExitAction = toggleValue;
            if(!hasOnExitAction)
            {
                onExitAction = OnDialogueAction.None;
                onExitActionParameter.Clear();
            }
            EditorUtility.SetDirty(this);
        }

        // Value controlled by Dialogue Editor GUI
        public void SetHasConditionSelect(bool toggleValue)
        {
            Undo.RecordObject(this, "Toggle Conditional Dialogue");
            hasConditionSelect = toggleValue;
            if(hasConditionSelect && condition.GetConditionSize() == 0)
            {
                condition.AddRootCondition();
            }
            else if(!hasConditionSelect && condition.GetConditionSize() > 0)
            {
                condition.RemoveAllConditions();
            }
            EditorUtility.SetDirty(this);
        }

        public int GetConditionSize()
        {
            if(condition == null) return 0;
            return condition.GetConditionSize();
        }

        public bool GetFoldout(int index)
        {
            return condition.GetFoldout(index);
        }

        public void SetFold(bool newFold, int index)
        {
            condition.SetFold(newFold, index);
        }

        public int GetEnterActionIndex()
        {
            return enterActionIndex;
        }

        public void SetEnterActionIndex(int newIndex)
        {
            Undo.RecordObject(this, "Change Action Objective");
            enterActionIndex = newIndex;
            EditorUtility.SetDirty(this);
        }

        public int GetExitActionIndex()
        {
            return exitActionIndex;
        }

        public void SetExitActionIndex(int newIndex)
        {
            Undo.RecordObject(this, "Change Action Objective");
            exitActionIndex = newIndex;
            EditorUtility.SetDirty(this);
        }

        public void AddNewRootCondition()
        {
            condition.AddRootCondition();
        }

        public void AddNewCondition(int index)
        {
            condition.AddNewCondition(index);
        }

        public void RemoveCondition(int indexAnd, int indexOr)
        {
            condition.RemoveCondition(indexAnd, indexOr);
        }
#endif
    }
}
