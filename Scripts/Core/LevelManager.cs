using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ButtonGame.Character;
using ButtonGame.Combat;
using ButtonGame.Inventories;
using ButtonGame.Locations;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using TMPro;
using ButtonGame.Quests;

namespace ButtonGame.Core
{
    public class LevelManager : MonoBehaviour
    {
        // Pause Control
        public bool isPaused = false;
        private bool isBattleActive = false;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject introScreen;
        [SerializeField] GameObject outroScreen;
        [SerializeField] GameObject postFightButtons;
        [SerializeField] GameObject rewardWindow;
        [SerializeField] GameObject questWindow;

        // Battle setup
        [SerializeField] protected EnemySpawnDB enemySpawnDB;
        [SerializeField] UnityEvent battleStart;
        protected GameObject playerGO;
        protected GameObject enemyGO;
        protected BaseStats[] enemyPrefabs;
        LocationList location;
        Coroutine introCoroutine = null;

        private void Start()
        {
            BattleSetup();
        }

        protected virtual void Update() 
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

        private void BattleSetup()
        {
            // Populate enemy list
            LocationManager locationManager = GetComponent<LocationManager>();
            location = locationManager.GetCurrentLocation();
            enemyPrefabs = enemySpawnDB.GetEnemyList(location);

            // Spawn enemy
            enemyGO = SpawnNewEnemy();
            enemyGO.transform.SetAsFirstSibling();

            AssignTargets();
        }

        private void BattleSetup(CharacterClass enemyName)
        {
            // Populate enemy list
            LocationList location = GetComponent<LocationManager>().GetCurrentLocation();
            enemyPrefabs = enemySpawnDB.GetEnemyList(location);

            // Spawn enemy
            enemyGO = SpawnNewEnemy(enemyName);
            enemyGO.transform.SetAsFirstSibling();

            AssignTargets();
        }

        private void AssignTargets()
        {
            // Find player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();

            // Assign opponent to scripts
            enemyGO.GetComponent<CombatEffects>().SetTarget(playerGO);
            enemyGO.GetComponent<EnemyAI>().SetTarget(playerController);
            playerController.SetEnemy(enemyGO.GetComponent<EnemyController>());
            playerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);
            GameObject followerGO = GameObject.FindGameObjectWithTag("Follower");
            if (followerGO != null)
            {
                followerGO.GetComponent<FollowerAI>().SetTarget(playerGO, enemyGO);
                followerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);
            }

            // Check whether to display quest window
            QuestList questList = playerGO.GetComponent<QuestList>();
            if(questList.GetActiveQuestCount() > 0)
            {
                questWindow.SetActive(true);
            }

            // Activate battle
            introCoroutine = StartCoroutine(BeginBattle());
        }

        protected GameObject SpawnNewEnemy()
        {
            // Choose random enemy from list
            int randomEnemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            BaseStats selectedEnemy = enemyPrefabs[randomEnemyIndex];
            return Instantiate(selectedEnemy, transform, false).gameObject;
        }

        protected GameObject SpawnNewEnemy(CharacterClass enemyType)
        {
            // Find enemy type from list
            foreach (var enemyPrefab in enemyPrefabs)
            {
                if(enemyPrefab.GetClass() == enemyType)
                {
                    return Instantiate(enemyPrefab, transform, false).gameObject;
                }
            }

            // If not found, spawn random available enemy
            return SpawnNewEnemy();
        }

        protected IEnumerator BeginBattle()
        {
            introScreen.SetActive(true);

            // Dialogue from quest checker can pause game at start
            while(isPaused)
            {
                yield return new WaitForSecondsRealtime(0.5f);
            }

            Time.timeScale = 1f;
            IntroFader introFader = introScreen.GetComponent<IntroFader>();
            yield return introFader.BattleIntro(1.5f);
            StartBattle();
            yield return introFader.FadeIntroOverlay();
            introScreen.SetActive(false);
        }

        private void RestartBattle()
        {
            Destroy(enemyGO);
            outroScreen.SetActive(false);
            rewardWindow.SetActive(false);
            postFightButtons.SetActive(false);
            BattleSetup();
        }

        private void RestartBattle(CharacterClass enemyName)
        {
            Destroy(enemyGO);
            outroScreen.SetActive(false);
            rewardWindow.SetActive(false);
            postFightButtons.SetActive(false);
            BattleSetup(enemyName);
        }

        private IEnumerator BattleRewards()
        {
            outroScreen.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            yield return outroScreen.GetComponent<OutroFader>().BattleOutro();
            yield return new WaitForSecondsRealtime(2f);
            rewardWindow.SetActive(true);
            postFightButtons.SetActive(true);
        }

        // PUBLIC

        public void PauseGame()
        {
            Time.timeScale = 0;
            if(isBattleActive)
            {
                pauseScreen.SetActive(true);
                postFightButtons.SetActive(true);
            }
            isPaused = !isPaused;
        }

        public void UnpauseGame()
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
            postFightButtons.SetActive(false);
            isPaused = !isPaused;
        }

        public void ResumeRestart()
        {
            if(isBattleActive)
            {
                UnpauseGame();
            }
            else
            {
                RestartBattle();
            }
        }

        public void ResumeRestart(CharacterClass enemyName)
        {
            RestartBattle(enemyName);
        }

        public void DialoguePause()
        {
            // Split this function into another method. From dialogue trigger
            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
            }
            PauseGame();
        }

        public void DialogueResume()
        {
            UnpauseGame();
            introCoroutine = StartCoroutine(BeginBattle());
        }

        public void StartBattle()
        {
            isBattleActive = true;
            isPaused = false;
            battleStart.Invoke();
        }

        public void EndBattle(string tag)
        {
            isBattleActive = false;
            isPaused = false;
            questWindow.SetActive(false);
            Time.timeScale = 0.3f;
            if (tag != "Player")
            {
                GetComponent<LocationManager>().DecrementDistanceRemaining();
                string s = enemyGO.GetComponent<BaseStats>().GetStatText(Stat.Name);
                playerGO.GetComponent<PlayerBattleStats>().AddEnemyKill(s, location.ToString());
                StartCoroutine(BattleRewards());
            }
            else
            {
                // Coroutine for losing screen
                var outroText = outroScreen.GetComponentInChildren<TextMeshProUGUI>();
                outroText.text = "DEFEATED";
                Inventory inventory = enemyGO.GetComponent<Inventory>();
                inventory.ClearInventory();
                
                StartCoroutine(BattleRewards());
            }
        }

        public bool IsBattleActive()
        {
            return isBattleActive;
        }
    }
}
