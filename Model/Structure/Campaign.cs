using CampaignTracker.Model.Creatures;
using CampaignTracker.Model.Combats;

namespace CampaignTracker.Model.Structure
{
    public class Campaign : DataElement
    {
        public CampaignSystem System { get; set; } = CampaignSystem.DnD5e;

        public List<Session> Sessions { get; set; } = [];
        public List<Combat> Combats { get; set; } = [];
        public List<PlayerCharacter> PlayerCharacters { get; set; } = [];
        public List<StaticCreature> Npcs { get; set; } = [];
        public List<StaticCreature> Enemies { get; set; } = [];

        public void AddPlayerCharacter(PlayerCharacter playerCharacter)
        {
            AddCreature(PlayerCharacters, playerCharacter);
        }

        public void RemovePlayerCharacter(PlayerCharacter playerCharacter)
        {
            RemoveCreature(PlayerCharacters, playerCharacter);

            foreach (var combat in Combats.ToList())
            {
                combat.RemovePlayerCharacter(playerCharacter);
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemovePlayerCharacter(playerCharacter);
            }
        }

        public void AddNpc(StaticCreature npc)
        {
            AddCreature(Npcs, npc);
        }

        public void RemoveNpc(StaticCreature npc)
        {
            RemoveCreature(Npcs, npc);

            foreach (var combat in Combats.ToList())
            {
                combat.RemoveNpc(npc);
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemoveNpc(npc);
            }
        }

        public void AddEnemy(StaticCreature enemy)
        {
            AddCreature(Enemies, enemy);
        }

        public void RemoveEnemy(StaticCreature enemy)
        {
            RemoveCreature(Enemies, enemy);

            foreach (var combat in Combats.ToList())
            {
                combat.RemoveEnemy(enemy);
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemoveEnemy(enemy);
            }
        }

        private static void AddCreature<TCreature>(List<TCreature> creatures, TCreature creature)
            where TCreature : Creature
        {
            if (!creatures.Any(existing => existing.GUID == creature.GUID))
            {
                creatures.Add(creature);
            }
        }

        private static void RemoveCreature<TCreature>(List<TCreature> creatures, TCreature creature)
            where TCreature : Creature
        {
            creatures.RemoveAll(existing => existing.GUID == creature.GUID);
        }
    }
}
