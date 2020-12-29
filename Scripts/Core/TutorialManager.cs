using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Character;
using ButtonGame.Combat;
using ButtonGame.Dialogue;
using ButtonGame.Quests;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace ButtonGame.Core
{
    public class TutorialManager : LevelManager
    {
        [SerializeField]
        GameObject playerHUD;
        [SerializeField]
        GameObject playerAtkButtons;
        [SerializeField]
        GameObject questUI;
        [SerializeField]
        ButtonGame.Dialogue.Dialogue[] dialogues;
        QuestList questList;
        Health playerHealth;
        Mana playerMana;

        AIConversant convo = null;
        int battleCounter = 0;
        int atkCounter = 0;
        List<AtkIconScript> atkButtons = new List<AtkIconScript>();
        Coroutine coTutorial;

        private void Awake() 
        {
            // Forcefully skip spawning follower
            convo = GetComponent<AIConversant>();
        }

        private void Start() 
        {
            convo.StartDialogue(dialogues[0]);
        }

        private void Update() 
        {
            if(playerHealth != null && playerHealth.GetPercentage() < 50f)
            {
                playerHealth.GainAttribute(100f);
            }
            if(playerMana != null && playerMana.GetPercentage() < 20f)
            {
                playerMana.GainAttribute(50f);
            }
        }

        public void BeginTutorialBattle()
        {
            CharacterClass nextEnemy = enemyPrefabs[battleCounter].GetClass();
            BattleSetup(nextEnemy);
            battleCounter++;
        }

        private void BattleSetup()
        {
            // Find player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            questList = playerGO.GetComponent<QuestList>();

            // Spawn enemy
            enemyGO = SpawnNewEnemy();
            enemyGO.transform.SetAsFirstSibling();

            // Assign opponent to scripts
            enemyGO.GetComponent<CombatEffects>().SetTarget(playerGO);
            enemyGO.GetComponent<EnemyAI>().SetTarget(playerController);
            playerController.SetEnemy(enemyGO.GetComponent<EnemyController>());
            playerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);

            // Activate battle
            StartCoroutine(BeginBattle());
        }

        private void BattleSetup(CharacterClass enemyName)
        {
            // Find and activate player
            playerGO = GameObject.FindGameObjectWithTag("Player");
            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            questList = playerGO.GetComponent<QuestList>();
            playerHealth = playerGO.GetComponent<Health>();
            playerMana = playerGO.GetComponent<Mana>();
            playerHUD.SetActive(true);
            playerAtkButtons.SetActive(true);

            // Spawn enemy
            enemyGO = SpawnNewEnemy(enemyName);
            enemyGO.transform.SetAsFirstSibling();

            // Assign opponent to scripts
            enemyGO.GetComponent<CombatEffects>().SetTarget(playerGO);
            enemyGO.GetComponent<EnemyAI>().SetTarget(playerController);
            playerController.SetEnemy(enemyGO.GetComponent<EnemyController>());
            playerGO.GetComponent<CombatEffects>().SetTarget(enemyGO);

            bool isStartingSetup = (battleCounter == 0);
            if(isStartingSetup)
            {
                foreach (Transform item in playerAtkButtons.transform)
                {
                    atkButtons.Add(item.GetComponent<AtkIconScript>());
                    item.gameObject.SetActive(false);
                    item.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        AtkSkillUsed(item);
                    });
                }
            }

            // Activate battle
            coTutorial = StartCoroutine(BeginBattle(isStartingSetup));
        }

        private IEnumerator BeginBattle(bool shouldPause)
        {
            StartBattle();
            if(shouldPause)
            {
                yield return new WaitForSeconds(2.5f);
                PauseGame();
            }
            coTutorial = null;
        }

        private void RestartBattle()
        {
            Destroy(enemyGO);
            CharacterClass nextEnemy = enemyPrefabs[battleCounter].GetClass();
            BattleSetup(nextEnemy);
            battleCounter++;
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
            isPaused = true;
        }

        new public void UnpauseGame()
        {
            if(coTutorial != null)
            {
                StopCoroutine(coTutorial);
            }
            Time.timeScale = 1f;
            isPaused = false;
            ActivateSkills();
        }

        private void AtkSkillUsed(Transform atk)
        {
            if(atk.name == "Skill 1")
            {
                Quest quest = Quest.GetByName("Understanding your skills");
                if(!questList.HasObjectiveCompleted(quest, "Use Aura Blast"))
                {
                    questList.CompleteObjective(quest, "Use Aura Blast");
                    playerGO.GetComponent<PlayerConversant>().Quit();
                    StartCoroutine(DelayStartDialogue(2f, dialogues[1], 1));
                }
            }
            else if(atk.name == "Skill 2" && !isPaused)
            {
                Quest quest = Quest.GetByName("Understanding your skills");
                if (!questList.HasObjectiveCompleted(quest, "Use Magic Missiles"))
                {
                    questList.CompleteObjective(quest, "Use Magic Missiles");
                    playerGO.GetComponent<PlayerConversant>().Quit();
                    StartCoroutine(DelayStartDialogue(2f, dialogues[2], -1));
                }
            }
            else if(atk.name == "Skill 9" && !isPaused)
            {
                Quest quest = Quest.GetByName("Understanding your skills");
                if (!questList.HasObjectiveCompleted(quest, "Use Magic Shield"))
                {
                    questList.CompleteObjective(quest, "Use Magic Shield");
                    playerGO.GetComponent<PlayerConversant>().Quit();
                    StartCoroutine(DelayStartDialogue(2f, dialogues[3], -1));
                }
            }
        }

        private void ActivateSkills()
        {
            switch (atkCounter)
            {
                case 1:
                    questUI.SetActive(true);
                    atkButtons[0].gameObject.SetActive(true);
                    atkButtons[0].StartBattle();
                    break;
                case 2:
                    atkButtons[1].gameObject.SetActive(true);
                    atkButtons[1].StartBattle();
                    break;
                case 3:
                    coTutorial = StartCoroutine(DisableSkills(new int[] {0, 1}));

                    atkButtons[8].gameObject.SetActive(true);
                    atkButtons[8].StartBattle();
                    RestartBattle();
                    break;
                case 4:
                    if(coTutorial != null) StopCoroutine(coTutorial);
                    
                    coTutorial = StartCoroutine(DisableSkills(new int[] { 8 }));
                    atkButtons[6].gameObject.SetActive(true);
                    atkButtons[6].StartBattle();
                    atkButtons[7].gameObject.SetActive(true);
                    atkButtons[7].StartBattle();
                    playerGO.GetComponent<PlayerController>().SetEnemy(enemyGO.GetComponent<EnemyController>());
                    break;

            }
            atkCounter++;
        }

        private IEnumerator DelayStartDialogue(float timeDelay, ButtonGame.Dialogue.Dialogue dialogue, int atkIndex)
        {
            yield return new WaitForSeconds(timeDelay);
            convo.StartDialogue(dialogue);
            if(atkIndex > 0 && atkIndex < atkButtons.Count)
            {
                atkButtons[atkIndex].gameObject.SetActive(true);
            }
        }

        private IEnumerator DisableSkills(int[] atkIndexes)
        {
            foreach (int i in atkIndexes)
            {
                atkButtons[i].StartBattle();
            }
            yield return null;
            foreach (int i in atkIndexes)
            {
                atkButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
