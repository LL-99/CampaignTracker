namespace CampaignTracker.Model.Creatures
{
    /// <summary>
    /// Class for non-changing creatures, such as monsters with fixed stat blocks or NPCs that remain the same across the campaign
    /// </summary>
    public class StaticCreature : Creature
    {
        public override CreatureType Type => CreatureType.Static;

        public CreatureStats Stats { get; set; }
    }
}
