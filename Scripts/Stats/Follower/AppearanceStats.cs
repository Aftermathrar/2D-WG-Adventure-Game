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
        Dictionary<BodyPart, float> fatDistribution = null;

        [System.Serializable]
        private class BaseBodyStats
        {
            public float height;
            public float baseWeight;
            public float fatWeight;
            public float percentBodyFat;
            public EyeColors eyeColor;
            public HairColors hairColor;
            public BodyTypes bodyType;

            const float defaultHeight = 70f;
            const float defaultBaseWeight = 120f;

            public void GenerateBodyStats()
            {
                height = UnityEngine.Random.Range(60f, 80f);
                float heightScale = height / defaultHeight * height / defaultHeight;
                baseWeight = heightScale * defaultBaseWeight;

                // Random BF % to determine starting total weight
                percentBodyFat = UnityEngine.Random.Range(12f, 30f) / 100;
                fatWeight = baseWeight / (1 - percentBodyFat) - baseWeight;

                int bodyTypeCount = Enum.GetNames(typeof(BodyTypes)).Length;
                int randomType = UnityEngine.Random.Range(0, bodyTypeCount);
                bodyType = (BodyTypes)randomType;
            }

            public float GetWeight()
            {
                return baseWeight + fatWeight;
            }

            public float GetLinearHeightScale()
            {
                return height / defaultHeight;
            }

            public void GainWeight(float amount)
            {
                fatWeight += amount;
                percentBodyFat = fatWeight / (baseWeight + fatWeight);
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
            Debug.Log("Total: " + baseBody.GetWeight() + " Height: " + baseBody.height + " Bodytype: " + baseBody.bodyType.ToString());
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

        public float GetBodyPartWeight(BodyPart bodyPart)
        {
            float bpWeight;
            bpWeight = fatDistribution[bodyPart] * baseBody.fatWeight;
            return bpWeight;
        }

        public float GetBodyPartSize(BodyPart bodyPart)
        {
            float baseSize;
            float areaWeight;
            switch (bodyPart)
            {
                case BodyPart.Arms:
                        baseSize = 10f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Breasts:
                        baseSize = 34f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Chest);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Chest:
                        baseSize = 34f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Breasts);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Visceral:
                        baseSize = 21f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Stomach);
                        areaWeight += GetBodyPartWeight(BodyPart.Waist);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Stomach:
                        baseSize = 21f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Visceral);
                        areaWeight += GetBodyPartWeight(BodyPart.Waist);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Waist:
                        baseSize = 21f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Visceral);
                        areaWeight += GetBodyPartWeight(BodyPart.Stomach);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Hips:
                        baseSize = 30f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Butt);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Butt:
                        baseSize = 30f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight += GetBodyPartWeight(BodyPart.Hips);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
                case BodyPart.Thighs:
                        baseSize = 15f;
                        baseSize *= baseBody.GetLinearHeightScale();
                        areaWeight = GetBodyPartWeight(bodyPart);
                        areaWeight /= (1 + baseBody.percentBodyFat);
                        return baseSize + areaWeight;
            }
            return -1;
        }

        private void BuildFatLookup() 
        {
            if(fatDistribution != null) return;

            fatDistribution = new Dictionary<BodyPart, float>();

            var baseFatDistribution = bodyTypeDB.GetFatDistribution(baseBody.bodyType);
            foreach (var baseFat in baseFatDistribution.baseBodyPartFat)
            {
                float bpFat = baseFat.fatDistribution * (1 + UnityEngine.Random.Range(-0.2f, 0.5f));
                fatDistribution[baseFat.bodyPart] = bpFat;
            }

            float totalDistPercent = fatDistribution.Sum(x => x.Value);
            var normalizedDistribution = new Dictionary<BodyPart, float>();
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
            fatDistribution = (Dictionary<BodyPart, float>)stateDict["dist"];
            DebugGetSizes();
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