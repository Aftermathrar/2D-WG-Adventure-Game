using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Button.UI
{
    public class ButtonClick : MonoBehaviour
    {
        [SerializeField] Text animText = null;
        [SerializeField] RectTransform animOverlay = null;
        float animTime;
        float animLock;

        public List<ButtonScript> buttonsOnCooldown = new List<ButtonScript>();

        void Start()
        {
            animTime = Mathf.Infinity;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            animTime += Time.fixedDeltaTime;
            UpdateButtonText();
            UpdateAnimText();
        }

        private void UpdateButtonText()
        {
            for (int i = 0; i < buttonsOnCooldown.Count; i++)
            {
                buttonsOnCooldown[i].timeSinceLastAttack += Time.fixedDeltaTime;
                if (buttonsOnCooldown[i].timeSinceLastAttack >= buttonsOnCooldown[i].maxCooldown)
                {
                    buttonsOnCooldown[i].ResetButton();
                    buttonsOnCooldown.Remove(buttonsOnCooldown[i]);
                }
                else
                {
                    buttonsOnCooldown[i].UpdateTimer();
                }
            }
        }

        public void StartAttack(ButtonScript buttonScript)
        {
            StartCooldown(buttonScript);
            animLock = buttonScript.GetStat(Stats.AttackStat.Windup);
            animTime = 0;
            buttonScript.UpdateTimer();
            Enemy enemy = GetComponent<Enemy>();
            enemy.health -= buttonScript.GetStat(Stats.AttackStat.Damage);
            enemy.HealthUpdate();
        }

        public void StartCooldown(ButtonScript buttonScript)
        {
            if(!buttonsOnCooldown.Contains(buttonScript))
            {
                buttonScript.timeSinceLastAttack = 0;
                buttonsOnCooldown.Add(buttonScript);
            }
        }

        public bool CanAttack()
        {
            if(animTime >= animLock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateAnimText()
        {
            float animPercent = 0;
            if(animTime >= animLock)
            {
                animText.text = "Ready";
            }
            else
            {
                
                animText.text = string.Format("{1} {0:0.00}", (animLock - animTime), "Casting:");
                animPercent = Mathf.Clamp01((animLock - animTime) / animLock);
            }
            animOverlay.localScale = new Vector3(animPercent, 1, 1);
        }
    }
}
