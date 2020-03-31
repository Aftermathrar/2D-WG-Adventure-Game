using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.EffectIcon
{
    public class EffectIconImage : MonoBehaviour
    {
        public void SetIcon(Sprite sprite)
        {
            Image iconImage = GetComponent<Image>();
            iconImage.overrideSprite = sprite;
        }
    }
}
