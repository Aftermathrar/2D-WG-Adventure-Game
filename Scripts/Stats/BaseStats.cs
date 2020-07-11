using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
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
            // For stat modifiers, 1 is Additive, 0 is Percentage. From .asset values
            float[] statModifiers = GetStatModifiers(stat);
            return (GetBaseStat(stat) + statModifiers[1]) * (1 + statModifiers[0] / 100);
            // return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat)/100);
        }

        public float GetRawStat(Stat stat)
        {
            return GetBaseStat(stat);
        }

        public CharacterClass GetClass()
        {
            return characterClass;
        }

        // public string GetClassString()
        // {
        //     return characterClass.ToString();
        // }

        private float GetBaseStat(Stat stat)
        {
            return float.Parse(baseStatDB.GetStat(characterClass, stat));
        }

        private float[] GetStatModifiers(Stat stat)
        {
            float[] total = new float[] {0, 0};
            foreach (IStatModifier fxProvider in GetComponents<IStatModifier>())
            {
                float[] result = fxProvider.GetStatEffectModifiers(stat);
                total[0] += result[0];
                total[1] += result[1];
            }
            return total;
        }
    }
}
