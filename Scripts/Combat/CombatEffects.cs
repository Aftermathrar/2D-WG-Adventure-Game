using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Character;
using ButtonGame.Resources;
using ButtonGame.Stats;
using ButtonGame.UI.EffectIcon;
using UnityEngine;

public class CombatEffects : MonoBehaviour, IEffectProvider, IAttackEffectProvider
{
    [SerializeField] EffectDB effectDB;
    [SerializeField] PlayerController player;
    [SerializeField] EnemyController enemy;
    [SerializeField] EffectIconSpawner fxIconSpawner;

    BaseStats selfStats;

    Health selfHealth;
    Health targetHealth;

    Mana selfMana;

    EffectStat fxStat;
    EffectName fxName;
    string fxID;
    int fxIconCount;
    Color32 fxIconColor = new Color32(148, 200, 112, 255);

    Dictionary<string, float[]> buffList = new Dictionary<string, float[]>();
    [SerializeField] List<Sprite> fxIcons = new List<Sprite>();

    private void Start() 
    {
        selfStats = GetComponent<BaseStats>();
        selfHealth = GetComponent<Health>();
        if (gameObject.tag == "Player")
        {
            selfMana = GetComponent<Mana>();
        }
    }

    public void SetTarget(GameObject target)
    {
        targetHealth = target.GetComponent<Health>();
    }

    // Set invulnerability time from attack
    public void ToggleInvuln()
    {
        selfHealth.ToggleInvuln();
    }

    public void BuffSelf(string ID, Color32 color)
    {
        fxID = ID;
        fxName = (EffectName)Enum.Parse(typeof(EffectName), ID);
        fxIconColor = color;
        string v = effectDB.GetEffectStat(EffectStat.EffectType, fxName);
        switch (v)
        {
            case "Stat Over Time":
                BuffStatOverTime();
                break;
            case "Base Boost":
                BuffStat();
                break;
            case "Atk Boost":
                BuffStat();
                break;
            default:
                Debug.Log("Error with Effect " + fxName + " - ID: " + ID);
                break;
        }
    }

    public void DebuffTarget(string ID, Color32 color)
    {
        if(targetHealth == null) { return; }

        CombatEffects enemyEffects = targetHealth.GetComponent<CombatEffects>();
        enemyEffects.BuffSelf(ID, color);
    }

