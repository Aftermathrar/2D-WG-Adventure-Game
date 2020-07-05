using System.Collections.Generic;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    [RequireComponent(typeof(SaveableEntity))]
    public class FollowerStats : MonoBehaviour, ISaveable
    {
        Dictionary<string, FollowerSetup> characterDict = null;

        // GUID key to appearance stat record
        [System.Serializable]
        private struct FollowerSetup
        {
            public BaseStats charClass;
            public AppearanceStats appearance;
            public PersonalityStats personality;
            public InternalStats internalStats;
            public FollowerPositions position;
        }
    
        private void Awake() 
        {
            
        }

        

        public object CaptureState()
        {
            return characterDict;
        }

        public void RestoreState(object state)
        {
            characterDict = (Dictionary<string, FollowerSetup>)state;
        }
    }

}