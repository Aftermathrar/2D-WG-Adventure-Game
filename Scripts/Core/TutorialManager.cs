using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Character;
using ButtonGame.Combat;
using ButtonGame.Dialogue;
using ButtonGame.Inventories;
using ButtonGame.Locations;
using ButtonGame.Quests;
using ButtonGame.SceneManagement;
using ButtonGame.Stats.Enums;
using ButtonGame.UI;
using ButtonGame.UI.EffectIcon;
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
        [SerializeField]
        List<EquipableItem> startingGear = new List<EquipableItem>();
        [SerializeField]
        HitTimerSpawner hitTimerSpawner;
        [SerializeField]
        EffectIconSpawner effectIconSpawner;
        QuestList questList;
        Health playerHealth;
        Mana playerMana;
        Equipment playerEquipment;

        AIConversant convo = null;
        int battleCounter = 0;
        int atkCounter = 0;
        List<AtkIconScript> atkButtons = new List<AtkIconScript>();
        Coroutine coTutorial;
        bool isFading = false;

        private void Awake()
        {
            // Forcefully skip spawning follower
            convo = GetComponent<AIConversant>();
            playerGO = GameObject.FindGameObjectWithTag("Player");
            questList = playerGO.GetComponent<QuestList>();
        }

        private void Start()
        {
            convo.StartDialogue(dialogues[0]);
        }

        protected override void Update() 
        {
            if(playerHealth != null && playerHealth.GetPercentage() < 50f)
            {
                playerHealth.GainAttribute(100f);
            }
            if(playerMana != null && playerMana.GetPercentage() < 20f)
            {
                playerMana.GainAttribute(50f);
            }


            Quest quest = Quest.GetByName("Setting out");
            if (questList.HasQuest(quest) && !isFading)
            {
                isFading = true;
                StartCoroutine(EndTutorialFade(1f));
            }
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                quest = Quest.GetByName("Understanding your skills");
                // if (questList.HasObjectiveCompleted(quest, "Use Punch"))
                if(atkCounter == 6 && !questList.HasQuestCompleted("Understanding your skills"))
                {
                    questList.CompleteObjective(quest, "Finish the demonstration");
                    playerGO.GetComponent<PlayerConversant>().Quit();
                }
            }
        }

        public void BeginTutorialBattle()
        {
            LocationList location = GetComponent<LocationManager>().GetCurrentLocation();
            Debug.Log(location.ToString());
            enemyPrefabs = enemySpawnDB.GetEnemyList(location);
            CharacterClass nextEnemy = enemyPrefabs[battleCounter].GetClass();
            BattleSetup(nextEnemy);
            battleCounter++;
        }

        private void BattleSetup(CharacterClass enemyName)
        {
            // Find and activate player
            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            playerHealth = playerGO.GetComponent<Health>();
            playerMana = playerGO.GetComponent<Mana>();
            playerEquipment = playerGO.GetComponent<Equipment>();
            playerHUD.SetActive(true);
            playerAtkButtons.SetActive(true);
            questList.onQuestUpdated += QuestCompleteCheck;

            // Spawn starting equipment
            foreach (EquipableItem equipableItem in startingGear)
            {
                int equipIndex = playerEquipment.TryAddItem(equipableItem);
                if (equipIndex >= 0)
                {
                    EquipLocation equipLocation = equipableItem.GetAllowedEquipLocation();
                    playerEquipment.AddItem(equipLocation, equipableItem, equipIndex);
                }
            }
            playerHealth.RecalculateMaxHealth();
            playerMana.RecalculateMaxMana();

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
            Quest quest = Quest.GetByName("Understanding your skills");
            if(atk.name == "Skill 1")
            {
                if(!questList.HasObjectiveCompleted(quest, "Use Aura Blast"))
                {
                    questList.CompleteObjective(quest, "Use Aura Blast");
                    playerGO.GetComponent<PlayerConversant>().Quit();
                    StartCoroutine(DelayStartDialogue(2f, dialogues[1], 1));
                }
            }
            else if(atk.name == "Skill 2" && !questList.HasObjectiveCompleted(quest, "Use Magic Missiles"))
            {
                questList.CompleteObjective(quest, "Use Magic Missiles");
                playerGO.GetComponent<PlayerConversant>().Quit();
                StartCoroutine(DelayStartDialogue(2f, dialogues[2], -1));
            }
            else if(atk.name == "Skill 9" && !questList.HasObjectiveCompleted(quest, "Use Magic Shield"))
            {
                questList.CompleteObjective(quest, "Use Magic Shield");
                playerGO.GetComponent<PlayerConversant>().Quit();
                StartCoroutine(DelayStartDialogue(2f, dialogues[3], -1));
            }
            else if(atk.name == "Skill 7" && !questList.HasObjectiveCompleted(quest, "Use Punch"))
            {
                questList.CompleteObjective(quest, "Use Punch");
                playerGO.GetComponent<PlayerConversant>().Quit();

                StartCoroutine(DelayStartDialogue(3f, dialogues[4], -1));
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
                case 5:
                    foreach (var atk in atkButtons)
                    {
                        atk.gameObject.SetActive(true);
                        atk.enabled = true;

                        atk.StartBattle();
                    }
                    playerGO.GetComponent<PlayerController>().SetEnemy(enemyGO.GetComponent<EnemyController>());
                    break;
                 case 6:
                    Debug.Log("case 6 activated??");
                     break;
            }
            atkCounter++;
        }

        private void QuestCompleteCheck()
        {
            if(questList.HasQuestCompleted("Understanding your skills") && !questList.HasQuest(Quest.GetByName("Setting out")))
            {
                playerGO.GetComponent<Fighter>().enabled = false;
                foreach (Transform item in hitTimerSpawner.transform)
                {
                    hitTimerSpawner.ReturnToStack(item.GetComponent<HitTimer>());
                }
                effectIconSpawner.DeactivateAllIcons();
                playerHUD.SetActive(false);
                playerAtkButtons.SetActive(false);
                Destroy(enemyGO);
                StopCoroutine(coTutorial);
                coTutorial = StartCoroutine(DelayStartDialogue(1f, dialogues[5], -1));
            }
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

        private IEnumerator EndTutorialFade(float timeDelay)
        {
            yield return new WaitForSeconds(timeDelay);
            GetComponentInChildren<ChangeSceneButton>().ChangeScene();
        }

        private IEnumerator DisableSkills(int[] atkIndexes)
        {
            foreach (int i in atkIndexes)
            {
                atkButtons[i].StartBattle();
                atkButtons[i].enabled = false;
            }
            yield return null;
            foreach (int i in atkIndexes)
            {
                atkButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
