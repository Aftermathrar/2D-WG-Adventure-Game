using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Inventories;
using ButtonGame.Stats;
using UnityEngine;

namespace ButtonGame.UI.Stats
{
    public class FollowerStatDisplay : StatDisplay
    {
        [SerializeField] FollowerManager followerManager = null;

        protected override void Start()
        {
            followerManager ??= GameObject.FindGameObjectWithTag("LevelManager").GetComponent<FollowerManager>();
            GetFollowerComponents();

            characterEquipment.equipmentUpdated += RedrawStatDisplay;
        }

        protected override void OnEnable()
        {
            GetFollowerComponents();
        }

        private void GetFollowerComponents()
        {
            GameObject followerGO;
            if(followerManager.GetActiveFollowerObject(out followerGO))
            {
                characterEquipment = followerGO.GetComponent<Equipment>();
                characterStats = followerGO.GetComponent<BaseStats>();
                RedrawStatDisplay();
            }
        }

        public override void OnEffectChange()
        {
            GetFollowerComponents();
        }
    }
}
