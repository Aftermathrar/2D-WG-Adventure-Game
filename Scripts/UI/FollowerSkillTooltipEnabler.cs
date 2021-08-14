using System.Collections;
using System.Collections.Generic;
using ButtonGame.Core;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.UI.Stats;
using UnityEngine;

namespace ButtonGame.UI
{
    public class FollowerSkillTooltipEnabler : MonoBehaviour
    {
        List<FollowerSkillDisplay> skillDisplays = new List<FollowerSkillDisplay>();

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                skillDisplays.Add(transform.GetChild(i).GetComponent<FollowerSkillDisplay>());
            }
        }

        private void OnEnable()
        {
            ResetSkillTooltips();
        }

        private void ResetSkillTooltips()
        {
            foreach (var skillDisplay in skillDisplays)
            {
                skillDisplay.RedrawSkillInfo();
            }
        }

        public void OnFollowerChange()
        {
            FollowerManager followerManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<FollowerManager>();
            
            CharacterClass followerClass;
            if(followerManager.GetActiveFollowerClass(out followerClass))
            {
                foreach (var skillDisplay in skillDisplays)
                {
                    skillDisplay.OnFollowerChange(followerClass);
                }
            }
        }
    }
}
