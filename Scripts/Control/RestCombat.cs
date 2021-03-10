using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Core;
using UnityEngine;
using UnityEngine.UI;

public class RestCombat : MonoBehaviour
{
    [SerializeField] Button restButton = null;
    [SerializeField] Health playerHealth = null;
    [SerializeField] Mana playerMana = null;
    [SerializeField] LevelManager battleManager = null;

    private void Awake() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
        playerMana = player.GetComponent<Mana>();
        battleManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
    }

    private void OnEnable() 
    {        
        if(battleManager.IsBattleActive())
        {
            restButton.gameObject.SetActive(false);
        }
    }

    private void OnDisable() 
    {
        restButton.gameObject.SetActive(true);
    }

    public void RestoreAttributes()
    {
        if(playerHealth != null)
        {
            playerHealth.GainAttribute(playerHealth.GetMaxAttributeValue());
        }

        if(playerMana != null)
        {
            playerMana.GainAttribute(playerMana.GetMaxAttributeValue());
        }

        GameObject obj = GameObject.FindGameObjectWithTag("Follower");
        if(obj != null)
        {
            Mana followerMana = obj.GetComponent<Mana>();
            followerMana.GainAttribute(followerMana.GetMaxAttributeValue());
        }
    }
}
