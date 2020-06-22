namespace ButtonGame.Combat
{
    public interface ITargetable
    {
        void HandleAttack(IAtkSkill callingScript);
    }
}