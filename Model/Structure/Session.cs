using CampaignTracker.Model.Combats;

namespace CampaignTracker.Model.Structure
{
    public class Session : DataElement
    {
        public Campaign Campaign { get; set; }
        public List<Combat> Combats { get; set; } = [];


        public Session(Campaign campaign)
        {
            Campaign = campaign;
        }

        public void AddCombat(Combat combat)
        {
            Combats.Add(combat);
            combat.Sessions.Add(this);
        }
    }
}