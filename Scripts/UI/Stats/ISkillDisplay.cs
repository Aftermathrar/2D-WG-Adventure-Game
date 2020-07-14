using ButtonGame.Inventories;

namespace ButtonGame.Stats
{
    public interface ISkillDisplay : ITooltipProvider
    {
        int GetSkillDescription();
        string GetAttackStat(int i);
    }
}