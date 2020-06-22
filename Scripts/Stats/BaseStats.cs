using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] CharacterClass characterClass;
        [SerializeField] BaseStatDB baseStatDB;

        public string GetStatText(Stat stat)
        {
            return baseStatDB.GetStat(characterClass, stat);
        }

        public float GetStat(Stat stat)
        {
            // For stat modifiers, 0 is Additive, 1 is Percentage. From .asset values
            return (GetBaseStat(stat) + GetStatModifiers(stat)[1]) * (1 + GetStatModifiers(stat)[0] / 100);
            // return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat)/100);
        }

        private float GetBaseStat(Stat stat)
        {
            return float.Parse(baseStatDB.GetStat(characterClass, stat));
        }

        public string GetClass()
        {
            return characterClass.ToString();
        }

        private float[] GetStatModifiers(Stat stat)
        {
            float[] total = new float[] {0, 0};
            foreach (IEffectProvider fxProvider in GetComponents<IEffectProvider>())
            {
                foreach (float[] modifier in fxProvider.GetStatEffectModifiers(stat))
                {
                    total[0] += modifier[0];
                    total[1] += modifier[1];
                }
            }
            return total;
        }
    }
}
