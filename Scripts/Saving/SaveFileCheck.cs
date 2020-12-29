using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Saving
{
    public class SaveFileCheck : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI buttonText;
        private void Start() 
        {
            string[] saveFiles = Directory.GetFiles(Application.persistentDataPath, "*.sav");
            if(saveFiles.Length == 0)
            {
                GetComponent<Button>().interactable = false;
                buttonText.text = "<color=#7A7A7A64>" + buttonText.text;
            }
        }
    }
}
