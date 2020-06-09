using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ButtonGame.Resources
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground;
        [SerializeField] TextMeshProUGUI barCount;
    }
}