    private void BuffStatOverTime()
    {
        float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
        if (buffDuration == 0)
        {
            if(effectDB.GetEffectStat(EffectStat.StatsAffected, fxName) == "Mana")
            {
                selfMana.GainMana(float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName)));
            }
            else
            {
                float healValue = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName));
                if(healValue < 0)
                {
                    selfHealth.TakeDamage(Mathf.Abs(healValue), false, false);
                }
                else
                {
                    selfHealth.GainHealth(healValue);
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

    private IEnumerator BuffOverTime(float buffDuration)
    {
        EffectName coEffect = fxName;
        float tickRate = float.Parse(effectDB.GetEffectStat(EffectStat.TickRate, fxName));
        string stat = effectDB.GetEffectStat(EffectStat.StatsAffected, fxName);
        bool isPercent = int.Parse(effectDB.GetEffectStat(EffectStat.Additive, fxName)) == 0;
        float fxValue = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName));
        do
        {
            // Effect strength times stack count
            float fxValResult = fxValue * buffList[coEffect.ToString()][0];
            if (stat == "Mana")
            {
                if(isPercent)
                {
                    fxValResult = (fxValue/100) * selfMana.GetMaxMana();
                }
                selfMana.GainMana(fxValResult);
            }
            else
            {
                if(isPercent)
                {
                    fxValResult = (fxValue/100) * selfHealth.GetMaxHealthPoints();
                }
                if(fxValResult >= 0)
                {
                    selfHealth.GainHealth(fxValResult);
                }
                else
                {
                    selfHealth.TakeDamage(Mathf.Abs(fxValResult), false, false);
                }
            }
            yield return new WaitForSeconds(tickRate);
        } while (buffList[coEffect.ToString()][1] <= buffDuration);

        List<string> removeIDs = new List<string>();
        removeIDs.Add(coEffect.ToString());
        RemoveBuffs(removeIDs);

        yield return null;
    }

    private void BuffStat()
    {
        float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));

        if (buffDuration == 0)
        {
            // idk
            Debug.Log(fxName.ToString() + " effect has duration of 0");
        }

        float[] currentFXInfo = GetCurrentFXInfo();

        buffList[fxID] = currentFXInfo;
    }

    private float[] GetCurrentFXInfo()
    {
        float fxStacks = 0;
        float curFXTime = 0;
        float maxStacks = float.Parse(effectDB.GetEffectStat(EffectStat.Stack, fxName));
        // Get stacks
        if(buffList.ContainsKey(fxID))
        {
            fxStacks = buffList[fxID][0];
            curFXTime = buffList[fxID][1];
        }
        else
        {
            int iconSelect = int.Parse(effectDB.GetEffectStat(EffectStat.Icon, fxName));
            if(iconSelect > 0)
            {
                fxIconSpawner.Spawn(fxID.ToString(), fxIconCount, fxIcons[iconSelect - 1], fxIconColor);
                fxIconCount += 1;
            }
        }

        if (fxStacks<maxStacks)
        {
            fxStacks += 1;
            if(maxStacks > 1)
            {
                fxIconSpawner.UpdateStacks(fxID, fxStacks);
            }
        }
        if(int.Parse(effectDB.GetEffectStat(EffectStat.Refreshable, fxName)) == 1)
        {
            curFXTime = 0;
        }

        float[] newFXInfo = new float[2];
        newFXInfo[0] = fxStacks;
        newFXInfo[1] = curFXTime;

        return newFXInfo;
    }

    public void BuffTimer()
    {
        if (buffList.Count == 0) return;

        EffectName effectName;
        List<string> removeIDs = new List<String>();
        foreach (string id in buffList.Keys)
        {
            buffList[id][1] += Time.deltaTime;
            
            effectName = (EffectName)Enum.Parse(typeof(EffectName), id);
            float fxDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, effectName));
            if (buffList[id][1] >= fxDuration && effectDB.GetEffectStat(EffectStat.EffectType, effectName) != "Stat Over Time")
            {
                removeIDs.Add(id);
            }
            else
            {
                float fillPercent = buffList[id][1] / fxDuration;
                fxIconSpawner.UpdateIconFill(id, fillPercent);
            }
        }
        RemoveBuffs(removeIDs);
    }

    private void RemoveBuffs(List<string> removeIDs)
    {
        foreach (string id in removeIDs)
        {
            fxIconCount -= 1;
            buffList.Remove(id);
            fxIconSpawner.Destroy(id);
        }
    }

    public Health GetTarget()
    {
        return targetHealth;
    }

    public IEnumerable<float> GetAddivitiveModifiers(Stat stat)
    {
        if (buffList.Count == 0) yield return 0;

        float result = 0;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
            // Skip if not a Additive buff value
            if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 1) continue;

            // Check if effect type boosts Stats, otherwise desired EffectStats aren't assigned
            if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Base Boost")
            {
                string fxBaseStats = effectDB.GetEffectStat(EffectStat.StatsAffected, effect);
                // Compare the Effect stat to the provided stat
                if (fxBaseStats.Contains(stat.ToString()))
                {
                    // Check and assign value, then check if buff is consumed on activation
                    float fxVal = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, effect));
                    fxVal *= buffList[id][0];
                    result += fxVal;
                    if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                    {
                        removeIDs.Add(id);
                    }
                }
            }
        }
        
        RemoveBuffs(removeIDs);

        yield return result;
    }

    public IEnumerable<float> GetPercentageModifiers(Stat stat)
    {
        if (buffList.Count == 0) yield return 0;

        float result = 0;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
            // Skip if not a Additive buff value
            if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 0) continue;

            // Check if effect type boosts Stats, otherwise desired EffectStats aren't assigned
            if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Base Boost")
            {
                string fxBaseStats = effectDB.GetEffectStat(EffectStat.StatsAffected, effect);
                // Compare the Effect stat to the provided stat
                if (fxBaseStats.Contains(stat.ToString()))
                {
                    // Check and assign value, then check if buff is consumed on activation
                    float fxVal = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, effect));
                    fxVal *= buffList[id][0];
                    result += fxVal;
                    if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                    {
                        removeIDs.Add(id);
                    }
                }
            }
        }

        RemoveBuffs(removeIDs);

        yield return result;
    }

    public IEnumerable<float> GetAtkAddivitiveModifiers(AttackType atkType, AttackStat attackStat)
    {
        if (buffList.Count == 0) yield return 0;

        float result = 0;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
            // Skip if not a Additive buff value
            if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 1) continue;

            // Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
            if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
            {
                // Get affected skill list and compare with the provided AtkType
                string fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect);
                if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkType.ToString()))
                {
                    string fxAttackStats = effectDB.GetEffectStat(EffectStat.AtkStatsAffected, effect);
                    // Compare the Effect stat to the provided stat
                    if (fxAttackStats.Contains(attackStat.ToString()))
                    {
                        // Check and assign value, then check if buff is consumed on activation
                        result += float.Parse(effectDB.GetEffectStat(EffectStat.AtkEffectValues, effect));
                        if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                        {
                            removeIDs.Add(id);
                        }
                    }
                }
            }
        }

        RemoveBuffs(removeIDs);

        yield return result;
    }

    public IEnumerable<float> GetAtkPercentageModifiers(AttackType atkType, AttackStat attackStat)
    {
        if (buffList.Count == 0) yield return 0;

        float result = 0;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
            // Skip if not a Percentage buff value
            if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 0) continue;

            // Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
            if (effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
            {
                // Get affected skill list and compare with the provided AtkType
                string fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect);
                if (fxAttackTypes == "All" || fxAttackTypes.Contains(atkType.ToString()))
                {
                    string fxAttackStats = effectDB.GetEffectStat(EffectStat.AtkStatsAffected, effect);
                    // Compare the Effect stat to the provided stat
                    if (fxAttackStats.Contains(attackStat.ToString()))
                    {
                        // Check and assign value, then check if buff is consumed on activation
                        result += float.Parse(effectDB.GetEffectStat(EffectStat.AtkEffectValues, effect));
                        if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                        {
                            removeIDs.Add(id);
                        }
                    }
                }
            }
        }

        RemoveBuffs(removeIDs);

        yield return result;
    }

    public IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat)
    {
        if(buffList.Count == 0) yield return false;

        bool result = false;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
            // Skip if not a boolean buff value
            if(int.Parse(effectDB.GetEffectStat(EffectStat.Additive, effect)) != 2) continue;

            // Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
            if(effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
            {
                // Get affected skill list and compare with the provided AtkType
                string fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect);
                if(fxAttackTypes == "All" || fxAttackTypes.Contains(atkType.ToString()))
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

        RemoveBuffs(removeIDs);
        
        yield return result;
    }
}
