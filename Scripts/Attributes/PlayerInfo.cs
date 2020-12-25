using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButtonGame.Attributes
{
    public class PlayerInfo : MonoBehaviour, ISaveable, ISlotInfo
    {
        [SerializeField]
        string playerName;
        [SerializeField]
        string playerRank;
        [SerializeField]
        string time;
        [SerializeField]
        string currentQuest;
        [SerializeField]
        int currentScene = -1;
        Dictionary<string, string> infoLookup = null;

        public Dictionary<string, string> GetPlayerInfo()
        {
            BuildLookup();
            return infoLookup;
        }

        private void BuildLookup()
        {
            infoLookup = new Dictionary<string, string>();
            infoLookup["name"] = playerName;
            infoLookup["rank"] = playerRank;
            infoLookup["time"] = time;
            infoLookup["quest"] = currentQuest;
            infoLookup["scene"] = SceneManager.GetActiveScene().buildIndex.ToString();
        }

        public object CaptureState()
        {
            BuildLookup();
            return infoLookup;
        }

        public void RestoreState(object state)
        {
            infoLookup = (Dictionary<string, string>)state;
            playerName = infoLookup["name"];
            playerRank = infoLookup["rank"];
            time = infoLookup["time"];
            currentQuest = infoLookup["quest"];
            currentScene = int.Parse(infoLookup["scene"]);
        }
    }
}
