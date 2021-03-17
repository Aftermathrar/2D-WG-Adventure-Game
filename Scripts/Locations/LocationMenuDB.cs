using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Locations
{
    [CreateAssetMenu(fileName = "LocationButtonDB", menuName = "Locations/LocationButtonDB")]
    public class LocationMenuDB : ScriptableObject 
    {
        [SerializeField] LocationMenuList[] locationMenuLists;
        Dictionary<LocationList, Dictionary<TownNodeList, TownNodeMenuInformation[]>> nodeMenuLookup;
        TownNodeMenuInformation[] nodeMenus = null;

        public IEnumerable<LocationList> GetLocations()
        {
            foreach (var locationMenu in locationMenuLists)
            {
                yield return locationMenu.location;
            }
        }

        public int GetMenuCount(LocationList location, TownNodeList townNode)
        {
            BuildLookup();
            var townNodeLookup = new Dictionary<TownNodeList, TownNodeMenuInformation[]>();
            if(nodeMenuLookup.TryGetValue(location, out townNodeLookup))
            {
                if(townNodeLookup.TryGetValue(townNode, out nodeMenus))
                {
                    return nodeMenus.Length;
                }
                nodeMenus = null;
            }
            return -1;
        }

        public IEnumerable<TownNodeList> GetMainNodes(LocationList location)
        {
            BuildLookup();
            if(nodeMenuLookup.ContainsKey(location))
            {
                foreach (TownNodeList townNode in nodeMenuLookup[location].Keys)
                {
                    yield return townNode;
                }
            }
        }

        public LocationMenuBuilderDB GetMenuBuilderDB(LocationList location)
        {
            foreach (var locationMenu in locationMenuLists)
            {
                if(locationMenu.location == location)
                {
                    return locationMenu.menuBuilderDB;
                }
            }
            return null;
        }

        public string GetMenuTitle(int index)
        {
            return nodeMenus[index].titleText;
        }

        public string GetMenuSubtitle(int index)
        {
            return nodeMenus[index].subtitleText;
        }

        public Sprite GetMenuSprite(int index)
        {
            return nodeMenus[index].sprite;
        }

        public Color GetMenuColor(int index)
        {
            return nodeMenus[index].spriteColor;
        }

        public TownNodeList GetConnectedNode(int index)
        {
            return nodeMenus[index].connectedNode;
        }

        public TownNodeList GetConnectedNode(LocationList locationQuery, TownNodeList nodeQuery, int index)
        {
            return nodeMenuLookup[locationQuery][nodeQuery][index].connectedNode;
        }

        public bool IsMenu(int index)
        {
            return nodeMenus[index].isMenu;
        }

        public bool HasNPCSpawn(LocationList locationQuery, TownNodeList nodeQuery, int index)
        {
            return nodeMenuLookup[locationQuery][nodeQuery][index].hasNPCSpawn;
        }

        private void BuildLookup()
        {
            if(nodeMenuLookup != null) return;

            nodeMenuLookup = new Dictionary<LocationList, Dictionary<TownNodeList, TownNodeMenuInformation[]>>();

            foreach (var location in locationMenuLists)
            {
                var townNodeLookup = new Dictionary<TownNodeList, TownNodeMenuInformation[]>();
                foreach (var node in location.townNodes)
                {
                    townNodeLookup[node.townNode] = node.menuInformation;
                }
                nodeMenuLookup[location.location] = townNodeLookup;
            }
        }

        [System.Serializable]
        private class LocationMenuList
        {
            [SerializeField] string locationName;
            public LocationList location;
            public LocationMenuBuilderDB menuBuilderDB;
            public TownNodeMenu[] townNodes;
        }

        [System.Serializable]
        private class TownNodeMenu
        {
            [SerializeField] string townNodeName;
            public TownNodeList townNode;
            public TownNodeMenuInformation[] menuInformation;
        }

        [System.Serializable]
        private class TownNodeMenuInformation
        {
            public string titleText;
            public string subtitleText;
            public Sprite sprite;
            public Color spriteColor = Color.white;
            public TownNodeList connectedNode;
            public bool isMenu = false;
            public bool hasNPCSpawn = false;
        }
    }
}
