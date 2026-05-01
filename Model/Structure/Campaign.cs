using CampaignTracker.Model.Characters;
using CampaignTracker.Model.Combats;

namespace CampaignTracker.Model.Structure
{
    public class Campaign : DataElement
    {
        public List<Session> Sessions { get; set; } = [];
        public List<Combat> Combats { get; set; } = [];
        public List<Character> Characters { get; set; } = [];


        public List<Character> PlayerCharacters => Characters.Where(c => c.CharacterType == CharacterType.Player).ToList();
    }
}
