using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ButtonGame.Character;
using ButtonGame.Combat;
using ButtonGame.Inventories;
using ButtonGame.Locations;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using TMPro;

namespace ButtonGame.Core
{
    public class LevelManager : MonoBehaviour
    {
        // Pause Control
        public static bool isPaused;
        private bool isBattleActive = false;
        [SerializeField] GameObject pauseScreen;
        [SerializeField] GameObject introScreen;
        [SerializeField] GameObject outroScreen;
        [SerializeField] GameObject buttonContainer;
        [SerializeField] GameObject rewardWindow;

        // Battle setup
        [SerializeField] protected EnemySpawnDB enemySpawnDB;
        [SerializeField] UnityEvent battleStart;
        protected GameObject playerGO;
        protected GameObject enemyGO;
        protected BaseStats[] enemyPrefabs;
        LocationList location;

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
            location = GetComponent<LocationManager>().GetCurrentLocation();
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

            // Activate battle
            StartCoroutine(BeginBattle());
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
            Debug.Log("Enemy of type " + enemyType.ToString() + " not found! Spawning random enemy.");

            // Choose random enemy from list
            int randomEnemyIndex = UnityEngine.Random.Range(0, enemyPrefabs.Length);
            BaseStats selectedEnemy = enemyPrefabs[randomEnemyIndex];
            return Instantiate(selectedEnemy, transform, false).gameObject;
        }

        protected IEnumerator BeginBattle()
        {
            Time.timeScale = 1f;
            introScreen.SetActive(true);
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
            buttonContainer.SetActive(false);
            BattleSetup();
        }

        private void RestartBattle(CharacterClass enemyName)
        {
            Destroy(enemyGO);
            outroScreen.SetActive(false);
            rewardWindow.SetActive(false);
            buttonContainer.SetActive(false);
            BattleSetup(enemyName);
        }

        private IEnumerator BattleRewards()
        {
            outroScreen.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
            yield return outroScreen.GetComponent<OutroFader>().BattleOutro();
            yield return new WaitForSecondsRealtime(2f);
            rewardWindow.SetActive(true);
            buttonContainer.SetActive(true);
        }

        private void PauseGame()
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            buttonContainer.SetActive(true);
            isPaused = !isPaused;
        }

        public void UnpauseGame()
        {
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
            buttonContainer.SetActive(false);
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

        public void StartBattle()
        {
            isBattleActive = true;
            isPaused = false;
            battleStart.Invoke();
        }

        public void EndBattle(string tag)
        {
            isBattleActive = false;
            Time.timeScale = 0.3f;
            if (tag != "Player")
            {
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
