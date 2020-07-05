using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat.Skills
{
    public class Wrath : AtkIconScript
    {

        public override void StartCooldown()
        {
            timeSinceAtkStart += Time.deltaTime;
            if (timeSinceAtkStart >= cooldownDelay)
            {
                timeSinceAtkOnCooldown = 0;
                // Check for cdr on crit
                if (isCrit) CooldownReduction();
                atkCooldownText.gameObject.SetActive(true);
                fighter.activeAttack -= StartCooldown;
                fighter.activeAttack += UpdateTimer;
            }
        }

        private void CooldownReduction()
        {
            timeSinceAtkOnCooldown += 4f;
        }
    }
}
