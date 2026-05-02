using CampaignTracker.Model.Combats;
using CampaignTracker.Model.Creatures;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Structure
{
    public class Session : DataElement
    {
        public List<Guid> PlayerCharacterGUIDs { get; private set; } = [];
        public List<Guid> NpcGUIDs { get; private set; } = [];
        public List<Guid> EnemyGUIDs { get; private set; } = [];
        public List<Guid> CombatGUIDs { get; private set; } = [];
        public string Name { get; set; } = string.Empty;
        public DateTime DateUtc { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Campaign Campaign { get; private set; }

        [JsonIgnore]
        public List<PlayerCharacter> PlayerCharacters { get; private set; } = [];

        [JsonIgnore]
        public List<StaticCreature> Npcs { get; private set; } = [];

        [JsonIgnore]
        public List<StaticCreature> Enemies { get; private set; } = [];

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

        public Session(Campaign campaign)
        {
            Campaign = campaign;

            if (!campaign.Sessions.Any(session => session.GUID == GUID))
            {
                campaign.Sessions.Add(this);
            }
        }

        public void PostInit(Campaign campaign)
        {
            Campaign = campaign;
            PlayerCharacters = [];
            Npcs = [];
            Enemies = [];
            Combats = [];

            foreach (var combat in campaign.Combats.Where(combat => CombatGUIDs.Contains(combat.GUID) || combat.SessionGUIDs.Contains(GUID)))
            {
                AddCombat(combat);
            }

            foreach (var playerCharacter in campaign.PlayerCharacters.Where(playerCharacter => PlayerCharacterGUIDs.Contains(playerCharacter.GUID)))
            {
                AddPlayerCharacter(playerCharacter);
            }

            foreach (var npc in campaign.Npcs.Where(npc => NpcGUIDs.Contains(npc.GUID)))
            {
                AddNpc(npc);
            }

            foreach (var enemy in campaign.Enemies.Where(enemy => EnemyGUIDs.Contains(enemy.GUID)))
            {
                AddEnemy(enemy);
            }
        }

        public void AddCombat(Combat combat)
        {
            AddCombatDirect(combat);
            combat.AddSessionFromSession(this);
            AddCombatCreatures(combat);
        }

        public void RemoveCombat(Combat combat)
        {
            if (!RemoveCombatDirect(combat))
            {
                return;
            }

            combat.RemoveSessionFromSession(this);
            RemoveCombatCreaturesIfUnused(combat);
        }

        public void AddPlayerCharacter(PlayerCharacter playerCharacter)
        {
            AddCreature(PlayerCharacters, PlayerCharacterGUIDs, playerCharacter);
        }

        public void RemovePlayerCharacter(PlayerCharacter playerCharacter)
        {
            if (!RemoveCreature(PlayerCharacters, PlayerCharacterGUIDs, playerCharacter))
            {
                return;
            }

            foreach (var combat in Combats.ToList())
            {
                combat.RemovePlayerCharacter(playerCharacter);
            }
        }

        public void AddNpc(StaticCreature npc)
        {
            AddCreature(Npcs, NpcGUIDs, npc);
        }

        public void RemoveNpc(StaticCreature npc)
        {
            if (!RemoveCreature(Npcs, NpcGUIDs, npc))
            {
                return;
            }

            foreach (var combat in Combats.ToList())
            {
                combat.RemoveNpc(npc);
            }
        }

        public void AddEnemy(StaticCreature enemy)
        {
            AddCreature(Enemies, EnemyGUIDs, enemy);
        }

        public void RemoveEnemy(StaticCreature enemy)
        {
            if (!RemoveCreature(Enemies, EnemyGUIDs, enemy))
            {
                return;
            }

            foreach (var combat in Combats.ToList())
            {
                combat.RemoveEnemy(enemy);
            }
        }

        internal void AddCombatFromCombat(Combat combat)
        {
            AddCombatDirect(combat);
            AddCombatCreatures(combat);
        }

        internal void RemoveCombatFromCombat(Combat combat)
        {
            if (RemoveCombatDirect(combat))
            {
                RemoveCombatCreaturesIfUnused(combat);
            }
        }

        internal void AddPlayerCharacterFromCombat(PlayerCharacter playerCharacter)
        {
            AddPlayerCharacter(playerCharacter);
        }

        internal void RemovePlayerCharacterFromCombat(Combat combat, PlayerCharacter playerCharacter)
        {
            if (!Combats.Any(relatedCombat => relatedCombat.GUID != combat.GUID && relatedCombat.PlayerCharacters.Any(pc => pc.GUID == playerCharacter.GUID)))
            {
                RemoveCreature(PlayerCharacters, PlayerCharacterGUIDs, playerCharacter);
            }
        }

        internal void AddNpcFromCombat(StaticCreature npc)
        {
            AddNpc(npc);
        }

        internal void RemoveNpcFromCombat(Combat combat, StaticCreature npc)
        {
            if (!Combats.Any(relatedCombat => relatedCombat.GUID != combat.GUID && relatedCombat.Npcs.Any(relatedNpc => relatedNpc.GUID == npc.GUID)))
            {
                RemoveCreature(Npcs, NpcGUIDs, npc);
            }
        }

        internal void AddEnemyFromCombat(StaticCreature enemy)
        {
            AddEnemy(enemy);
        }

        internal void RemoveEnemyFromCombat(Combat combat, StaticCreature enemy)
        {
            if (!Combats.Any(relatedCombat => relatedCombat.GUID != combat.GUID && relatedCombat.Enemies.Any(relatedEnemy => relatedEnemy.GUID == enemy.GUID)))
            {
                RemoveCreature(Enemies, EnemyGUIDs, enemy);
            }
        }

        private void AddCombatCreatures(Combat combat)
        {
            foreach (var playerCharacter in combat.PlayerCharacters)
            {
                AddPlayerCharacter(playerCharacter);
            }

            foreach (var npc in combat.Npcs)
            {
                AddNpc(npc);
            }

            foreach (var enemy in combat.Enemies)
            {
                AddEnemy(enemy);
            }
        }

        private void RemoveCombatCreaturesIfUnused(Combat combat)
        {
            foreach (var playerCharacter in combat.PlayerCharacters)
            {
                RemovePlayerCharacterFromCombat(combat, playerCharacter);
            }

            foreach (var npc in combat.Npcs)
            {
                RemoveNpcFromCombat(combat, npc);
            }

            foreach (var enemy in combat.Enemies)
            {
                RemoveEnemyFromCombat(combat, enemy);
            }
        }

        private void AddCombatDirect(Combat combat)
        {
            if (!Combats.Any(existing => existing.GUID == combat.GUID))
            {
                Combats.Add(combat);
            }

            if (!CombatGUIDs.Contains(combat.GUID))
            {
                CombatGUIDs.Add(combat.GUID);
            }
        }

        private bool RemoveCombatDirect(Combat combat)
        {
            var removed = Combats.RemoveAll(existing => existing.GUID == combat.GUID) > 0;
            removed = CombatGUIDs.Remove(combat.GUID) || removed;

            return removed;
        }

        private static void AddCreature<TCreature>(List<TCreature> creatures, List<Guid> creatureGUIDs, TCreature creature)
            where TCreature : Creature
        {
            if (!creatures.Any(existing => existing.GUID == creature.GUID))
            {
                creatures.Add(creature);
            }

            if (!creatureGUIDs.Contains(creature.GUID))
            {
                creatureGUIDs.Add(creature.GUID);
            }
        }

        private static bool RemoveCreature<TCreature>(List<TCreature> creatures, List<Guid> creatureGUIDs, TCreature creature)
            where TCreature : Creature
        {
            var removed = creatures.RemoveAll(existing => existing.GUID == creature.GUID) > 0;
            removed = creatureGUIDs.Remove(creature.GUID) || removed;

            return removed;
        }
    }
}
