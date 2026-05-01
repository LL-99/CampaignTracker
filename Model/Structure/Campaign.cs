using CampaignTracker.Model.Combats;

namespace CampaignTracker.Model.Structure
{
    public class Campaign : DataElement
    {
        public List<Session> Sessions { get; set; } = [];
        public List<Combat> Combats { get; set; } = [];
    }
}
