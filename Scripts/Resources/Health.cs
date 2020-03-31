using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ButtonGame.Stats;
using GameDevTV.Utils;
using ButtonGame.Core;

namespace ButtonGame.Resources
{
    public class Health : MonoBehaviour
    {
        [SerializeField] bool displayDamageTaken = false;
        [SerializeField] bool checkForBlock = false;
        [SerializeField] TakeDamageEvent takeDamage;

        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float, bool> { }

        // float missingHealthPoints = 70;
        LazyValue<float> healthPoints;
        GuardController guard = null;
        bool isDead = false;
        bool isInvulnerable = false;

        private void Awake()
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
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

        public void TakeDamage(float damage, bool isCrit = false)
        {
            if(isInvulnerable) return;
            
            if(checkForBlock && guard != null)
            {
                // check if player is blocking
                if (guard.IsBlocking())
                {
                    // reduce damage, GuardController displays guard message and reflect damage
                    float damageReduction = GetComponent<BaseStats>().GetStat(Stat.BlockAmount);
                    damage = Mathf.Max(damage - damageReduction, 0);
                }
            }
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);

            if (displayDamageTaken) 
            {
                takeDamage.Invoke(damage, isCrit);
            }

            if (healthPoints.value == 0)
            {
                Die();
                // Do stuff to end the battle
            }
        }

        public void GainHealth(float heal)
        {
            healthPoints.value = Mathf.Min(healthPoints.value + heal, GetComponent<BaseStats>().GetStat(Stat.Health));
        }

        private void Die()
        {
            if(isDead) { return; }
            isDead = true;
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public float GetHealthPoints()
        {
            return healthPoints.value;
        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }
    }
}
