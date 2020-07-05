using System;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "MonAtkIconDB", menuName = "Stats/UI/MonAtkIconDB", order = 1)]
    public class MonAtkIconDB : ScriptableObject
    {
        [SerializeField] DBMonAtkIcon[] monAtkIcons;
        Dictionary<MonAtkName, Sprite[]> lookupTable = null;

        public Sprite[] GetSprite(MonAtkName atkName)
        {
            BuildLookup();

            return lookupTable[atkName];
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<MonAtkName, Sprite[]>();

            foreach (DBMonAtkIcon atkIcon in monAtkIcons)
            {
                lookupTable[atkIcon.monAtkName] = atkIcon.sprite;
            }
        }

        [System.Serializable]
        class DBMonAtkIcon
        {
            public MonAtkName monAtkName;
            public Sprite[] sprite;
        }
    }
}