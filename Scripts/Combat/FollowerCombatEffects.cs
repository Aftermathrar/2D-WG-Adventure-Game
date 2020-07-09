using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class FollowerCombatEffects : CombatEffects
    {
        CombatEffects playerEffects;
        Mana selfMana;

        private void Start()
        {
            if (gameObject.tag == "Follower")
            {
                selfMana = GetComponent<Mana>();
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if(player != null)
                {
                    playerEffects = player.GetComponent<CombatEffects>();
                }
            }
        }

        protected override void BuffStatOverTime()
        {
            float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
            if (buffDuration == 0)
            {
                if (effectDB.GetEffectStat(EffectStat.StatsAffected, fxName) == "Mana")
                {
                    selfMana.GainAttribute(float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName)));
                }
                else
                {
                    float healValue = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName));
                    if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, fxName)) == 0)
                    {
                        healValue *= selfHealth.GetMaxAttributeValue() / 100;
                    }

                    if (healValue < 0)
                    {
                        selfHealth.TakeDamage(Mathf.Abs(healValue), false, false);
                    }
                    else
                    {
                        selfHealth.GainAttribute(healValue);
                    }
                }
                return;
            }

            bool isCoRoutineActive = buffList.ContainsKey(fxID);
            float[] currentFXInfo = GetCurrentFXInfo();
            buffList[fxID] = currentFXInfo;
            if (!isCoRoutineActive)
            {
                StartCoroutine(BuffOverTime(buffDuration));
            }
        }

        protected override IEnumerator BuffOverTime(float buffDuration)
        {
            float tickRate = float.Parse(effectDB.GetEffectStat(EffectStat.TickRate, fxName));
            string stat = effectDB.GetEffectStat(EffectStat.StatsAffected, fxName);
            bool isPercent = int.Parse(effectDB.GetEffectStat(EffectStat.Additive, fxName)) == 0;
            float fxValue = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName));
            string fxNameOverTime = fxName.ToString();
            do
            {
                // Effect strength times stack count
                float fxValResult = fxValue * buffList[fxNameOverTime][0];
                if (stat == "Mana")
                {
                    if (isPercent)
                    {
                        fxValResult = (fxValue / 100) * selfMana.GetMaxAttributeValue();
                    }
                    selfMana.GainAttribute(fxValResult);
                }
                else
                {
                    if (isPercent)
                    {
                        fxValResult = (fxValue / 100) * selfHealth.GetMaxAttributeValue();
                    }
                    if (fxValResult >= 0)
                    {
                        selfHealth.GainAttribute(fxValResult);
                    }
                    else
                    {
                        selfHealth.TakeDamage(Mathf.Abs(fxValResult), false, false);
                    }
                }
                yield return new WaitForSeconds(tickRate);
            } while (buffList[fxNameOverTime][1] <= buffDuration && isBattleActive);

            removeIDs.Add(fxNameOverTime);
        }
    }
}
