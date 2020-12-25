using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace ButtonGame.Saving
{
    [CreateAssetMenu(fileName = "SaveSlotDB", menuName = "Stats/SaveSlotDB")]
    public class SaveSlotDB : ScriptableObject
    {
        // [SerializeField] List<SaveSlot> saves = new List<SaveSlot>();
        Dictionary<string, SaveRecord> saveLookup = null;

        public Dictionary<string, string> GetSaveRecord(string saveFile)
        {
            if (saveLookup == null)
            {
                BuildLookup();
            }

            if(!saveLookup.ContainsKey(saveFile))
            {
                return null;
            }

            Dictionary<string, string> saveInfo = new Dictionary<string, string>();
            saveInfo["name"] = saveLookup[saveFile].playerName;
            saveInfo["rank"] = saveLookup[saveFile].rank;
            saveInfo["time"] = saveLookup[saveFile].time;
            saveInfo["quest"] = saveLookup[saveFile].quest;
            saveInfo["scene"] = saveLookup[saveFile].scene;

            return saveInfo;
        }

        public void AddSaveRecord(string saveFile)
        {
            if(saveLookup == null)
            {
                BuildLookup();
            }

            ISlotInfo slotInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<ISlotInfo>();
            var state = (Dictionary<string, string>)slotInfo.CaptureState();

            SaveRecord newRecord = new SaveRecord();

            newRecord.playerName = state["name"];
            newRecord.rank = state["rank"];
            newRecord.time = state["time"];
            newRecord.quest = state["quest"];
            newRecord.scene = state["scene"];

            saveLookup[saveFile] = newRecord;
        }

        private void BuildLookup()
        {
            saveLookup = new Dictionary<string, SaveRecord>();

            List<string> filePaths = Directory.GetFiles(Application.persistentDataPath, "*.sav").ToList();
            foreach (var filePath in filePaths)
            {
                string fileName = filePath.Replace(Application.persistentDataPath + "\\", "");
                fileName = fileName.Replace(".sav", "");
                Debug.Log(fileName);

                var stateDict = new Dictionary<string, object>();
                using (FileStream stream = File.Open(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    stateDict = (Dictionary<string, object>)formatter.Deserialize(stream);
                }

                stateDict = (Dictionary<string, object>)stateDict["player"];
                var state = (Dictionary<string, string>)stateDict["ButtonGame.Attributes.PlayerInfo"];
                SaveRecord saveRecord = new SaveRecord();

                saveRecord.playerName = state["name"];
                saveRecord.rank = state["rank"];
                saveRecord.time = state["time"];
                saveRecord.quest = state["quest"];
                saveRecord.scene = state["scene"];

                saveLookup[fileName] = saveRecord;
            }
        }

        [System.Serializable]
        class SaveSlot
        {
            public string saveFile;
            public SaveRecord record;
        }

        [System.Serializable]
        class SaveRecord
        {
            public string playerName;
            public string rank;
            public string time;
            public string quest;
            public string scene;
        }
    }
}
