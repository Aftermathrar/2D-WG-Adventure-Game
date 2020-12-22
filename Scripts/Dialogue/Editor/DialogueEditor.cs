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
            EditorGUILayout.LabelField("Condition");
            GUILayout.EndHorizontal();

            if (node.GetHasOnEnterAction())
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("OnEnter:", GUILayout.Width(49));
                node.SetOnEnterAction(EditorGUILayout.TextField(node.GetOnEnterAction()));
                GUILayout.EndHorizontal();
            }

            if (node.GetHasOnExitAction())
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("OnExit:", GUILayout.Width(49));
                node.SetOnExitAction(EditorGUILayout.TextField(node.GetOnExitAction()));
                GUILayout.EndHorizontal();
            }

            if (node.GetHasConditionSelect())
            {
                LayoutConditionSelectionUI(node);
            }

            GUILayout.EndArea();
        }

        private static void LayoutConditionSelectionUI(DialogueNode node)
        {
            ConditionPredicate predicate = (ConditionPredicate)EditorGUILayout.EnumPopup(node.GetCondition());

            node.SetCondition(predicate);
            List<string> parameterList = new List<string>();
            int removeCount = 1;

            if (predicate == ConditionPredicate.None)
            {
            }
            else if (predicate == ConditionPredicate.HasQuest)
            {
                EditorQuestSelect(node, parameterList);
            }
            else if (predicate == ConditionPredicate.CompleteQuest)
            {
                EditorQuestSelect(node, parameterList);
            }
            else if (predicate == ConditionPredicate.CompleteObjective)
            {
                string[] objectiveList = node.GetParameters().ToArray();
                if(objectiveList.Length > 0)
                {
                    Quest questSelect = GenerateQuestSelect(objectiveList[0]);
                    if(questSelect != null)
                    {
                        parameterList.Add(questSelect.name);
                    }
                    else
                    {
                        parameterList.Add("");
                    }
                    for (int i = 1; i < objectiveList.Length; i++)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Objective:", GUILayout.Width(58));
                        if(questSelect != null)
                        {

                            string[] questObjectives = questSelect.GetObjectives().ToArray();
                            node.SetObjectiveIndex(EditorGUILayout.Popup(node.GetObjectiveIndex(), questObjectives));
                            parameterList.Add(questObjectives[node.GetObjectiveIndex()]);
                        }
                        else
                        {
                            EditorGUILayout.Popup(0, new string[] {""});
                            parameterList.Add("");
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else if (predicate == ConditionPredicate.HasItem)
            {
                removeCount = 2;
                string[] itemList = node.GetParameters().ToArray();
                if(itemList.Length > 0)
                {
                    for (int i = 0; i < itemList.Length; i++)
                    {
                        InventoryItem item = InventoryItem.GetFromID(itemList[0]);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Item:", GUILayout.Width(40));
                        item = (InventoryItem)EditorGUILayout.ObjectField(item, typeof(InventoryItem), false);
                        GUILayout.EndHorizontal();
                        i++;
                        if (item != null)
                        {
                            parameterList.Add(item.GetItemID());
                            if(i < itemList.Length)
                            {
                                int itemCount = 1;
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Item Count:", GUILayout.Width(70));
                                string countString = EditorGUILayout.TextField(itemList[i]);
                                GUILayout.EndHorizontal();
                                if(!int.TryParse(countString, out itemCount))
                                {
                                    Debug.Log("This parameter only takes a number!");
                                }
                                parameterList.Add(itemCount.ToString());
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
            if (GUILayout.Button("Add Parameter"))
            {
                parameterList.Add("");
            }
            if (GUILayout.Button("Remove"))
            {
                for (int i = 0; i < removeCount; i++)
                {
                    parameterList.RemoveAt(parameterList.Count() - 1);
                }
            }
            node.SetConditionNegate(EditorGUILayout.Toggle(node.GetConditionNegate()));
            EditorGUILayout.LabelField("Negate");
            GUILayout.EndHorizontal();

            node.SetConditionParameters(parameterList);
        }

        private static void EditorQuestSelect(DialogueNode node, List<string> parameterList)
        {
            List<Quest> questParameters = new List<Quest>();
            string[] nodeParameters = node.GetParameters().ToArray();

            foreach (string parameter in node.GetParameters())
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

        private object CaptureParameter(ScriptableObject newParameter)
        {
            ConditionParameters state = new ConditionParameters();
            state.parameter = newParameter;
            return state;
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
                controlPointOffset.y *= 0.05f;
                controlPointOffset.x *= 0.6f;

                Handles.DrawBezier(startPosition, endPosition, 
                    startPosition + controlPointOffset, 
                    endPosition - controlPointOffset,
                    Color.white, null, 4f);
            }
        }

        private void SetNodeStyleAndSize(DialogueNode node, out GUIStyle style, out GUIStyle wrapStyle)
        {
            //Style select and height calculation
            int heightPadding = 80;
            style = nodeStyle;
            if (node.IsPlayerSpeaking())
            {
                style = playerNodeStyle;
            }
            else
            {
                heightPadding += 20;
            }

            if (node.GetHasOnEnterAction()) heightPadding += 20;
            if (node.GetHasOnExitAction()) heightPadding += 20;
            if (node.GetHasConditionSelect()) 
            {
                heightPadding += 40;
                heightPadding += 20 * node.GetParameters().Count();
            }

            wrapStyle = new GUIStyle(EditorStyles.textField);
            wrapStyle.wordWrap = true;
            node.SetNodeHeight(heightPadding + wrapStyle.CalcHeight(new GUIContent(node.GetText()),
                node.GetRect().width - style.padding.left - style.padding.right));
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

        [System.Serializable]
        class ConditionParameters
        {
            public ScriptableObject parameter;
        }
    }
}
