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

    public record CreatureStats(float HP, DamageType[] Resistances, DamageType[] Vulnerabilities);
}
