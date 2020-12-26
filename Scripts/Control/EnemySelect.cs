using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Control
{
    public class EnemySelect : MonoBehaviour
    {
        [SerializeField] CharacterClass enemySelect;
        [SerializeField] GameObject enemySelectionButtons = null;
        LevelManager battleManager = null;
        GameObject tooltipWindow = null;

        private void OnEnable()
        {
            tooltipWindow = GameObject.FindGameObjectWithTag("TooltipWindow");
            battleManager = GameObject.FindGameObjectWithTag("BattleManager").GetComponent<LevelManager>();
        }

        public void Continue()
        {
            if(battleManager.IsBattleActive())
            {
                battleManager.ResumeRestart();
            }
            else
            {
                enemySelectionButtons.SetActive(true);
                tooltipWindow.SetActive(false);
            }
        }

        public void ChooseEnemy()
        {
            if(battleManager == null)
            {
                battleManager = (LevelManager)GameObject.FindObjectOfType(typeof(LevelManager));
            }
            tooltipWindow.SetActive(true);
            battleManager.ResumeRestart(enemySelect);
            enemySelectionButtons.SetActive(false);
        }
    }
}
