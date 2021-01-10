using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ButtonGame.Inventories.Editor
{
    public class InventoryItemEditor : EditorWindow
    {
        InventoryItem selected = null;
        [MenuItem("Window/InventoryItem Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(InventoryItemEditor), false, "Inventory Item");
        }

        public static void ShowEditorWindow(InventoryItem candidate)
        {
            InventoryItemEditor window = GetWindow(typeof(InventoryItemEditor), false, "Inventory Item") as InventoryItemEditor;
            if(candidate)
            {
                window.OnSelectionChange();
            }
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            InventoryItem candidate = EditorUtility.InstanceIDToObject(instanceID) as InventoryItem;
            if(candidate != null)
            {
                ShowEditorWindow(candidate);
                return true;
            }
            return false;
        }

        private void OnSelectionChange() 
        {
            var candidate = EditorUtility.InstanceIDToObject(Selection.activeInstanceID) as InventoryItem;
            if(candidate == null) return;
            selected = candidate;
            Repaint();
        }

        private void OnGUI() 
        {
            if(!selected)
            {
                EditorGUILayout.HelpBox("No InventoryItem Selected", MessageType.Error);
                return;
            }
            EditorGUILayout.HelpBox($"{selected.name}/{selected.GetDisplayName()}", MessageType.Info);
            selected.DrawCustomInspector();
        }
    }
}
