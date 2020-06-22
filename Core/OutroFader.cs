using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ButtonGame.Core
{
    public class OutroFader : MonoBehaviour
    {
        [SerializeField] CanvasGroup background = null;
        [SerializeField] TextMeshProUGUI clearText = null;

        private void OnEnable()
        {
            background.alpha = 0f;
            clearText.enabled = true;
        }

        public IEnumerator BattleOutro()
        {
            background.alpha = 0f;
            do
            {
                background.alpha += Time.unscaledDeltaTime / 0.5f;
                yield return null;
            } while (background.alpha < 1);
        }
    }
}
