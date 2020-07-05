using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health = null;
        [SerializeField] Image imgOverlay;
        [SerializeField] TextMeshProUGUI healthText;

        void Update()
        {
            if (healthText != null)
            {
                healthText.text = string.Format("{0:0}/{1,0}", health.GetAttributeValue(), health.GetMaxAttributeValue());
            }
            imgOverlay.fillAmount = health.GetFraction();
        }
    }
}
