namespace CampaignTracker.Model.Creatures
{
    /// <summary>
    /// Class for evolving/changing creatures that may have different stats per combat
    /// </summary>
    public class PlayerCharacter : Creature
    {
        public override CreatureType Type => CreatureType.Player;

        public Dictionary<string, CreatureStats> Stats { get; set; } = new();
    }
}