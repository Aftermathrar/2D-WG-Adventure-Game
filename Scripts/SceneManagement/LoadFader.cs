using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.SceneManagement
{
    public class LoadFader : MonoBehaviour
    {
        CanvasGroup canvasGroup;

        private void Awake() 
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
        }

        public void FadeInImmediate()
        {
            canvasGroup.alpha = 0;
        }
    }
}
