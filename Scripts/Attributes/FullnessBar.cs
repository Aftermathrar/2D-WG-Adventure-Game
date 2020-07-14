using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Attributes
{
    public class FullnessBar : MonoBehaviour
    {
        [SerializeField] Fullness fullness = null;
        [SerializeField] Image imgOverlay;
        [SerializeField] TextMeshProUGUI fullnessText;

        private void OnEnable() 
        {
            if(fullness == null)
            {
                GameObject followerGO = GameObject.FindGameObjectWithTag("Follower");

                // disable script updating if no follower found
                if (followerGO == null)
                {
                    gameObject.SetActive(false);
                    return;
                }

                fullness = followerGO.GetComponent<Fullness>();
            }
        }

        void Update()
        {
            if (fullnessText != null)
            {
                fullnessText.text = string.Format("{0:0}/{1,0}", fullness.GetAttributeValue(), fullness.GetMaxAttributeValue());
            }
            imgOverlay.fillAmount = fullness.GetFraction();
        }
    }
}
