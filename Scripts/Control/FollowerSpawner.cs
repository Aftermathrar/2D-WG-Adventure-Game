using System.Collections;
using System.Collections.Generic;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
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

        public SaveableClone SpawnActiveFollower(CharacterClass followerClass, string followerUUID, object state = null) 
        {
            foreach (var prefab in followerPrefabs)
            {
                if(prefab.GetClass() == followerClass)
                {
                    Transform followerTransform = Instantiate(prefab, parentTransform).transform;
                    SaveableClone saveableClone = followerTransform.GetComponent<SaveableClone>();
                    saveableClone.SetUniqueIdentifier(followerUUID);
                    if(state != null) saveableClone.RestoreState(state);
                    followerTransform.SetSiblingIndex(siblingIndex);

                    return saveableClone;
                }
            }
            return null;
        }

        public SaveableClone SpawnBackgroundFollower(CharacterClass followerClass, string followerUUID, object state = null)
        {
            if(isCombat) return null;

            foreach (var prefab in nonCombatPrefabs)
            {
                if (prefab.GetClass() == followerClass)
                {
                    BaseStats followerBase = Instantiate(prefab, parentTransformBackground);
                    SaveableClone saveableClone = followerBase.GetComponent<SaveableClone>();
                    saveableClone.SetUniqueIdentifier(followerUUID);
                    if(state != null) saveableClone.RestoreState(state);

                    return saveableClone;
                }
            }
            return null;
        }
    }
}
