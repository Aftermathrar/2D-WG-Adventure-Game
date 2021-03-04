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
        [SerializeField] Transform parentTransform;
        [SerializeField] BaseStats[] followerPrefabs;
        [SerializeField] Transform parentTransformBackground;
        [SerializeField] BaseStats[] nonCombatPrefabs;
        [SerializeField] int siblingIndex = 1;
        [SerializeField] bool isCombat;

        public void SpawnActiveFollower(CharacterClass followerClass, string followerUUID, object state = null) 
        {
            foreach (var prefab in followerPrefabs)
            {
                if(prefab.GetClass() == followerClass)
                {
                    GameObject followerGO = Instantiate(prefab, parentTransform).gameObject;
                    SaveableEntity saveableEntity = followerGO.GetComponent<SaveableEntity>();
                    saveableEntity.SetUniqueIdentifier(followerUUID);
                    if(state != null) saveableEntity.RestoreState(state);
                    followerGO.transform.SetSiblingIndex(siblingIndex);
                }
            }
        }

        public void SpawnBackgroundFollower(CharacterClass followerClass, string followerUUID, object state = null)
        {
            if(isCombat) return;

            foreach (var prefab in nonCombatPrefabs)
            {
                if (prefab.GetClass() == followerClass)
                {
                    GameObject followerGO = Instantiate(prefab, parentTransformBackground).gameObject;
                    SaveableEntity saveableEntity = followerGO.GetComponent<SaveableEntity>();
                    saveableEntity.SetUniqueIdentifier(followerUUID);
                    if(state != null) saveableEntity.RestoreState(state);
                }
            }
        }
    }
}
