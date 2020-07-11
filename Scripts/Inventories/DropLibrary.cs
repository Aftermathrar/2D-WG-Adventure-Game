using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [CreateAssetMenu(menuName = ("Inventory/Drop Library"))]
    public class DropLibrary : ScriptableObject
    {
        // - Drop Chance
        // - Min drops
        // - Max drops
        // - Potential Drops
        //   - Relative chance
        //   - Min items
        //   - Max items

        [SerializeField] DropConfig[] potentialDrops;
        [SerializeField] float[] dropChancePercentage;
        [SerializeField] int[] minDrops;
        [SerializeField] int[] maxDrops;

        [System.Serializable]
        class DropConfig
        {
            public InventoryItem[] item;
            public float[] relativeChance;
            public int[] minNumber;
            public int[] maxNumber;
            public int GetRandomNumber(int dropTier)
            {
                if(!GetByLevel(item, dropTier).IsStackable())
                {
                    return 1;
                }
                int min = GetByLevel(minNumber, dropTier);
                int max = GetByLevel(maxNumber, dropTier);
                return UnityEngine.Random.Range(min, max + 1);
            }
        }

        public struct Dropped
        {
            public InventoryItem item;
            public int number;
        }

        public IEnumerable<Dropped> GetRandomDrops(int dropTier)
        {
            int minDrop = GetByLevel(minDrops, dropTier);
            for(int i = 0; i < minDrop; i++)
            {
                yield return GetRandomDrop(dropTier);
            }
            for (int i = minDrop; i < GetRandomNumberOfDrops(dropTier); i++)
            {
                if (!ShouldRandomDrop(dropTier))
                {
                    continue;
                }
                yield return GetRandomDrop(dropTier);
            }
        }

        bool ShouldRandomDrop(int dropTier)
        {
            return Random.Range(0, 100) < GetByLevel(dropChancePercentage, dropTier);
        }

        int GetRandomNumberOfDrops(int dropTier)
        {
            int min = GetByLevel(minDrops, dropTier);
            int max = GetByLevel(maxDrops, dropTier) + 1;
            return Random.Range(min, max);
        }

        Dropped GetRandomDrop(int dropTier)
        {
            Dropped randomDrop = new Dropped();
            DropConfig dropConfig = SelectRandomItem(dropTier);
            randomDrop.item = GetByLevel(dropConfig.item, dropTier);
            randomDrop.number = dropConfig.GetRandomNumber(dropTier);
            
            return randomDrop;
        }

        DropConfig SelectRandomItem(int dropTier)
        {
            float totalChance = GetTotalChance(dropTier);
            float randomRoll = Random.Range(0, totalChance);
            float chanceTotal = 0;
            foreach (var drop in potentialDrops)
            {
                chanceTotal += GetByLevel(drop.relativeChance, dropTier);
                if(chanceTotal > randomRoll)
                {
                    return drop;
                }
            }
            return null;
        }

        float GetTotalChance(int dropTier)
        {
            float totalChance = 0;
            foreach (var drop in potentialDrops)
            {
                totalChance += GetByLevel(drop.relativeChance, dropTier);
            }
            return totalChance;
        }

        static T GetByLevel<T>(T[] values, int dropTier)
        {
            if(values.Length == 0)
            {
                return default;
            }
            if(dropTier > values.Length)
            {
                return values[values.Length - 1];
            }
            if(dropTier <= 0)
            {
                return default;
            }
            return values[dropTier - 1];
        }
    }
}
