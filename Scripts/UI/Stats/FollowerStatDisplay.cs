using System.Collections;
using System.Collections.Generic;
using ButtonGame.Inventories;
using ButtonGame.Stats;
using UnityEngine;

namespace ButtonGame.UI.Stats
{
    public class FollowerStatDisplay : StatDisplay
    {
        protected override void Start()
        {
            statTextLines = GetComponentsInChildren<StatText>();
            GetFollowerComponents();
            RedrawStatDisplay();

            characterEquipment.equipmentUpdated += RedrawStatDisplay;
        }

        protected override void OnEnable()
        {
            statTextLines = GetComponentsInChildren<StatText>();
            GetFollowerComponents();
            RedrawStatDisplay();
        }

        private void GetFollowerComponents()
        {
            GameObject followerGO = GameObject.FindGameObjectWithTag("Follower");

            // disable script updating if no follower found
            if (followerGO == null)
            {
                gameObject.SetActive(false);
                return;
            }

            characterEquipment = followerGO.GetComponent<Equipment>();
            characterStats = followerGO.GetComponent<BaseStats>();
        }
    }
}
