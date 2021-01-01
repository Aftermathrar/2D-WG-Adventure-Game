using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ButtonGame.Stats;
using ButtonGame.Character;
using ButtonGame.Combat;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using ButtonGame.Control;
using ButtonGame.Saving;
using TMPro;
using ButtonGame.Inventories;

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
        [SerializeField] FollowerCollection followers;
        [SerializeField] BaseStats[] followerPrefabs;
        [SerializeField] protected BaseStats[] enemyPrefabs;
        [SerializeField] UnityEvent battleStart;
        protected GameObject playerGO;
        protected GameObject enemyGO;
        GameObject followerGO;

        private void Awake()
        {
            // Spawn follower
            Debug.Log("Level Manager");
            FollowerRole companionToSpawn = new FollowerRole();
            companionToSpawn = followers.GetFollowerIdentifier(FollowerPosition.Combat);
            string followerUUID = companionToSpawn.Identifier;

            if (followerUUID == string.Empty)
            {
                // Handle no follower without spawning a random new one
                // int randomFollowerIndex = UnityEngine.Random.Range(0, followerPrefabs.Length);
                // BaseStats selectedFollower = followerPrefabs[randomFollowerIndex];
                // followerGO = Instantiate(selectedFollower, transform, false).gameObject;

                // companionToSpawn.FollowerClass = followerGO.GetComponent<BaseStats>().GetClass();
                // companionToSpawn.Identifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();

                // string followerIdentifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();
                // followers.AddNewFollower(FollowerPosition.Combat, companionToSpawn);
            }
            else
            {
                foreach (BaseStats healClass in followerPrefabs)
                {
                    if (companionToSpawn.FollowerClass == healClass.GetClass())
                    {
                        followerGO = Instantiate(healClass, transform, false).gameObject;
                        followerGO.transform.SetSiblingIndex(1);
                        followerGO.GetComponent<SaveableEntity>().SetUniqueIdentifier(followerUUID);
                        break;
                    }
                }
            }
        }

        private void Start()
        {
            BattleSetup();
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

        private void BattleSetup()
        {
            // Find player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();

            // Spawn enemy
            enemyGO = SpawnNewEnemy();
            enemyGO.transform.SetAsFirstSibling();

            // Assign opponent to scripts
            enemyGO.GetComponent<CombatEffects>().SetTarget(playerGO);
            enemyGO.GetComponent<EnemyAI>().SetTarget(playerController);
            playerController.SetEnemy(enemyGO.GetComponent<EnemyController>());
            playerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);
            if(followerGO != null) 
            {
                followerGO.GetComponent<FollowerAI>().SetTarget(playerGO, enemyGO);
                followerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);
            }

            // Activate battle
            StartCoroutine(BeginBattle());
        }

        private void BattleSetup(CharacterClass enemyName)
        {
            // Find player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();

            // Spawn enemy
            enemyGO = SpawnNewEnemy(enemyName);
            enemyGO.transform.SetAsFirstSibling();

            // Assign opponent to scripts
            enemyGO.GetComponent<CombatEffects>().SetTarget(playerGO);
            enemyGO.GetComponent<EnemyAI>().SetTarget(playerController);
            playerController.SetEnemy(enemyGO.GetComponent<EnemyController>());
            playerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);
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

        protected GameObject SpawnNewEnemy(CharacterClass enemyName)
        {
            // Find enemy type from list
            foreach (var enemyPrefab in enemyPrefabs)
            {
                if(enemyPrefab.GetClass() == enemyName)
                {
                    return Instantiate(enemyPrefab, transform, false).gameObject;
                }
            }
            Debug.Log("Enemy of type " + enemyName.ToString() + " not found! Spawning random enemy.");

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
