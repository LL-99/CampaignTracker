using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Services
{
    public interface IDataService
    {
        Campaign? Campaign { get; }

        bool IsInitialized { get; }

        string GetCampaignString() => JsonConvert.SerializeObject(Campaign, Formatting.Indented);

        Task InitializeAsync();

        Task PersistAsync();

        void ClearCampaign();

        Task ClearCampaignAsync();

        bool TrySetCampaignString(string json);

        Task<bool> TrySetCampaignStringAsync(string json);
    }
}
