namespace CampaignTracker.Model.Creatures
{
    public enum DamageType
    {
        // 5e
        Acid, 
        Bludgeoning, 
        Cold, 
        Fire, 
        Force, 
        Lightning, 
        Necrotic, 
        Piercing, 
        Poison, 
        Psychic, 
        Radiant, 
        Slashing, 
        Thunder,

        BludgeoningMagic,
        PiercingMagic,
        SlashingMagic,
    }

    public static class DamageTypeColors
    {
        public static readonly IReadOnlyDictionary<DamageType, string> Colors = new Dictionary<DamageType, string>
        {
            [DamageType.Acid] = "#4d7c0f",
            [DamageType.Bludgeoning] = "#78716c",
            [DamageType.Cold] = "#0284c7",
            [DamageType.Fire] = "#dc2626",
            [DamageType.Force] = "#7c3aed",
            [DamageType.Lightning] = "#ca8a04",
            [DamageType.Necrotic] = "#4c1d95",
            [DamageType.Piercing] = "#475569",
            [DamageType.Poison] = "#15803d",
            [DamageType.Psychic] = "#c026d3",
            [DamageType.Radiant] = "#d97706",
            [DamageType.Slashing] = "#b91c1c",
            [DamageType.Thunder] = "#2563eb"
        };

        public static string GetColor(DamageType damageType)
        {
            return Colors.TryGetValue(damageType, out var color)
                ? color
                : "#64748b";
        }
    }

    public enum Condition
    {
        // 5e
        Blinded, 
        Charmed, 
        Deafened, 
        Exhaustion, 
        Frightened, 
        Grappled, 
        Incapacitated, 
        Invisible, 
        Paralyzed, 
        Petrified, 
        Poisoned, 
        Prone, 
        Restrained, 
        Stunned, 
        Unconscious, 
        Surprised
    }

    public class CreatureStats
    {
        public CreatureStats()
        {
        }

        public CreatureStats(float hp, DamageType[] resistances, DamageType[] vulnerabilities)
        {
            HP = hp;
            Resistances = resistances;
            Vulnerabilities = vulnerabilities;
        }

        public float HP { get; set; }
        public DamageType[] Resistances { get; set; } = [];
        public DamageType[] Vulnerabilities { get; set; } = [];
    }
}
