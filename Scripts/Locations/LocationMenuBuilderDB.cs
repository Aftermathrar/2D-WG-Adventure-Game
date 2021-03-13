using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using UnityEngine;

namespace ButtonGame.Locations
{
    [CreateAssetMenu(fileName = "LocationMenuBuilderDB", menuName = "Locations/LocationMenuBuilderDB")]
    public class LocationMenuBuilderDB : ScriptableObject 
    {
        [SerializeField] NodeMenuBuilder[] menuBuilders;

        [System.Serializable]
        private class NodeMenuBuilder
        {
            [SerializeField] string townNodeName;
            public TownNodeList townNode;
            public bool isMerchant = false;
            public MenuCategory[] menuCategories;
        }

        [System.Serializable]
        private class MenuCategory
        {
            public string category;
            public MenuContent[] menuContents;
        }

        [System.Serializable]
        private class MenuContent
        {
            public InventoryItem item;
        }
    }
}