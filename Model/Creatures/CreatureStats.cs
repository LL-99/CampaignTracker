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
        Thunder
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
