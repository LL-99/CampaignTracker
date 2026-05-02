namespace CampaignTracker.Model.Creatures
{
    /// <summary>
    /// Class for evolving/changing creatures that may have different stats per combat
    /// </summary>
    public class PlayerCharacter : Creature
    {
        public override CreatureType Type => CreatureType.Player;

        public List<PlayerCharacterStatConfiguration> StatConfigurations { get; set; } = [];
    }

    public class PlayerCharacterStatConfiguration
    {
        public string ClassesAndLevels { get; set; } = string.Empty;
        public float HP { get; set; }
    }
}
