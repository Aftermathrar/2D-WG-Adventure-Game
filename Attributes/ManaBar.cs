using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ButtonGame.Attributes
{
    public class ManaBar : MonoBehaviour
    {
        [SerializeField] Mana mana = null;
        [SerializeField] Image imgOverlay;
        [SerializeField] TextMeshProUGUI manaText;

        void Update()
        {
            if (manaText != null)
            {
                manaText.text = string.Format("{0:0}/{1,0}", mana.GetMana(), mana.GetMaxMana());
            }
            imgOverlay.fillAmount = mana.GetFraction();
        }
    }
}