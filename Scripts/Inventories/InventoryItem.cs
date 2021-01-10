using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Inventories
{
    /// <summary>
    /// A ScriptableObject that represents any item that can be put in an
    /// inventory.
    /// </summary>
    /// <remarks>
    /// In practice, you are likely to use a subclass such as `ActionItem` or
    /// `EquipableItem`.
    /// </remarks>
    [CreateAssetMenu(menuName = ("Inventory/Item"))]
    public class InventoryItem : ScriptableObject, ISerializationCallbackReceiver, ITooltipProvider    
    {
        // CONFIG DATA
        [Tooltip("Auto-generated UUID for saving/loading. Clear this field if you want to generate a new one.")]
        [SerializeField] string itemID = null;
        [Tooltip("Item name to be displayed in UI.")]
        [SerializeField] string displayName = null;
        [Tooltip("Category name to be displayed in UI.")]
        [SerializeField] ItemCategories categoryName = ItemCategories.Material;
        [Tooltip("Item description to be displayed in UI.")]
        [SerializeField] List<TooltipDescriptionField> description = new List<TooltipDescriptionField>();
        [Tooltip("The UI icon to represent this item in the inventory.")]
        [SerializeField] Sprite icon = null;
        [Tooltip("Sale value to be displayed in UI.")]
        [SerializeField] float value = 0;
        // [Tooltip("The prefab that should be spawned when this item is dropped.")]
        // [SerializeField] Pickup pickup = null;
        [Tooltip("If true, multiple items of this type can be stacked in the same inventory slot.")]
        [SerializeField] bool stackable = false;


        // STATE
        static Dictionary<string, InventoryItem> itemLookupCache;

        // PUBLIC

        /// <summary>
        /// Get the inventory item instance from its UUID.
        /// </summary>
        /// <param name="itemID">
        /// String UUID that persists between game instances.
        /// </param>
        /// <returns>
        /// Inventory item instance corresponding to the ID.
        /// </returns>
        public static InventoryItem GetFromID(string itemID)
        {
            if (itemLookupCache == null)
            {
                itemLookupCache = new Dictionary<string, InventoryItem>();
                var itemList = Resources.LoadAll<InventoryItem>("");
                foreach (var item in itemList)
                {
                    if (itemLookupCache.ContainsKey(item.itemID))
                    {
                        Debug.LogError(string.Format("Looks like there's a duplicate GameDevTV.UI.InventorySystem ID for objects: {0} and {1}", itemLookupCache[item.itemID], item));
                        continue;
                    }

                    itemLookupCache[item.itemID] = item;
                }
            }

            if (itemID == null || !itemLookupCache.ContainsKey(itemID)) return null;
            return itemLookupCache[itemID];
        }
        
        // /// <summary>
        // /// Spawn the pickup gameobject into the world.
        // /// </summary>
        // /// <param name="position">Where to spawn the pickup.</param>
        // /// <returns>Reference to the pickup object spawned.</returns>
        // public Pickup SpawnPickup(Vector3 position)
        // {
        //     var pickup = Instantiate(this.pickup);
        //     pickup.transform.position = position;
        //     pickup.Setup(this);
        //     return pickup;
        // }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemID()
        {
            return itemID;
        }

        public bool IsStackable()
        {
            return stackable;
        }
        
        public string GetDisplayName()
        {
            return displayName;
        }

        public string GetCategoryName()
        {
            return categoryName.ToString();
        }

        public float GetValue()
        {
            return value;
        }

        public IEnumerable<TooltipDescriptionField> GetDescriptionFields()
        {
            return description;
        }

        public virtual object GetModifiers() { return null; }

        public virtual void SetModifiers(object state) { }

#if UNITY_EDITOR
        bool drawinventoryItem = true;
        protected GUIStyle foldoutStyle;
        [NonSerialized] protected GUIStyle contentStyle;

        public virtual void DrawCustomInspector()
        {
            contentStyle = new GUIStyle {padding = new RectOffset(15, 15, 0, 0)};
            foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;
            drawinventoryItem = EditorGUILayout.BeginFoldoutHeaderGroup(drawinventoryItem, "InventoryItem Data", foldoutStyle);
            if(!drawinventoryItem) return;

            EditorGUILayout.BeginVertical(contentStyle);
            SetItemID(EditorGUILayout.TextField("ItemID (clear to reset)", GetItemID()));
            SetDisplayName(EditorGUILayout.TextField("Display Name", GetDisplayName()));
            SetIcon((Sprite)EditorGUILayout.ObjectField("Item Icon", GetIcon(), typeof(Sprite), false));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Description Fields", MessageType.None);
            if (GUILayout.Button("Add New Description Line", GUILayout.Width(200)))
            {
                AddDescription();
            }
            EditorGUILayout.EndHorizontal();
            bool removeDescriptionField = false;
            int removeDescIndex = 0;
            for (int i = 0; i < description.Count; i++)
            {
                SetDescription(EditorGUILayout.TextField("Description", description[i].description), i);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Description Line", GUILayout.Width(150)))
                {
                    removeDescriptionField = true;
                    removeDescIndex = i;
                }
                SetDescriptionIcon((Sprite)EditorGUILayout.ObjectField("Icon", description[i].iconImage, typeof(Sprite), false), i);
                EditorGUILayout.EndHorizontal();
            }
            if (removeDescriptionField)
            {
                RemoveDescription(removeDescIndex);
            }
            SetStackable(EditorGUILayout.Toggle("Stackable", IsStackable()));
            SetValue(EditorGUILayout.FloatField("Value", GetValue()));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void SetItemID(string newItemID)
        {
            if (itemID == newItemID) return;
            SetUndo("Change ItemID");
            itemID = newItemID;
            Dirty();
        }

        public void SetDisplayName(string newDisplayName)
        {
            if(displayName == newDisplayName) return;
            SetUndo("Change DisplayName");
            displayName = newDisplayName;
            Dirty();
        }

        public void SetIcon(Sprite newIcon)
        {
            if(icon == newIcon) return;
            SetUndo("Change Icon");
            icon = newIcon;
            Dirty();
        }

        public void SetDescription(string newDescription, int index)
        {
            if(description[index].description == newDescription) return;
            SetUndo("Change Description");
            description[index].description = newDescription;
            Dirty();
        }

        public void SetDescriptionIcon(Sprite newIcon, int index)
        {
            if(description[index].iconImage == newIcon) return;
            SetUndo("Change Icon");
            description[index].iconImage = newIcon;
            description[index].hasIcon = (newIcon != null);
            Dirty();
        }

        public void SetStackable(bool newIsStackable)
        {
            if(stackable == newIsStackable) return;
            SetUndo("Change Stackable");
            stackable = newIsStackable;
            Dirty();
        }

        public void AddDescription()
        {
            SetUndo("Add Description Field");
            TooltipDescriptionField newDescriptionField = new TooltipDescriptionField();
            description.Add(newDescriptionField);
            Dirty();
        }

        public void RemoveDescription(int index)
        {
            SetUndo("Remove DescriptionField");
            description.RemoveAt(index);
            Dirty();
        }

        public void SetValue(float newValue)
        {
            if(value == newValue) return;
            SetUndo("Change Item Value");
            value = newValue;
            Dirty();
        }

        protected void SetUndo(string message)
        {
            Undo.RecordObject(this, message);
        }

        protected void Dirty()
        {
            EditorUtility.SetDirty(this);
        }

        protected bool FloatEquals(float value1, float value2)
        {
            return Mathf.Abs(value1 - value2) < .001f;
        }
#endif

        // PRIVATE
        
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Generate and save a new UUID if this is blank.
            if (string.IsNullOrWhiteSpace(itemID))
            {
                itemID = System.Guid.NewGuid().ToString();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Require by the ISerializationCallbackReceiver but we don't need
            // to do anything with it.
        }
    }
}
