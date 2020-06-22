﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Combat;
using ButtonGame.Core;
using ButtonGame.Attributes;
using ButtonGame.Stats;
using TMPro;

namespace ButtonGame.Character
{
    public class EnemyAI : MonoBehaviour, IAction, IBattleState
    {
        [SerializeField] MonAtkDB monAtkDB;
        [SerializeField] MonAtkIconDB monAtkIconDB;
        [SerializeField] TextMeshProUGUI nameText;
        MonsterAttacks monAttacks;
        EnemyController enemy;
        MonsterAttackManager attackManager;
        BaseStats baseStats;
        Health health;
        Fighter fighter;
        
        MonAtkName curAtk;
        float timeSinceLastAttack = 0;
        float timeSinceAttackStarted = 0;
        float timeSinceLastHit = Mathf.Infinity;
        float idleTimeBetweenHits = Mathf.Infinity;
        float atkLeadTime = Mathf.Infinity;
        float totalAtkTime = Mathf.Infinity;
        int currentHitCount;
        float[] maxTimeToHit;
        float maxHitCount;
        bool isCrit = false;
        bool[] isEffectOnHit;
        string myName;

        [SerializeField] Health target;
        [SerializeField] HitTimer hitTimer;
        List<HitTimer> hitTimers = new List<HitTimer>();
        [SerializeField] Queue<MonAtkName> atkQueue = new Queue<MonAtkName>();

        private bool isBattleActive;

        private void Awake() 
        {
            enemy = GetComponent<EnemyController>();
            attackManager = GetComponent<MonsterAttackManager>();
            monAttacks = GetComponent<MonsterAttacks>();
            baseStats = GetComponent<BaseStats>();
            health = GetComponent<Health>();
            fighter = GetComponent<Fighter>();
        }

        private void Start()
        {
            AssignSkillVariables();
            StartIdle();
            enemy.enemyAttack += TryAttack;
        }

        public void SetTarget(PlayerController player)
        {
            target = player.GetComponent<Health>();
        }

        private void AssignSkillVariables()
        {
            myName = baseStats.GetStatText(Stat.Name) + " ";
            nameText.text = myName;
            timeSinceLastAttack = 0;
            idleTimeBetweenHits = baseStats.GetStat(Stat.IdleTime);
        }

        public void TryAttack()
        {
            if(!isBattleActive)
            {
                return;
            }
            timeSinceLastAttack += Time.deltaTime;
            MonAtkName triggerAtkName = attackManager.CheckHPTrigger(health.GetPercentage());
            if (triggerAtkName != MonAtkName.None)
            {
                atkQueue.Clear();
                atkQueue.Enqueue(triggerAtkName);
            }

            if (timeSinceLastAttack >= idleTimeBetweenHits && enemy.CanAttack())
            {
                if(atkQueue.Count == 0) 
                {
                    atkQueue = attackManager.GetAttackPattern();
                }
                else
                {
                    curAtk = atkQueue.Dequeue();
                    timeSinceAttackStarted = 0;
                    float atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
                    atkSpeed = 1 / atkSpeed * 100;
                    totalAtkTime = GetAttackStat(MonAtkStat.TotalTime, 0) * atkSpeed;
                    atkLeadTime = GetAttackStat(MonAtkStat.LeadTime, 0) * atkSpeed;
                    maxHitCount = GetAttackStat(MonAtkStat.HitCount, 0);
                    Sprite[] sprites = monAtkIconDB.GetSprite(curAtk);
                    GetComponentInParent<ActionScheduler>().StartAction(this);

                    if(maxHitCount >=1)
                    {
                        int maxEffectCount = monAtkDB.GetAttackStat(MonAtkStat.EffectID, curAtk).Length;
                        isEffectOnHit = new bool[maxEffectCount];
                        for (int i = 0; i < maxEffectCount; i++)
                        {
                            string[] fxApply = monAtkDB.GetAttackStat(MonAtkStat.ApplyEffect, curAtk);
                            isEffectOnHit[i] = fxApply[i] == "OnHit";
                        }

                        for (int i = 0; i < maxHitCount; i++)
                        {
                            float newPosX = -300 + (i * 80);
                            HitTimer instance = Instantiate(hitTimer, new Vector3(newPosX, -160, 0), Quaternion.identity);
                            instance.transform.SetParent(enemy.transform, false);
                            hitTimers.Add(instance);

                            instance.SetSprite(sprites[i]);

                            Color32 atkColor = new Color32(254, 195, 30, 255);
                            StartCoroutine(instance.EnemyBorderFill(atkLeadTime, atkColor));
                        }
                    }
                    // Set attack stats and start attack action
                    enemy.startAttack(totalAtkTime);
                    enemy.SetNewStatus(myName + GetAttackString(MonAtkStat.LeadText), atkLeadTime);
                    enemy.enemyAttack -= TryAttack;
                    enemy.enemyAttack += AttackLeadTime;
                }
            }
        }

        public void AttackLeadTime()
        {
            if (IsAttackReady())
            {
                enemy.enemyAttack -= AttackLeadTime;
                AttackAction();
            }
        }

        private void AttackAction()
        {
            timeSinceLastAttack = 0;
            timeSinceLastHit = 0;
            currentHitCount = 0;
            float atkSpeed = baseStats.GetStat(Stat.AttackSpeed);
            atkSpeed = 1 / atkSpeed * 100;

            CheckEffectActivation(false);

            if (GetAttackStat(MonAtkStat.InvulnTime, 0) > 0)
            {
                fighter.StartInvulnTime(GetAttackStat(MonAtkStat.InvulnTime, 0));
            }

            if(maxHitCount > 0)
            {
                maxTimeToHit = monAttacks.GetAttackStatArray(MonAtkStat.TimeToHit, curAtk);
                fighter.activeAttack += UpdateTimeToHit;
                string s = monAtkDB.GetAttackStat(MonAtkStat.AttackText, curAtk)[currentHitCount];
                enemy.SetNewStatus(myName + s, maxTimeToHit[0] * atkSpeed);
                maxTimeToHit[0] = maxTimeToHit[0] * atkSpeed;
                
                hitTimers[0].SetFighter(fighter);
                hitTimers[0].SetFillTime(maxTimeToHit[0]);
            }
        }

