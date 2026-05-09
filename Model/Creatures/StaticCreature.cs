namespace CampaignTracker.Model.Creatures
{
    /// <summary>
    /// Class for non-changing creatures, such as monsters with fixed stat blocks or NPCs that remain the same across the campaign
    /// </summary>
    public class StaticCreature : Creature
    {
        public override CreatureType Type => CreatureType.Static;
        public override string DisplayName => string.IsNullOrWhiteSpace(Source) ? Name : (Name + " (" + Source + ")");

        public CreatureStats Stats { get; set; } = new();

        public float? ChallengeRating { get; set; }

        public string? Source { get; set; }
    }
}
