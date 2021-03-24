using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Locations
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] LocationMenuDB locationMenuDB;
        [SerializeField] FollowerMenu popupFollowerMenu;
        [SerializeField] NodeMenu popupNodeMenu;
        [SerializeField] LocationMenuSlotUI[] menuSlots;
        LocationList currentLocation;
        TownNodeList[] connectedNodes;
        bool[] isMenuNode;

        private void Start() 
        {
            GridLayoutGroup gridLayout = GetComponent<GridLayoutGroup>();
            RectTransform thisRect = GetComponent<RectTransform>();
            gridLayout.cellSize = new Vector2(gridLayout.cellSize.x, thisRect.rect.height);
        }

        public void MakeMainMenu(LocationList location)
        {
            currentLocation = location;
            int nodeMenuCount = locationMenuDB.GetMenuCount(location, TownNodeList.Main);
            SetupMenuSlots(nodeMenuCount);
        }

        public void ChangeNodeOnClick(int slot)
        {
            if(!isMenuNode[slot])
            {
                MakeNodeMenu(connectedNodes[slot]);
            }
            else
            {
                if(connectedNodes[slot] == TownNodeList.ManageParty)
                {
                    popupFollowerMenu.OpenMenu();
                    return;
                }
                LocationMenuBuilderDB menuBuilderDB = locationMenuDB.GetMenuBuilderDB(currentLocation);
                popupNodeMenu.OpenMenu(menuBuilderDB, connectedNodes[slot]);
            }
        }

        public IEnumerable<LocationList> GetLocations()
        {
            foreach (var locationList in locationMenuDB.GetLocations())
            {
                yield return locationList;
            }
        }

        public IEnumerable<TownNodeList> GetLocationMainNodes(LocationList locationQuery)
        {
            foreach (TownNodeList node in locationMenuDB.GetMainNodes(locationQuery))
            {
                yield return node;
            }
        }

        public int GetNodeMenuCount(LocationList locationQuery, TownNodeList nodeQuery)
        {
            return locationMenuDB.GetMenuCount(locationQuery, nodeQuery);
        }

        public bool HasNPCSpawn(LocationList locationQuery, TownNodeList nodeQuery, int nodeIndex)
        {
            return locationMenuDB.HasNPCSpawn(locationQuery, nodeQuery, nodeIndex);
        }

        public TownNodeList GetConnectedNode(LocationList locationQuery, TownNodeList nodeQuery, int nodeIndex)
        {
            return locationMenuDB.GetConnectedNode(locationQuery, nodeQuery, nodeIndex);
        }

        private void MakeNodeMenu(TownNodeList newNode)
        {
            int nodeMenuCount = locationMenuDB.GetMenuCount(currentLocation, newNode);
            SetupMenuSlots(nodeMenuCount);
        }

        private void SetupMenuSlots(int nodeMenuCount)
        {
            connectedNodes = new TownNodeList[nodeMenuCount];
            isMenuNode = new bool[nodeMenuCount];

            for (int i = 0; i < menuSlots.Length; i++)
            {
                if (nodeMenuCount <= i)
                {
                    menuSlots[i].gameObject.SetActive(false);
                    continue;
                }

                string slotTitle = locationMenuDB.GetMenuTitle(i);
                string slotSubtitle = locationMenuDB.GetMenuSubtitle(i);
                Sprite slotSprite = locationMenuDB.GetMenuSprite(i);
                Color slotColor = locationMenuDB.GetMenuColor(i);
                
                menuSlots[i].SlotSetup(slotTitle, slotSubtitle, slotSprite, slotColor, i);
                menuSlots[i].gameObject.SetActive(true);

                connectedNodes[i] = locationMenuDB.GetConnectedNode(i);
                isMenuNode[i] = locationMenuDB.IsMenu(i);
            }
        }
    }
}
