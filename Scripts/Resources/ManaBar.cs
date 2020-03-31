using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ButtonGame.Resources
{
    public class ManaBar : MonoBehaviour
    {
        [SerializeField] Mana mana = null;
        [SerializeField] RectTransform foreground;
        [SerializeField] TextMeshProUGUI manaText;

        void Update()
        {
            if (manaText != null)
            {
                manaText.text = string.Format("{0:0}/{1,0}", mana.GetMana(), mana.GetMaxMana());
            }
            foreground.localScale = new Vector3(mana.GetFraction(), 1, 1);
        }
    }
}