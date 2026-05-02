namespace CampaignTracker.Model.Creatures
{
    public abstract class Creature : DataElement
    {
        public abstract CreatureType Type { get; }
        public string Name { get; set; }
    }
}