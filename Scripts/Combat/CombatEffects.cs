using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using ButtonGame.UI.EffectIcon;
using UnityEngine;
using ButtonGame.Saving;

namespace ButtonGame.Combat
{
    public class CombatEffects : MonoBehaviour, ISaveable, IEffectProvider, IAttackEffectProvider, IMonAtkEffectProvider
    {
        [SerializeField] EffectDB effectDB;
        [SerializeField] EffectIconDB fxIconDB;
        [SerializeField] EffectIconSpawner fxIconSpawner;

        BaseStats selfStats;

        Health selfHealth;
        Health targetHealth;
        Mana selfMana;

        EffectStat fxStat;
        EffectName fxName;
        string fxID;
        int fxIconCount;
        bool isBattleActive;
        
        Dictionary<string, float[]> buffList = new Dictionary<string, float[]>();
        //[SerializeField] List<Sprite> fxIcons = new List<Sprite>();
        Sprite fxIcon = null;

        private void Awake() 
        {
            selfStats = GetComponent<BaseStats>();
            selfHealth = GetComponent<Health>();
        }

        private void Start()
        {
            if (gameObject.tag == "Player")
            {
                selfMana = GetComponent<Mana>();
            }
        }

        private void Update() 
        {
            if (isBattleActive && buffList.Count > 0) 
                BuffTimer();
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

        public void BuffSelf(string ID)
        {
            fxID = ID;
            fxName = (EffectName)Enum.Parse(typeof(EffectName), ID);
            string buffType = effectDB.GetEffectStat(EffectStat.EffectType, fxName);
            switch (buffType)
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

        public void DebuffTarget(string ID)
        {
            if (targetHealth == null) { return; }

            CombatEffects enemyEffects = targetHealth.GetComponent<CombatEffects>();
            enemyEffects.BuffSelf(ID);
        }

        private void BuffStatOverTime()
        {
            float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
            if (buffDuration == 0)
            {
                if (effectDB.GetEffectStat(EffectStat.StatsAffected, fxName) == "Mana")
                {
                    selfMana.GainMana(float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName)));
                }
                else
                {
                    float healValue = float.Parse(effectDB.GetEffectStat(EffectStat.EffectValues, fxName));
                    if (int.Parse(effectDB.GetEffectStat(EffectStat.Additive, fxName)) == 0)
                    {
                        healValue *= selfHealth.GetMaxHealthPoints() / 100;
                    }

                    if (healValue < 0)
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
                        fxValResult = (fxValue / 100) * selfMana.GetMaxMana();
                    }
                    selfMana.GainMana(fxValResult);
                }
                else
                {
                    if (isPercent)
                    {
                        fxValResult = (fxValue / 100) * selfHealth.GetMaxHealthPoints();
                    }
                    if (fxValResult >= 0)
                    {
                        selfHealth.GainHealth(fxValResult);
                    }
                    else
                    {
                        selfHealth.TakeDamage(Mathf.Abs(fxValResult), false, false);
                    }
                }
                yield return new WaitForSeconds(tickRate);
            } while(buffList[fxNameOverTime][1] <= buffDuration && isBattleActive);

            List<string> removeIDs = new List<string>();
            removeIDs.Add(fxNameOverTime);
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
            if (buffList.ContainsKey(fxID))
            {
                fxStacks = buffList[fxID][0];
                curFXTime = buffList[fxID][1];
            }
            else
            {
                int iconSelect = int.Parse(effectDB.GetEffectStat(EffectStat.Icon, fxName));
                if (iconSelect > 0)
                {
                    fxIconSpawner.Spawn(fxID.ToString(), fxIconCount, fxIconDB.GetSprite(fxName));
                    fxIconCount += 1;
                }
            }

