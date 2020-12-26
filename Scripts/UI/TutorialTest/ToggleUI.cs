using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ButtonGame.UI.TutorialTest
{
    public class ToggleUI : MonoBehaviour
    {
        [SerializeField] GameObject windowToToggle;
        [SerializeField] TextMeshProUGUI buttonLabel;
        [SerializeField] string activeText;
        [SerializeField] string inactiveText;

        public void ToggleWindow()
        {
            windowToToggle.SetActive(!windowToToggle.activeSelf);
            if(windowToToggle.activeSelf)
                buttonLabel.text = activeText;
            else
                buttonLabel.text = inactiveText;
        }
    }
}
