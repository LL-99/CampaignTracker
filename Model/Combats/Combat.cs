using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Combats
{
    public class Combat : DataElement
    {
        public ActionLog ActionLog { get; set; }

        [JsonIgnore]
        public Campaign Campaign { get; set; }

        [JsonIgnore]
        public List<Session> Sessions { get; set; } = [];

        public Combat(Campaign campaign)
        {
            Campaign = campaign;
            Campaign.Combats.Add(this);
        }

        public void AddSession(Session session)
        {
            Sessions.Add(session);
            session.Combats.Add(this);
        }
    }
}
