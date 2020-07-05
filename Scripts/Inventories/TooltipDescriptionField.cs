using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [System.Serializable]
    public class TooltipDescriptionField
    {
        public bool hasIcon;
        public Sprite iconImage = null;
        [TextArea] public string description = null;
    }
}
