using CampaignTracker.Model.Creatures;
using CampaignTracker.Model.Combats;

namespace CampaignTracker.Model.Structure
{
    public class Campaign : DataElement
    {
        public CampaignSystem System { get; set; }

        public List<Session> Sessions { get; set; } = [];
        public List<Combat> Combats { get; set; } = [];
        public List<PlayerCharacter> PlayerCharacters { get; set; } = [];
        public List<StaticCreature> CustomCreatures { get; set; } = [];
    }
}