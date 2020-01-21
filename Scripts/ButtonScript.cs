using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button.Stats;
using System;

namespace Button.UI
{
    public class ButtonScript : MonoBehaviour
    {
        [SerializeField] AttackType attackType;
        [SerializeField] AttackDB attackDB = null;
        [SerializeField] ButtonClick buttonManager = null;
        [SerializeField] Text btnText = null;

        [SerializeField] public float timeSinceLastAttack = Mathf.Infinity;
        public float maxCooldown;

        public event Action onCooldown;

        private void Start() 
        {
            ResetButton();
            maxCooldown = GetStat(Stats.AttackStat.Cooldown);
        }

        public void OnClick()
        {
            if (IsAttackReady() && buttonManager.CanAttack())
            {
                buttonManager.StartAttack(this);
            }
        }

        public float GetStat(AttackStat stat)
        {
            return float.Parse(attackDB.GetAttackStat(stat, attackType));
        }

        public string GetName()
        {
            return attackDB.GetAttackStat(AttackStat.Name, attackType);
        }

        public int GetID()
        {
            return int.Parse(attackDB.GetAttackStat(AttackStat.ID, attackType));
        }

        public bool IsAttackReady()
        {
            if (timeSinceLastAttack >= maxCooldown)
            {
                return true;
            }
            return false;
        }

        public void ResetButton()
        {
            btnText.text = attackDB.GetAttackStat(AttackStat.Name, attackType);
        }

        public void UpdateTimer()
        {
            btnText.text = string.Format("{0:0.000}", maxCooldown - timeSinceLastAttack);
        }
    }
}