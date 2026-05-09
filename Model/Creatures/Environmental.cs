namespace CampaignTracker.Model.Creatures
{
    public class Environmental : Creature
    {
        public static readonly Environmental Gravity = new()
        {
            GUID = Guid.Parse("0196b482-9b94-7d50-a1e9-c15d91d2b3a1"),
            Name = "Gravity",
            HP = null
        };

        public override CreatureType Type => CreatureType.Environmental;

        public float? HP { get; set; }
    }
}