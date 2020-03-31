using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Resources;
using UnityEngine;

namespace ButtonGame.Combat
{

    public class Fighter : MonoBehaviour
    {
        // Damage calculation handled on atkBtnScripts
        // Get target to apply the damage
        // Get effect from atk to pass to CombatEffects

        CombatEffects effects;
        Health target;

        float invulnDuration = 0f;
        float invulnTimer = Mathf.Infinity;

        public event Action activeAttack;
        
        private void Start() 
        {
            effects = GetComponent<CombatEffects>();
            target = effects.GetTarget();
            activeAttack += effects.BuffTimer;
            activeAttack += effects.DebuffTimer;
        }

        private void Update() 
        {
            activeAttack?.Invoke();
        }

        public void StartInvulnTime(float invulnTime)
        {
            effects.ToggleInvuln();
            invulnDuration = invulnTime;
            invulnTimer = 0;
            activeAttack += InvulnTimer;
        }

        public void InvulnTimer()
        {
            invulnTimer += Time.deltaTime;
            if(invulnTimer >= invulnDuration)
            {
                effects.ToggleInvuln();
                activeAttack -= InvulnTimer;
            }
        }

        public void PassEffect(float ID, string effectTarget)
        {
            if(ID > 0)
            {
                if(effectTarget == "Self")
                {
                    effects.BuffSelf(ID);
                }
                else
                {
                    effects.DebuffTarget(ID);
                }
            }
        }
    }
}
