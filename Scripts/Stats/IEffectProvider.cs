using System.Collections.Generic;

namespace ButtonGame.Stats
{
    public interface IEffectProvider
    {
        IEnumerable<float> GetAddivitiveModifiers(Stat stat);
        IEnumerable<float> GetPercentageModifiers(Stat stat);
    }
}