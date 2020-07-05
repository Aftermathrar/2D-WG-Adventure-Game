using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Combat.Skills
{
    public class AuraBlast : AtkIconScript
    {
        public void ResetCooldown()
        {
            timeSinceAtkStart = Mathf.Infinity;
            timeSinceAtkOnCooldown = Mathf.Infinity;
            ResetButton();
        }
    }
}
