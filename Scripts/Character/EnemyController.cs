using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace ButtonGame.Character
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] RectTransform statusOverlay;
        float actionTimer;  
        float actionLockTime;
        float statusOverlayTime;
        string curStatus;

        private bool isBattleActive;

        public event Action enemyAttack;

        private void Awake()
        {
            curStatus = "Idle";
            actionTimer = 0f;
            statusOverlayTime = 2f;
            statusText.text = curStatus;
        }

        private void Update() 
        {
            if(!isBattleActive) 
                return;

            actionTimer += Time.deltaTime;
            enemyAttack?.Invoke();
            UpdateStatusText();
        }

        public void startAttack(float lockTime)
        {
            actionLockTime = lockTime;
            actionTimer = 0;
        }

        public void startIdle(float idleTime)
        {
            curStatus = "Idle";
            actionLockTime = 0f;
            statusOverlayTime = idleTime;
            actionTimer = 0;
        }

        public bool CanAttack()
        {
            if (actionTimer >= actionLockTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetNewStatus(String txt, float statusTime)
        {
            curStatus = txt;
            if (statusTime == 0) statusTime = actionLockTime;
            statusOverlayTime = statusTime;
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            float statusPercent = 0;
        
            statusText.text = curStatus;
            statusPercent = Mathf.Clamp01((statusOverlayTime - actionTimer) / statusOverlayTime);
        
            statusOverlay.localScale = new Vector3(statusPercent, 1, 1);
        }

        public void UpdateBattleStatus(bool isActive)
        {
            isBattleActive = isActive;
        }
    }
}
