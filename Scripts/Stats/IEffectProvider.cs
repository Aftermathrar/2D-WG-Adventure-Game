using System.Collections.Generic;

namespace ButtonGame.Stats
{
    public interface IEffectProvider
    {
        IEnumerable<float[]> GetStatEffectModifiers(Stat stat);
    }
}