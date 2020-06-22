using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.UI.Inventories
{
    public class TooltipIconText : MonoBehaviour
    {
        [SerializeField] Image tooltipIcon;
        [SerializeField] Text tooltipText;

        public Image GetIcon()
        {
            return tooltipIcon;
        }

        public Text GetText()
        {
            return tooltipText;
        }

        public void DisableIcon()
        {
            tooltipIcon.gameObject.SetActive(false);
        }
    }
}
