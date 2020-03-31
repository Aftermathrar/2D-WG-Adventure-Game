using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.EffectIcon
{
    public class EffectIconSpawner : MonoBehaviour
    {
        [SerializeField] EffectIconImage fxIcon = null;

        public void Spawn(int count, Sprite sprite)
        {
            EffectIconImage instance = Instantiate<EffectIconImage>(fxIcon, transform);
            instance.SetIcon(sprite);
        }
    }
}
