using System.Collections;
using System.Collections.Generic;
using ButtonGame.Locations;
using ButtonGame.Saving;
using ButtonGame.Stats;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Control
{
    public class FeedeeSpawner : MonoBehaviour
    {
        [SerializeField] Transform parentTransform;
        [SerializeField] BaseFeedeeStats[] feedeePrefabs;
        [SerializeField] TownNodeList activeNode;

        public SaveableClone SpawnNewNPC(FeedeeClass feedeeClass, string feedeeUUID, object state = null)
        {
            foreach (var prefab in feedeePrefabs)
            {
                if(prefab.GetClass() == feedeeClass)
                {
                    BaseFeedeeStats feedee = Instantiate(prefab, parentTransform);
                    SaveableClone saveableClone = feedee.GetComponent<SaveableClone>();
                    saveableClone.SetUniqueIdentifier(feedeeUUID);
                    if(state != null) saveableClone.RestoreState(state);

                    return saveableClone;
                }
            }
            return null;
        }

        public void SetActiveNode(TownNodeList newNode)
        {
            activeNode = newNode;
        }
    }
}