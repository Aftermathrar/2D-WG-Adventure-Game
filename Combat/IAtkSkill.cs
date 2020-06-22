using ButtonGame.Attributes;

namespace ButtonGame.Combat
{
    public interface IAtkSkill
    {
        void SetTarget(Health gameObject);
        void CalculateReflectDamage();
    }
}