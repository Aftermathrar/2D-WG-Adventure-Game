using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.SaveSystem
{
    public class SaveSlotUI : MonoBehaviour
    {
        [SerializeField] SaveSlotDB saveSlotDB;
        [SerializeField] ChangeSceneButton sceneChangeObj;
        [SerializeField] Image icon = null;
        [SerializeField] Text slotText;
        [SerializeField] Text playerName;
        [SerializeField] Text rankText;
        [SerializeField] Text timeText;
        [SerializeField] Text questText;
        
        SaveMenu saveMenu;
        Button button;
        
        int saveSlot;
        string slotName = "Save Slot ";
        string saveFile = "save";
        bool isSaving = false;
        int sceneToLoad = -1;

        private void Awake() 
        {
            button = GetComponent<Button>();
            saveSlot = transform.GetSiblingIndex();
            if(saveSlot == 0)
            {
                slotName = "Auto Save";
                saveFile = "auto";
            }
            else
            {
                slotName += saveSlot;
                saveFile += saveSlot;
            }
            slotText.text = slotName;
            saveMenu = GetComponentInParent<SaveMenu>();
        }

        private void OnEnable()
        {
            Dictionary<string, string> infoLookup = saveSlotDB.GetSaveRecord(saveFile);
            if(infoLookup != null)
            {
                playerName.text = infoLookup["name"];
                rankText.text = infoLookup["rank"];
                timeText.text = infoLookup["time"];
                questText.text = infoLookup["quest"];
                sceneToLoad = int.Parse(infoLookup["scene"]);
            }
            else
            {
                playerName.text = "";
                rankText.text = "";
                timeText.text = "";
                questText.text = "";
                sceneToLoad = -1;
            }
            button.onClick.RemoveAllListeners();
            SetSlotFunction();
        }

        public void SetSlotFunction()
        {
            isSaving = saveMenu.GetComponent<SaveMenu>().IsSaving();
            if (isSaving)
            {
                button.onClick.AddListener(() => Save());
            }
            else
            {
                button.onClick.AddListener(() => Load());
            }
        }

        private void Save()
        {
            SavingWrapper savingWrapper = (SavingWrapper)GameObject.FindObjectOfType(typeof(SavingWrapper));
            if (savingWrapper != null)
            {
                saveSlotDB.AddSaveRecord(saveFile);
                savingWrapper.Save(saveFile);
            }
            saveMenu.CloseMenu();
        }

        private void Load()
        {
            if(sceneToLoad < 0)
            {
                saveMenu.CloseMenu();
                return;
            }
            sceneChangeObj.SetSceneToLoad(sceneToLoad);
            sceneChangeObj.ChangeScene(saveFile);
        }
    }
}
