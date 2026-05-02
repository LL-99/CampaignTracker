using CampaignTracker.Model.Combats;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Structure
{
    public class Session : DataElement
    {
        public List<Guid> CombatGUIDs { get; private set; } = [];
        public string Name { get; set; }

        [JsonIgnore]
        public Campaign Campaign { get; private set; }

        [JsonIgnore]
        public List<Combat> Combats { get; private set; } = [];

        public Session()
        {
            Campaign = null!;
        }

        public Session(List<Guid> combatGUIDs)
        {
            CombatGUIDs = combatGUIDs;
            Campaign = null!;
        }

        public void PostInit(Campaign campaign)
        {
            Campaign = campaign;
            Combats = campaign.Combats
                .Where(combat => CombatGUIDs.Contains(combat.GUID) || combat.SessionGUIDs.Contains(GUID))
                .ToList();

            CombatGUIDs = Combats.Select(combat => combat.GUID).Distinct().ToList();
        }


        public Session(Campaign campaign)
        {
            Campaign = campaign;
            Campaign.Sessions.Add(this);
        }

        public void AddCombat(Combat combat)
        {
            if (!Combats.Contains(combat))
            {
                Combats.Add(combat);
            }

            if (!CombatGUIDs.Contains(combat.GUID))
            {
                CombatGUIDs.Add(combat.GUID);
            }

            if (!combat.Sessions.Contains(this))
            {
                combat.Sessions.Add(this);
            }

            if (!combat.SessionGUIDs.Contains(GUID))
            {
                combat.SessionGUIDs.Add(GUID);
            }
        }
    }
}
