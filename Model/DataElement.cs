namespace CampaignTracker.Model
{
    public class DataElement
    {
        public Guid GUID { get; private set; } = Guid.CreateVersion7();
    }
}
