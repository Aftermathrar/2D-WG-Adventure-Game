using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Stats.Follower
{
    public interface IWearable
    {
        float GetWearableSize(BodyParts bodyPart);
        void SetMeasurement(BodyParts bodyPart, float newSize);
    }
}
