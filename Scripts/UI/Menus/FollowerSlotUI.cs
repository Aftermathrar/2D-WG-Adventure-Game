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
        [SerializeField] FollowerManager followerManager;
        [SerializeField] Image icon = null;
        [SerializeField] Text slotText;
        [SerializeField] Text followerName;
        [SerializeField] Text rankText;
        [SerializeField] Text jobText;
        [SerializeField] Text hungerText;

        Button button;

        int slot;
        
        private void Awake() 
        {
            button = GetComponent<Button>();
            slot = transform.GetSiblingIndex();
        }

        private void OnEnable() 
        {
            GameObject followerGO = followerManager.GetFollowerObject(slot);
            if(followerGO == null) return;

            NPCInfo info = followerGO.GetComponent<NPCInfo>();

            slotText.text = followerManager.GetFollowerClass(slot);
            followerName.text = info.GetCharacterInfo("name");
            rankText.text = info.GetCharacterInfo("rank");
            jobText.text = followerManager.GetFollowerPosition(slot);
            hungerText.text = ((int)followerGO.GetComponent<Fullness>().GetAttributeValue()).ToString();
        }

        public void ChangeFollower()
        {
            followerManager.ChangeActiveFollower(slot);
            OnEnable();
        }
    }
}
