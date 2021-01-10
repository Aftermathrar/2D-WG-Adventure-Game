using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [CreateAssetMenu(menuName = ("Inventory/Consumable"))]
    public class ConsumableItem : InventoryItem
    {
        [SerializeField] float size = 100;
        [SerializeField] float calories = 100;
        float calorieDensity;

        private void Start() 
        {
            calorieDensity = calories / size;
        }

        public float GetSize()
        {
            return size;
        }

        public float GetCalories()
        {
            return calories;
        }

#if UNITY_EDITOR
        bool drawConsumableItem = true;
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            EditorGUILayout.EndFoldoutHeaderGroup();

            drawConsumableItem = EditorGUILayout.BeginFoldoutHeaderGroup(drawConsumableItem, "ConsumableItem Data", foldoutStyle);
            if(!drawConsumableItem) return;

            EditorGUILayout.BeginVertical(contentStyle);
            SetSize(EditorGUILayout.FloatField("Size", size));
            SetCalories(EditorGUILayout.FloatField("Calories", calories));
            EditorGUILayout.EndVertical();
        }

        private void SetSize(float newSize)
        {
            if(FloatEquals(size, newSize)) return;
            SetUndo("Change Item Size");
            size = newSize;
            Dirty();
        }

        private void SetCalories(float newCalories)
        {
            if(FloatEquals(calories, newCalories)) return;
            SetUndo("Change Item Calories");
            calories = newCalories;
            Dirty();
        }
#endif
    }
}
