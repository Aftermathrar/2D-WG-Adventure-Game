using System;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using UnityEngine;

namespace ButtonGame.Stats
{
    [CreateAssetMenu(fileName = "EffectIconDB", menuName = "Stats/UI/EffectIconDB", order = 0)]
    public class EffectIconDB : ScriptableObject
    {
        [SerializeField] DBEffectIcon[] effectIcons;
        Dictionary<EffectName, Sprite> lookupTable = null;

        public Sprite GetSprite(EffectName fxName)
        {
            BuildLookup();

            return lookupTable[fxName];
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return;

            lookupTable = new Dictionary<EffectName, Sprite>();

            foreach (DBEffectIcon fxIcon in effectIcons)
            {
                lookupTable[fxIcon.effectName] = fxIcon.sprite;
            }
        }

        [System.Serializable]
        class DBEffectIcon
        {
            public EffectName effectName;
            public Sprite sprite;
        }
    }
}
