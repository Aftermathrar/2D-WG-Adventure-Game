using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class FollowerMenu : MonoBehaviour
    {
        [SerializeField] FollowerManager manager;
        [SerializeField] Image background;
        [SerializeField] GameObject followerPanel;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] RectTransform menuHeader;
        [SerializeField] RectTransform menuContent;
        [SerializeField] Transform horizPrefab;
        [SerializeField] FollowerSlotUI slotPrefab;

        private void Awake() 
        {
            if(background == null)
            {
                background = GetComponent<Image>();
            }
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.U))
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            background.enabled = true;
            titleText.text = "No Active Companion";
            ChooseFollower();
            followerPanel.SetActive(true);
        }

        private void ChooseFollower()
        {
            int followerCount = manager.GetFollowerCount();

            // Grid is a vertical group of horizontal groups, int to track when to spawn new group
            int contentSlotCount = 0;
            Transform currentHorizGroup = null;
            FollowerSlotUI currentSlot;

            for (int i = 0; i < followerCount; i++)
            {
                if(manager.GetFollowerPosition(i) == "Combat")
                {
                    titleText.text = "Current Companion:";
                    currentSlot = menuHeader.GetChild(1).GetComponent<FollowerSlotUI>();
                }
                else
                {
                    // Find index for horizontal group
                    int column = contentSlotCount % 2;
                    int row = (contentSlotCount - column) / 2;

                    if(row >= menuContent.childCount)
                    {
                        currentHorizGroup = Instantiate(horizPrefab, menuContent);
                    }
                    else
                    {
                        currentHorizGroup = menuContent.GetChild(row);
                    }
                    currentHorizGroup.gameObject.SetActive(true);
                    currentSlot = currentHorizGroup.GetChild(column).GetComponent<FollowerSlotUI>();
                    contentSlotCount++;
                }
                currentSlot.SlotSetup(manager, i);
                currentSlot.gameObject.SetActive(true);
            }
        }

        public void CloseMenu()
        {
            background.enabled = false;
            followerPanel.SetActive(false);
        }
    }
}
