using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// An inventory item that can be equipped to the player. Weapons could be a
    /// subclass of this.
    /// </summary>
    [CreateAssetMenu(menuName = ("Inventory/Equipment"))]
    public class EquipableItem : InventoryItem
    {
        // CONFIG DATA
        [Tooltip("Where are we allowed to put this item.")]
        [SerializeField] protected EquipLocation allowedEquipLocation = EquipLocation.Weapon;
        [SerializeField] List<EquipmentStats> equipmentStats = new List<EquipmentStats>();

        [SerializeField] protected virtual CharacterClass equipClass 
        {
            get { return CharacterClass.Player; }
        }

        // PUBLIC

        public EquipLocation GetAllowedEquipLocation()
        {
            return allowedEquipLocation;
        }

        public bool IsPlayerEquipment()
        {
            return equipClass == CharacterClass.Player;
        }

        public Dictionary<Stat, float[]> GetStatValues()
        {
            var statDict = new Dictionary<Stat, float[]>();
            foreach (var equipStat in equipmentStats)
            {
                if(!statDict.ContainsKey(equipStat.stat))
                {
                    float[] val = new float[2] {0, 0};
                    int i = equipStat.isAdditive ? 1 : 0;
                    val[i] = equipStat.value;
                    statDict[equipStat.stat] = val;
                }
                else
                {
                    int i = equipStat.isAdditive ? 1 : 0;
                    statDict[equipStat.stat][i] = equipStat.value;
                }
            }
            return statDict;
        }

#if UNITY_EDITOR
        bool drawEquipmentData = true;
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            EditorGUILayout.EndFoldoutHeaderGroup();

            drawEquipmentData = EditorGUILayout.BeginFoldoutHeaderGroup(drawEquipmentData, "Equipment Data", foldoutStyle);
            if(!drawEquipmentData) return;

            EditorGUILayout.BeginVertical(contentStyle);
            SetAllowedEquipLocation((EquipLocation)EditorGUILayout.EnumPopup("Equipable Location", GetAllowedEquipLocation()));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Equipment Stat Fields", MessageType.None);
            if (GUILayout.Button("Add New Stat Line", GUILayout.Width(200)))
            {
                AddEquipmentStat();
            }
            EditorGUILayout.EndHorizontal();
            bool removeEquipmentStat = false;
            int removeStatIndex = 0;
            for (int i = 0; i < equipmentStats.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SetEquipmentStat((Stat)EditorGUILayout.EnumPopup("Stat", equipmentStats[i].stat), i);
                if (GUILayout.Button("Remove Stat Line", GUILayout.Width(150)))
                {
                    removeEquipmentStat = true;
                    removeStatIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                SetEquipmentStatValue(EditorGUILayout.FloatField("Stat Value", equipmentStats[i].value), i);
                SetEquipmentStatAdditive(EditorGUILayout.Toggle("Is Additive", equipmentStats[i].isAdditive), i);
                EditorGUILayout.EndHorizontal();
            }
            if (removeEquipmentStat)
            {
                RemoveEquipmentStat(removeStatIndex);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void SetAllowedEquipLocation(EquipLocation newLocation)
        {
            if (allowedEquipLocation == newLocation) return;
            SetUndo("Change Equip Location");
            allowedEquipLocation = newLocation;
            Dirty();
        }

        private void AddEquipmentStat()
        {
            SetUndo("Add Equipment Stat");
            EquipmentStats newStat = new EquipmentStats();
            equipmentStats.Add(newStat);
            Dirty();
        }

        private void RemoveEquipmentStat(int index)
        {
            SetUndo("Remove Equipment Stat");
            equipmentStats.RemoveAt(index);
            Dirty();
        }

        private void SetEquipmentStat(Stat newStat, int index)
        {
            if(equipmentStats[index].stat == newStat) return;
            SetUndo("Change Equipment Stat");
            equipmentStats[index].stat = newStat;
            Dirty();
        }

        public void SetEquipmentStatValue(float newValue, int index)
        {
            if(FloatEquals(equipmentStats[index].value, newValue)) return;
            SetUndo("Change Equipment Stat Value");
            equipmentStats[index].value = newValue;
            Dirty();
        }

        private void SetEquipmentStatAdditive(bool newIsAdditive, int index)
        {
            if(equipmentStats[index].isAdditive == newIsAdditive) return;
            SetUndo("Toggle Stat isAdditive");
            equipmentStats[index].isAdditive = newIsAdditive;
            Dirty();
        }
#endif

        [System.Serializable]
        protected class EquipmentStats
        {
            public Stat stat;
            public float value;
            public bool isAdditive;
        }
    }
}