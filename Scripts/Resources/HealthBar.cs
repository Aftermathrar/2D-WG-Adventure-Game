using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ButtonGame.Resources
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground;
        [SerializeField] TextMeshProUGUI healthText;

        void Update()
        {
            if (healthText != null)
            {
                healthText.text = string.Format("{0:0}/{1,0}", health.GetHealthPoints(), health.GetMaxHealthPoints());
            }
            foreground.localScale = new Vector3(health.GetFraction(), 1, 1);
        }
    }
}
