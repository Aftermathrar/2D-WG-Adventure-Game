using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using ButtonGame.Resources;
using ButtonGame.Combat;

namespace ButtonGame.Character
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI animText = null;
        [SerializeField] RectTransform animOverlay = null;
        [SerializeField] Image overlayImage = null;
        [SerializeField] EnemyController enemy = null;
        Fighter fighter;
        float animTime;
        float animLock;
        float skillLock;
        float totalLock;

        // Naturally regen mana every 5s
        float manaRegenTime = 0;

        void Start()
        {
            animTime = Mathf.Infinity;
            foreach (AtkBtnScript atk in GetComponentsInChildren<AtkBtnScript>())
            {
                ITargetable[] targetCandidates = enemy.transform.GetComponents<ITargetable>();
                foreach (ITargetable targetable in targetCandidates)
                {
                    targetable.HandleAttack(atk);
                }
            }
            fighter = GetComponent<Fighter>();
            fighter.activeAttack += RegenMana;
        }

        // Update is called once per frame
        void Update()
        {
            animTime += Time.deltaTime;
            UpdateAnimText();
        }

        public void StartAttack(float[] skillLockTimes)
        {
            animLock = skillLockTimes[0];
            skillLock = skillLockTimes[1];
            totalLock = skillLockTimes[2];
            animTime = 0;
        }

        public bool CanAttack(float skillPriority)
        {
            if (animTime >= skillLock)
            {
                return true;
            }
            else if(skillPriority > 0 && animTime >= animLock)
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
            if (animTime >= totalLock)
            {
                animText.text = "Ready";
                overlayImage.color = new Color32(255, 255, 255, 255);
            }


            animPercent = Mathf.Clamp01((totalLock - animTime) / totalLock);
            if(animTime < animLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Preparing:", (totalLock - animTime));
                overlayImage.color = new Color32(155, 25, 255, 255);
            }
            else if(animTime < skillLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Attacking:", (totalLock - animTime));
                overlayImage.color = new Color32(25, 175, 255, 255);
            }
            else if(animTime < totalLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Recovery:", (totalLock - animTime));
                overlayImage.color = new Color32(230, 230, 230, 255);
            }
            animOverlay.localScale = new Vector3(animPercent, 1, 1);
        }

        public void DealDamage(float damage, bool isCrit = false)
        {
            //print("Getting health component");
            Health target = enemy.transform.GetComponent<Health>();
            target.TakeDamage(damage, isCrit);
        }

        public void RegenMana()
        {
            manaRegenTime += Time.deltaTime;
            if(manaRegenTime >= 5f)
            {
                GetComponent<Mana>().GainMana();
                manaRegenTime = 0;
            }
        }
    }

}