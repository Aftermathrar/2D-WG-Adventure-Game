using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Inventories;
using ButtonGame.Locations;
using ButtonGame.UI.Inventories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class NodeMenu : MonoBehaviour
    {
        [SerializeField] FeedeeManager manager;
        [SerializeField] Image background;
        [SerializeField] GameObject menuPanel;
        [SerializeField] RectTransform menuContent;
        [SerializeField] ItemMenuSlotUI slotPrefab;
        [SerializeField] RectTransform[] menuTabs;
        [SerializeField] TextMeshProUGUI[] tabTexts;
        
        LocationMenuBuilderDB menuBuilderDB = null;
        TownNodeList node;
        int currentTab;

        public void OpenMenu(LocationMenuBuilderDB locationMenuBuilder, TownNodeList townNode)
        {
            // Activate menu
            background.enabled = true;
            menuPanel.gameObject.SetActive(true);

            // Cache current menu info
            menuBuilderDB = locationMenuBuilder;
            node = townNode;
            currentTab = 0;
            
            SetupMenuTabs();
            SetupMenuSlots(currentTab);
        }

        public void CloseMenu()
        {
            background.enabled = false;
            menuPanel.gameObject.SetActive(false);
        }

        public void SwitchTab(int tabIndex)
        {
            if(tabIndex == currentTab) return;

            SetupMenuSlots(tabIndex);
            currentTab = tabIndex;
        }


        private void SetupMenuTabs()
        {
            int i = 0;

            foreach (string category in menuBuilderDB.GetCategories(node))
            {
                menuTabs[i].gameObject.SetActive(true);
                tabTexts[i].text = category;
                i++;
            }
            
            if(i < menuTabs.Length)
            {
                for (int j = i; j < menuTabs.Length; j++)                
                {
                    menuTabs[j].gameObject.SetActive(false);
                }
            }
        }

        private void SetupMenuSlots(int tabIndex)
        {
            int i = 0;
            int slotCount = menuContent.childCount;
            foreach (var item in menuBuilderDB.GetInventoryItems(node, tabIndex))
            {
                ItemMenuSlotUI slotUI;
                if(i < slotCount)
                {
                    slotUI = menuContent.GetChild(i).GetComponent<ItemMenuSlotUI>();
                    slotUI.gameObject.SetActive(true);
                }
                else
                {
                    slotUI = Instantiate(slotPrefab, menuContent);
                }
                
                slotUI.SlotSetup(tabTexts[tabIndex].text, item.GetIcon(), 
                    item.GetDisplayName(), item.GetValue(), 1f, 10f);
                
                i++;
            }

            if(i < slotCount && menuContent.GetChild(i).gameObject.activeSelf)
            {
                for(int j = i; j < slotCount; j++)
                {
                    menuContent.GetChild(j).gameObject.SetActive(false);
                }
            }
        }
    }
}