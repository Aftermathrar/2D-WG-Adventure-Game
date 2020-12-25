using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Saving
{
    public interface ISlotInfo
    {
        object CaptureState();
    }
}
