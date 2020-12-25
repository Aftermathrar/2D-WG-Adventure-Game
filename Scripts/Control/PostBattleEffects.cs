using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Combat;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.UI.EffectIcon;
using UnityEngine;

namespace ButtonGame.Control
{
    public class PostBattleEffects : MonoBehaviour
    {
        [SerializeField] EffectDB effectDB;
        [SerializeField] EffectIconDB fxIconDB;
        [SerializeField] CombatEffects buffEffects = null;
        [SerializeField] EffectIconSpawner fxIconSpawner = null;

        int fxIconCount;
        Dictionary<string, float[]> buffList = new Dictionary<string, float[]>();

        private void OnEnable() 
        {
            if(buffEffects == null || fxIconSpawner == null)
                return;

            buffList = buffEffects.GetBuffDictionary();
            RestoreIcons();
        }

        private void OnDisable() 
        {
            // RemoveBuffs();
        }

        private void RestoreIcons()
        {
            if (buffList.Count == 0) return;

            fxIconCount = 0;
            foreach (string id in buffList.Keys)
            {
                EffectName fxName = (EffectName)Enum.Parse(typeof(EffectName), id);
                fxIconSpawner.Spawn(id, fxIconCount, fxIconDB.GetSprite(fxName));
                float fxDuration = float.Parse(effectDB.GetEffectStat(EffectStat.Duration, fxName));
                float fillPercent = buffList[id][1] / fxDuration;
                float timeRemaining = fxDuration - buffList[id][1];
                fxIconSpawner.UpdateIconFill(id, fillPercent, timeRemaining);

                fxIconCount += 1;
            }
        }

        private void RemoveBuffs()
        {
            foreach (string id in buffList.Keys)
            {
                fxIconCount -= 1;
                fxIconSpawner.ReturnToPool(id);
            }
        }
    }
}