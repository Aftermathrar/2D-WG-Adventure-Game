using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Menus
{
    public class TravelSlotUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI locationText;
        Button button;

        private void Awake() 
        {
            button = GetComponent<Button>();
        }

        public void SetSlotInfo(string locationEnum, string locationName, bool isCombat)
        {
            locationText.text = locationName;
        }
    }
}
