using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Combat
{
    public class PlayerCombatEffects : CombatEffects, IAttackEffectProvider
    {
        Mana selfMana;

        private void Start()
        {
            if (gameObject.tag == "Player")
            {
                selfMana = GetComponent<Mana>();
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

        public IEnumerable<float[]> GetAtkStatModifiers(AttackType atkType, AttackStat attackStat)
        {
            if (buffList.Count == 0) yield return new float[] { 0, 0 };

            float[] result = new float[] { 0, 0 };
            //Run through each active buff
            foreach (string id in buffList.Keys)
            {
                EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);

                // Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
                if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
                {
                    // Skip if buff value is a bool
                    int modType = int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect));
                    if (modType == 2) continue;

                    // Get affected skill list and compare with the provided AtkType
                    string fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect);
                    if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkType.ToString()))
                    {
                        string fxAttackStats = effectDB.GetEffectStat(EffectStat.AtkStatsAffected, effect);
                        // Compare the Effect stat to the provided stat
                        if (fxAttackStats.Contains(attackStat.ToString()))
                        {
                            // Check and assign value, then check if buff is consumed on activation
                            result[modType] += float.Parse(effectDB.GetEffectStat(EffectStat.AtkEffectValues, effect));
                            if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1 && !removeIDs.Contains(id))
                            {
                                removeIDs.Add(id);
                            }
                        }
                    }
                }
            }

            yield return result;
        }

        public IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat)
        {
            if (buffList.Count == 0) yield return false;

            bool result = false;
            //Run through each active buff
            foreach (string id in buffList.Keys)
            {
                EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
                // Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
                if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
                {
                    // Skip if not a boolean buff value
                    if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 2) continue;

                    // Get affected skill list and compare with the provided AtkType
                    string fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect);
                    if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkType.ToString()))
                    {
                        string fxAttackStats = effectDB.GetEffectStat(EffectStat.AtkStatsAffected, effect);
                        // Compare the Effect stat to the provided stat
                        if (fxAttackStats.Contains(attackStat.ToString()))
                        {
                            // Check and assign value, then check if buff is consumed on activation
                            if (effectDB.GetEffectStat(EffectStat.AtkEffectValues, effect).ToLower() == "true")
                            {
                                result = true;
                            }
                            if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                            {
                                removeIDs.Add(id);
                            }
                        }
                    }
                }
            }

            yield return result;
        }
    }
}
