using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Core;
using ButtonGame.Locations;
using ButtonGame.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostFightOptions : MonoBehaviour
{
    [SerializeField] Button destinationButton = null;
    [SerializeField] Button restButton = null;
    [SerializeField] ChangeSceneButton loadDestination = null;
    [SerializeField] ChangeSceneButton loadLastLocation = null;
    [SerializeField] Health playerHealth = null;
    [SerializeField] Mana playerMana = null;
    [SerializeField] LevelManager battleManager = null;
    [SerializeField] LocationManager locationManager = null;
    [SerializeField] TextMeshProUGUI resumeText;
    [SerializeField] TextMeshProUGUI destinationText;

    bool isSetupComplete = false;

    private void Awake() 
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
        playerMana = player.GetComponent<Mana>();
        GameObject levelManagerGO = GameObject.FindGameObjectWithTag("LevelManager");
        battleManager = levelManagerGO.GetComponent<LevelManager>();
        locationManager = levelManagerGO.GetComponent<LocationManager>();
    }

    private void OnEnable() 
    {
        if(!isSetupComplete)
        {
            SetReturnLocation();
            SetTravelDestination();
            isSetupComplete = true;
        }

        if(battleManager.IsBattleActive())
        {
            destinationButton.gameObject.SetActive(false);
            restButton.gameObject.SetActive(false);
            resumeText.text = "Resume\nBattle";
        }
        else
        {
            bool isCombatDestination = locationManager.IsCombatArea(locationManager.GetTravelDestination()) ?? false;
            if(isCombatDestination)
            {
                destinationButton.gameObject.SetActive(false);
            }
            else
            {
                destinationButton.gameObject.SetActive(true);
                if (locationManager.GetDistanceRemaining() <= 0)
                {
                    destinationButton.interactable = true;
                }
                else
                {
                    destinationButton.interactable = false;
                    destinationText.text = locationManager.GetDistanceRemaining() + " Encounters\nRemaining";
                }
            }
        }
    }

    private void OnDisable()
    {
        destinationButton.gameObject.SetActive(true);
        restButton.gameObject.SetActive(true);
        resumeText.text = "Continue\nExploring";
        destinationText.text = "Reach\nDestination";
    }

    // PUBLIC

    public void SetReturnLocation()
    {
        LocationList returnLocation = locationManager.GetReturnLocation();
        loadLastLocation.SetDestination(returnLocation.ToString());

        bool isCombat = locationManager.IsCombatArea(returnLocation) ?? false;
        int sceneToLoad = isCombat ? 2 : 1;
        loadLastLocation.SetSceneToLoad(sceneToLoad);
    }

    public void SetTravelDestination()
    {
        LocationList destination = locationManager.GetTravelDestination();
        loadDestination.SetDestination(destination.ToString());

        bool isCombat = locationManager.IsCombatArea(destination) ?? false;
        int sceneToLoad = isCombat ? 2 : 1;
        loadDestination.SetSceneToLoad(sceneToLoad);
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

        GameObject followerObj = GameObject.FindGameObjectWithTag("Follower");
        if(followerObj != null)
        {
            Mana followerMana = followerObj.GetComponent<Mana>();
            followerMana.GainAttribute(followerMana.GetMaxAttributeValue());
        }
    }
}
