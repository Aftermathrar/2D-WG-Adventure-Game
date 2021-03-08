using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class PlayerStringInput : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI inputText;
        [SerializeField] TextMeshProUGUI blinkText;
        [SerializeField] GameObject fileSelectPanel;
        [SerializeField] GameObject nameConfirmPanel;
        [SerializeField] TextMeshProUGUI nameConfirmText;
        [SerializeField] Button confirmButton;
        [SerializeField] PlayerInfo playerInfo;
        Coroutine blink;

        private void OnEnable()
        {
            blink = StartCoroutine(Blink());
        }

        private void OnDisable()
        {
            StopCoroutine(blink);
        }

        private void Update()
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Backspace
                {
                    if (inputText.text.Length != 0)
                    {
                        inputText.text = inputText.text.Substring(0, inputText.text.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    if(inputText.text.Length > 0)
                    {
                        var info = new Dictionary<string, string>();
                        info["name"] = inputText.text;
                        info["rank"] = "No Rank";
                        info["time"] = "Monday 6:00 AM";
                        info["quest"] = "Prologue";
                        info["location"] = "Tutorial";
                        info["scene"] = SceneManager.GetActiveScene().buildIndex.ToString();
                        
                        nameConfirmPanel.SetActive(true);
                        playerInfo.SetPlayerInfo(info);
                        nameConfirmText.text = inputText.text;
                        confirmButton.Select();
                        transform.parent.gameObject.SetActive(false);
                    }
                }
                else if (inputText.text.Length <= 20)
                {
                    inputText.text += c;
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelInput();
            }
            else if(Input.GetKeyDown(KeyCode.Delete))
            {
                inputText.text = "";
            }
        }

        private IEnumerator Blink()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.6f);
                blinkText.text = "<#FFFFFF00>|";
                yield return new WaitForSeconds(0.6f);
                blinkText.text = "<#FFFFFFFF>|";
            }
        }

        public void CancelInput()
        {
            fileSelectPanel.SetActive(true);
            transform.parent.gameObject.SetActive(false);
        }
    }
}
