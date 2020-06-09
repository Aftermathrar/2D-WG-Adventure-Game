using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ButtonGame.Stats;
using ButtonGame.Character;
using System;

namespace ButtonGame.Core
{
    public class LevelManager : MonoBehaviour, IBattleState
    {
        // Pause Control
        public static bool isPaused;
        private bool isBattleActive = false;
        [SerializeField] GameObject pauseScreen;

        // Battle state control
        public event Action StartCurrentBattle;
        public event Action EndCurrentBattle;

        // Battle setup
        [SerializeField] Transform battleCanvas;
        [SerializeField] PlayerController playerController;
        [SerializeField] EnemyController enemyController;
        [SerializeField] BaseStats[] enemyStats;
        [SerializeField] float timeScale;

        // Combat log
        [SerializeField] int maxMessages = 25;
        [SerializeField] GameObject chatPanel;
        [SerializeField] GameObject textObject;
        [SerializeField] List<Message> messageList = new List<Message>();
        Color32 defaultColor = new Color32(195, 165, 93, 255);

        private void Start() 
        {
            // Choose random enemy from list
            int randomEnemyIndex;
            randomEnemyIndex = UnityEngine.Random.Range(0, enemyStats.Length);
            BaseStats selectedEnemy;
            selectedEnemy = enemyStats[randomEnemyIndex];

            // Spawn enemy
            GameObject enemyGO = Instantiate(selectedEnemy, battleCanvas).gameObject;
            enemyGO.transform.SetAsFirstSibling();
            enemyController = enemyGO.GetComponent<EnemyController>();

            // Assign opponent to scripts
            enemyController.GetComponent<CombatEffects>().SetTarget(playerController.gameObject);
            playerController.SetEnemy(enemyController);
            playerController.GetComponent<CombatEffects>().SetTarget(enemyController.gameObject);

            // Activate battle
            StartCurrentBattle += StartBattle;
            EndCurrentBattle += EndBattle;
            StartCoroutine(BeginBattle());
        }

        private void Update() 
        {
            if(Input.GetKeyDown(KeyCode.Tab) && isBattleActive)
            {
                if(!isPaused)
                {
                    PauseGame();
                }
                else
                {
                    UnpauseGame();
                }
            }
        }

        private IEnumerator BeginBattle()
        {
            yield return new WaitForSeconds(0f);
            BattleStart();
        }

        private void PauseGame()
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            isPaused = !isPaused;
        }

        public void UnpauseGame()
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
            isPaused = !isPaused;
        }

        public void SendMessageToChat(string text, Color32 color)
        {
            if(messageList.Count >= maxMessages)
            {
                Destroy(messageList[0].textObject.gameObject);
                messageList.Remove(messageList[0]);
            }

            Message newMessage = new Message();
            newMessage.text = text;

            GameObject newText = Instantiate(textObject, chatPanel.transform);
            newMessage.textObject = newText.GetComponent<Text>();
            newMessage.textObject.text = newMessage.text;
            newMessage.textObject.color = color;

            messageList.Add(newMessage);
        }

        public void DamageTakenCombatLog(float damage, bool isCrit)
        {
            Message damageMessage = new Message();
            Color32 messageColor = defaultColor;
            if(isCrit)
            {
                damageMessage.text = "Critical hit! You took " + damage + " damage!";
                messageColor = new Color32(219, 126, 90, 255);
            }
            else
            {
                damageMessage.text = "Hit taken for " + damage + " damage!";
            }

            SendMessageToChat(damageMessage.text, messageColor);
        }

        public void BlockSuccessCombatLog(bool isPerfect)
        {
            Message guardMessage = new Message();
            Color32 messageColor = defaultColor;
            if(isPerfect)
            {
                guardMessage.text = "Perfect block! Attack has been reflected!";
                messageColor = new Color32(219, 204, 90, 255);
            }
            else
            {
                guardMessage.text = "Blocked! Damage has been reduced.";
            }

            SendMessageToChat(guardMessage.text, messageColor);
        }

        public void BattleStart()
        {
            StartCurrentBattle?.Invoke();
        }

        public void BattleEnd()
        {
            EndCurrentBattle?.Invoke();
        }

        public void StartBattle()
        {
            isBattleActive = true;
        }

        public void EndBattle()
        {
            isBattleActive = false;
            Time.timeScale = 0.3f;
        }
    }

    [System.Serializable]
    public class Message
    {
        public string text;
        public Text textObject;
    }
}
