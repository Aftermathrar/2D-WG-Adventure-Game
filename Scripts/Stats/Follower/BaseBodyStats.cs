using System;

namespace ButtonGame.Stats.Follower
{
    [System.Serializable]
    public class BaseBodyStats
    {
        public float height;
        public float baseWeight;
        public float fatWeight;
        public EyeColors eyeColor;
        public HairColors hairColor;
        public BodyTypes bodyType;

        const float defaultHeight = 70f;
        const float defaultBaseWeight = 120f;

        private BaseBodyStats()
        {
            height = UnityEngine.Random.Range(60f, 80f);
            float heightScale = height / defaultHeight * height / defaultHeight;
            baseWeight = heightScale * defaultBaseWeight;

            // Random BF % to determine starting total weight
            float percentBodyFat = UnityEngine.Random.Range(12f, 30f) / 100;
            fatWeight = baseWeight / (1 - percentBodyFat) - baseWeight;

            int bodyTypeCount = Enum.GetNames(typeof(BodyTypes)).Length;
            int randomType = UnityEngine.Random.Range(0, bodyTypeCount);
            bodyType = (BodyTypes)randomType;
        }
    }
}