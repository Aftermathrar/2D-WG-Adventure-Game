using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ButtonGame.Combat
{
    public interface IBuffListProvider
    {
        Dictionary<string, float[]> GetBuffDictionary();
    }
}
