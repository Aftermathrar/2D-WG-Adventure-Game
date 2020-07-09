using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Combat;
using ButtonGame.Core;
using ButtonGame.Inventories;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using ButtonGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Character
{
    public class FollowerAI : MonoBehaviour, IAction, IBattleState
    {
        // Will need to add receivers for events such as 10m buff running out mid-fight (DONE) and
        // boss mechanics that require a specific response to override the normal action queue

        // References? Dependencies?
        [SerializeField] FollowerAttackIconDB followerAttackIconDB = null;
        [SerializeField] EffectDB effectDB = null;
        [SerializeField] FollowerCombatLog combatLog = null;
        [SerializeField] Image actBarOverlay = null;
        [SerializeField] HitTimerSpawner hitTimerSpawner = null;
        FollowerAttackManager attackManager;
        BaseStats baseStats;
        BaseFollowerAttackStats baseAttackStats;
        FollowerFighter fighter;
        Fullness fullness;
        Mana selfMana;
        Health playerHealth;
        Mana playerMana;
        Health targetHealth;

        // Character stats
        float attackSpeed;
        float cooldownReduction = 0f;
        float actionRecovery = 0f;
        float actionModifier = 0f;
        float digestSpeed = 0f;
        float manaConversion = 0f;

        // Action state info
        FollowerAttackName currentAttack;
        IAttribute resourceType;
        float timeSinceActionStart = 0f;
        float timeSinceLastAttack = Mathf.Infinity;
        float timeBetweenAttacks = 8000f;
        float timeToAct = Mathf.Infinity;
        bool isEffectOnHit;
        bool isAttackActive;
        bool isEatingActive;
        bool isFromQueue;

        // Hunger values
        float greed;
        float fatDesire;
        float starveValue = 0;
        float hungerValue = 20f;
        float stuffedValue = 80f;
        float preferredFullness = 50f;
        float maxFullness = 100f;
        float overfullPenalty;
        float eatingSpeed;

        // Consumable stuff
        Inventory inventory;
        [SerializeField] List<int> consumableIndex;
        ConsumableItem currentFood;
        int[] consumedItem;

        // Attack Management
        [Range(1, 100)]
        [SerializeField] int healThreshold = 30;
        [Range(1, 100)]
        [SerializeField] int manaThreshold = 30;
        [Range(1, 100)]
        [SerializeField] int regenThreshold = 80;
        Queue<FollowerAttackName> atkQueue = new Queue<FollowerAttackName>();
        Queue<string> debuffCleanseQueue = new Queue<string>();
        Dictionary<FollowerAttackName, float> skillCooldown;
        Dictionary<FollowerAttackName, Coroutine> skillRecastRoutines = new Dictionary<FollowerAttackName, Coroutine>();
        [SerializeField] FollowerAttackName[] editorAtkView;

        private bool isBattleActive;

        private void Awake() 
        {
            attackManager = GetComponent<FollowerAttackManager>();
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

            AssignHungerStats();
            BuildConsumableList();

            // Add basic buffs to queue
            StartingAttackQueue();
        }

        private void AssignHungerStats()
        {
            greed = baseStats.GetStat(Stat.Greed);
            fatDesire = baseStats.GetStat(Stat.FatDesire);
            preferredFullness += fatDesire / 2;
            maxFullness += greed / 4;
            starveValue += Mathf.Min(100f, greed / 2f);
            hungerValue = Mathf.Max(starveValue, starveValue + (preferredFullness - starveValue) * 0.4f);
            stuffedValue = maxFullness - (maxFullness - hungerValue) * 0.25f;
            overfullPenalty = 1 - (0.5f - fatDesire / 2);
            eatingSpeed = 50 / (1 + greed / 100);
        }

        private void BuildConsumableList()
        {
            float upperBound = fullness.GetMaxAttributeValue() * (maxFullness - hungerValue) / 100;
            int invSize = inventory.GetSize();
            for (int i = 0; i < invSize; i++)
            {
                ConsumableItem food = inventory.GetItemInSlot(i) as ConsumableItem;
                if(food == null) continue;

                if(food.GetSize() <= upperBound)
                {
                    consumableIndex.Add(i);
                }
            }
        }

        private void StartingAttackQueue()
        {
            FollowerAttackName attackFromPool = attackManager.GetAttackOfType(FollowerAttackPool.Aura);
            var followerAttackStats = attackManager.GetAttackStats(attackFromPool);
            float AuraDuration = fighter.GetEffectElapsedTime(followerAttackStats.EffectID.ToString(), true);
            if (AuraDuration == 0)
            {
                atkQueue.Enqueue(attackFromPool);
            }
            else
            {
                float recastTimer = followerAttackStats.RecastTimer - AuraDuration;
                skillRecastRoutines[attackFromPool] = StartCoroutine(SkillRecast(attackFromPool, recastTimer));
            }
            attackFromPool = attackManager.GetAttackOfType(FollowerAttackPool.Buff);
            atkQueue.Enqueue(attackFromPool);
            attackFromPool = attackManager.GetAttackOfType(FollowerAttackPool.Debuff);
            atkQueue.Enqueue(attackFromPool);
            if (playerHealth.GetPercentage() < regenThreshold)
            {
                attackFromPool = attackManager.GetAttackOfType(FollowerAttackPool.HealOverTime);
                atkQueue.Enqueue(attackFromPool);
            }
        }

        // Called during battle setup from LevelManager
        public void SetTarget(GameObject player, GameObject enemy)
        {
            playerHealth = player.GetComponent<Health>();
            playerMana = player.GetComponent<Mana>();
            inventory = player.GetComponent<Inventory>();
            targetHealth = enemy.GetComponent<Health>();
        }

        private void Update() 
        {
            if(isBattleActive)
            {
                if(isAttackActive)
                {
                    timeSinceActionStart += Time.deltaTime;
                    if (timeSinceActionStart >= timeToAct)
                    {
                        skillCooldown[currentAttack] = Time.time;
                        CheckEffectActivation(true);
                        CheckFullnessModifier();

                        isAttackActive = false;
                        timeSinceLastAttack = 0;
                    }
                    return;
                }
                else if(isEatingActive)
                {
                    timeSinceActionStart += Time.deltaTime;
                    if(timeSinceActionStart >= timeToAct)
                    {
                        inventory.RemoveFromSlot(consumedItem[0], consumedItem[1]);
                        fullness.EatFood(currentFood, consumedItem[1]);
                        CheckFullnessModifier();
                        CapacityExpansionCheck();

                        isEatingActive = false;
                        timeSinceLastAttack = 0;
                    }
                    return;
                }
                timeSinceLastAttack += (actionRecovery * actionModifier) * Time.deltaTime;
                DecideNextAction();
                UpdateActionBar();
            }
            editorAtkView = atkQueue.ToArray();
        }

        private void DecideNextAction()
        {
            if (timeSinceLastAttack >= timeBetweenAttacks)
            {
                // actionModifier should probably affect this...
                float foodValue = fullness.DigestFood(digestSpeed);
                // TODO: figure out base mana recovery when stomach is empty
                float manaAmount = Mathf.Max(1, foodValue * manaConversion);
                selfMana.GainAttribute(manaAmount);
                timeSinceLastAttack = 0;
                string chatMessage = null;

                float fullPercent = fullness.GetPercentage();
                // Check for hunger and whether inventory has consumable
                if (fullPercent <= starveValue && consumableIndex.Count > 0)
                {
                    chatMessage = GetItemToEat();
                }
                else if(AttackInQueue())
                {
                    FollowerAttackStats attackStats = attackManager.GetAttackStats(currentAttack);

                    // Check if have mana to cover attack cost
                    if (attackStats.Cost < selfMana.GetAttributeValue())
                    {
                        attackSpeed = 100 / fighter.GetStat(Stat.AttackSpeed);
                        timeToAct = attackStats.CastTime * attackSpeed;
                        actionModifier = attackStats.ActionModifier;
                        selfMana.UseMana(attackStats.Cost);
                        timeSinceActionStart = 0;
                        isAttackActive = true;
                        isEffectOnHit = attackStats.ApplyEffect == ApplyEffectOn.OnHit;

                        Sprite sprite = followerAttackIconDB.GetSprite(currentAttack);
                        SpawnHitTimer(sprite);
                        CheckEffectActivation(false);

                        if (isFromQueue) atkQueue.Dequeue();
                        if(attackStats.isRecastSkill)
                        {
                            float recastTime = attackStats.RecastTimer + timeToAct;
                            skillRecastRoutines[currentAttack] = StartCoroutine(SkillRecast(currentAttack, recastTime));
                        }
                        chatMessage = "Casting " + attackStats.Name;
                    }
                    else
                    {
                        // Message out of mana, extra check for hunger
                        chatMessage = "I'm OOMing!";
                        actionModifier = 2f;
                        if (fullPercent <= hungerValue && consumableIndex.Count > 0)
                        {
                            chatMessage = GetItemToEat();
                        }
                    }
                }
                // check if under soft cap for hunger
                else if(fullPercent <= hungerValue && consumableIndex.Count > 0)
                {
                    chatMessage = GetItemToEat();
                }
                else
                {
                    chatMessage = "Nothing to do?";
                    actionModifier = 2f;
                }
                
                combatLog.SendMessageToChat(chatMessage);
            }
        }

        private bool AttackInQueue()
        {
            isFromQueue = false;
            KeyValuePair<FollowerAttackName, int> attackAndCost;
            // Check player HP threshold over attack queue
            float hp = playerHealth.GetPercentage();
            float mana = playerMana.GetAttributeValue();
            if (hp <= healThreshold)
            {
                attackAndCost = attackManager.GetAttackCost(FollowerAttackPool.HealBig);
                if (!IsOnCooldown(attackAndCost.Key) && attackAndCost.Value <= mana)
                {
                    currentAttack = attackAndCost.Key;
                    resourceType = playerHealth as IAttribute;
                    return true;
                }
            }

            // Check player mana
            if(playerMana.GetPercentage() <= manaThreshold)
            {
                attackAndCost = attackManager.GetAttackCost(FollowerAttackPool.ManaGain);
                if (!IsOnCooldown(attackAndCost.Key))
                {
                    currentAttack = attackAndCost.Key;
                    resourceType = playerMana as IAttribute;
                    return true;
                }
            }

            // Check for lower priority heal over time threshold
            if (hp < regenThreshold)
            {
                attackAndCost = attackManager.GetAttackCost(FollowerAttackPool.HealOverTime);
                if(!IsAttackQueued(attackAndCost.Key))
                {
                    atkQueue.Enqueue(attackAndCost.Key);
                }
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

        private string GetItemToEat()
        {
            int foodIndex = consumableIndex[Random.Range(0, consumableIndex.Count)];

            currentFood = inventory.GetItemInSlot(foodIndex) as ConsumableItem;
            int qty = inventory.GetCountInSlot(foodIndex);
            float foodSize = currentFood.GetSize();
            float lowerBound = fullness.GetMaxAttributeValue() * (preferredFullness - starveValue) / 100;
            int qtyToEat = 1;

            if(foodSize < lowerBound)
            {
                qtyToEat = Mathf.Min(qty, Mathf.CeilToInt(lowerBound / foodSize));
            }

            // Remove item from consumable list if it'll empty the slot's stack
            if(qtyToEat >= qty) consumableIndex.Remove(foodIndex);

            // Cache info about the item for action completion
            consumedItem = new int[2];
            consumedItem[0] = foodIndex;
            consumedItem[1] = qtyToEat;
            actionModifier = 1f;
            timeToAct = foodSize / eatingSpeed;
            isEatingActive = true;
            timeSinceActionStart = 0;
            string s = "Eating " + qtyToEat + " " + currentFood.GetDisplayName();

            Sprite sprite = currentFood.GetIcon();
            SpawnHitTimer(sprite);

            return s;
        }

        private void CheckFullnessModifier()
        {
            //Check if penalty should be applied
            float fullPercent = fullness.GetPercentage();
            if (fullPercent > 100)
            {
                float overfullPercent = (fullPercent - 100) / Mathf.Max(0.1f, (maxFullness - 100));
                actionModifier *= overfullPenalty * overfullPercent;
            }
        }

        private void CapacityExpansionCheck()
        {
            float fullPercent = fullness.GetPercentage();
            if (fullPercent > stuffedValue)
            {
                float overstuffed = fullPercent - stuffedValue;
                float stuffedRange = Mathf.Max(1, maxFullness - stuffedValue);
                float overstuffedPercent = overstuffed / stuffedRange * 100;

                // Roll for increase chance based on percentage into stuffed range
                int randInt = Random.Range(0, 100);
                if (overstuffedPercent > randInt)
                {
                    float capacityIncrease = (greed + fatDesire) * overstuffedPercent / 1000000f;
                    fullness.IncreaseCapacity(capacityIncrease);
                }
            }
        }

        private IEnumerator SkillRecast(FollowerAttackName attackName, float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            // Run proficiency check for optimal timing, otherwise call for recheck after X seconds
            combatLog.SendMessageToChat("Queuing up " + attackManager.GetAttackStats(attackName).Name);
            atkQueue.Enqueue(attackName);
            skillRecastRoutines.Remove(attackName);
        }

        private void SpawnHitTimer(Sprite sprite)
        {
            HitTimer instance = hitTimerSpawner.SpawnHitTimer(0);
            instance.SetSprite(sprite);
            instance.SetFillTime(timeToAct);
        }

        private bool IsOnCooldown(FollowerAttackName attack)
        {
            if (skillCooldown.ContainsKey(attack))
            {
                FollowerAttackStats attackStats = attackManager.GetAttackStats(attack);
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
                FollowerAttackStats attackStats = attackManager.GetAttackStats(currentAttack);
                EffectTarget atkFXTarget = attackStats.EffectTarget;
                if(atkFXTarget == EffectTarget.None)
                {
                    if(attackStats.movePool == FollowerAttackPool.Cleanse)
                    {
                        fighter.ClearEffect(debuffCleanseQueue.Dequeue());
                    }
                    else
                    {
                        float healAmount = attackStats.Power / 100 * resourceType.GetMaxAttributeValue();
                        // healAmount *= baseStats.GetStat(Stat.Spirit) / 100;
                        resourceType.GainAttribute(healAmount);
                    }
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

        public void OnPlayerDebuff(string debuffID)
        {
            if(attackManager.HasAttackInPool(FollowerAttackPool.Cleanse))
            {
                FollowerAttackName cleanseAttack = attackManager.GetAttackOfType(FollowerAttackPool.Cleanse);
                if(!IsAttackQueued(cleanseAttack))
                {
                    combatLog.SendMessageToChat("Queuing up a cleanse!");
                    debuffCleanseQueue.Enqueue(debuffID);
                    atkQueue.Enqueue(cleanseAttack);
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
            isAttackActive = false;
            isEatingActive = false;
            skillCooldown = new Dictionary<FollowerAttackName, float>();
            timeSinceLastAttack = Mathf.Infinity;
            if(skillRecastRoutines.Count > 0)
            {
                foreach (var pair in skillRecastRoutines)
                {
                    StopCoroutine(skillRecastRoutines[pair.Key]);
                }
            }
            atkQueue.Clear();
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
