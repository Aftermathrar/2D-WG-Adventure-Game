using System.Collections.Generic;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats
{
    public interface IStatModifier
    {
        // IEnumerable<float[]> GetStatEffectModifiers(Stat stat);
        float[] GetStatEffectModifiers(Stat stat);
    }
}