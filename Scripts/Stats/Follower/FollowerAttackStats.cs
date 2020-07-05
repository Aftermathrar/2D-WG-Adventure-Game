using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats.Follower
{
    [System.Serializable]
    public struct FollowerAttackStats
    {
        public string Name;
        public string Description;
        public int ID;
        public float Cost;
        public float Cooldown;
        public float CastTime;
        public float RecastTimer;
        public float ActionModifier;
        public EffectName EffectID;
        public EffectTarget EffectTarget;
        public ApplyEffectOn ApplyEffect;
        public float Power;
    }
}