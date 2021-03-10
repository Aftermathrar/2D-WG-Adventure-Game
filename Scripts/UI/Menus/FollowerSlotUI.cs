using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class FollowerSlotUI : MonoBehaviour
    {   
        [SerializeField] Text classText;
        [SerializeField] Text followerName;
        [SerializeField] Text rankText;
        [SerializeField] Text jobText;
        [SerializeField] Text hungerText;

        [SerializeField] LayoutElement layout = null;
        [SerializeField] RectTransform parentRect = null;

        Button button;
        FollowerManager followerManager;

        [SerializeField]
        int slot = -1;
        
        private void Awake() 
        {
            button = GetComponent<Button>();
        }

        private void OnDisable() 
        {
            gameObject.SetActive(false);
        }

        public void SlotSetup(FollowerManager manager, int slotNumber)
        {
            followerManager = manager;
            slot = slotNumber;
            layout.preferredWidth = (parentRect.rect.width - 20) / 2;

            GameObject followerGO = followerManager.GetFollowerObject(slot);
            if (followerGO == null) return;

            NPCInfo info = followerGO.GetComponent<NPCInfo>();

            classText.text = (followerManager.GetFollowerClass(slot) == "Priest") ? "Priest" : "Witch Doctor";
            followerName.text = info.GetCharacterInfo("name");
            rankText.text = info.GetCharacterInfo("rank");
            jobText.text = GetPositionToDisplay();
            hungerText.text = GetHungerDisplay(followerGO.GetComponent<Fullness>().GetPercentage());
        }

        public void ChangeFollower()
        {
            followerManager.ChangeActiveFollower(slot);
            GetComponentInParent<FollowerMenu>().CloseMenu();
        }

        private string GetPositionToDisplay()
        {
            string position = followerManager.GetFollowerPosition(slot);
            switch (position)
            {
                case "Combat":
                    return "In Party";
                case "Home":
                    return "Idle";
                default:
                    return "Lost to the Void";
            }
        }

        private string GetHungerDisplay(float hungerPercent)
        {
            if(hungerPercent <= 10)
                return "Starving";
            else if(hungerPercent <= 50)
                return "Hungry";
            else if(hungerPercent <= 80)
                return "Content";
            else if(hungerPercent <= 100)
                return "Stuffed";
            else
                return "Overstuffed";
        }
    }
}
