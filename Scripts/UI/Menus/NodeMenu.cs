using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
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
        [SerializeField] FeedeeManager feedeeManager;
        [SerializeField] Image background;
        [SerializeField] GameObject menuPanel;
        [SerializeField] RectTransform menuContent;
        [SerializeField] NPCSlotUI npcSlotUI;
        [SerializeField] CharacterInfoDescription npcInfoDescription;
        [SerializeField] ItemInfoDescription itemInfoDescription;
        [SerializeField] ItemMenuSlotUI slotPrefab;
        [SerializeField] RectTransform[] menuTabs;
        [SerializeField] TextMeshProUGUI[] tabTexts;
        [SerializeField] Button[] choiceButtons;

        //Cache
        Image[] tabImages;
        List<ItemMenuSlotUI> itemSlots;
        List<Button> itemSlotButtons;
        List<Button[]> slotActionButtons;
        Button npcSlotButton;
        Inventory playerInventory;

        // Assigned on menu open
        LocationMenuBuilderDB menuBuilderDB = null;
        TownNodeList node;
        int currentTab;
        int sellTab = -1;
        Coroutine slotCoroutine = null;

        private void Awake()
        {
            if (feedeeManager == null) GetComponentInParent<FeedeeManager>();

            int tabCount = menuTabs.Length;
            tabImages = new Image[tabCount];

            for (int i = 0; i < tabCount; i++)
            {
                tabImages[i] = menuTabs[i].GetComponent<Image>();
            }
        }

        private void Start() 
        {
            //Register item slots for pooling, buttons for onClick listeners
            itemSlots = new List<ItemMenuSlotUI>();
            itemSlotButtons = new List<Button>();
            slotActionButtons = new List<Button[]>();
            for (int i = 0; i < menuContent.childCount; i++)
            {
                itemSlots.Add(menuContent.GetChild(i).GetComponent<ItemMenuSlotUI>());
                itemSlotButtons.Add(itemSlots[i].GetComponent<Button>());
                slotActionButtons.Add(itemSlots[i].GetActionButtons());
            }
            npcSlotButton = npcSlotUI.GetComponent<Button>();

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

            // Pull shopkeeper data
            GameObject _feedeeGO = feedeeManager.GetFeedeeAtNode(townNode);
            if(_feedeeGO != null)
            {
                npcSlotUI.SlotSetup(_feedeeGO, townNode.ToString());
                npcInfoDescription.SetCharacterInfo(_feedeeGO);
                npcSlotButton.onClick.AddListener(() => npcInfoDescription.SetCharacterInfo(_feedeeGO));
                NPCInfo npcInfo = _feedeeGO.GetComponent<NPCInfo>();
                choiceButtons[0].onClick.AddListener(() => npcInfo.TalkToNPC());
                choiceButtons[1].interactable = false;
                choiceButtons[2].interactable = false;
                choiceButtons[3].interactable = false;
            }
            
            // Label tabs and populate menu items
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
            npcSlotButton.onClick.RemoveAllListeners();
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

        public int BuyItem(InventoryItem item, int count)
        {
            float cost = item.GetValue() * count;
            int purchaseCount = 0;
            if(playerInventory.HasMoneyFor(cost))
            {
                if(item.IsStackable())
                {
                    if(playerInventory.AddToFirstEmptySlot(item, count))
                        {
                            playerInventory.AddMoney(-cost);
                            purchaseCount = count;
                        }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        if(playerInventory.AddToFirstEmptySlot(item, 1))
                        {
                            playerInventory.AddMoney(-item.GetValue());
                            purchaseCount++;
                        }
                    }
                }
            }

            return purchaseCount;
        }

        public int SellItem(int inventorySlot, int tryCount)
        {
            int sellCount = 0;
            bool reloadMenu = false;
            InventoryItem item = playerInventory.GetItemInSlot(inventorySlot);

            if(item.IsStackable())
            {
                int invCount = playerInventory.GetItemCount(item);
                sellCount = Mathf.Min(tryCount, invCount);
                playerInventory.RemoveItemFromInventory(item, sellCount);
                playerInventory.AddMoney(item.GetValue() * sellCount);
                if(invCount <= tryCount) reloadMenu = true;
            }
            else
            {
                playerInventory.RemoveFromSlot(inventorySlot, 1);
                playerInventory.AddMoney(item.GetValue());
                reloadMenu = true;
            }

            if(reloadMenu)
            {
                slotCoroutine = StartCoroutine(SetupSellMenuSlots());
            }
            return sellCount;
        }

        public bool CraftItem(InventoryItem item, int tryCount, out int craftCount)
        {
            craftCount = 0;
            return false;
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
            string action = tabTexts[tabIndex].text;
            foreach (var item in menuBuilderDB.GetInventoryItems(node, tabIndex))
            {
                ItemMenuSlotUI slotUI = GetSlotUI(slotCount, i);

                slotUI.SlotSetup(action, item.GetIcon(), item.GetDisplayName(), 
                    item.GetValue(), playerInventory.GetItemCount(item), 10f);
                
                itemSlotButtons[i].onClick.RemoveAllListeners();
                itemSlotButtons[i].onClick.AddListener(() => itemInfoDescription.SetItemInfo(item));

                // Add listener for buy/craft X buttons
                for (int j = 0; j < slotActionButtons[i].Length; j++)
                {
                    int btnIndex = j;
                    int itemCountDelta = 0;
                    slotActionButtons[i][j].onClick.RemoveAllListeners();
                    slotActionButtons[i][j].onClick.AddListener(() =>
                    {
                        switch (action)
                        {
                            case "Buy":
                                itemCountDelta = BuyItem(item, slotUI.GetActionButtonCount(btnIndex));
                                break;
                            case "Craft":
                                CraftItem(item, slotUI.GetActionButtonCount(btnIndex), out itemCountDelta);
                                break;
                        }
                        slotUI.ChangeOwnedValue(itemCountDelta);
                    });
                }
                
                i++;
                if ((i % 10) == 0)
                {
                    yield return null;
                }
            }

            if(i < slotCount)
            {
                yield return null;
                for(int j = i; j < slotCount; j++)
                {
                    menuContent.GetChild(j).gameObject.SetActive(false);
                }
            }
            // Resize content window
            int contentHeight = i * 120;
            menuContent.sizeDelta = new Vector2(menuContent.sizeDelta.x, contentHeight);
        }

        IEnumerator SetupSellMenuSlots()
        {
            yield return null;
            int slotCount = itemSlots.Count;
            int currentSlot = 0;
            bool isInventoryEmpty = true;
            Vector2 oldMenuPosition = new Vector2(menuContent.position.x, menuContent.position.y);

            for (int i = 0; i < playerInventory.GetSize(); i++)
            {
                int itemCount = playerInventory.GetCountInSlot(i);
                if (itemCount > 0)
                {
                    isInventoryEmpty = false;
                    InventoryItem item = playerInventory.GetItemInSlot(i);
                    ItemMenuSlotUI slotUI = GetSlotUI(slotCount, currentSlot);
                    float sellValue = item.GetValue() / 2f;

                    slotUI.SlotSetup("Sell", item.GetIcon(), item.GetDisplayName(), 
                        sellValue, itemCount, 10f);

                    itemSlotButtons[currentSlot].onClick.RemoveAllListeners();
                    itemSlotButtons[currentSlot].onClick.AddListener(() => itemInfoDescription.SetItemInfo(item));

                    for (int j = 0; j < slotActionButtons[currentSlot].Length; j++)
                    {
                        int inventorySlot = i;
                        int btnIndex = slotUI.GetActionButtonCount(j);
                        int itemCountDelta = 0;
                        slotActionButtons[currentSlot][j].onClick.RemoveAllListeners();
                        slotActionButtons[currentSlot][j].onClick.AddListener(() =>
                        {
                            itemCountDelta = SellItem(inventorySlot, btnIndex);
                            slotUI.ChangeOwnedValue(-itemCountDelta);
                        });
                    }
                    
                    currentSlot++;
                    if((currentSlot % 10) == 0)
                    {
                        yield return null;
                    }
                }
            }

            if(isInventoryEmpty)
            {
                slotCoroutine = StartCoroutine(ClearSlotsAfterClose());
            }
            else if(currentSlot < slotCount)
            {
                yield return null;
                for (int i = currentSlot; i < slotCount; i++)
                {
                    menuContent.GetChild(i).gameObject.SetActive(false);
                }
            }

            // Resize content window
            int contentHeight = currentSlot * 120;
            menuContent.sizeDelta = new Vector2(menuContent.sizeDelta.x, contentHeight);
            menuContent.position = oldMenuPosition;
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
                RectTransform slotRect = slotUI.GetComponent<RectTransform>();
                slotRect.offsetMax = new Vector2(5, -(5 + currentSlot * 120));
                slotRect.sizeDelta = new Vector2(1193, 120);
                itemSlots.Add(slotUI);
                itemSlotButtons.Add(slotUI.GetComponent<Button>());
                slotActionButtons.Add(slotUI.GetActionButtons());
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