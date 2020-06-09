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
            return (GetBaseStat(stat) + GetAdditiveModifier(stat)) * (1 + GetPercentageModifier(stat)/100);
        }

        private float GetBaseStat(Stat stat)
        {
            return float.Parse(baseStatDB.GetStat(characterClass, stat));
        }

        public string GetClass()
        {
            return characterClass.ToString();
        }

        private float GetAdditiveModifier(Stat stat)
        {
            float total = 0;
            foreach (IEffectProvider fxProvider in GetComponents<IEffectProvider>())
            {
                foreach (float modifier in fxProvider.GetAddivitiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            float total = 0;
            foreach (IEffectProvider fxProvider in GetComponents<IEffectProvider>())
            {
                foreach (float modifier in fxProvider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }
    }
}
