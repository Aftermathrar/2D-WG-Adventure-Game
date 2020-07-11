using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using ButtonGame.UI.EffectIcon;
using UnityEngine;
using ButtonGame.Saving;
using UnityEngine.Events;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Combat
{
    public abstract class CombatEffects : MonoBehaviour, ISaveable, IStatModifier
    {
        [SerializeField] protected EffectDB effectDB;
        [SerializeField] EffectIconDB fxIconDB;
        [SerializeField] EffectIconSpawner fxIconSpawner;

        protected BaseStats selfStats;
        protected Health selfHealth;
        protected Health targetHealth;

        protected EffectStat fxStat;
        protected EffectName fxName;
        protected string fxID;
        int fxIconCount;
        protected bool isBattleActive;

        protected Dictionary<string, float[]> buffList = new Dictionary<string, float[]>();
        protected Dictionary<string, Coroutine> coBuffList = new Dictionary<string, Coroutine>();
        protected List<string> removeIDs = new List<string>();
        Sprite fxIcon = null;

        public UnityEvent EffectsUpdated;

        private void Awake() 
        {
            selfStats = GetComponent<BaseStats>();
            selfHealth = GetComponent<Health>();
        }

        private void Update() 
        {
            if (isBattleActive && buffList.Count > 0) 
                BuffTimer();
        }

        private void LateUpdate() {
            if(removeIDs.Count > 0)
                RemoveBuffs();
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
            if(!isBattleActive) return;
            
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

        public virtual void DebuffTarget(string ID)
        {
            if (targetHealth == null) { return; }

            CombatEffects enemyEffects = targetHealth.GetComponent<CombatEffects>();
            enemyEffects.BuffSelf(ID);
        }

        protected virtual void BuffStatOverTime()
        {
            float buffDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
            if (buffDuration == 0)
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
                return;
            }

            bool isCoRoutineActive = buffList.ContainsKey(fxID);
            float[] currentFXInfo = GetCurrentFXInfo();
            buffList[fxID] = currentFXInfo;
            if (!isCoRoutineActive)
            {
                coBuffList[fxID] = StartCoroutine(BuffOverTime(buffDuration));
            }
        }

        protected virtual IEnumerator BuffOverTime(float buffDuration)
        {
            float tickRate = float.Parse(effectDB.GetEffectStat(EffectStat.TickRate, fxName));
            string[] stat = effectDB.GetEffectStat(EffectStat.StatsAffected, fxName).Split(new char[] { ',' });
            string[] fxAdditiveData = effectDB.GetEffectStat(EffectStat.Additive, fxName).Split(new char[] { ',' });
            string[] fxValueData = effectDB.GetEffectStat(EffectStat.EffectValues, fxName).Split(new char[] { ',' });
            string fxNameOverTime = fxName.ToString();
            do
            {
                for (int i = 0; i < fxAdditiveData.Length; i++)
                {
                    bool isPercent = int.Parse(fxAdditiveData[i]) == 0;
                    float fxValue = float.Parse(fxValueData[i]);
                    // Effect strength times stack count
                    float fxValResult = fxValue * buffList[fxNameOverTime][0];
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
            } while(buffList[fxNameOverTime][1] <= buffDuration && isBattleActive);

            removeIDs.Add(fxNameOverTime);

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
            EffectsUpdated.Invoke();
        }

        protected float[] GetCurrentFXInfo()
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
                fxIconSpawner.Spawn(fxID.ToString(), fxIconCount, fxIconDB.GetSprite(fxName));
                fxIconCount += 1;
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
                    coBuffList[fxID] = StartCoroutine(BuffOverTime(fxDuration));
                }

                fxIconCount += 1;
            }
        }

        private void RemoveBuffs()
        {
            foreach (string id in removeIDs)
            {
                if(buffList.ContainsKey(id))
                {
                    fxIconCount -= 1;
                    buffList.Remove(id);
                    fxIconSpawner.ReturnToPool(id);

                    if(coBuffList.ContainsKey(id)) 
                        StopCoroutine(coBuffList[id]);
                }
            }

            removeIDs.Clear();
            EffectsUpdated.Invoke();
        }

        private void RemoveTemporaryBuffs()
        {
            foreach (var key in buffList.Keys)
            {
                fxName = (EffectName)Enum.Parse(typeof(EffectName), key);
                if(float.Parse(effectDB.GetEffectStat(EffectStat.Persistent, fxName)) < 1)
                {
                    removeIDs.Add(key);
                }
            }
        }

        public Health GetTarget()
        {
            return targetHealth;
        }

        public Dictionary<string, float[]> GetBuffDictionary()
        {
            return buffList;
        }

        public bool HasEffect(string ID)
        {
            return buffList.ContainsKey(ID);
        }

        public float GetEffectElapsedTime(string ID)
        {
            if(buffList.ContainsKey(ID))
            {
                return buffList[ID][1];
            }
            return 0;
        }

        public void ClearEffect(string buffID)
        {
            removeIDs.Add(buffID);
        }

        public float[] GetStatEffectModifiers(Stat stat)
        {
            if (buffList.Count == 0) return new float[] {0, 0};

            float[] result = new float[] {0, 0};
            //Run through each active buff
            foreach (string id in buffList.Keys)
            {
                EffectName effect = (EffectName)Enum.Parse(typeof(EffectName), id);
                // Check if effect type boosts Stats, otherwise desired EffectStats aren't assigned
                if (effectDB.GetEffectStat(EffectStat.EffectType, effect) != "Base Boost") continue;

                // Split Additive entry based for mixed base boosts
                string[] fxAdditiveData = effectDB.GetEffectStat(EffectStat.Additive, effect).Split(new char[] { ',' });
                string[] fxBaseStats = effectDB.GetEffectStat(EffectStat.StatsAffected, effect).Split(new char[] { ',' });
                string[] fxValueData = effectDB.GetEffectStat(EffectStat.EffectValues, effect).Split(new char[] { ',' });

                for (int i = 0; i < fxAdditiveData.Length; i++)
                {
                    // Compare the Effect stat to the provided stat
                    if (fxBaseStats[i] == stat.ToString())
                    {
                        // Check and assign value, then check if buff is consumed on activation
                        float fxVal = float.Parse(fxValueData[i]);
                        fxVal *= buffList[id][0];
                        // Store result in percentage or additive slot of array
                        result[int.Parse(fxAdditiveData[i])] += fxVal;
                        if (int.Parse(effectDB.GetEffectStat(EffectStat.Consumed, effect)) == 1)
                        {
                            removeIDs.Add(id);
                        }
                        break;
                    }
                }
            }

            return result;
        }

        public void StartBattle()
        {
            isBattleActive = true;
        }

        public void EndBattle()
        {
            isBattleActive = false;
            StopAllCoroutines();
            RemoveTemporaryBuffs();
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