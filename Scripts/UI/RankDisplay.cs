using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ButtonGame.Attributes;

namespace ButtonGame.UI
{
    public class RankDisplay : MonoBehaviour
    {
        [SerializeField] bool isPlayer = false;
        [SerializeField] TextMeshProUGUI rankText;

        private void Start() 
        {
            if(rankText == null) return;
            string sRank;

            if(isPlayer)
            {
                sRank = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().GetPlayerInfo("rank");
            }
            else
            {
                GameObject followerGO = GameObject.FindGameObjectWithTag("Follower");
                if(followerGO == null)
                {
                    sRank = "";
                }
                else
                {
                    sRank = followerGO.GetComponent<NPCInfo>().GetCharacterInfo("rank");
                }
            }
            
            rankText.text = sRank;
        }
    }
}
