using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.EffectIcon
{
    public class EffectIconImage : MonoBehaviour
    {
        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI stackText;
        [SerializeField] Image timerOverlay;
        [SerializeField] TextMeshProUGUI timerText;

        public void SetIcon(Sprite sprite)
        {
            iconImage.overrideSprite = sprite;
            timerOverlay.overrideSprite = sprite;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.localPosition = pos;
        }

        public void UpdateStacks(string stack)
        {
            stackText.text = stack;
        }

        public void UpdateIconFill(float fillPercent, float t)
        {
            timerOverlay.fillAmount = fillPercent;
            string s = null;
            if(t >= 60)
            {
                s = "m";
                t /= 60;
            }
            timerText.text = string.Format("{0:0}{1}", Mathf.CeilToInt(t), s);
        }

        public IEnumerator MoveHorizontal(float xOffset)
        {
            Vector3 startPosition = transform.localPosition;
            Vector3 targetPosition = new Vector3(startPosition.x - xOffset, startPosition.y, startPosition.z);
            for(float t = 0; t < 0.2f; t+=Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t/0.2f);

                yield return null;
            }

            transform.localPosition = targetPosition;
            yield return null;
        }
    }
}
