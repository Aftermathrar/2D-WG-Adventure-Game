using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Character;
using ButtonGame.Resources;
using ButtonGame.Stats;
using UnityEngine;
using UnityEngine.Events;

public class CombatEffects : MonoBehaviour, IEffectProvider, IAttackEffectProvider
{
    [SerializeField] EffectDB effectDB;
    [SerializeField] PlayerController player;
    [SerializeField] EnemyController enemy;
    
    BaseStats selfStats;
    BaseStats targetStats;

    Health selfHealth;
    Health targetHealth;

    Mana selfMana;

    EffectStat fxStat;
    EffectName fxName;
    string fxID;
    int fxIconCount;

    Dictionary<string, float[]> buffList = new Dictionary<string, float[]>();
    Dictionary<string, float[]> debuffList = new Dictionary<string, float[]>();
    [SerializeField] List<Sprite> fxIcons = new List<Sprite>();

    [SerializeField] NewBuffEvent newBuff;

    [System.Serializable]
    public class NewBuffEvent : UnityEvent<int, Sprite> {}

    private void Awake()
    {
        selfStats = GetComponent<BaseStats>();
        selfHealth = GetComponent<Health>();
        if (gameObject.tag == "Player")
        {
            selfMana = GetComponent<Mana>();
            targetStats = enemy.GetComponent<BaseStats>();
            targetHealth = enemy.GetComponent<Health>();
        }
        else
        {
            targetStats = player.GetComponent<BaseStats>();
            targetHealth = player.GetComponent<Health>();
        }
    }

    // Set invulnerability time from attack
    public void ToggleInvuln()
    {
        selfHealth.ToggleInvuln();
    }

    // Receive and set effect
    // If effect has duration, add to timer array
    // Use timer to remove effects when expired

    public void BuffSelf(float ID)
    {
        if(ID == 0) return;

        fxID = ID.ToString();
        fxName = effectDB.GetEffectName(ID);
        string v = effectDB.GetEffectStat(EffectStat.EffectType, fxName);
        newBuff.Invoke(0, fxIcons[0]);
        switch (v)
        {
            case "Stat Over Time":
                BuffStatOverTime();
                break;
            case "Base Boost":
                BuffBaseStat();
                break;
            case "Atk Boost":
                BuffAttackStat();
                break;
            default:
                Debug.Log("Error with Effect " + fxName + " - ID: " + ID);
                break;
        }
    }

    public void DebuffTarget(float ID)
    {

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
                selfHealth.GainHealth(float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName)));
            }
        }
        // Set up coroutine to repeat stat gain over time
    }

    private void BuffBaseStat()
    {
        float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));

        if(buffDuration == 0)
        {
            // idk
        }

        // Set up collection to track effects active and reset duration if needed
        // Separate collection to track stacks
    }

    private void BuffAttackStat()
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

        if (fxStacks < maxStacks)
        {
            fxStacks += 1;
        }
        if(int.Parse(effectDB.GetEffectStat(EffectStat.Refreshable, fxName)) == 1)
        {
            curFXTime = 0;
        }

        float[] currentFXInfo = new float[2];
        currentFXInfo[0] = fxStacks;
        currentFXInfo[1] = curFXTime;

        buffList[fxID] = currentFXInfo;
    }

    public void BuffTimer()
    {
        if (buffList.Count == 0) return;

        EffectName effectName;
        List<string> removeIDs = new List<String>();
        foreach (string id in buffList.Keys)
        {
            buffList[id][1] += Time.deltaTime;
            
            effectName = effectDB.GetEffectName(float.Parse(id));
            if(buffList[id][1] >= float.Parse(effectDB.GetEffectStat(EffectStat.Duration, effectName)))
            {
                removeIDs.Add(id);
            }
        }
        foreach (string id in removeIDs)
        {
            buffList.Remove(id);
        }
    }

    public void DebuffTimer()
    {
        if (debuffList.Count == 0) return;

        EffectName effectName;
        foreach (string id in debuffList.Keys)
        {
            debuffList[id][1] += Time.deltaTime;

            effectName = effectDB.GetEffectName(float.Parse(id));
            if (debuffList[id][1] >= float.Parse(effectDB.GetEffectStat(EffectStat.Duration, effectName)))
            {
                debuffList.Remove(id);
            }
        }
    }

    public Health GetTarget()
    {
        return targetHealth;
    }

    public IEnumerable<float> GetAddivitiveModifiers(Stat stat)
    {
        // yield return float.Parse(effectDB.GetEffectStat(EffectStat.Description, fxName));
        yield return 0;
    }

    public IEnumerable<float> GetPercentageModifiers(Stat stat)
    {
        // yield return float.Parse(effectDB.GetEffectStat(EffectStat.ID, fxName));
        yield return 0;
    }

    public IEnumerable<float> GetAtkAddivitiveModifiers(AttackType atkType, AttackStat attackStat)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<float> GetAtkPercentageModifiers(AttackType atkType, AttackStat attackStat)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat)
    {
        if(buffList.Count == 0) yield return false;

        bool result = false;
        List<string> removeIDs = new List<string>();
        //Run through each active buff
        foreach (string id in buffList.Keys)
        {
            EffectName effect = effectDB.GetEffectName(float.Parse(id));
            //Check if effect type boosts Attack, otherwise desired EffectStats aren't assigned
            if(effectDB.GetEffectStat(EffectStat.EffectType, effect) == "Atk Boost")
            {
                // Get affected skill list and compare with the provided AtkType
                string[] fxAttackTypes = effectDB.GetEffectStat(EffectStat.AtkTypesAffected, effect).Split(',');
                foreach (string s in fxAttackTypes)
                {
                    if(s == atkType.ToString())
                    {   
                        // Compare the Effect stat to the provided stat
                        if(effectDB.GetEffectStat(EffectStat.AtkStatsAffected, effect) == attackStat.ToString())
                        {
                            // Check and assign value, then check if buff is consumed on activation
                            if (effectDB.GetEffectStat(EffectStat.AtkEffectValues, effect).ToLower() == "true")
                            {
                                result = true;
                                if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                                {
                                    removeIDs.Add(id);
                                }

                                yield return result;
                            }
                        }
                    }
                }
            }
        }
        foreach (string id in removeIDs)
        {
            buffList.Remove(id);
        }
        
        yield return result;
    }
}
