using System.Collections;
using System.Collections.Generic;
using ButtonGame.Locations;
using ButtonGame.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class TravelMenu : MonoBehaviour
    {
        LocationManager locationManager = null;
        Image background;

        [SerializeField] TextMeshProUGUI titleLocationText;
        [SerializeField] TextMeshProUGUI distanceText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] Transform slotContainer;
        [SerializeField] ChangeSceneButton changeSceneButton;
        [SerializeField] RectTransform[] menuTabs;
        [SerializeField] Button[] choiceButtons;

        Image[] tabImages;
        List<TravelSlotUI> travelSlots;
        List<Button> travelButtons;
        int currentTab;

        private void Awake() 
        {
            locationManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LocationManager>();
            background = GetComponent<Image>();

            int tabCount = menuTabs.Length;
            tabImages = new Image[tabCount];
            for (int i = 0; i < tabCount; i++)
            {
                tabImages[i] = menuTabs[i].GetComponent<Image>();
            }

            // Register travel slots for setup and onClick management
            travelSlots = new List<TravelSlotUI>();
            travelButtons = new List<Button>();

            foreach (Transform child in slotContainer)
            {
                travelSlots.Add(child.GetComponent<TravelSlotUI>());
                travelButtons.Add(child.GetComponent<Button>());
            }

            currentTab = 0;
        }

        public void SetTravelInfo(LocationList location)
        {
            titleLocationText.text = locationManager.GetLocationName(location);
            distanceText.text = "Distance: " + locationManager.GetLocationDistance(location) + " hours";
            descriptionText.text = locationManager.GetLocationDescription(location);

            bool isCombat = locationManager.IsCombatArea(location) ?? false;

            choiceButtons[0].interactable = true;
            choiceButtons[0].onClick.AddListener(() => SlowTravelToDestation(location));
            
            choiceButtons[1].interactable = locationManager.HasTraveled(location);
            choiceButtons[1].onClick.AddListener(() => FastTravelToDestination(location));
        }

        public void SlowTravelToDestation(LocationList location)
        {
            locationManager.SetTravelDestination(location);
            locationManager.SetDistanceRemaining(location);

            int sceneIndex = 2;
            changeSceneButton.SetSceneToLoad(sceneIndex);
            changeSceneButton.SetDestination(locationManager.GetLocationName(LocationList.SmallForest));
            changeSceneButton.ChangeScene();
        }

        public void FastTravelToDestination(LocationList location)
        {
            bool isCombat = locationManager.IsCombatArea(location) ?? false;

            int sceneindex = isCombat ? 2 : 1;
            changeSceneButton.SetSceneToLoad(sceneindex);
            changeSceneButton.SetDestination(locationManager.GetLocationName(location));
            changeSceneButton.ChangeScene();
        }

        public void SwitchTab(int tabIndex)
        {
            if (tabIndex == currentTab) return;

            FormatTabs(currentTab, tabIndex);
            currentTab = tabIndex;
            SetupMenuSlots();
        }

        private void FormatTabs(int oldIndex, int newIndex)
        {
            menuTabs[oldIndex].offsetMax = new Vector2(0, 0);
            tabImages[oldIndex].color = new Color32(255, 255, 255, 70);
            menuTabs[newIndex].offsetMax = new Vector2(0, -10);
            tabImages[newIndex].color = new Color32(255, 255, 255, 255);
        }

        private void SetupMenuSlots()
        {
            int i = 0;

            foreach (LocationList loc in locationManager.GetAvailableLocations())
            {
                string locName = locationManager.GetLocationName(loc);
                bool isCombat = locationManager.IsCombatArea(loc) ?? false;

                if (isCombat == (currentTab == 1))
                {
                    TravelSlotUI slot = travelSlots[i];
                    slot.SetSlotInfo(loc.ToString(), locName, isCombat);
                    slot.gameObject.SetActive(true);

                    travelButtons[i].onClick.RemoveAllListeners();
                    travelButtons[i].onClick.AddListener(() => SetTravelInfo(loc));

                    i++;
                }
            }

            while (i < travelSlots.Count)
            {
                travelSlots[i].gameObject.SetActive(false);
                i++;
            }
        }

        public void OpenMenu()
        {
            background.enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);

            SetupMenuSlots();
        }

        public void CloseMenu()
        {
            background.enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            // Reset tab indicators back to normal
            FormatTabs(currentTab, 0);
            titleLocationText.text = "Choose a Location";
            descriptionText.text = "";
            distanceText.text = "";
            foreach (var choice in choiceButtons)
            {
                choice.interactable = false;
            }
        }
    }
}
