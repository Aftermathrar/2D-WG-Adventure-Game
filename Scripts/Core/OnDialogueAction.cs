using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Core
{
    public enum OnDialogueAction
    {
        None,
        GiveQuest,
        CompleteObjective,
        GiveItem,
        StartBattle,
        EndBattle,
        PauseBattle,
        ResumeBattle,
        MoveWindow,
        LocationAvailable
    }
}
