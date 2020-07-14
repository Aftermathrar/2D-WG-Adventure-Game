using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using UnityEngine;

namespace ButtonGame.Control
{
    public class FollowerSpawner : MonoBehaviour// , ISaveable
    {
        [SerializeField] FollowerCollection followers;
        [SerializeField] BaseStats[] followerPrefabs;
        [SerializeField] Transform HUDTransform;

        // Dictionary<string, FollowerRole> followerDict = null;

        GameObject followerGO = null;
        // string followerIdentifier = null;

        private void Awake()
        {
            FollowerRole companionToSpawn = new FollowerRole();
            companionToSpawn = followers.GetFollowerIdentifier(FollowerPosition.Combat);
            string followerUUID = companionToSpawn.Identifier;

            if(followerUUID == string.Empty)
            {
                // Debug.Log("Making new follower");
                int randomFollowerIndex = UnityEngine.Random.Range(0, followerPrefabs.Length);
                BaseStats selectedFollower = followerPrefabs[randomFollowerIndex];
                followerGO = Instantiate(selectedFollower, HUDTransform).gameObject;

                FollowerRole newCompanion = new FollowerRole();
                newCompanion.FollowerClass = followerGO.GetComponent<BaseStats>().GetClass();
                newCompanion.Identifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();

                // followerDict = new Dictionary<string, FollowerRole>();
                // followerDict[followerIdentifier] = newCompanion;
                followers.AddNewFollower(FollowerPosition.Combat, newCompanion);
            }
            else
            {
                foreach (BaseStats healClass in followerPrefabs)
                {
                    if(companionToSpawn.FollowerClass == healClass.GetClass())
                    {
                        followerGO = Instantiate(healClass, HUDTransform).gameObject;

                        // followerDict = new Dictionary<string, FollowerRole>();
                        // followerIdentifier = followerUUID;
                        followerGO.GetComponent<SaveableEntity>().SetUniqueIdentifier(followerUUID);
                        // followerDict[followerUUID] = newCompanion;
                    }
                }
            }
        }

        // private void SpawnFollower()
        // {
        //     foreach (var followerIdentifier in followerDict)
        //     {
        //         if (followerIdentifier.Value.role == "Combat")
        //         {
        //             if(followerIdentifier.Value.followerClass == CharacterClass.Priest)
        //             {
        //                 followerGO = Instantiate(followerPrefabs[0], HUDTransform).gameObject;
        //                 return;
        //             }
        //             else
        //             {
        //                 followerGO = Instantiate(followerPrefabs[1], HUDTransform).gameObject;
        //                 return;
        //             }
        //         }
        //     }
        // }

        // public string GetFollower()
        // {
        //     return followerIdentifier;
        // }

        // public object CaptureState()
        // {
        //     Debug.Log("Saving: " + followerDict[followerIdentifier].followerClass.ToString() + " " + followerDict[followerIdentifier].role + " " + followerIdentifier);
        //     return followerDict;
        // }

        // public void RestoreState(object state)
        // {
        //     followerDict = (Dictionary<string, FollowerRole>)state;
        //     if(followerDict == null) return;

        //     SpawnFollower();
        // }
    }
}
