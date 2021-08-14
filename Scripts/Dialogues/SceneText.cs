using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Dialogues
{
    [CreateAssetMenu(fileName = "New Scene Text", menuName = "Dialogue/Scene Text", order = 1)]
    public class SceneText : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<SceneTextNode> nodes = new List<SceneTextNode>();
        [SerializeField]
        Vector2 newNodeOffset = new Vector2(300, 0);

        Dictionary<string, SceneTextNode> nodeLookup = new Dictionary<string, SceneTextNode>();

        private void OnValidate()
        {
            nodeLookup.Clear();

            foreach (SceneTextNode node in GetAllNodes())
            {
                nodeLookup[node.name] = node;
            }
        }

        public IEnumerable<SceneTextNode> GetAllNodes()
        {
            return nodes;
        }

        public string GetText(GameObject conversantGO)
        {
            string sceneText = "";
            foreach (var node in nodes)
            {
                sceneText += node.GetText(conversantGO) + " ";
            }
            return sceneText;
        }

        public IEnumerable<SceneTextNode> GetAllChildren(SceneTextNode parentNode)
        {
            foreach (string childID in parentNode.GetChildren())
            {
                if (nodeLookup.ContainsKey(childID))
                {
                    yield return nodeLookup[childID];
                }
            }
        }

#if UNITY_EDITOR
        public void CreateNode(SceneTextNode parent)
        {
            SceneTextNode newNode = MakeNode(parent);
            Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");
            Undo.RecordObject(this, "Added Dialogue Node");
            AddNode(newNode);
        }

        public void DeleteNode(SceneTextNode nodeToDelete)
        {
            Undo.RecordObject(this, "Delete Dialogue Node");
            nodes.Remove(nodeToDelete);
            OnValidate();
            CleanDanglingChildren(nodeToDelete);
            Undo.DestroyObjectImmediate(nodeToDelete);
        }

        private void CleanDanglingChildren(SceneTextNode nodeToDelete)
        {
            foreach (SceneTextNode node in GetAllNodes())
            {
                node.RemoveChild(nodeToDelete.name);
            }
        }

        private SceneTextNode MakeNode(SceneTextNode parent)
        {
            SceneTextNode newNode = CreateInstance<SceneTextNode>();
            newNode.name = Guid.NewGuid().ToString();
            if (parent != null)
            {
                parent.AddChild(newNode.name);
                newNode.SetVariableText(!parent.IsVariableText());
                newNode.SetPosition(parent.GetRect().position + newNodeOffset);
            }

            return newNode;
        }

        private void AddNode(SceneTextNode newNode)
        {
            nodes.Add(newNode);
            OnValidate();
        }
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count == 0)
            {
                SceneTextNode newNode = MakeNode(null);
                AddNode(newNode);
            }

            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (SceneTextNode node in GetAllNodes())
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }

                }
            }
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
