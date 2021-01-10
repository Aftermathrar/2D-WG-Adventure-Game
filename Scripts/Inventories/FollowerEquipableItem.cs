using System.Collections;
using System.Collections.Generic;
using ButtonGame.Stats.Enums;
using ButtonGame.Stats.Follower;
using UnityEngine;

namespace ButtonGame.Inventories
{
    [CreateAssetMenu(menuName = ("Inventory/Follower Equipment"))]
    public class FollowerEquipableItem : EquipableItem, IWearable
    {
        Dictionary<BodyParts, float> measurements = null;
        [SerializeField] List<WearableModifiers> modifiers = new List<WearableModifiers>();
        [SerializeField] protected override CharacterClass equipClass
        {
            get { return CharacterClass.Priest; }
        }

        private void Awake()
        {
            BuildMeasurementLookup();
        }

        private void BuildMeasurementLookup()
        {
            if (measurements != null) return;

            switch (allowedEquipLocation)
            {
                case EquipLocation.Robe:
                case EquipLocation.Innerwear:
                    measurements = new Dictionary<BodyParts, float>();
                    foreach (var modifier in modifiers)
                    {
                        measurements[modifier.bodyPart] = modifier.size;
                    }
                    // measurements[BodyParts.Breasts] = 0;
                    // measurements[BodyParts.Waist] = 0;
                    // measurements[BodyParts.Hips] = 0; 
                    // measurements[BodyParts.Thighs] = 0;
                    break;
                default:
                    measurements = new Dictionary<BodyParts, float>();
                    break;
            }
        }

        public IEnumerable<float> GetWearableSizes()
        {
            BuildMeasurementLookup();

            foreach (var pair in measurements)
            {
                yield return pair.Value;
            }
        }

        public float GetWearableSize(BodyParts bodyPart)
        {
            BuildMeasurementLookup();
            if (measurements.ContainsKey(bodyPart))
            {
                return measurements[bodyPart];
            }
            return -1;
        }

        public void AddMeasurements()
        {
            BuildMeasurementLookup();
            var newMeasurements = new Dictionary<BodyParts, float>();
            foreach (var key in measurements.Keys)
            {
                newMeasurements[key] = measurements[key] + Random.Range(0, 5);
            }
            measurements = newMeasurements;
        }

        public void SetMeasurement(BodyParts bodyPart, float newSize)
        {
            BuildMeasurementLookup();
            if (measurements.ContainsKey(bodyPart))
            {
                measurements[bodyPart] = newSize;
            }
        }

        public override object GetModifiers()
        {
            return measurements;
        }

        public override void SetModifiers(object state)
        {
            measurements = (Dictionary<BodyParts, float>)state;
        }

        [System.Serializable]
        private class WearableModifiers
        {
            public BodyParts bodyPart;
            public float size;
        }
    }
}
