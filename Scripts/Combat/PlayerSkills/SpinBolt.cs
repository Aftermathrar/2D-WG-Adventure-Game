using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Character;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Combat.Skills
{
    public class SpinBolt : AtkIconScript
    {
        AuraBlast auraBlast;
        // Add glyph component to player and initialize to check for rhk reset

        protected override void Awake()
        {
            thisButton = GetComponent<Button>();
            atkIcon = GetComponent<Image>().sprite;
            player = GetComponentInParent<PlayerController>();
            guard = player.GetComponent<GuardController>();
            mana = player.GetComponent<Mana>();
            fighter = player.GetComponent<Fighter>();
            attackValues = player.GetComponent<BaseAttackStats>();

            auraBlast = player.GetComponentInChildren<AuraBlast>();
        }

        protected override void UpdateTimeToHit()
        {
            timeSinceLastHit += Time.deltaTime;

            if (hitTimers.Count == 0)
            {
                fighter.activeAttack -= UpdateTimeToHit;
                return;
            }

            if (timeSinceLastHit >= maxTimeToHit)
            {
                hitTimers.Remove(hitTimers[0]);
                timeSinceLastHit = 0;
                // On deal damage, calculate current buffs and update next buffs

                float damage = CalculateDamage();
                if (target.TakeDamage(damage, isCrit))
                {
                    player.DamageDealt(damage);
                    CheckEffectActivation(true);

                    // Check for 60% rhk reset
                    if(currentHitCount == 0) CheckCooldownReset();

                    // Shake on final hit
                    if(currentHitCount + 1 == maxHitCount)
                    {
                        shaker = target.GetComponent<UIShake>();
                        StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude, shakeCount));
                    }
                }
                currentHitCount += 1;

                if (currentHitCount >= maxHitCount || hitTimers.Count == 0)
                {
                    fighter.activeAttack -= UpdateTimeToHit;
                }
                else
                {
                    timeSinceLastHit = 0;
                    for (int i = 0; i < hitTimers.Count; i++)
                    {
                        hitTimers[i].SetFighter(fighter);
                        hitTimers[i].SetMoveTime(.2f);
                    }

                    maxTimeToHit = GetStat(AttackStat.TimeToHit, currentHitCount);
                    float atkSpeed = fighter.GetStat(Stat.AttackSpeed);
                    atkSpeed = 1 / atkSpeed * 100;
                    maxTimeToHit *= atkSpeed;

                    hitTimers[0].SetFillTime(maxTimeToHit);
                }
            }
        }

        private void CheckCooldownReset()
        {
            int randResetValue = Random.Range(0,100);
            if(randResetValue < 60)
            {
                auraBlast.ResetCooldown();
            }
        }
    }
}
