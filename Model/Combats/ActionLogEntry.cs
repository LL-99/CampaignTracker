using CampaignTracker.Model.Creatures;

namespace CampaignTracker.Model.Combats
{
    public enum EffectType
    {
        Damage,
        Condition,

        TemporaryHP,
        Heal,
        Buff,
    }

    public class ActionLogEntry 
    {
        public Guid Combat { get; set; }
        public Guid[] Actors { get; set; }
        public ActionEffect[] Effects { get; set; }    
    }

    public class ActionEffect
    {
        public EffectType Type { get; set; }
        public Guid[] Targets { get; set; }
        public string Notes { get; set; }
    }

    public class ActionEffect_Damage
    {
        public ActionEffect_DamageEntry[] DamageInstances { get; set; }
    }

    public class ActionEffect_DamageEntry
    {
        public DamageType DamageType { get; set; }
        public float BaseDamage { get; set; }
        public float DamageMultiplier { get; set; }
    }

    public class ActionEffect_Condition
    {
        public Condition Condition { get; set; }
        public float BaseDamage { get; set; }
        public float DamageMultiplier { get; set; }
    }
}