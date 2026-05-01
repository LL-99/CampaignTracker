using CampaignTracker.Model.Combats;
using CampaignTracker.Model.Structure;

namespace CampaignTracker.Model
{
    public class DataService
    {
        public static readonly DataService Instance = new();

        public Campaign Campaign { get; private set; }


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
    }
}