namespace CampaignTracker.Model
{
    public class DataElement
    {
        public Guid GUID { private get; set; } = Guid.CreateVersion7();
    }
}
