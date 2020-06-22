using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ButtonGame.Attributes;
using ButtonGame.Combat;

namespace ButtonGame.Character
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI animText = null;
        [SerializeField] Image statusOverlay = null;
        [SerializeField] TextMeshProUGUI dpsText = null;
        [SerializeField] EnemyController enemy = null;
        float animTime;
        float animLock;
        float skillLock;
        float totalLock;
        float timeInBattle;
        float totalDamage;

        // Naturally regen mana every 5s
        float manaRegenTime = 0;

        void Start()
        {
            animTime = Mathf.Infinity;
        }

        public void SetEnemy(EnemyController enemyController)
        {
            enemy = enemyController;
            
            ITargetable targetCandidate = enemy.transform.GetComponent<ITargetable>();
            foreach (IAtkSkill atk in GetComponentsInChildren<IAtkSkill>())
            {
                targetCandidate.HandleAttack(atk);
            }

            timeInBattle = 0f;
            totalDamage = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            animTime += Time.deltaTime;
            UpdateAnimText();
            timeInBattle += Time.deltaTime;
            UpdateDPSText();
            RegenMana();
        }

        public bool CanAttack(float skillPriority)
        {
            if(enemy == null)
            {
                return false;
            }
            
            if (animTime >= skillLock)
            {
                return true;
            }
            else if (skillPriority > 0 && animTime >= animLock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StartAttack(float[] skillLockTimes)
        {
            animLock = skillLockTimes[0];
            skillLock = skillLockTimes[1];
            totalLock = skillLockTimes[2];
            animTime = 0;
        }

        private void UpdateAnimText()
        {
            float animPercent = 0;
            if (animTime >= totalLock)
            {
                animText.text = "Ready";
                statusOverlay.color = new Color32(255, 255, 255, 255);
            }


            animPercent = Mathf.Clamp01((totalLock - animTime) / totalLock);
            if(animTime < animLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Preparing:", (totalLock - animTime));
                statusOverlay.color = new Color32(229, 60, 36, 255);
            }
            else if(animTime < skillLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Attacking:", (totalLock - animTime));
                statusOverlay.color = new Color32(229, 179, 36, 255);
            }
            else if(animTime < totalLock)
            {
                animText.text = string.Format("{0} {1:0.00}", "Recovery:", (totalLock - animTime));
                statusOverlay.color = new Color32(102, 229, 106, 255);
            }
            statusOverlay.fillAmount = animPercent;
        }

        public void UpdateDPSText()
        {
            int dps = 0;
            if(timeInBattle > 0)
            {
                dps = Mathf.CeilToInt(totalDamage / timeInBattle);
            }
            dpsText.text = "DPS: " + dps;
        }

        public void DamageDealt(float damage)
        {
            totalDamage += damage;
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