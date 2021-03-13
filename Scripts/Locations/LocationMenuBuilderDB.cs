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
        Dictionary<TownNodeList, MenuCategory[]> nodeMenuLookup = null;

        public IEnumerable<string> GetCategories(TownNodeList townNode)
        {
            BuildLookup();

            foreach (MenuCategory menuCategory in nodeMenuLookup[townNode])
            {
                yield return menuCategory.category;
            }
        }

        public IEnumerable<InventoryItem> GetInventoryItems(TownNodeList townNode, int tabIndex)
        {
            BuildLookup();
            
            foreach (InventoryItem item in nodeMenuLookup[townNode][tabIndex].menuContents)
            {
                yield return item;
            }
        }

        private void BuildLookup()
        {
            if(nodeMenuLookup != null) return;

            nodeMenuLookup = new Dictionary<TownNodeList, MenuCategory[]>();

            foreach (var menuBuilder in menuBuilders)
            {
                nodeMenuLookup[menuBuilder.townNode] = menuBuilder.menuCategories;
            }
        }

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
            public InventoryItem[] menuContents;
        }
    }
}