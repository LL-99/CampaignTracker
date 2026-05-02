using CampaignTracker.Model.Combats;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Structure
{
    public class Session : DataElement
    {
        public List<Guid> CombatGUIDs { get; private set; } = [];

        [JsonIgnore]
        public Campaign Campaign { get; private set; }

        [JsonIgnore]
        public List<Combat> Combats { get; private set; } = [];


        public Session(Campaign campaign)
        {
            Campaign = campaign;
            Campaign.Sessions.Add(this);
        }

        public void AddCombat(Combat combat)
        {
            Combats.Add(combat);
            combat.Sessions.Add(this);
        }
    }
}