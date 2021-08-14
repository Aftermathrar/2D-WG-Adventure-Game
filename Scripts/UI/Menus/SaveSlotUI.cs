using System.Collections;
using System.Collections.Generic;
using System.Text;
using ButtonGame.Saving;
using ButtonGame.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
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
        [SerializeField] Text locationText;
        
        SaveMenu saveMenu;
        Button button;
        
        int saveSlot;
        string slotName = "Save Slot ";
        string saveFile = "save";
        SaveMenu.SaveFunction isSaving = SaveMenu.SaveFunction.Save;
        string locationToLoad;
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
            SetSlotInfo();
        }

        private void SetSlotInfo()
        {
            Dictionary<string, string> infoLookup = saveSlotDB.GetSaveRecord(saveFile);
            if (infoLookup != null)
            {
                playerName.text = infoLookup["name"];
                rankText.text = infoLookup["rank"];
                timeText.text = infoLookup["time"];
                // locationText.text = infoLookup["quest"];
                locationToLoad = infoLookup["location"];
                locationText.text = locationToLoad;
                sceneToLoad = int.Parse(infoLookup["scene"]);
            }
            else
            {
                playerName.text = "";
                rankText.text = "";
                timeText.text = "";
                locationText.text = "";
                sceneToLoad = -1;
            }
            button.onClick.RemoveAllListeners();
            SetSlotFunction();

            saveMenu.onUpdateMenuFunction += SetSlotFunction;
        }

        public void SetSlotFunction()
        {
            button.onClick.RemoveAllListeners();
            isSaving = saveMenu.GetComponent<SaveMenu>().GetSaveFunction();
            if (isSaving == SaveMenu.SaveFunction.Save)
            {
                button.onClick.AddListener(() => Save());
            }
            else if(isSaving == SaveMenu.SaveFunction.Load)
            {
                button.onClick.AddListener(() => Load());
            }
            else if(isSaving == SaveMenu.SaveFunction.Delete)
            {
                button.onClick.AddListener(() => Delete());
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

            saveMenu.onUpdateMenuFunction -= SetSlotFunction;
            saveMenu.CloseMenu();
        }

        private void Load()
        {
            if(sceneToLoad < 0)
            {
                saveMenu.onUpdateMenuFunction -= SetSlotFunction;
                saveMenu.CloseMenu();
                return;
            }
            sceneChangeObj.SetDestination(locationToLoad);
            sceneChangeObj.SetSceneToLoad(sceneToLoad);
            sceneChangeObj.ChangeScene(saveFile);
        }

        private void Delete()
        {
            SavingWrapper savingWrapper = (SavingWrapper)GameObject.FindObjectOfType(typeof(SavingWrapper));
            if(savingWrapper != null)
            {
                saveSlotDB.RemoveSaveRecord(saveFile);
                savingWrapper.Delete(saveFile);
            }
            
            SetSlotInfo();
            saveMenu.CancelDelete();
        }
    }
}
