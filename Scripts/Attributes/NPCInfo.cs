using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Attributes
{
    public class NPCInfo : MonoBehaviour, ISaveable
    {
        [SerializeField]
        string npcName;
        [SerializeField]
        string npcRank;
        [SerializeField]
        string currentQuest;
        Dictionary<string, string> infoLookup = null;

        public Dictionary<string, string> GetCharacterLookup()
        {
            BuildLookup();
            return infoLookup;
        }

        public string GetCharacterInfo(string key)
        {
            BuildLookup();
            return infoLookup[key];
        }

        public void SetCharacterInfo(string key, string newInfo)
        {
            BuildLookup();
            if(infoLookup.ContainsKey(key))
            {
                infoLookup[key] = newInfo;
                UpdateProperties();
            }
        }

        private void SetCharacterInfo(Dictionary<string, string> newInfoLookup)
        {
            infoLookup = newInfoLookup;
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            npcName = infoLookup["name"];
            npcRank = infoLookup["rank"];
            currentQuest = infoLookup["quest"];
        }

        private void BuildLookup()
        {
            if(infoLookup == null)
            {
                infoLookup = new Dictionary<string, string>();
            }
            infoLookup["name"] = npcName;
            infoLookup["rank"] = npcRank;
            infoLookup["quest"] = currentQuest;
        }

        public object CaptureState()
        {
            BuildLookup();
            return infoLookup;
        }

        public void RestoreState(object state)
        {
            SetCharacterInfo((Dictionary<string, string>)state);
        }
    }
}
