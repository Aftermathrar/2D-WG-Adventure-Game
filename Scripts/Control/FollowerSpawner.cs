using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using UnityEngine;

namespace ButtonGame.Control
{
    public class FollowerSpawner : MonoBehaviour
    {
        [SerializeField] FollowerCollection followers;
        [SerializeField] BaseStats[] followerPrefabs;
        [SerializeField] Transform HUDTransform;

        GameObject followerGO = null;

        private void Awake()
        {
            FollowerRole companionToSpawn = new FollowerRole();
            companionToSpawn = followers.GetFollowerIdentifier(FollowerPosition.Combat);
            string followerUUID = companionToSpawn.Identifier;

            // If UUID is blank, spawn a random NPC from the prefabs and register it
            if(followerUUID == string.Empty)
            {
                int randomFollowerIndex = UnityEngine.Random.Range(0, followerPrefabs.Length);
                BaseStats selectedFollower = followerPrefabs[randomFollowerIndex];
                followerGO = Instantiate(selectedFollower, HUDTransform).gameObject;

                FollowerRole newCompanion = new FollowerRole();
                newCompanion.FollowerClass = followerGO.GetComponent<BaseStats>().GetClass();
                newCompanion.Identifier = followerGO.GetComponent<SaveableEntity>().GenerateNewUniqueIdentifier();

                followers.AddNewFollower(FollowerPosition.Combat, newCompanion);
            }
            else // Spawn NPC from SO Dictionary
            {
                foreach (BaseStats healClass in followerPrefabs)
                {
                    if(companionToSpawn.FollowerClass == healClass.GetClass())
                    {
                        followerGO = Instantiate(healClass, HUDTransform).gameObject;
                        followerGO.GetComponent<SaveableEntity>().SetUniqueIdentifier(followerUUID);
                    }
                }
            }
        }
    }
}
