using CampaignTracker.Model.Structure;

namespace CampaignTracker.Model.Combats
{
    public class Combat : DataElement
    {
        public Campaign Campaign { get; set; }
        public HashSet<Session> Sessions { get; set; } = [];
        public ActionLog ActionLog { get; set; }

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
