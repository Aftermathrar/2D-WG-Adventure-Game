using System;
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

        public void PassEffect(string ID, string effectTarget)
        {
            if(ID != "None")
            {
                if(effectTarget == "Self")
                {
                    Color32 fxIconColor = new Color32(148, 200, 112, 255);
                    effects.BuffSelf(ID, fxIconColor);
                }
                else
                {
                    Color32 fxIconColor = new Color32(200, 148, 85, 255);
                    effects.DebuffTarget(ID, fxIconColor);
                }
            }
        }
    }
}
