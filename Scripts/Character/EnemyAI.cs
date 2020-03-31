using System.Collections;
using System.Collections.Generic;
using ButtonGame.Combat;
using ButtonGame.Core;
using TMPro;
using UnityEngine;
using System;
using ButtonGame.Resources;
using ButtonGame.Stats;

namespace ButtonGame.Character
{
    public class EnemyAI : MonoBehaviour, IAction
    {
        [SerializeField] TextMeshProUGUI statusText = null;
        [SerializeField] EnemyController enemy;
        // [SerializeField] MonsterAttackDB MonsterAttackDB;
        [SerializeField] RectTransform overlay = null;
        BaseStats baseStats;
        Fighter fighter;
        
        float timeSinceLastAttack = 0;
        float timeSinceLastHit = Mathf.Infinity;
        int currentHitCount;
        float maxCooldown;
        float maxTimeToHit;
        float idleTimeBetweenHits;
        float atkLeadTime;
        float maxHitCount;

        [SerializeField] Health target;
        [SerializeField] HitTimer hitTimer;
        List<HitTimer> hitTimers = new List<HitTimer>();

        private void Awake() 
        {
            baseStats = GetComponent<BaseStats>();
            fighter = GetComponentInParent<Fighter>();
        }

        private void Start()
        {
            timeSinceLastAttack = 0;
            maxCooldown = 5f;
            maxHitCount = 3f;
            maxTimeToHit = 1f;
            idleTimeBetweenHits = 2f;
            atkLeadTime = 1f;
            ResetStatus();
        }

        private void Update() 
        {
            timeSinceLastAttack += Time.deltaTime;

            if (timeSinceLastAttack >= idleTimeBetweenHits) 
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if (enemy.CanAttack())
            {
                enemy.startAttack(maxCooldown);
                enemy.SetNewStatus("Attacking...");
            }
            if (IsAttackReady())
            {
                timeSinceLastAttack = 0 - maxCooldown;
                timeSinceLastHit = 0;
                currentHitCount = 0;
                GetComponentInParent<ActionScheduler>().StartAction(this);
                // fighter.activeAttack += UpdateTimer;
                fighter.activeAttack += UpdateTimeToHit;

                for (int i = 0; i < maxHitCount; i++)
                {
                    float newPosX = -130 + (i * 50);
                    HitTimer myObjectInstance = Instantiate(hitTimer, new Vector3(newPosX, 20, 0), Quaternion.identity);
                    myObjectInstance.transform.SetParent(enemy.transform, false);
                    hitTimers.Add(myObjectInstance);

                    myObjectInstance.SetFillTime(maxTimeToHit, i);
                }
            }
        }

        private bool IsAttackReady()
        {
            if (timeSinceLastAttack >= atkLeadTime)
            {
                return true;
            }
            return false;
        }

        private void ResetStatus()
        {
            statusText.text = "Idle"; // attackDB.GetAttackStat(AttackStat.Name, attackType);
            overlay.localScale = new Vector3(0, 1, 1);
            // fighter.activeAttack -= UpdateTimer;
        }

        // public void UpdateTimer()
        // {
        //     if (timeSinceLastAttack >= maxCooldown)
        //     {
        //         ResetStatus();
        //     }
        //     else
        //     {
        //         statusText.text = string.Format("{0:0.000}", maxCooldown - timeSinceLastAttack);
        //         float overlayPercent = Mathf.Clamp01((maxCooldown - timeSinceLastAttack) / maxCooldown);
        //         overlay.localScale = new Vector3(overlayPercent, 1, 1);
        //     }
        // }

        public void UpdateTimeToHit()
        {
            timeSinceLastHit += Time.deltaTime;

            float maskPercent = 0;
            maskPercent = Mathf.Clamp01((maxTimeToHit - timeSinceLastHit) / maxTimeToHit);

            if (timeSinceLastHit >= maxTimeToHit)
            {
                hitTimers.Remove(hitTimers[0]);
                currentHitCount += 1;
                timeSinceLastHit = 0;
                target.TakeDamage(10f);

                if (currentHitCount >= maxHitCount)
                {
                    fighter.activeAttack -= UpdateTimeToHit;
                }
                else
                {
                    timeSinceLastHit = 0;
                    for (int i = 0; i < hitTimers.Count; i++)
                    {
                        hitTimers[i].SetMoveTime(.2f);
                        hitTimers[i].SetFillTime(maxTimeToHit, i);
                    }
                }
            }
        }

        // private float CalculateDamage()
        // {
        //     float total = 0;
        //     float AtkPower = baseStats.GetStat(Stat.Attack);
        //     AtkPower = AtkPower * GetStatArray(AttackStat.Power)[currentHitCount];

        //     float enemyDef = target.GetComponentInParent<BaseStats>().GetStat(Stat.Defense);

        //     total = Mathf.Ceil(AtkPower / enemyDef);

        //     // if (CalculateCriticalHit())
        //     // {
        //     //     float critPower = 1 + baseStats.GetStat(Stat.CritDamage) / 100;
        //     //     total = total * critPower;
        //     // }

        //     return total;
        // }

        // public float[] GetStatArray(AttackStat stat)
        // {
        //     string[] atkStats = null;
        //     atkStats = attackDB.GetAttackStat(stat, attackType);
        //     float[] atkFloats = new float[atkStats.Length];
        //     for (int i = 0; i < atkStats.Length; i++)
        //     {
        //         atkFloats[i] = float.Parse(atkStats[i]);
        //     }
        //     return atkFloats;
        // }

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