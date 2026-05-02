using CampaignTracker.Model.Combats;
using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Model
{
    public class DataService
        : IDataService
    {
        public Campaign? Campaign { get; private set; }


        public DataService()
        {
            Campaign = new();

            var c1 = new Combat(Campaign);
            var c2 = new Combat(Campaign);

            var s1 = new Session(Campaign);
            var s2 = new Session(Campaign);

            s1.AddCombat(c1);
            s1.AddCombat(c2);

            c2.AddSession(s2);
        }

        public void ClearCampaign()
        {
            Campaign = null;
        }

        public bool TrySetCampaignString(string json)
        {
            Campaign? campaign;

            try
            {
                campaign = JsonConvert.DeserializeObject<Campaign>(json);
            }
            catch (JsonException)
            {
                return false;
            }

            if (campaign is null)
            {
                return false;
            }

            InitCampaignReferences(campaign);
            Campaign = campaign;

            return true;
        }

        private static void InitCampaignReferences(Campaign campaign)
        {
            foreach (var combat in campaign.Combats)
            {
                combat.PostInit(campaign);
            }

            foreach (var session in campaign.Sessions)
            {
                session.PostInit(campaign);
            }
        }
    }
}
