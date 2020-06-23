using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ButtonGame.Stats;

[System.Serializable]
public struct QuestObjective
{
    public BaseStats baseStats;
    // public  { Interact, Kill, Gather }
    // public ObjectiveType objectiveType;
    public string objectiveResume;

    public GameObject interactTarget;

    public string killTarget;
    public int ammountKillTarget;

    public string gatherTarget;
    public int ammountGatherTarget;
}

// [CreateAssetMenu(fileName = "Quest", menuName = "New Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public GameObject questGiver;
    public GameObject questEnder;
    public QuestObjective[] objectives;
}