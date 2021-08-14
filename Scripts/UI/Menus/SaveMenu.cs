using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] GameObject savePanel;
        [SerializeField] TextMeshProUGUI titleText;
        SaveFunction saveFunction = SaveFunction.Save;
        bool isSaving = false;

        public event Action onUpdateMenuFunction;

        internal enum SaveFunction
        {
            Save,
            Load,
            Delete
        }

        private void Awake() 
        {
            if(background == null)
            {
                background = GetComponent<Image>();
            }
        }

        public void Save()
        {
            background.enabled = true;
            titleText.text = "Save Game";
            saveFunction = SaveFunction.Save;
            isSaving = true;
            savePanel.SetActive(true);
        }

        public void Load()
        {
            background.enabled = true;
            titleText.text = "Load Game";
            saveFunction = SaveFunction.Load;
            isSaving = false;
            savePanel.SetActive(true);
        }

        public void Delete()
        {
            background.enabled = true;
            titleText.text = "Delete Saved Game";
            saveFunction = SaveFunction.Delete;
            savePanel.SetActive(true);
            onUpdateMenuFunction?.Invoke();
        }

        public void CancelDelete()
        {
            if(isSaving)
            {
                titleText.text = "Save Game";
                saveFunction = SaveFunction.Save;
            }
            else
            {
                titleText.text = "Load Game";
                saveFunction = SaveFunction.Load;
            }
            onUpdateMenuFunction?.Invoke();
        }

        internal SaveFunction GetSaveFunction()
        {
            return saveFunction;
        }

        internal bool IsSaving()
        {
            return isSaving;
        }

        public void CloseMenu()
        {
            background.enabled = false;
            savePanel.SetActive(false);
        }
    }
}
