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

        private void OnEnable() 
        {
            if(mana == null)
            {
                GameObject followerGO = GameObject.FindGameObjectWithTag("Follower");

                // disable script updating if no follower found
                if (followerGO == null)
                {
                    gameObject.SetActive(false);
                    return;
                }

                mana = followerGO.GetComponent<Mana>();
            }
        }

        void Update()
        {
            if (manaText != null)
            {
                manaText.text = string.Format("{0:0}/{1,0}", mana.GetAttributeValue(), mana.GetMaxAttributeValue());
            }
            imgOverlay.fillAmount = mana.GetFraction();
        }
    }
}