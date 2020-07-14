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
        [SerializeField] Transform battleCanvas;
        [SerializeField] BaseStats[] followerPrefabs;
        [SerializeField] BaseStats[] enemyPrefabs;
        [SerializeField] UnityEvent battleStart;
        GameObject playerGO;
        GameObject enemyGO;
        GameObject followerGO;

        private void Awake()
        {
            // Spawn follower
            FollowerRole companionToSpawn = new FollowerRole();
            companionToSpawn = followers.GetFollowerIdentifier(FollowerPosition.Combat);
            string followerUUID = companionToSpawn.Identifier;

            if (followerUUID == string.Empty)
            {
                int randomFollowerIndex = UnityEngine.Random.Range(0, followerPrefabs.Length);
                BaseStats selectedFollower = followerPrefabs[randomFollowerIndex];
                followerGO = Instantiate(selectedFollower, battleCanvas).gameObject;

                companionToSpawn.FollowerClass = followerGO.GetComponent<BaseStats>().GetClass();
                companionToSpawn.Identifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();

                string followerIdentifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();
                followers.AddNewFollower(FollowerPosition.Combat, companionToSpawn);
            }
            else
            {
                foreach (BaseStats healClass in followerPrefabs)
                {
                    if (companionToSpawn.FollowerClass == healClass.GetClass())
                    {
                        followerGO = Instantiate(healClass, battleCanvas).gameObject;
                        followerGO.GetComponent<SaveableEntity>().SetUniqueIdentifier(followerUUID);
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
            // Choose random enemy from list
            int randomEnemyIndex= UnityEngine.Random.Range(0, enemyPrefabs.Length);
            BaseStats selectedEnemy= enemyPrefabs[randomEnemyIndex];

            // Find player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();

            // Spawn enemy
            enemyGO = Instantiate(selectedEnemy, battleCanvas).gameObject;
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

        private IEnumerator BeginBattle()
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
            }
        }
    }
}
