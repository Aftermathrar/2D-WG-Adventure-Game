using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace ButtonGame.Combat
{
    public class EnemyCombatEffects : CombatEffects, IMonAtkEffectProvider
    {
        [SerializeField] OnDebuffEvent debuffTarget;
        [System.Serializable]
        public class OnDebuffEvent : UnityEvent<string> { }

        public override void DebuffTarget(string ID)
        {
            if (targetHealth == null) { return; }

            CombatEffects enemyEffects = targetHealth.GetComponent<CombatEffects>();
            enemyEffects.BuffSelf(ID);
            debuffTarget.Invoke(ID);
        }

        public IEnumerable<float[]> GetMonAtkStatModifiers(MonAtkName atkName, MonAtkStat attackStat)
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
                    if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkName.ToString()))
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

        public IEnumerable<bool> GetMonAtkBooleanModifiers(MonAtkName atkName, MonAtkStat attackStat)
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
                    if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkName.ToString()))
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