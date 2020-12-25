using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.SaveSystem
{
    public class SaveMenu : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] GameObject savePanel;
        [SerializeField] TextMeshProUGUI titleText;
        bool isSaving = true;

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
            isSaving = true;
            savePanel.SetActive(true);
        }

        public void Load()
        {
            background.enabled = true;
            titleText.text = "Load Game";
            isSaving = false;
            savePanel.SetActive(true);
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
