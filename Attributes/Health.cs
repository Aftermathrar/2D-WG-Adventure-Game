using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ButtonGame.Stats;
using GameDevTV.Utils;
using ButtonGame.Core;
using ButtonGame.Combat;
using ButtonGame.Saving;

namespace ButtonGame.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] bool checkForBlock = false;
        [SerializeField] TakeDamageEvent takeDamage;
        [SerializeField] OnDeathEvent onDeath;


        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float, bool> { }
        [System.Serializable]
        public class OnDeathEvent : UnityEvent<string> { }

        // float missingHealthPoints = 70;
        LazyValue<float> healthPoints;
        BaseStats baseStats;
        GuardController guard = null;
        bool isDead = false;
        bool isInvulnerable = false;

        private void Awake()
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
            baseStats = GetComponent<BaseStats>();
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Start()
        {
            healthPoints.ForceInit();
            if (checkForBlock)
            {
                guard = GetComponent<GuardController>();
            }
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void ToggleInvuln()
        {
            isInvulnerable = !isInvulnerable;
        }

        public bool TakeDamage(float damage, bool isCrit = false, bool isBlockable = true)
        {
            if(isInvulnerable) return false;
            
            if(checkForBlock && guard != null)
            {
                // check if player is blocking
                if (IsBlocking(isBlockable) && isBlockable)
                {
                    // reduce damage, GuardController displays guard message and reflect damage
                    float damageReduction = baseStats.GetStat(Stat.BlockAmount);
                    damage = Mathf.Max(damage - damageReduction, 0);
                }
            }
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if (damage > 0) 
            {
                takeDamage.Invoke(damage, isCrit);
            }

            if (healthPoints.value == 0)
            {
                Die();
                // Do stuff to end the battle
            }

            return (damage > 0);
        }

        public void GainHealth(float heal)
        {
            healthPoints.value = Mathf.Min(healthPoints.value + heal, baseStats.GetStat(Stat.Health));
        }

        private void Die()
        {
            if(isDead) { return; }
            isDead = true;
            GetComponent<ActionScheduler>().CancelCurrentAction();
            onDeath.Invoke(gameObject.tag);
        }

        public bool IsBlocking(bool shouldReflect = true)
        {
            return guard.IsBlocking(shouldReflect);
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return healthPoints.value / baseStats.GetStat(Stat.Health);
        }

        public float GetHealthPoints()
        {
            return healthPoints.value;
        }

        public float GetMaxHealthPoints()
        {
            return baseStats.GetStat(Stat.Health);
        }

        public object CaptureState()
        {
            return healthPoints.value;
        }

        public void RestoreState(object state)
        {
            healthPoints.value = (float)state;
            // Debug.Log("health is " + (float)state);
            if(healthPoints.value <= 0)
            {
                print("Player dead, reviving to 1HP");
                healthPoints.value = 1f;
            }
        }
    }
}
