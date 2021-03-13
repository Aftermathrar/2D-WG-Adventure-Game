using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats
{
    public interface IFeedeeStatModifier
    {
        float[] GetFeedeeStatEffectModifiers(FeedeeStat stat);
    }
}
