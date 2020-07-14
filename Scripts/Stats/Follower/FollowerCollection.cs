using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    [CreateAssetMenu(fileName = "FollowerCollection", menuName = "Stats/Follower/Collection", order = 2)]
    public class FollowerCollection : ScriptableObject
    {
        [SerializeField] FollowerTypeList[] followers;

        Dictionary<FollowerPosition, FollowerRole> lookupTable = null;

        public FollowerRole GetFollowerIdentifier(FollowerPosition position)
        {
            BuildLookup();

            return lookupTable[position];
        }

        public void AddNewFollower(FollowerPosition position, FollowerRole role)
        {
            BuildLookup();

            foreach (FollowerTypeList followerType in followers)
            {
                if(followerType.Position == position)
                {
                    followerType.Role.FollowerClass = role.FollowerClass;
                    followerType.Role.Identifier = role.Identifier;
                    lookupTable[position] = followerType.Role;
                    return;
                }
            }
        }

        private void BuildLookup()
        {
            if(lookupTable != null) return;

            lookupTable = new Dictionary<FollowerPosition, FollowerRole>();

            foreach (FollowerTypeList followerType in followers)
            {
                string s = followerType.Role.Identifier;
                lookupTable[followerType.Position] = followerType.Role;
            }
        }
    }

    [System.Serializable]
    class FollowerTypeList
    {
        public FollowerPosition Position;
        public FollowerRole Role;
    }

    [System.Serializable]
    public struct FollowerRole
    {
        public CharacterClass FollowerClass;
        public string Identifier;
    }
}