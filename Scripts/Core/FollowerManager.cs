using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Control;
using ButtonGame.Events;
using ButtonGame.Saving;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Core
{
    [RequireComponent(typeof(FollowerSpawner))]
    public class FollowerManager : MonoBehaviour, ISaveable
    {
        [SerializeField] GameObject followerEquipToggle;
        [SerializeField] int activeFollowerIndex = -1;
        [SerializeField] CharacterClass[] classChoices; 
        [SerializeField] List<FollowerEntry> followers = new List<FollowerEntry>();
        [SerializeField] List<SaveableClone> followerSaveables = new List<SaveableClone>();
        [SerializeField] GameEvent followerChangeEvent;
        [SerializeField] GameEvent followerEffectEvent;
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
            // Get SaveableEntity component on Manager in order to generate UUID
            SaveableEntity saveable = GetComponent<SaveableEntity>();
            int followerCount = followers.Count;

            newFollower.position = (activeFollowerIndex >= 0) ? FollowerPosition.Home : FollowerPosition.Combat;
            newFollower.followerClass = newClass;
            newFollower.name = "Test NPC " + followerCount.ToString();
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
            
            // FollowerSaveables is reference list to spawned NPCs
            NPCInfo info = followerSaveables[followerCount].GetComponent<NPCInfo>();
            info.SetCharacterInfo("name", newFollower.name);
            info.SetCharacterInfo("rank", "Rank E");
        }

        public void ChangeActiveFollower(int index)
        {
            if (index == activeFollowerIndex) return;

            int i;
            for (i = 0; i < followers.Count; i++)
            {
                if (followers[i].position == FollowerPosition.Combat)
                {
                    followers[i].state = followerSaveables[i].CaptureState();
                    followers[i].position = FollowerPosition.Home;
                    break;
                }
            }

            followers[index].state = followerSaveables[index].CaptureState();
            followers[index].position = FollowerPosition.Combat;
            activeFollowerIndex = index;
            if(activeFollowerIndex < 0)
            {
                followerEquipToggle.SetActive(false);
            }

            // Respawn follower prefabs
            Destroy(followerSaveables[i].gameObject);
            Destroy(followerSaveables[index].gameObject);
            followerSaveables[i] = followerSpawner.SpawnBackgroundFollower(followers[i].followerClass, followers[i].identifier, followers[i].state);
            followerSaveables[index] = followerSpawner.SpawnActiveFollower(followers[index].followerClass, followers[index].identifier, followers[index].state);

            // Remove potential buffs from player
            if (followers[i].followerClass == CharacterClass.Priest)
            {
                followerEffectEvent.RaiseEvent(EffectName.DivineInfusion.ToString());
            }
            else
            {
                followerEffectEvent.RaiseEvent(EffectName.Berserk.ToString());
            }

            followerChangeEvent.RaiseEvent();
        }

        public int GetFollowerCount()
        {
            return followers.Count;
        }

        public bool GetActiveFollower(out string followerUUID)
        {
            if (activeFollowerIndex >= 0)
            {
                followerUUID = followers[activeFollowerIndex].identifier;
                return true;
            }
            followerUUID = "";
            return false;
        }

        public bool GetActiveFollowerIndex(out int returnIndex)
        {
            if(activeFollowerIndex >= 0)
            {
                returnIndex = activeFollowerIndex;
                return true;
            }
            returnIndex = -1;
            return false;
        }

        public bool GetActiveFollowerClass(out CharacterClass returnClass)
        {
            if(activeFollowerIndex >= 0)
            {
                returnClass = followers[activeFollowerIndex].followerClass;
                return true;
            }
            returnClass = CharacterClass.Priest;
            return false;
        }

        public string GetFollowerID(int index)
        {
            return followers[index].identifier;
        }

        public string GetFollowerClass(int index)
        {
            return followers[index].followerClass.ToString();
        }

        public string GetFollowerPosition(int index)
        {
            return followers[index].position.ToString();
        }

        public GameObject GetFollowerObject(int index)
        {
            return followerSaveables[index].gameObject;
        }

        public bool GetActiveFollowerObject(out GameObject returnFollowerObject)
        {
            if(activeFollowerIndex >= 0)
            {
                returnFollowerObject = followerSaveables[activeFollowerIndex].gameObject;
                return true;
            }
            returnFollowerObject = null;
            return false;
        }

        private void RegisterActiveFollower(FollowerEntry newActiveFollower)
        {
            activeFollowerIndex = followers.Count - 1;
            followerSaveables.Add(followerSpawner.SpawnActiveFollower(newActiveFollower.followerClass, newActiveFollower.identifier));
            followerEquipToggle.SetActive(true);
            followerChangeEvent.RaiseEvent();
        }

        private void RegisterBackgroundFollower(FollowerEntry newFollower)
        {
            followerSaveables.Add(followerSpawner.SpawnBackgroundFollower(newFollower.followerClass, newFollower.identifier));
        }

        public object CaptureState()
        {
            for (int i = 0; i < followers.Count; i++)
            {
                if(followerSaveables[i] == null) continue;
                followers[i].state = followerSaveables[i].CaptureState();
            }

            return followers;
        }

        public void RestoreState(object state)
        {
            followers = (List<FollowerEntry>)state;

            for (int i = 0; i < followers.Count; i++)
            {
                if (followers[i].position == FollowerPosition.Combat)
                {
                    activeFollowerIndex = i;
                    followerSaveables.Add(followerSpawner.SpawnActiveFollower(followers[i].followerClass, followers[i].identifier, followers[i].state));
                }
                else
                {
                    followerSaveables.Add(followerSpawner.SpawnBackgroundFollower(followers[i].followerClass, followers[i].identifier, followers[i].state));
                }
            }
            
            if(followers.Count == 0 && followerEquipToggle != null)
            {
                followerEquipToggle.SetActive(false);
                // followerChangeEvent.RaiseEvent();
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
