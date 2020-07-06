using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Combat;
using ButtonGame.Core;
using ButtonGame.Inventories;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Character
{
    public class FollowerAI : MonoBehaviour, IAction, IBattleState
    {
        [SerializeField] FollowerAttackDB followerAttackDB = null;
        [SerializeField] FollowerAttackIconDB followerAttackIconDB = null;
        [SerializeField] EffectDB effectDB = null;
        [SerializeField] FollowerCombatLog combatLog = null;
        BaseStats baseStats;
        BaseFollowerAttackStats baseAttackStats;
        FollowerFighter fighter;
        Fullness fullness;
        Mana selfMana;
        Health playerHealth;
        Mana playerMana;
        Health targetHealth;
        Inventory inventory;
        IAttribute resourceType;
        [SerializeField] Image actBarOverlay = null;

        FollowerAttackName currentAttack;
        float attackSpeed;
        float cooldownReduction = 0f;
        float actionRecovery = 0f;
        float actionModifier = 0f;
        float digestSpeed = 0f;
        float manaConversion = 0f;

        float timeSinceAttackStart = 0f;
        float timeSinceLastAttack = Mathf.Infinity;
        float timeBetweenAttacks = 8000f;
        float maxTimeToHit = Mathf.Infinity;
        bool isEffectOnHit;
        bool isAttackActive;
        bool isFromQueue;

        // Hunger values
        float greed;
        float fatDesire;
        float starveValue = 0;
        float hungerValue = 20f;
        float preferredFullness = 50f;
        float maxFullness = 100f;
        float overfullPenalty;

        [SerializeField] HitTimer hitTimer;
        [Range(1, 100)]
        [SerializeField] int healThreshold = 30;
        [Range(1, 100)]
        [SerializeField] int manaThreshold = 30;
        [Range(1, 100)]
        [SerializeField] int regenThreshold = 80;
        Queue<FollowerAttackName> atkQueue = new Queue<FollowerAttackName>();
        Dictionary<FollowerAttackName, float> skillCooldown;
        Dictionary<FollowerAttackName, Coroutine> skillRecastRoutines = new Dictionary<FollowerAttackName, Coroutine>();

        [SerializeField] FollowerAttackName[] editorAtkList;

        private bool isBattleActive;

        private void Awake() 
        {
            baseStats = GetComponent<BaseStats>();
            fighter = GetComponent<FollowerFighter>();
            fullness = GetComponent<Fullness>();
            selfMana = GetComponent<Mana>();
        }

        private void Start() 
        {
            skillCooldown = new Dictionary<FollowerAttackName, float>();
            float metabolism = baseStats.GetStat(Stat.Metabolism);
            attackSpeed = baseStats.GetStat(Stat.AttackSpeed);
            cooldownReduction = baseStats.GetStat(Stat.CooldownReduction);
            actionRecovery = attackSpeed * metabolism / 100;
            digestSpeed = metabolism / 24;
            manaConversion = metabolism * baseStats.GetStat(Stat.Spirit) / 10000;

            // Hunger stats
            // Preferred fullness is the min value to aim for when eating
            // Max fullness is the max value to accept when eating
            // Minimum "starve" value - overrides other actions
            // Soft "hungry" value - eat if no other actions
            greed = baseStats.GetStat(Stat.Greed);
            fatDesire = baseStats.GetStat(Stat.FatDesire);
            preferredFullness += fatDesire / 2;
            maxFullness += greed / 4;
            starveValue += greed / 2;
            hungerValue = Mathf.Max(starveValue, (preferredFullness - starveValue) * 0.4f);
            StartingAttackQueue();
        }

        private void StartingAttackQueue()
        {
            if (!fighter.HasEffect(FollowerAttackName.DivineInfusion.ToString()))
            {
                atkQueue.Enqueue(FollowerAttackName.DivineInfusion);
            }
            atkQueue.Enqueue(FollowerAttackName.SpiritBoost);
            atkQueue.Enqueue(FollowerAttackName.ExposeWeakness);
            if (playerHealth.GetPercentage() < regenThreshold)
            {
                atkQueue.Enqueue(FollowerAttackName.SoothingWind);
            }
        }

        public void SetTarget(GameObject player, GameObject enemy)
        {
            playerHealth = player.GetComponent<Health>();
            playerMana = player.GetComponent<Mana>();
            targetHealth = enemy.GetComponent<Health>();
        }

        private void Update() 
        {
            if(isBattleActive)
            {
                if(isAttackActive)
                {
                    timeSinceAttackStart += Time.deltaTime;
                    if (timeSinceAttackStart >= maxTimeToHit)
                    {
                        skillCooldown[currentAttack] = Time.time;
                        CheckEffectActivation(true);
                        isAttackActive = false;
                    }
                }

                timeSinceLastAttack += (actionRecovery * actionModifier) * Time.deltaTime;
                DecideNextAction();
                UpdateActionBar();
            }
            editorAtkList = atkQueue.ToArray();
        }

        private void DecideNextAction()
        {
            if (timeSinceLastAttack >= timeBetweenAttacks)
            {
                float foodValue = fullness.DigestFood(digestSpeed * actionModifier);
                selfMana.GainAttribute(foodValue * manaConversion);
                timeSinceLastAttack = 0;
                string chatMessage = null;

                float fullPercent = fullness.GetPercentage();
                // Check for hunger and whether inventory has consumable
                if (fullPercent <= 0)
                {
                    // eat something yo
                }
                else if(AttackInQueue())
                {
                    FollowerAttackStats attackStats = followerAttackDB.GetAttackStat(currentAttack);

                    // Check if have mana to cover attack cost
                    if (attackStats.Cost < selfMana.GetAttributeValue())
                    {
                        attackSpeed = fighter.GetStat(Stat.AttackSpeed);
                        float atkSpeed = 1 / attackSpeed * 100;
                        maxTimeToHit = attackStats.CastTime * atkSpeed;
                        actionModifier = attackStats.ActionModifier;
                        selfMana.UseMana(attackStats.Cost);
                        timeSinceAttackStart = 0;
                        isAttackActive = true;

                        Sprite sprite = followerAttackIconDB.GetSprite(currentAttack);

                        isEffectOnHit = attackStats.ApplyEffect == ApplyEffectOn.OnHit;
                        HitTimer instance = Instantiate(hitTimer, new Vector3(-75, 100, 0), Quaternion.identity);
                        instance.transform.SetParent(transform, false);
                        instance.SetSprite(sprite);
                        instance.SetFillTime(maxTimeToHit);

                        CheckEffectActivation(false);
                        if (isFromQueue)
                        {
                            float recastTime = attackStats.RecastTimer + maxTimeToHit;
                            skillRecastRoutines[currentAttack] = StartCoroutine(SkillRecast(currentAttack, recastTime));
                            atkQueue.Dequeue();
                        }
                        chatMessage = "Casting " + attackStats.Name;
                    }
                    else
                    {
                        chatMessage = "I'm OOMing!";
                        actionModifier = 2f;
                    }
                }
                // check if under soft cap 
                else if(fullPercent <= hungerValue)
                {
                    // eat something too
                }
                else
                {
                    chatMessage = "Nothing to do?";
                    actionModifier = 2f;
                }

                //Check if penalty should be applied
                if(fullPercent > 100)
                {
                    float overfullPercent = (fullPercent - 100) / (maxFullness - 100);
                    actionModifier *= 1 - (0.5f - fatDesire / 2) * overfullPercent;
                }
                
                combatLog.SendMessageToChat(chatMessage);
            }
        }

        private bool AttackInQueue()
        {
            isFromQueue = false;
            // Check player HP threshold over attack queue
            float hp = playerHealth.GetPercentage();
            if (hp <= healThreshold)
            {
                currentAttack = FollowerAttackName.Heal;
                if (!IsOnCooldown(currentAttack))
                {
                    resourceType = playerHealth as IAttribute;
                    return true;
                }
            }

            // Check player mana
            if(playerMana.GetPercentage() <= manaThreshold)
            {
                currentAttack = FollowerAttackName.ManaBoost;
                if (!IsOnCooldown(currentAttack))
                {
                    resourceType = playerMana as IAttribute;
                    return true;
                }
            }

            // Check for lower priority heal over time threshold
            if (hp < regenThreshold && !IsAttackQueued(FollowerAttackName.SoothingWind))
            {
                atkQueue.Enqueue(FollowerAttackName.SoothingWind);
            }
            
            if(atkQueue.Count > 0)
            {
                currentAttack = atkQueue.Peek();
                if(!IsOnCooldown(currentAttack))
                {
                    isFromQueue = true;
                    return true;
                }
                Debug.Log("Attack in queue is on cooldown? " + currentAttack);
            }
            
            return false;
        }

        private IEnumerator SkillRecast(FollowerAttackName attackName, float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            // Run proficiency check for optimal timing, otherwise call for recheck after X seconds
            combatLog.SendMessageToChat("Queuing up " + followerAttackDB.GetAttackStat(attackName).Name);
            atkQueue.Enqueue(attackName);
            skillRecastRoutines.Remove(attackName);
        }

        private bool IsOnCooldown(FollowerAttackName attack)
        {
            if (skillCooldown.ContainsKey(attack))
            {
                FollowerAttackStats attackStats = followerAttackDB.GetAttackStat(attack);
                float cooldown = attackStats.Cooldown * (1 - fighter.GetStat(Stat.CooldownReduction)/100);
                if (skillCooldown[currentAttack] >= (Time.time - cooldown))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAttackQueued(FollowerAttackName followerAttackName)
        {
            return (skillRecastRoutines.ContainsKey(followerAttackName) || atkQueue.Contains(followerAttackName));
        }

        private void CheckEffectActivation(bool isHit)
        {
            if(isEffectOnHit == isHit)
            {
                FollowerAttackStats attackStats = followerAttackDB.GetAttackStat(currentAttack);
                EffectTarget atkFXTarget = attackStats.EffectTarget;
                if(atkFXTarget == EffectTarget.None)
                {
                    float healAmount = attackStats.Power / 100 * resourceType.GetMaxAttributeValue();
                    // healAmount *= baseStats.GetStat(Stat.Spirit) / 100;
                    resourceType.GainAttribute(healAmount);
                    return;
                }

                string atkFXid = attackStats.EffectID.ToString();

                switch (atkFXTarget)
                {
                    case EffectTarget.Enemy:
                        fighter.PassEffectEnemy(atkFXid);
                        break;
                    case EffectTarget.Self:
                        fighter.PassEffectSelf(atkFXid);
                        break;
                    case EffectTarget.Player:
                        fighter.PassEffectPlayer(atkFXid);
                        break;
                    case EffectTarget.Party:
                        fighter.PassEffectParty(atkFXid);
                        break;
                    default:
                        Debug.Log("Effect has no target!");
                        break;
                }
            }
        }

        private void UpdateActionBar()
        {
            float fill = Mathf.Clamp01(timeSinceLastAttack / timeBetweenAttacks);
            actBarOverlay.fillAmount = fill;
        }

        public void StartBattle()
        {
            isBattleActive = true;
            if(atkQueue.Count == 0)
            {
                StartingAttackQueue();
            }
        }

        public void EndBattle()
        {
            isBattleActive = false;
            skillCooldown = new Dictionary<FollowerAttackName, float>();
            timeSinceLastAttack = Mathf.Infinity;
            if(skillRecastRoutines.Count > 0)
            {
                foreach (var pair in skillRecastRoutines)
                {
                    StopCoroutine(skillRecastRoutines[pair.Key]);
                }
            }
            Cancel();
        }

        public void Cancel()
        {
            StopAttack();
        }

        private void StopAttack()
        {
            HitTimer instance = GetComponentInChildren<HitTimer>();
            if(instance != null)
            {
                instance.Cancel();
            }
        }
    }
}
