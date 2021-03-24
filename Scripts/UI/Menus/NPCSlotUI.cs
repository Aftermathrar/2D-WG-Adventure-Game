using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class NPCSlotUI : MonoBehaviour
    {
        [SerializeField] GameObject goTMPro;
        [SerializeField] GameObject goIcon;
        [SerializeField] GameObject goText;
        [SerializeField] Text npcName;
        [SerializeField] Text classText;
        [SerializeField] Text jobText;

        Button button;
        NPCInfo feedeeInfo;

        private void Awake() 
        {
            button = GetComponent<Button>();
        }

        public void SlotSetup(GameObject _feedeeGO, string nodeString)
        {
            bool hasNPC = false;
            if(_feedeeGO) hasNPC = true;

            goTMPro.SetActive(!hasNPC);
            goIcon.SetActive(hasNPC);
            goText.SetActive(hasNPC);

            if(!hasNPC) return;
                
            feedeeInfo = _feedeeGO.GetComponent<NPCInfo>();

            npcName.text = feedeeInfo.GetCharacterInfo("name");
            classText.text = GetClassDisplayName(feedeeInfo.GetCharacterInfo("rank"));
            jobText.text = nodeString;
        }

        private string GetClassDisplayName(string npcClass)
        {
            // Used to split generic enum values into better titles
            switch (npcClass)
            {
                case "WitchDoctor":
                    return "Witch Doctor";
                case "TradeProfession":
                    return "Goods Merchant";
                case "GuildMember":
                    return "Guild Staff";
                default:
                    return npcClass;
            }
        }
    }
}
