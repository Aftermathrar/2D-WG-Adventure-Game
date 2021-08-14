using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using ButtonGame.Core;

namespace ButtonGame.Dialogues.Editor
{
    public class DialogueTextEditor : EditorWindow
    {
        SceneText selectedSceneText = null;
        [NonSerialized]
        GUIStyle nodeStyle;
        [NonSerialized]
        GUIStyle variableNodeStyle;
        [NonSerialized]
        SceneTextNode creatingNode = null;
        [NonSerialized]
        SceneTextNode nodeToDelete = null;
        [NonSerialized]
        SceneTextNode linkingParentNode = null;

        Vector2 scrollPosition;
        [NonSerialized]
        SceneTextNode draggingNode = null;
        [NonSerialized]
        Vector2 draggingOffset;
        [NonSerialized]
        bool draggingCanvas = false;
        [NonSerialized]
        Vector2 draggingCanvasOffset;

        const float canvasSize = 4000;
        const float backgroundSize = 50;

        [MenuItem("Window/Dialogue/Dialogue Text Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueTextEditor), false, "Scene Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            SceneText sceneText = EditorUtility.InstanceIDToObject(instanceID) as SceneText;
            if (sceneText != null)
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

            variableNodeStyle = new GUIStyle();
            variableNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            variableNodeStyle.padding = new RectOffset(10, 10, 8, 10);
            variableNodeStyle.border = new RectOffset(12, 12, 12, 12);

            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            SceneText newSceneText = Selection.activeObject as SceneText;
            if (newSceneText != null)
            {
                selectedSceneText = newSceneText;
                Repaint();
            }
        }

        private void OnGUI()
        {
            if (selectedSceneText == null)
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

                foreach (SceneTextNode node in selectedSceneText.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach (SceneTextNode node in selectedSceneText.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if (nodeToDelete != null)
                {
                    selectedSceneText.DeleteNode(nodeToDelete);
                    nodeToDelete = null;
                }
                if (creatingNode != null)
                {
                    selectedSceneText.CreateNode(creatingNode);
                    creatingNode = null;
                }

            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && draggingNode == null)
            {
                draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if (draggingNode != null)
                {
                    draggingOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = draggingNode;
                }
                else
                {
                    draggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                    Selection.activeObject = selectedSceneText;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && draggingNode != null)
            {
                draggingNode.SetPosition(Event.current.mousePosition + draggingOffset);
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseDrag && draggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && draggingNode != null)
            {
                draggingNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && draggingCanvas)
            {
                draggingCanvas = false;
            }
        }

        private void DrawNode(SceneTextNode node)
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
            node.SetVariableText(EditorGUILayout.Toggle(node.IsVariableText()));
            EditorGUILayout.LabelField("Is Variable Description?");
            GUILayout.EndHorizontal();

            if(node.IsVariableText())
            {
                node.SetDescriptionPredicate((TextDescriptionPredicate)EditorGUILayout.EnumPopup(node.GetDescriptionPredicate()));
                int descCount = node.GetDescriptionCount();
                for (int i = 0; i < descCount; i++)
                {
                    EditorGUILayout.FloatField(node.GetDescriptionLimit(i));
                    EditorGUILayout.TextField(node.GetDescriptionText(i));
                }
            }
            else
            {
                // Dialogue Text
                node.SetText(EditorGUILayout.TextArea(node.GetEditorText(), wrapStyle));
            }


            GUILayout.EndArea();
        }

        private void DrawConnections(SceneTextNode node)
        {
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);
            foreach (SceneTextNode childNode in selectedSceneText.GetAllChildren(node))
            {
                Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
                Vector3 controlPointOffset = endPosition - startPosition;
                if (controlPointOffset.x > 0)
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

        private void DrawLinkButtons(SceneTextNode node)
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

        private void SetNodeStyleAndSize(SceneTextNode node, out GUIStyle style, out GUIStyle wrapStyle)
        {
            //Style select and height calculation
            int heightPadding = 62;
            style = nodeStyle;
            if (node.IsVariableText())
            {
                style = variableNodeStyle;
                heightPadding += 40 * node.GetDescriptionCount();
            }
            else
            {
                heightPadding += 20;
            }

            // if (node.GetHasOnEnterAction())
            // {
            //     heightPadding += 20;
            //     heightPadding += 20 * node.GetOnEnterActionParameters().Count();
            // }
            // if (node.GetHasOnExitAction())
            // {
            //     heightPadding += 20;
            //     heightPadding += 20 * node.GetOnExitActionParameters().Count();
            // }
            // if (node.GetHasConditionSelect())
            // {
            //     int conditionSize = node.GetConditionSize();
            //     for (int i = 0; i < conditionSize; i++)
            //     {
            //         heightPadding += 20;
            //         if (node.GetFoldout(i))
            //         {
            //             ConditionPredicate[] predicates = node.GetConditionPredicates(i).ToArray();
            //             for (int j = 0; j < predicates.Length; j++)
            //             {
            //                 heightPadding += 42;
            //                 heightPadding += 20 * node.GetParameters(i, j).Count();
            //             }
            //         }
            //         else
            //         {
            //             heightPadding -= 1;
            //         }
            //     }
            // }

            wrapStyle = new GUIStyle(EditorStyles.textArea);
            wrapStyle.wordWrap = true;
            float nodeHeightCalc = wrapStyle.CalcHeight(new GUIContent(node.GetEditorText()),
                node.GetRect().width - style.padding.left - style.padding.right);

            node.SetNodeHeight(heightPadding + nodeHeightCalc);
        }

        private SceneTextNode GetNodeAtPoint(Vector2 point)
        {
            SceneTextNode foundNode = null;
            foreach (SceneTextNode node in selectedSceneText.GetAllNodes())
            {
                if (node.GetRect().Contains(point))
                {
                    foundNode = node;
                }
            }
            return foundNode;
        }
    }
}