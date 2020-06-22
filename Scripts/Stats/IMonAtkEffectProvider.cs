using System.Collections.Generic;

namespace ButtonGame.Stats
{
    public interface IMonAtkEffectProvider
    {
        IEnumerable<float[]> GetMonAtkStatModifiers(MonAtkName atkName, MonAtkStat stat);
        IEnumerable<bool> GetMonAtkBooleanModifiers(MonAtkName atkName, MonAtkStat stat);
    }
}