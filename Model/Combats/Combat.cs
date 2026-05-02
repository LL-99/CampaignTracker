using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Combats
{
    public class Combat : DataElement
    {
        public ActionLog ActionLog { get; set; }
        public List<Guid> SessionGUIDs { get; set; } = [];

        [JsonIgnore]
        public Campaign Campaign { get; set; }

        [JsonIgnore]
        public List<Session> Sessions { get; set; } = [];

        public Combat()
        {
            ActionLog = null!;
            Campaign = null!;
        }

        public Combat(Campaign campaign)
        {
            ActionLog = null!;
            Campaign = campaign;
            Campaign.Combats.Add(this);
        }

        public void PostInit(Campaign campaign)
        {
            Campaign = campaign;
            Sessions = campaign.Sessions
                .Where(session => SessionGUIDs.Contains(session.GUID) || session.CombatGUIDs.Contains(GUID))
                .ToList();

            SessionGUIDs = Sessions.Select(session => session.GUID).Distinct().ToList();
        }

        public void AddSession(Session session)
        {
            if (!Sessions.Contains(session))
            {
                Sessions.Add(session);
            }

            if (!SessionGUIDs.Contains(session.GUID))
            {
                SessionGUIDs.Add(session.GUID);
            }

            session.AddCombat(this);
        }
    }
}
