using System.Collections.Generic;

namespace ButtonGame.Stats
{
    public interface IAttackEffectProvider
    {
        IEnumerable<float> GetAtkAddivitiveModifiers(AttackType atkType, AttackStat attackStat);
        IEnumerable<float> GetAtkPercentageModifiers(AttackType atkType, AttackStat attackStat);
        IEnumerable<bool> GetAtkBooleanModifiers(AttackType atkType, AttackStat attackStat);
    }
}