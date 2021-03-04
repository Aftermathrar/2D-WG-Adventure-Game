using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.SceneManagement;
using ButtonGame.Stats;
using ButtonGame.Stats.Follower;
using TMPro;
using UnityEngine;

namespace ButtonGame.Control
{
    public class ChooseFollower : MonoBehaviour
    {
        [SerializeField] FollowerCollection followers;
        [SerializeField] SaveableEntity followerPrefab;

        public void ChooseNewFollower()
        {
            // Delete old save
            SavingWrapper savingWrapper = (SavingWrapper)GameObject.FindObjectOfType(typeof(SavingWrapper));
            if (savingWrapper != null)
            {
                savingWrapper.Delete();
            }

            // Create and register new follower
            FollowerRole followerRole = new FollowerRole();
            followerRole.FollowerClass = followerPrefab.GetComponent<BaseStats>().GetClass();
            followerRole.Identifier = followerPrefab.GenerateNewUniqueIdentifier();
            // followers.AddNewFollower(FollowerPosition.Combat, followerRole);
            GetComponent<ChangeSceneButton>().ChangeScene();
        }
    }
}
