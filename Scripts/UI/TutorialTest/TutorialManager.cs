using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButtonGame.UI.TutorialTest
{
    public class TutorialManager : MonoBehaviour, IPointerClickHandler
    {
        // Display Objects
        [SerializeField] Image currentImage;
        [SerializeField] TextMeshProUGUI currentText;
        [SerializeField] TextMeshProUGUI pageText;
        [SerializeField] TextMeshProUGUI titleText;

        // Tutorial info list
        [SerializeField] [TextArea] List<string> tutorialMessages;
        [SerializeField] List<Sprite> tutorialImages;
        [SerializeField] [TextArea] string[] tutorialTitles;
        int currentIndex = 0;

        private void OnEnable() 
        {
            currentIndex = 0;
            UpdateTutorialSlide();
        }

        private void UpdateTutorialSlide()
        {
            currentImage.sprite = tutorialImages[currentIndex];
            currentText.text = tutorialMessages[currentIndex];
            titleText.text = tutorialTitles[currentIndex];
            pageText.text = "Page " + (currentIndex + 1) + " of 13";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.pointerId == -2)
                currentIndex = Mathf.Max(0, currentIndex - 1);
            else
                currentIndex++;


            if(currentIndex < tutorialMessages.Count)
            {
                UpdateTutorialSlide();
                return;
            }

            transform.parent.gameObject.SetActive(false);
        }
    }
}
