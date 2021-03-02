using System;
using System.Collections.Generic;
using System.Linq;
using ButtonGame.Saving;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    public class AppearanceStats : MonoBehaviour, ISaveable
    {
        [SerializeField] BodyTypeDB bodyTypeDB;
        [SerializeField] string charName;
        [SerializeField] BaseBodyStats baseBody = null;
        Dictionary<BodyParts, float> fatDistribution = null;

        [System.Serializable]
        private class BaseBodyStats
        {
            public float height;
            public float heightScale;
            public float reciprocalHeightScale;     // Used to normalize part weight for description
            public float baseWeight;
            public float fatWeight;
            public EyeColors eyeColor;
            public HairColors hairColor;
            public float hairLength;
            public BodyTypes bodyType;

            const float defaultHeight = 70f;
            const float defaultBaseWeight = 120f;

            public void GenerateBodyStats()
            {
                height = UnityEngine.Random.Range(60f, 80f);
                heightScale = height / defaultHeight * height / defaultHeight;
                reciprocalHeightScale = 1 / heightScale;
                baseWeight = heightScale * defaultBaseWeight;

                // Random BF % to determine starting total weight
                float percentBodyFat = UnityEngine.Random.Range(12f, 30f) / 100;
                fatWeight = baseWeight / (1 - percentBodyFat) - baseWeight;

                int bodyTypeCount = Enum.GetNames(typeof(BodyTypes)).Length;
                int randomType = UnityEngine.Random.Range(0, bodyTypeCount);
                bodyType = (BodyTypes)randomType;

                randomType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(EyeColors)).Length);
                eyeColor = (EyeColors)randomType;

                randomType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(HairColors)).Length);
                hairColor = (HairColors)randomType;
                hairLength = UnityEngine.Random.Range(3, 24);
            }

            public float GetWeight()
            {
                return baseWeight + fatWeight;
            }

            public float GetBodyFatPercent()
            {
                float bfPercent = fatWeight / (baseWeight + fatWeight) * 100;
                return bfPercent;
            }

            public float GetLinearHeightScale()
            {
                return height / defaultHeight;
            }

            public float GetInverseLinearHeightScale()
            {
                return defaultHeight / height;
            }

            public void GainWeight(float amount)
            {
                fatWeight += amount;
            }
        }

        private void Start() 
        {
            if(fatDistribution == null)
            {
                BuildFatLookup();
                baseBody = new BaseBodyStats();
                baseBody.GenerateBodyStats();
            }
            // Debug.Log("Total: " + baseBody.GetWeight() + " Height: " + baseBody.height + " Bodytype: " + baseBody.bodyType.ToString());
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                baseBody.GainWeight(10);
                DebugGetSizes();
                Debug.Log("Total: " + baseBody.GetWeight() + " Height: " + baseBody.height + " Bodytype: " + baseBody.bodyType.ToString());
            }
            else if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                baseBody.GainWeight(-10);
                DebugGetSizes();
                Debug.Log("Total: " + baseBody.GetWeight() + " Height: " + baseBody.height + " Bodytype: " + baseBody.bodyType.ToString());
            }
        }

        public float GetHeight()
        {
            return baseBody.height;
        }

        public float GetWeight()
        {
            return baseBody.GetWeight();
        }

        public float GetBodyFatPercent()
        {
            return baseBody.GetBodyFatPercent();
        }

        public float GetBodyPartWeight(BodyParts bodyPart)
        {
            float bpWeight;
            bpWeight = fatDistribution[bodyPart] * baseBody.fatWeight;
            return bpWeight;
        }

        public float GetNormalizedBodyPartWeight(BodyParts bodyPart)
        {
            float bpWeight = GetBodyPartWeight(bodyPart);
            bpWeight *= baseBody.reciprocalHeightScale;
            return bpWeight;
        }

        public float GetBodyPartSize(BodyParts bodyPart)
        {
            float baseSize;
            float fatSize;
            switch (bodyPart)
            {
                case BodyParts.Arms:
                    baseSize = 10f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    break;
                case BodyParts.Breasts:
                    baseSize = 34f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Chest);
                    break;
                case BodyParts.Chest:
                    baseSize = 34f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Breasts);
                    break;
                case BodyParts.Visceral:
                    baseSize = 21f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Stomach);
                    fatSize += GetBodyPartWeight(BodyParts.Waist);
                    break;
                case BodyParts.Stomach:
                    baseSize = 21f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Visceral);
                    fatSize += GetBodyPartWeight(BodyParts.Waist);
                    break;
                case BodyParts.Waist:
                    baseSize = 21f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Visceral);
                    fatSize += GetBodyPartWeight(BodyParts.Stomach);
                    break;
                case BodyParts.Hips:
                    baseSize = 30f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Butt);
                    break;
                case BodyParts.Butt:
                    baseSize = 30f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    fatSize += GetBodyPartWeight(BodyParts.Hips);
                    break;
                case BodyParts.Thighs:
                    baseSize = 15f;
                    fatSize = GetBodyPartWeight(bodyPart);
                    break;
                default:
                    baseSize = -1f;
                    fatSize = -1f;
                    break;
            }
            baseSize *= baseBody.GetLinearHeightScale();
            fatSize *= baseBody.GetInverseLinearHeightScale();
            fatSize /= (1 + baseBody.GetBodyFatPercent() / 100);
            return baseSize + fatSize;
        }

        public string GetEyeColor()
        {
            return baseBody.eyeColor.ToString();
        }

        public string GetHairColor()
        {
            return baseBody.hairColor.ToString();
        }

        public float GetHairLength()
        {
            return baseBody.hairLength;
        }

        private void BuildFatLookup() 
        {
            if(fatDistribution != null) return;

            fatDistribution = new Dictionary<BodyParts, float>();

            var baseFatDistribution = bodyTypeDB.GetFatDistribution(baseBody.bodyType);
            foreach (var baseFat in baseFatDistribution.baseBodyPartFat)
            {
                float bpFat = baseFat.fatDistribution * (1 + UnityEngine.Random.Range(-0.2f, 0.5f));
                fatDistribution[baseFat.bodyPart] = bpFat;
            }

            float totalDistPercent = fatDistribution.Sum(x => x.Value);
            var normalizedDistribution = new Dictionary<BodyParts, float>();
            foreach (var bodyPart in fatDistribution.Keys)
            {
                normalizedDistribution[bodyPart] = fatDistribution[bodyPart] / totalDistPercent;
            }
            fatDistribution = normalizedDistribution;
        }

        public object CaptureState()
        {
            var state = new Dictionary<string, object>();
            state["name"] = charName;
            state["body"] = baseBody;
            state["dist"] = fatDistribution;
            return state;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            charName = (string)stateDict["name"];
            baseBody = (BaseBodyStats)stateDict["body"];
            fatDistribution = (Dictionary<BodyParts, float>)stateDict["dist"];
            // DebugGetSizes();
        }

        private void DebugGetSizes()
        {
            foreach (var key in fatDistribution.Keys)
            {
                if (GetBodyPartSize(key) > 0)
                {
                    Debug.Log(key.ToString() + " distribution: " + fatDistribution[key] + " Weight: " + GetBodyPartWeight(key) + " Size: " + GetBodyPartSize(key));
                }
                else
                {
                    Debug.Log(key.ToString() + " distribution: " + fatDistribution[key] + " Weight: " + GetBodyPartWeight(key));
                }
            }
        }
    }
}