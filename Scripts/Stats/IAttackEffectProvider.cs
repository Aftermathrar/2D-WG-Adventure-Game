using System.Collections.Generic;
using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats
{
    public interface IAttackEffectProvider
    {
        IEnumerable<float[]> GetAtkStatModifiers(AttackType atkType, AttackStat attackStat);
        IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat);
    }
}