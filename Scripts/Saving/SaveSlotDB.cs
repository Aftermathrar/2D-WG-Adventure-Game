using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ButtonGame.Saving
{
    [CreateAssetMenu(fileName = "SaveSlotDB", menuName = "Stats/SaveSlotDB")]
    public class SaveSlotDB : ScriptableObject
    {
        [SerializeField] List<SaveSlot> saves = new List<SaveSlot>();
        Dictionary<string, SaveRecord> saveLookup = null;

        public Dictionary<string, string> GetSaveRecord(string saveFile)
        {
            BuildLookup();

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
            BuildLookup();

            ISlotInfo slotInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<ISlotInfo>();
            var state = (Dictionary<string, string>)slotInfo.CaptureState();

            SaveRecord newRecord = new SaveRecord();

            newRecord.playerName = state["name"];
            newRecord.rank = state["rank"];
            newRecord.time = state["time"];
            newRecord.quest = state["quest"];
            newRecord.scene = state["scene"];

            bool saveFound = false;
            foreach (SaveSlot save in saves)
            {
                if(save.saveFile == saveFile)
                {
                    save.record = newRecord;
                    saveFound = true;
                }
            }
            if(!saveFound)
            {
                SaveSlot newSave = new SaveSlot();
                newSave.saveFile = saveFile;
                newSave.record = newRecord;
                saves.Add(newSave);
            }
        }

        private void BuildLookup()
        {
            saveLookup = new Dictionary<string, SaveRecord>();

            foreach (SaveSlot save in saves)
            {
                saveLookup[save.saveFile] = save.record;
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
