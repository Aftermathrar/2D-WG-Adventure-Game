using ButtonGame.Stats.Enums;

namespace ButtonGame.Stats.Follower
{
    [System.Serializable]
    public class FollowerAttackStats
    {
        public CharacterClass HealingClass;
        public string Name;
        public string Description;
        public int ID;
        public int Cost;
        public float Cooldown;
        public float CastTime;
        public bool isRecastSkill;
        public float RecastTimer;
        public float ActionModifier;
        public EffectName EffectID;
        public EffectTarget EffectTarget;
        public ApplyEffectOn ApplyEffect;
        public float Power;
        public FollowerAttackPool movePool;
    }
}
