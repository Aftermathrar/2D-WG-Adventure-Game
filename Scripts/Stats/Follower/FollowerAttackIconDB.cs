using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    [CreateAssetMenu(fileName = "FollowerAttackIconDB", menuName = "Stats/UI/FollowerAttackIconDB", order = 3)]
    public class FollowerAttackIconDB : ScriptableObject
    {
        [SerializeField] DBFollowerAttackIcon[] followerAttackIcon;
        Dictionary<FollowerAttackName, Sprite> lookupTable;

        public Sprite GetSprite(FollowerAttackName attackName)
        {
            BuildLookup();

            return lookupTable[attackName];
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<FollowerAttackName, Sprite>();

            foreach (DBFollowerAttackIcon icon in followerAttackIcon)
            {
                lookupTable[icon.attackName] = icon.sprite;
            }
        }
    }

    [System.Serializable]
    class DBFollowerAttackIcon
    {
        public FollowerAttackName attackName;
        public Sprite sprite;
    }
}