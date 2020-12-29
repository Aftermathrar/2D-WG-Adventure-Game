using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.UI
{
    public class UIShake : MonoBehaviour
    {
        Vector3 originalPos = new Vector3();
        RectTransform m_RectTransform;

        private void Awake() 
        {
            m_RectTransform = GetComponent<RectTransform>();
        }

        private void Start() 
        {
            originalPos = m_RectTransform.anchoredPosition;
        }

        public IEnumerator Shake(float duration, float magnitude, int shakeCount)
        {
            float elapsedTime = 0;
            float randX = 0;
            float randY = 0;

            Vector3[] shakePositions = new Vector3[shakeCount];
            for (int i = 0; i < shakePositions.Length; i++)
            {
                randX = Random.Range(-1, 1) * magnitude + originalPos.x;
                randY = Random.Range(-0.5f, 0.5f) * magnitude + originalPos.y;
                shakePositions[i] = new Vector3(randX, randY, 0);
            }

            Vector3 shakePos = m_RectTransform.anchoredPosition;
            float shakeDuration = duration / shakePositions.Length / 2;
            for (int i = 0; i < shakePositions.Length; i++)
            {
                while(elapsedTime < shakeDuration)
                {
                    float shakePercent = elapsedTime / shakeDuration;
                    m_RectTransform.anchoredPosition = Vector3.Lerp(shakePos, shakePositions[i], shakePercent);
                    elapsedTime += Time.deltaTime;

                    yield return null;
                }
                elapsedTime = 0;
                shakePos = m_RectTransform.anchoredPosition;
            }

            shakeDuration = duration / 2;
            
            while(elapsedTime < shakeDuration)
            {
                float postShakePercent = elapsedTime / shakeDuration;
                m_RectTransform.anchoredPosition = Vector3.Lerp(shakePos, originalPos, postShakePercent);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
            m_RectTransform.anchoredPosition = originalPos;
        }
    }
}
