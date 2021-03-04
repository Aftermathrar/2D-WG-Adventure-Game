using System.Collections;
using System.Collections.Generic;
using ButtonGame.Control;
using ButtonGame.Saving;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Core
{
    [RequireComponent(typeof(FollowerSpawner))]
    public class FollowerManager : MonoBehaviour, ISaveable
    {
        [SerializeField] string activeFollowerUUID = "";
        [SerializeField] CharacterClass[] classChoices; 
        [SerializeField] List<FollowerEntry> followers = new List<FollowerEntry>();
        [SerializeField] Transform npcContainer = null;
        FollowerSpawner followerSpawner = null;

        private void Awake() 
        {
            followerSpawner = GetComponent<FollowerSpawner>();
        }

        public void AddNewFollower()
        {
            int randChoice = UnityEngine.Random.Range(0, classChoices.Length);
            AddNewFollower(classChoices[randChoice]);
        }

        public void AddNewFollower(CharacterClass newClass)
        {
            FollowerEntry newFollower = new FollowerEntry();
            SaveableEntity saveable = GetComponent<SaveableEntity>();

            newFollower.position = (activeFollowerUUID == "") ? FollowerPosition.Combat : FollowerPosition.Home;
            newFollower.followerClass = newClass;
            newFollower.name = "Test NPC " + followers.Count.ToString();
            newFollower.identifier = saveable.GenerateNewUniqueIdentifier("");

            followers.Add(newFollower);
            if(newFollower.position == FollowerPosition.Combat)
            {
                RegisterActiveFollower(newFollower);
            }
            else
            {
                RegisterBackgroundFollower(newFollower);
            }
        }

        public bool GetActiveFollower(out string followerUUID)
        {
            followerUUID = activeFollowerUUID;
            return (followerUUID != "") ? true : false;
        }

        private void RegisterActiveFollower(FollowerEntry newActiveFollower)
        {
            activeFollowerUUID = newActiveFollower.identifier;
            followerSpawner.SpawnActiveFollower(newActiveFollower.followerClass, activeFollowerUUID);
        }

        private void RegisterBackgroundFollower(FollowerEntry newFollower)
        {
            followerSpawner.SpawnBackgroundFollower(newFollower.followerClass, newFollower.identifier);
        }

        public object CaptureState()
        {
            foreach (var follower in followers)
            {
                if(follower.position == FollowerPosition.Combat)
                {
                    follower.state = GameObject.FindGameObjectWithTag("Follower").GetComponent<SaveableEntity>().CaptureState();
                    continue;
                }
                else if(npcContainer != null)
                {
                    foreach (Transform item in npcContainer.transform)
                    {
                        SaveableEntity saveableEntity = item.GetComponent<SaveableEntity>();
                        if (saveableEntity.GetUniqueIdentifier() == follower.identifier)
                        {
                            follower.state = saveableEntity.CaptureState();
                            break;
                        }
                    }
                }
            }

            return followers;
        }

        public void RestoreState(object state)
        {
            followers = (List<FollowerEntry>)state;
            foreach (var follower in followers)
            {
                if(follower.position == FollowerPosition.Combat)
                {
                    activeFollowerUUID = follower.identifier;
                    followerSpawner.SpawnActiveFollower(follower.followerClass, activeFollowerUUID, follower.state);
                }
                else
                {
                    followerSpawner.SpawnBackgroundFollower(follower.followerClass, follower.identifier, follower.state);
                }
            }
        }
        
        [System.Serializable]
        private class FollowerEntry
        {
            public FollowerPosition position;
            public CharacterClass followerClass;
            public string name;
            public string identifier;
            public object state;
        }
    }
}
