using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ButtonGame.Core
{
    public class IntroFader : MonoBehaviour
    {
        [SerializeField] CanvasGroup background = null;
        [SerializeField] Image bgImage = null;
        [SerializeField] Text readyText = null;
        [SerializeField] TextMeshProUGUI cdText = null;
        [SerializeField] Image cdOverlay = null;

        private void OnEnable()
        {
            background.alpha = 1;
            cdOverlay.fillAmount = 1f;
            readyText.text = "READY";
            bgImage.enabled = true;
        }

        public IEnumerator BattleIntro(float t)
        {
            cdText.text = t.ToString();
            float cdTime = 0;

            do
            {
                cdText.text = string.Format("{0:0.000}", t - cdTime);
                cdOverlay.fillAmount = 1 - Mathf.Clamp01(cdTime/t);
                cdTime += Time.deltaTime;
                yield return null;
            } while (cdTime < t);

            cdText.text = null;
            cdOverlay.fillAmount = 0;
            readyText.text = "GO!!";
            bgImage.enabled = false;
        }

        public IEnumerator FadeIntroOverlay()
        {
            background.alpha = 1;

            while (background.alpha > 0)
            {
                background.alpha -= Time.deltaTime / 0.5f;
                yield return null;
            }
            yield return null;
        }
    }
}
