using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    [CreateAssetMenu(fileName = "BodyTypeDB", menuName = "Stats/Follower/BodyTypeDB", order = 6)]
    public partial class BodyTypeDB : ScriptableObject 
    {
        [SerializeField]
        BaseBody[] bodyTypeDB = null;
        Dictionary<BodyTypes, BaseFatDistribution> bodyLookup = null;


        [System.Serializable]
        public class BaseBody
        {
            public BodyTypes bodyType;
            public BaseFatDistribution baseFatDistribution;
        }

        [System.Serializable]
        public class BaseFatDistribution
        {
            public BaseBodyPartFatDistribution[] baseBodyPartFat;
        }

        [System.Serializable]
        public class BaseBodyPartFatDistribution
        {
            public BodyParts bodyPart;
            public float fatDistribution;
        }

        public BaseFatDistribution GetFatDistribution(BodyTypes bodyType)
        {
            BuildLookup();

            return bodyLookup[bodyType];
        }

        private void BuildLookup()
        {
            if(bodyLookup != null) return;

            bodyLookup = new Dictionary<BodyTypes, BaseFatDistribution>();

            foreach (var body in bodyTypeDB)
            {
                bodyLookup[body.bodyType] = body.baseFatDistribution;
            }
        }
    }
}
