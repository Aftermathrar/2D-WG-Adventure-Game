using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButtonGame.UI.TutorialTest
{
    public class MenuClassDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] TextMeshProUGUI classDescription;
        public void OnPointerEnter(PointerEventData eventData)
        {
            classDescription.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            classDescription.enabled = false;
        }
    }
}
