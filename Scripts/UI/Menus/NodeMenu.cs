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
        //Cache
        Image[] tabImages;
        List<ItemMenuSlotUI> itemSlots;
        Inventory playerInventory;

        // Assigned on menu open
        LocationMenuBuilderDB menuBuilderDB = null;
        TownNodeList node;
        int currentTab;
        int sellTab = -1;
        Coroutine slotCoroutine = null;

        private void Start() 
        {
            int tabCount = menuTabs.Length;
            tabImages = new Image[tabCount];

            for (int i = 0; i < tabCount; i++)
            {
                tabImages[i] = menuTabs[i].GetComponent<Image>();
            }

            itemSlots = new List<ItemMenuSlotUI>();
            for (int i = 0; i < menuContent.childCount; i++)
            {
                itemSlots.Add(menuContent.GetChild(i).GetComponent<ItemMenuSlotUI>());
            }

            playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        }

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
            slotCoroutine = StartCoroutine(SetupMenuSlots(currentTab));
        }

        public void CloseMenu()
        {
            background.enabled = false;
            menuPanel.gameObject.SetActive(false);
            if(slotCoroutine != null) StopCoroutine(slotCoroutine);
            // Reset tab indicators back to normal, disable all slot gameobjects
            FormatTabs(currentTab, 0);
            StartCoroutine(ClearSlotsAfterClose());
        }

        public void SwitchTab(int tabIndex)
        {
            if (tabIndex == currentTab) return;
            if (slotCoroutine != null) StopCoroutine(slotCoroutine);

            if (tabIndex == sellTab)
            {
                slotCoroutine = StartCoroutine(SetupSellMenuSlots());
            }
            else
            {
                slotCoroutine = StartCoroutine(SetupMenuSlots(tabIndex));
            }
            FormatTabs(currentTab, tabIndex);
            currentTab = tabIndex;
        }

        private void FormatTabs(int oldIndex, int newIndex)
        {
            menuTabs[oldIndex].offsetMax = new Vector2(0, 0);
            tabImages[oldIndex].color = new Color32(255, 255, 255, 70);
            menuTabs[newIndex].offsetMax = new Vector2(0, -10);
            tabImages[newIndex].color = new Color32(255, 255, 255, 255);
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

            if(menuBuilderDB.GetIsMerchant(node))
            {
                menuTabs[i].gameObject.SetActive(true);
                tabTexts[i].text = "Sell";
                sellTab = i;
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

        IEnumerator SetupMenuSlots(int tabIndex)
        {
            yield return null;
            int i = 0;
            int slotCount = itemSlots.Count;
            foreach (var item in menuBuilderDB.GetInventoryItems(node, tabIndex))
            {
                ItemMenuSlotUI slotUI = GetSlotUI(slotCount, i);
                
                slotUI.SlotSetup(tabTexts[tabIndex].text, item.GetIcon(), 
                    item.GetDisplayName(), item.GetValue(), playerInventory.GetItemCount(item), 10f);
                
                i++;
            }

            if(i < slotCount)
            {
                yield return null;
                for(int j = i; j < slotCount; j++)
                {
                    menuContent.GetChild(j).gameObject.SetActive(false);
                }
            }
        }

        IEnumerator SetupSellMenuSlots()
        {
            yield return null;
            int slotCount = itemSlots.Count;
            int currentSlot = 0;
            for (int i = 0; i < playerInventory.GetSize(); i++)
            {
                int itemCount = playerInventory.GetCountInSlot(i);
                if (itemCount > 0)
                {
                    InventoryItem item = playerInventory.GetItemInSlot(i);
                    ItemMenuSlotUI slotUI = GetSlotUI(slotCount, currentSlot);

                    slotUI.SlotSetup("Sell", item.GetIcon(), item.GetDisplayName(),
                        item.GetValue(), itemCount, 10f);
                    
                    currentSlot++;
                    if((currentSlot % 10) == 0)
                    {
                        yield return null;
                    }
                }
            }
        }

        private ItemMenuSlotUI GetSlotUI(int slotCount, int currentSlot)
        {
            ItemMenuSlotUI slotUI;
            if (currentSlot < slotCount)
            {
                slotUI = itemSlots[currentSlot];
                slotUI.gameObject.SetActive(true);
            }
            else
            {
                slotUI = Instantiate(slotPrefab, menuContent);
                itemSlots.Add(slotUI);
            }
            return slotUI;
        }

        IEnumerator ClearSlotsAfterClose()
        {
            foreach (Transform menuSlot in menuContent.transform)
            {
                yield return null;
                menuSlot.gameObject.SetActive(false);
            }
        }
    }
}