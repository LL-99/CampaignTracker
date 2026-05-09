using Microsoft.JSInterop;

namespace CampaignTracker.Services
{
    public class IndexedDbService(IJSRuntime jsRuntime)
        : IIndexedDbService
    {
        public async Task<string?> LoadCampaignJsonAsync()
        {
            try
            {
                return await jsRuntime.InvokeAsync<string?>("campaignTracker.indexedDb.loadCampaignJson");
            }
            catch (JSException)
            {
                return null;
            }
        }

        public async Task SaveCampaignJsonAsync(string json)
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("campaignTracker.indexedDb.saveCampaignJson", json);
            }
            catch (JSException)
            {
            }
        }

        public async Task ClearCampaignAsync()
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("campaignTracker.indexedDb.clearCampaign");
            }
            catch (JSException)
            {
            }
        }
    }
}
