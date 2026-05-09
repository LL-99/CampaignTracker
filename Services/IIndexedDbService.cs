namespace CampaignTracker.Services
{
    public interface IIndexedDbService
    {
        Task<string?> LoadCampaignJsonAsync();

        Task SaveCampaignJsonAsync(string json);

        Task ClearCampaignAsync();
    }
}