            if (fxStacks < maxStacks)
            {
                fxStacks += 1;
                if (maxStacks > 1)
                {
                    fxIconSpawner.UpdateStacks(fxID, fxStacks);
                }
            }
            if (int.Parse(effectDB.GetEffectStat(EffectStat.Refreshable, fxName)) == 1)
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
                    float timeRemaining = fxDuration - buffList[id][1];
                    fxIconSpawner.UpdateIconFill(id, fillPercent, timeRemaining);
                }
            }
            RemoveBuffs(removeIDs);
        }

        private void RestoreIcons()
        {
            if(buffList.Count == 0) return;

            fxIconCount = 0;
            foreach (string id in buffList.Keys)
            {
                fxName = (EffectName)Enum.Parse(typeof(EffectName), id);
                fxIconSpawner.Spawn(id, fxIconCount, fxIconDB.GetSprite(fxName));
                float fxDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
                float fillPercent = buffList[id][1] / fxDuration;
                float timeRemaining = fxDuration - buffList[id][1];
                fxIconSpawner.UpdateIconFill(id, fillPercent, timeRemaining);

                if(effectDB.GetEffectStat(EffectStat.EffectType, fxName) == "Stat Over Time")
                {
                    StartCoroutine(BuffOverTime(fxDuration));
                }

                fxIconCount += 1;
            }
        }

        private void RemoveBuffs(List<string> removeIDs)
        {
            foreach (string id in removeIDs)
            {
                if(buffList.ContainsKey(id))
                {
                    fxIconCount -= 1;
                    buffList.Remove(id);
                    fxIconSpawner.Destroy(id);
                }
            }
        }

        public Health GetTarget()
        {
            return targetHealth;
        }

        public IEnumerable<float[]> GetStatEffectModifiers(Stat stat)
        {
            if (buffList.Count == 0) yield return new float[] {0, 0};

            float[] result = new float[] {0, 0};
            List<string> removeIDs = new List<string>();
            //Run through each active buff
            foreach (string id in buffList.Keys)
            {
                EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
                // Check if effect type boosts Stats, otherwise desired EffectStats aren't assigned
                if (effectDB.GetEffectStat(EffectStat.EffectType, effect) != "Base Boost") continue;

                // Split Additive entry based for mixed base boosts
                string[] fxAdditiveData = effectDB.GetEffectStat(EffectStat.Additive, effect).Split(new char[] { ',' });

                for (int i = 0; i < fxAdditiveData.Length; i++)
                {
                    string[] fxBaseStats = effectDB.GetEffectStat(EffectStat.StatsAffected, effect).Split(new char[] { ',' });
                    // Compare the Effect stat to the provided stat
                    if (fxBaseStats[i] == stat.ToString())
                    {
                        // Check and assign value, then check if buff is consumed on activation
                        string[] fxValueData = effectDB.GetEffectStat(EffectStat.EffectValues, effect).Split(new char[] { ',' });
                        float fxVal = float.Parse(fxValueData[i]);
                        fxVal *= buffList[id][0];
                        // Store result in percentage or additive slot of array
                        result[int.Parse(fxAdditiveData[i])] += fxVal;
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

        public IEnumerable<float[]> GetAtkStatModifiers(AttackType atkType, AttackStat attackStat)
        {
            if (buffList.Count == 0) yield return new float[] {0, 0};

            float[] result = new float[] {0, 0};
            List<string> removeIDs = new List<string>();
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
            

            RemoveBuffs(removeIDs);

            yield return result;
        }

        public IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat)
        {
            if (buffList.Count == 0) yield return false;

            bool result = false;
            List<string> removeIDs = new List<string>();
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

            RemoveBuffs(removeIDs);

            yield return result;
        }

        public IEnumerable<float[]> GetMonAtkStatModifiers(MonAtkName atkName, MonAtkStat attackStat)
        {
            if (buffList.Count == 0) yield return new float[] { 0, 0 };

            float[] result = new float[] { 0, 0 };
            List<string> removeIDs = new List<string>();
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


            RemoveBuffs(removeIDs);

            yield return result;
        }

        public IEnumerable<bool> GetMonAtkBooleanModifiers(MonAtkName atkName, MonAtkStat attackStat)
        {
            if (buffList.Count == 0) yield return false;

            bool result = false;
            List<string> removeIDs = new List<string>();
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

            RemoveBuffs(removeIDs);

            yield return result;
        }

        public void StartBattle()
        {
            isBattleActive = true;
        }

        public void EndBattle()
        {
            isBattleActive = false;
        }

        public object CaptureState()
        {
            return buffList;
        }

        public void RestoreState(object state)
        {
            buffList = (Dictionary<string, float[]>)state;
            RestoreIcons();
        }
    }
}