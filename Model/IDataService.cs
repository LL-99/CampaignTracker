using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Model
{
    public interface IDataService
    {
        Campaign? Campaign { get; }

        string GetCampaignString() => JsonConvert.SerializeObject(Campaign, Formatting.Indented);

        void ClearCampaign();

        bool TrySetCampaignString(string json);
    }
}
