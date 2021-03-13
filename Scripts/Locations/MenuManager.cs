using System.Collections;
using System.Collections.Generic;
using ButtonGame.UI.Menus;
using UnityEngine;

namespace ButtonGame.Locations
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] LocationMenuDB locationMenuDB;
        [SerializeField] NodeMenu popupNodeMenu;
        [SerializeField] LocationMenuSlotUI[] menuSlots;
        LocationList currentLocation;
        TownNodeList[] connectedNodes;
        bool[] isMenuNode;

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
                LocationMenuBuilderDB menuBuilderDB = locationMenuDB.GetMenuBuilderDB(currentLocation);
                popupNodeMenu.OpenMenu(menuBuilderDB, connectedNodes[slot]);
            }
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
