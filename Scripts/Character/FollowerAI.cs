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
        // Will need to add receivers for events such as 10m buff running out mid-fight and
        // boss mechanics that require a specific response to override the normal action queue

        // References? Dependencies?
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
        IAttribute resourceType;
        [SerializeField] Image actBarOverlay = null;

        // Character stats
        float attackSpeed;
        float cooldownReduction = 0f;
        float actionRecovery = 0f;
        float actionModifier = 0f;
        float digestSpeed = 0f;
        float manaConversion = 0f;

        // Action state info
        FollowerAttackName currentAttack;
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
        float preferredFullness = 50f;
        float maxFullness = 100f;
        float overfullPenalty;
        float eatingSpeed;

        // Consumable stuff
        Inventory inventory;
        [SerializeField] List<int> consumableIndex;
        ConsumableItem currentFood;
        int[] consumedItem;

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
            starveValue += greed / 2;
            hungerValue = Mathf.Max(starveValue, (preferredFullness - starveValue) * 0.4f);
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
                        isEatingActive = false;
                        timeSinceLastAttack = 0;
                    }
                    return;
                }
                timeSinceLastAttack += (actionRecovery * actionModifier) * Time.deltaTime;
                DecideNextAction();
                UpdateActionBar();
            }
        }

        private void DecideNextAction()
        {
            if (timeSinceLastAttack >= timeBetweenAttacks)
            {
                float foodValue = fullness.DigestFood(digestSpeed * actionModifier);
                // TODO: figure out base mana recovery when stomach is empty
                float manaAmount = Mathf.Max(1, foodValue * manaConversion);
                selfMana.GainAttribute(manaAmount);
                timeSinceLastAttack = 0;
                string chatMessage = null;

                float fullPercent = fullness.GetPercentage();
                // Check for hunger and whether inventory has consumable
                if (fullPercent <= 0 && consumableIndex.Count > 0)
                {
                    chatMessage = GetItemToEat();
                }
                else if(AttackInQueue())
                {
                    FollowerAttackStats attackStats = followerAttackDB.GetAttackStat(currentAttack);

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

                        if (isFromQueue)
                        {
                            float recastTime = attackStats.RecastTimer + timeToAct;
                            skillRecastRoutines[currentAttack] = StartCoroutine(SkillRecast(currentAttack, recastTime));
                            atkQueue.Dequeue();
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

                //Check if penalty should be applied
                if(fullPercent > 100)
                {
                    float overfullPercent = (fullPercent - 100) / (maxFullness - 100);
                    actionModifier *= overfullPenalty * overfullPercent;
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

        private string GetItemToEat()
        {
            int foodIndex = consumableIndex[Random.Range(0, consumableIndex.Count)];

            currentFood = inventory.GetItemInSlot(foodIndex) as ConsumableItem;
            int qty = inventory.GetCountInSlot(foodIndex);
            float foodSize = currentFood.GetSize();
            float lowerBound = fullness.GetMaxAttributeValue() * (preferredFullness - starveValue) / 100;
            Debug.Log(lowerBound);
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

        private IEnumerator SkillRecast(FollowerAttackName attackName, float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);
            // Run proficiency check for optimal timing, otherwise call for recheck after X seconds
            combatLog.SendMessageToChat("Queuing up " + followerAttackDB.GetAttackStat(attackName).Name);
            atkQueue.Enqueue(attackName);
            skillRecastRoutines.Remove(attackName);
        }

        private void SpawnHitTimer(Sprite sprite)
        {
            HitTimer instance = Instantiate(hitTimer, new Vector3(-75, 100, 0), Quaternion.identity);
            instance.transform.SetParent(transform, false);
            instance.SetSprite(sprite);
            instance.SetFillTime(timeToAct);
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
