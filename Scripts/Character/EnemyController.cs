using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using ButtonGame.Stats;

namespace ButtonGame.Character
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] RectTransform statusOverlay;
        float statusTime;
        float statusLock;
        string curStatus;

        public event Action enemyAttack;

        private void Start()
        {
            curStatus = "Idle";
            statusTime = 0f;
            statusText.text = curStatus;
        }

        private void Update() 
        {
            statusTime += Time.deltaTime;
            enemyAttack?.Invoke();
            UpdateStatusText();
        }

        public void startAttack(float lockTime)
        {
            statusLock = lockTime;
            statusTime = 0;
        }

        public void DealDamage(float damage)
        {
            // remove player health
        }

        public bool CanAttack()
        {
            if (statusTime >= statusLock)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetNewStatus(String txt)
        {
            curStatus = txt;
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            float statusPercent = 0;
            if (statusTime >= statusLock)
            {
                curStatus = "Idle";
                statusText.text = curStatus;
            }
            else
            {
                statusText.text = curStatus;
                statusPercent = Mathf.Clamp01((statusLock - statusTime) / statusLock);
            }
            statusOverlay.localScale = new Vector3(statusPercent, 1, 1);
        }
    }
}
