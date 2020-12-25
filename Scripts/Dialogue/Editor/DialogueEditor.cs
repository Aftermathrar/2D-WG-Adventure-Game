using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using ButtonGame.Quests;
using ButtonGame.Core;
using System.Linq;
using ButtonGame.Inventories;

namespace ButtonGame.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow
    {
        Dialogue selectedDialogue = null;
        [NonSerialized]
        GUIStyle nodeStyle;
        [NonSerialized]
        GUIStyle playerNodeStyle;
        [NonSerialized]
        DialogueNode draggingNode = null;
        [NonSerialized]
        Vector2 draggingOffset;
        [NonSerialized]
        DialogueNode creatingNode = null;
        [NonSerialized]
        DialogueNode nodeToDelete = null;
        [NonSerialized]
        DialogueNode linkingParentNode = null;
        Vector2 scrollPosition;
        [NonSerialized]
        bool draggingCanvas = false;
        [NonSerialized]
        Vector2 draggingCanvasOffset;

        const float canvasSize = 4000;
        const float backgroundSize = 50;

        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if(dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }
            return false;
        }

        private void OnEnable() 
        {
            Selection.selectionChanged += OnSelectionChanged;
            
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.padding = new RectOffset(10, 10, 8, 10);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            playerNodeStyle = new GUIStyle();
            playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            playerNodeStyle.padding = new RectOffset(10, 10, 8, 10);
            playerNodeStyle.border = new RectOffset(12, 12, 12, 12);

            OnSelectionChanged();
        }

        private void OnDisable() 
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if(newDialogue != null)
            {
                selectedDialogue = newDialogue;
                Repaint();
            }
        }

        private void OnGUI() 
        {
            if(selectedDialogue == null)
            {
                EditorGUILayout.LabelField("No dialogue selected.");
            }
            else
            {
                ProcessEvents();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, true);
                
                Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
                Texture2D backgroundTex = Resources.Load("background") as Texture2D;
                float tileCount = canvasSize / backgroundSize;
                Rect texCoords = new Rect(0, 0, tileCount, tileCount);
                GUI.DrawTextureWithTexCoords(canvas, backgroundTex, texCoords);

                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if(nodeToDelete != null)
                {
                    selectedDialogue.DeleteNode(nodeToDelete);
                    nodeToDelete = null;
                }
                if(creatingNode != null)
                {
                    selectedDialogue.CreateNode(creatingNode);
                    creatingNode = null;
                }
            }
        }

        private void ProcessEvents()
        {
            if(Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if(draggingNode != null)
                {
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = draggingNode;
                }
                else
                {
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    Selection.activeObject = selectedDialogue;
                }
            }
            else if(Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                draggingNode.SetPosition(Event.current.mousePosition + draggingOffset);
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if(Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }
        }

        private void DrawNode(DialogueNode node)
        {
            GUIStyle style, wrapStyle;
            SetNodeStyleAndSize(node, out style, out wrapStyle);

            GUILayout.BeginArea(node.GetRect(), style);

            // Create, Link, Destroy buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                creatingNode = node;
            }
            DrawLinkButtons(node);
            if (GUILayout.Button("x"))
            {
                nodeToDelete = node;
            }
            GUILayout.EndHorizontal();

            // Toggle Is Player Speaking
            GUILayout.BeginHorizontal();
            node.SetPlayerSpeaking(EditorGUILayout.Toggle(node.IsPlayerSpeaking()));
            EditorGUILayout.LabelField("Is Player speaking?");
            GUILayout.EndHorizontal();
            if (!node.IsPlayerSpeaking())
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Speaker:", GUILayout.Width(51));
                node.SetSpeaker(EditorGUILayout.TextField(node.GetSpeaker()));
                GUILayout.EndHorizontal();
            }

            // Dialogue Text
            node.SetText(EditorGUILayout.TextArea(node.GetText(), wrapStyle));

            // Toggles For Actions and Conditions
            GUILayout.BeginHorizontal();
            node.SetHasOnEnterAction(EditorGUILayout.Toggle(node.GetHasOnEnterAction()));
            EditorGUILayout.LabelField("Enter", GUILayout.Width(34));
            node.SetHasOnExitAction(EditorGUILayout.Toggle(node.GetHasOnExitAction()));
            EditorGUILayout.LabelField("Exit", GUILayout.Width(30));
            node.SetHasConditionSelect(EditorGUILayout.Toggle(node.GetHasConditionSelect()));
            if(node.GetHasConditionSelect())
            {
                EditorGUILayout.LabelField("Condition", GUILayout.Width(68));
                if (GUILayout.Button("New"))
                {
                    node.AddNewRootCondition();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Condition");
            }
            GUILayout.EndHorizontal();

            if (node.GetHasOnEnterAction())
            {
                OnDialogueAction enterAction;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("OnEnter:", GUILayout.Width(49));
                enterAction = (OnDialogueAction)EditorGUILayout.EnumPopup(node.GetOnEnterAction());
                GUILayout.EndHorizontal();
                node.SetOnEnterAction(enterAction);

                string[] actionParams = node.GetOnEnterActionParameters().ToArray();
                List<string> onEnterActions = new List<string>();
                int actionIndex = node.GetEnterActionIndex();

                BuildDialogueActionsSelect(node, enterAction, actionParams, onEnterActions, actionIndex);

                node.SetOnEnterActionParameters(onEnterActions);
                node.SetEnterActionIndex(actionIndex);
            }

            if (node.GetHasOnExitAction())
            {
                OnDialogueAction exitAction;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("OnExit:", GUILayout.Width(49));
                exitAction = (OnDialogueAction)EditorGUILayout.EnumPopup(node.GetOnExitAction());
                GUILayout.EndHorizontal();
                node.SetOnExitAction(exitAction);

                string[] actionParams = node.GetOnExitActionParameters().ToArray();
                List<string> onExitActions = new List<string>();
                int actionIndex = node.GetExitActionIndex();

                BuildDialogueActionsSelect(node, exitAction, actionParams, onExitActions, actionIndex);

                node.SetOnExitActionParameters(onExitActions);
                node.SetExitActionIndex(actionIndex);
            }

            if (node.GetHasConditionSelect())
            {
                LayoutConditionSelectionUI(node);
            }

            GUILayout.EndArea();
        }

        private static void BuildDialogueActionsSelect(
            DialogueNode node, OnDialogueAction dialogueAction, 
            string[] actionParams, List<string> dialogueActions, int actionIndex)
        {
            switch (dialogueAction)
            {
                case OnDialogueAction.None:
                    break;
                case OnDialogueAction.CompleteObjective:
                    Quest selectedQuest;
                    for (int i = 0; i < actionParams.Length; i++)
                    {
                        selectedQuest = GenerateQuestSelect(actionParams[i]);
                        GUILayout.BeginHorizontal();
                        if (selectedQuest != null)
                        {
                            dialogueActions.Add(selectedQuest.name);
                            string[] questObjectives = selectedQuest.GetObjectives().ToArray();
                            if (actionIndex >= questObjectives.Length)
                            {
                                actionIndex = 0;
                            }

                            EditorGUILayout.LabelField("Objective:", GUILayout.Width(58));
                            actionIndex = EditorGUILayout.Popup(actionIndex, questObjectives);
                            dialogueActions.Add(questObjectives[actionIndex]);
                        }
                        else
                        {
                            dialogueActions.Add("");
                            EditorGUILayout.LabelField("Objective:", GUILayout.Width(58));
                            dialogueActions.Add(EditorGUILayout.TextField(actionParams[1]));
                        }
                        GUILayout.EndHorizontal();
                        i++;
                    }
                    break;
                case OnDialogueAction.GiveQuest:
                    selectedQuest = GenerateQuestSelect(actionParams[0]);
                    if (selectedQuest != null)
                    {
                        dialogueActions.Add(selectedQuest.name);
                    }
                    else
                    {
                        dialogueActions.Add(actionParams[0]);
                    }
                    break;
                case OnDialogueAction.GiveItem:
                    InventoryItem item = InventoryItem.GetFromID(actionParams[0]);
                    item = GenerateItemSelect(item);
                    if (item != null)
                    {
                        dialogueActions.Add(item.GetItemID());

                    }
                    else
                    {
                        dialogueActions.Add("");
                    }
                    dialogueActions.Add(GenerateItemCountField(actionParams[1]).ToString());
                    break;
            }
        }

        private static void LayoutConditionSelectionUI(DialogueNode node)
        {
            int conditionSize = node.GetConditionSize();
            for(int k = 0; k < conditionSize; k++)
            {
                bool foldout = node.GetFoldout(k);
                node.SetFold(EditorGUILayout.BeginFoldoutHeaderGroup(foldout, $"Condition Group {k}"), k);
                if(foldout)
                {
                    ConditionPredicate[] predicates = node.GetConditionPredicates(k).ToArray();
                    for (int i = 0; i < predicates.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Condition:", GUILayout.Width(60));
                        ConditionPredicate newPredicate = (ConditionPredicate)EditorGUILayout.EnumPopup(predicates[i]);
                        node.SetConditionPredicate(newPredicate, k, i);
                        if(GUILayout.Button("-"))
                        {
                            node.RemoveCondition(k, i);
                            conditionSize = node.GetConditionSize();
                        }
                        GUILayout.EndHorizontal();

                        List<string> parameterList = new List<string>();
                        int removeCount = 1;

                        if (newPredicate == ConditionPredicate.None)
                        {
                        }
                        else if (newPredicate == ConditionPredicate.HasQuest)
                        {
                            EditorQuestSelect(node, parameterList, k, i);
                        }
                        else if (newPredicate == ConditionPredicate.CompleteQuest)
                        {
                            EditorQuestSelect(node, parameterList, k, i);
                        }
                        else if (newPredicate == ConditionPredicate.CompleteObjective)
                        {
                            string[] objectiveList = node.GetParameters(k, i).ToArray();
                            if (objectiveList.Length > 0)
                            {
                                Quest questSelect = GenerateQuestSelect(objectiveList[0]);
                                if (questSelect != null)
                                {
                                    parameterList.Add(questSelect.name);
                                }
                                else
                                {
                                    parameterList.Add("");
                                }
                                for (int j = 1; j < objectiveList.Length; j++)
                                {
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Objective:", GUILayout.Width(58));
                                    if (questSelect != null)
                                    {
                                        string[] questObjectives = questSelect.GetObjectives().ToArray();
                                        int objectiveIndex = EditorGUILayout.Popup(node.GetObjectiveIndex(k, i), questObjectives);
                                        node.SetObjectiveIndex(objectiveIndex, k, i);
                                        parameterList.Add(questObjectives[objectiveIndex]);
                                    }
                                    else
                                    {
                                        EditorGUILayout.Popup(0, new string[] { "" });
                                        parameterList.Add("");
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                        else if (newPredicate == ConditionPredicate.HasItem)
                        {
                            removeCount = 2;
                            string[] itemList = node.GetParameters(k, i).ToArray();
                            if (itemList.Length > 0)
                            {
                                for (int j = 0; j < itemList.Length; j++)
                                {
                                    InventoryItem item = InventoryItem.GetFromID(itemList[j]);
                                    item = GenerateItemSelect(item);
                                    j++;
                                    if (item != null)
                                    {
                                        parameterList.Add(item.GetItemID());
                                        if (j < itemList.Length)
                                        {
                                            parameterList.Add(GenerateItemCountField(itemList[j]).ToString());
                                        }
                                        else
                                        {
                                            parameterList.Add("");
                                        }
                                    }
                                    else
                                    {
                                        parameterList.Add("");
                                    }
                                }
                            }

                        }
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add"))
                        {
                            parameterList.Add("");
                        }
                        if (GUILayout.Button("Remove"))
                        {
                            for (int j = 0; j < removeCount; j++)
                            {
                                parameterList.RemoveAt(parameterList.Count() - 1);
                            }
                        }
                        node.SetConditionNegate(EditorGUILayout.Toggle(node.GetConditionNegate(k, i)), k, i);
                        EditorGUILayout.LabelField("Negate", GUILayout.Width(74));
                        if(GUILayout.Button("New"))
                        {
                            node.AddNewCondition(k);
                        }
                        GUILayout.EndHorizontal();

                        node.SetConditionParameters(parameterList, k, i);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private static int GenerateItemCountField(string itemString)
        {
            int itemCount;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Count:", GUILayout.Width(70));
            string countString = EditorGUILayout.TextField(itemString);
            GUILayout.EndHorizontal();
            if (!int.TryParse(countString, out itemCount))
            {
                Debug.Log("This parameter only takes a number!");
            }
            itemCount = Math.Max(itemCount, 1);
            return itemCount;
        }

        private static InventoryItem GenerateItemSelect(InventoryItem item)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item:", GUILayout.Width(40));
            item = (InventoryItem)EditorGUILayout.ObjectField(item, typeof(InventoryItem), false);
            GUILayout.EndHorizontal();
            return item;
        }

        private static void EditorQuestSelect(
            DialogueNode node, List<string> parameterList, int indexAnd, int indexOr)
        {
            List<Quest> questParameters = new List<Quest>();
            string[] nodeParameters = node.GetParameters(indexAnd, indexOr).ToArray();

            foreach (string parameter in node.GetParameters(indexAnd, indexOr))
            {
                Quest questSelect = GenerateQuestSelect(parameter);
                questParameters.Add(questSelect);
            }

            foreach (Quest questParameter in questParameters)
            {
                if (questParameter == null)
                {
                    parameterList.Add("");
                    continue;
                }
                parameterList.Add(questParameter.name);
            }
        }

        private static Quest GenerateQuestSelect(string parameter)
        {
            Quest questSelect = Quest.GetByName(parameter);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quest:", GUILayout.Width(40));
            questSelect = (Quest)EditorGUILayout.ObjectField(questSelect, typeof(Quest), false);
            GUILayout.EndHorizontal();
            return questSelect;
        }

        private void DrawLinkButtons(DialogueNode node)
        {
            if (linkingParentNode == null)
            {
                if (GUILayout.Button("Link"))
                {
                    linkingParentNode = node;
                }
            }
            else if (node == linkingParentNode)
            {
                if (GUILayout.Button("Cancel"))
                {
                    linkingParentNode = null;
                }
            }
            else if (linkingParentNode.GetChildren().Contains(node.name))
            {
                if (GUILayout.Button("Unlink"))
                {
                    linkingParentNode.RemoveChild(node.name);
                    linkingParentNode = null;
                }
            }
            else
            {
                if (GUILayout.Button("Child"))
                {
                    linkingParentNode.AddChild(node.name);
                    linkingParentNode = null;
                }
            }
        }

        private void DrawConnections(DialogueNode node)
        {
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);
            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
                Vector3 controlPointOffset = endPosition - startPosition;
                if(controlPointOffset.x > 0)
                {
                    controlPointOffset.y *= 0.05f;
                    controlPointOffset.x *= 0.6f;
                }
                else
                {
                    controlPointOffset.y *= 0.8f;
                    controlPointOffset.x = 50f;
                }

                Handles.DrawBezier(startPosition, endPosition, 
                    startPosition + controlPointOffset, 
                    endPosition - controlPointOffset,
                    Color.white, null, 4f);
            }
        }

        private void SetNodeStyleAndSize(DialogueNode node, out GUIStyle style, out GUIStyle wrapStyle)
        {
            //Style select and height calculation
            int heightPadding = 82;
            style = nodeStyle;
            if (node.IsPlayerSpeaking())
            {
                style = playerNodeStyle;
            }
            else
            {
                heightPadding += 20;
            }

            if (node.GetHasOnEnterAction()) 
            {
                heightPadding += 20;
                heightPadding += 20 * node.GetOnEnterActionParameters().Count();
            }
            if (node.GetHasOnExitAction()) 
            {
                heightPadding += 20;
                heightPadding += 20 * node.GetOnExitActionParameters().Count();
            }
            if (node.GetHasConditionSelect())
            {
                int conditionSize = node.GetConditionSize();
                for (int i = 0; i < conditionSize; i++)
                {
                    heightPadding += 20;
                    if(node.GetFoldout(i))
                    {
                        ConditionPredicate[] predicates = node.GetConditionPredicates(i).ToArray();
                        for (int j = 0; j < predicates.Length; j++)
                        {
                            heightPadding += 42;
                            heightPadding += 20 * node.GetParameters(i, j).Count();
                        }
                    }
                    else
                    {
                        heightPadding -= 1;
                    }
                }
            }

            wrapStyle = new GUIStyle(EditorStyles.textField);
            wrapStyle.wordWrap = true;
            float nodeHeightCalc = wrapStyle.CalcHeight(new GUIContent(node.GetText()), 
                node.GetRect().width - style.padding.left - style.padding.right);
                
            node.SetNodeHeight(heightPadding + nodeHeightCalc);
        }

        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null;
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if(node.GetRect().Contains(point))
                {
                    foundNode = node;
                }
            }
            return foundNode;
        }

        private object CaptureParameter(ScriptableObject newParameter)
        {
            ConditionParameters state = new ConditionParameters();
            state.parameter = newParameter;
            return state;
        }

        [System.Serializable]
        class ConditionParameters
        {
            public ScriptableObject parameter;
        }
    }
}