        private bool IsAttackReady()
        {
            if (timeSinceAttackStarted >= atkLeadTime)
            {
                return true;
            }
            timeSinceAttackStarted += Time.deltaTime;
            return false;
        }

        public void UpdateTimeToHit()
        {
            timeSinceLastHit += Time.deltaTime;

            if (timeSinceLastHit >= maxTimeToHit[currentHitCount])
            {
                hitTimers.Remove(hitTimers[0]);
                timeSinceLastHit = 0;

                float damage = 0;
                if(bool.Parse(monAtkDB.GetAttackStat(MonAtkStat.IsFlatDmg, curAtk)[currentHitCount]))
                {
                    damage = GetAttackStat(MonAtkStat.FlatDamage, currentHitCount);
                } 
                else
                {
                    damage = CalculateDamage();
                }
                bool isUnblockable = bool.Parse(monAtkDB.GetAttackStat(MonAtkStat.Unblockable, curAtk)[0].ToString());
                bool isHit = target.TakeDamage(damage, isCrit, !isUnblockable);
                if(isHit || isUnblockable) CheckEffectActivation(true);
                currentHitCount += 1;

                if (currentHitCount >= maxHitCount)
                {
                    fighter.activeAttack -= UpdateTimeToHit;
                    timeSinceLastAttack = 0;
                    enemy.enemyAttack += TryAttack;
                    StartIdle();
                }
                else
                {
                    timeSinceLastHit = 0;
                    for (int i = 0; i < hitTimers.Count; i++)
                    {
                        string s = monAtkDB.GetAttackStat(MonAtkStat.AttackText, curAtk)[currentHitCount];
                        enemy.SetNewStatus(myName + s, maxTimeToHit[currentHitCount]);
                        hitTimers[i].SetFighter(fighter);
                        hitTimers[i].SetMoveTime(.2f);
                    }
                    if(hitTimers.Count > 0)
                    {
                        hitTimers[0].SetFillTime(maxTimeToHit[currentHitCount]);
                    }
                }
            }
        }
        private float CalculateDamage()
        {
            float total = 0;
            float atkPower = baseStats.GetStat(Stat.AttackPower);
            // Debug.Log("Base Attack: " + atkPower);
            atkPower *= GetAttackStat(MonAtkStat.Power, currentHitCount);

            float enemyDef = target.GetComponentInParent<BaseStats>().GetStat(Stat.Defense);
            total = Mathf.Ceil(atkPower / enemyDef);

            if (CalculateCriticalHit())
            {
                // Monsters don't have crit damage stat, subbing in 2x damage for now
                float critPower = 2f; // baseStats.GetStat(Stat.CritDamage) / 100;
                total *= critPower;
            }

            return total;
        }

        private bool CalculateCriticalHit()
        {
            float baseCritChance = 0;
            isCrit = false;

            float skillCritMod = GetAttackStat(MonAtkStat.CritMod, currentHitCount);
            float critFactor = baseStats.GetStat(Stat.CritFactor);
            float targetCritResist = target.GetComponentInParent<BaseStats>().GetStat(Stat.CritResist);
            // Need glyph factor bonus to be added
            baseCritChance = (1.2f * skillCritMod * critFactor) / (10f * targetCritResist);

            if (baseCritChance >= 1)
            {
                isCrit = true;
            }

            float critChance = 0;
            // Add glyph multiplier bonus later
            float glyphBonusMult = 1f;

            critChance = baseCritChance + glyphBonusMult * baseCritChance * (1 - baseCritChance);
            if (critChance >= 1 || UnityEngine.Random.Range(0f, 1f) <= critChance)
            {
                isCrit = true;
            }

            return isCrit;
        }

        private void CheckEffectActivation(bool isHit)
        {
            string[] atkFXid = monAtkDB.GetAttackStat(MonAtkStat.EffectID, curAtk);
            string[] atkFXTarget = monAtkDB.GetAttackStat(MonAtkStat.EffectTarget, curAtk);
            for (int i = 0; i < isEffectOnHit.Length; i++)
            {
                if (isEffectOnHit[i] == isHit && i == currentHitCount)
                    fighter.PassEffect(atkFXid[i], atkFXTarget[i]);
            }
        }

        private float GetAttackStat(MonAtkStat stat, int i)
        {
            float value = monAttacks.GetAttackStat(stat, curAtk, i);
            return value;
        }

        private string GetAttackString(MonAtkStat stat)
        {
            return monAtkDB.GetAttackStat(stat, curAtk)[0];
        }

        private void StartIdle()
        {
            enemy.startIdle(idleTimeBetweenHits);
        }

        public void StartBattle()
        {
            isBattleActive = true;
            
            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            }
        }

        public void EndBattle()
        {
            isBattleActive = false;
            Cancel();
            if(target.IsDead())
            {
                enemy.SetNewStatus(myName + "is victorious!", 1f);
            } else
            {
                enemy.SetNewStatus(myName + "has died!", 1f);
            }
        }

        public void Cancel()
        {
            StopAttack();
        }

        private void StopAttack()
        {
            fighter.activeAttack -= UpdateTimeToHit;
            for (int i = 0; i < hitTimers.Count; i++)
            {
                hitTimers[i].Cancel();
            }
            hitTimers.Clear();
        }
    }
}