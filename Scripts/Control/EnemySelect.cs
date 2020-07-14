using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Control
{
    public class EnemySelect : MonoBehaviour
    {
        [SerializeField] LevelManager battleManager = null;
        [SerializeField] CharacterClass enemySelect;
        [SerializeField] GameObject enemySelectionButtons = null;
        [SerializeField] GameObject tooltipWindow = null;

        public void Continue()
        {
            if (battleManager == null)
            {
                battleManager = (LevelManager)GameObject.FindObjectOfType(typeof(LevelManager));
            }
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
