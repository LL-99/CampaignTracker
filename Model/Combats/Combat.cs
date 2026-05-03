using CampaignTracker.Model.Creatures;
using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Model.Combats
{
    public class Combat : DataElement
    {
        public string Name { get; set; } = string.Empty;
        public ActionLog ActionLog { get; set; }
        public List<Guid> SessionGUIDs { get; private set; } = [];
        public List<Guid> PlayerCharacterGUIDs { get; private set; } = [];
        public List<Guid> NpcGUIDs { get; private set; } = [];
        public List<Guid> EnemyGUIDs { get; private set; } = [];
        public List<CombatPlayerCharacterStatConfiguration> PlayerCharacterStatConfigurations { get; private set; } = [];

        [JsonIgnore]
        public Campaign Campaign { get; set; }

        [JsonIgnore]
        public List<Session> Sessions { get; set; } = [];

        [JsonIgnore]
        public List<PlayerCharacter> PlayerCharacters { get; private set; } = [];

        [JsonIgnore]
        public List<StaticCreature> Npcs { get; private set; } = [];

        [JsonIgnore]
        public List<StaticCreature> Enemies { get; private set; } = [];

        public Combat()
        {
            ActionLog = null!;
            Campaign = null!;
        }

        public Combat(Campaign campaign)
        {
            ActionLog = null!;
            Campaign = campaign;

            if (!campaign.Combats.Any(combat => combat.GUID == GUID))
            {
                campaign.Combats.Add(this);
            }
        }

        public void PostInit(Campaign campaign)
        {
            Campaign = campaign;
            Sessions = campaign.Sessions
                .Where(session => SessionGUIDs.Contains(session.GUID) || session.CombatGUIDs.Contains(GUID))
                .ToList();

            SessionGUIDs = Sessions.Select(session => session.GUID).Distinct().ToList();
            PlayerCharacters = campaign.PlayerCharacters.Where(playerCharacter => PlayerCharacterGUIDs.Contains(playerCharacter.GUID)).ToList();
            PlayerCharacterGUIDs = PlayerCharacters.Select(playerCharacter => playerCharacter.GUID).Distinct().ToList();
            PlayerCharacterStatConfigurations = PlayerCharacterStatConfigurations
                .Where(selection => PlayerCharacters.Any(playerCharacter => playerCharacter.GUID == selection.PlayerCharacterGUID)
                    && PlayerCharacters
                        .First(playerCharacter => playerCharacter.GUID == selection.PlayerCharacterGUID)
                        .StatConfigurations
                        .Any(statConfiguration => statConfiguration.GUID == selection.StatConfigurationGUID))
                .GroupBy(selection => selection.PlayerCharacterGUID)
                .Select(group => group.First())
                .ToList();

            foreach (var playerCharacter in PlayerCharacters)
            {
                EnsurePlayerCharacterStatConfiguration(playerCharacter);
            }

            Npcs = campaign.Npcs.Where(npc => NpcGUIDs.Contains(npc.GUID)).ToList();
            NpcGUIDs = Npcs.Select(npc => npc.GUID).Distinct().ToList();
            Enemies = campaign.Enemies.Where(enemy => EnemyGUIDs.Contains(enemy.GUID)).ToList();
            EnemyGUIDs = Enemies.Select(enemy => enemy.GUID).Distinct().ToList();
        }

        public void AddSession(Session session)
        {
            AddSessionDirect(session);
            session.AddCombatFromCombat(this);
        }

        public void RemoveSession(Session session)
        {
            if (!RemoveSessionDirect(session))
            {
                return;
            }

            session.RemoveCombatFromCombat(this);
        }

        public void AddPlayerCharacter(PlayerCharacter playerCharacter)
        {
            if (!AddCreature(PlayerCharacters, PlayerCharacterGUIDs, playerCharacter))
            {
                return;
            }

            foreach (var session in Sessions)
            {
                session.AddPlayerCharacterFromCombat(playerCharacter);
            }

            EnsurePlayerCharacterStatConfiguration(playerCharacter);
        }

        public void RemovePlayerCharacter(PlayerCharacter playerCharacter)
        {
            if (!RemoveCreature(PlayerCharacters, PlayerCharacterGUIDs, playerCharacter))
            {
                return;
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemovePlayerCharacterFromCombat(this, playerCharacter);
            }

            PlayerCharacterStatConfigurations.RemoveAll(selection => selection.PlayerCharacterGUID == playerCharacter.GUID);
        }

        public PlayerCharacterStatConfiguration? GetSelectedStatConfiguration(PlayerCharacter playerCharacter)
        {
            EnsurePlayerCharacterStatConfiguration(playerCharacter);

            var selectedStatConfigurationGuid = PlayerCharacterStatConfigurations
                .FirstOrDefault(selection => selection.PlayerCharacterGUID == playerCharacter.GUID)
                ?.StatConfigurationGUID;

            return playerCharacter.StatConfigurations
                .FirstOrDefault(statConfiguration => statConfiguration.GUID == selectedStatConfigurationGuid);
        }

        public void SetSelectedStatConfiguration(PlayerCharacter playerCharacter, PlayerCharacterStatConfiguration statConfiguration)
        {
            if (!PlayerCharacters.Any(existing => existing.GUID == playerCharacter.GUID)
                || !playerCharacter.StatConfigurations.Any(existing => existing.GUID == statConfiguration.GUID))
            {
                return;
            }

            var existingSelection = PlayerCharacterStatConfigurations
                .FirstOrDefault(selection => selection.PlayerCharacterGUID == playerCharacter.GUID);

            if (existingSelection is null)
            {
                PlayerCharacterStatConfigurations.Add(new CombatPlayerCharacterStatConfiguration
                {
                    PlayerCharacterGUID = playerCharacter.GUID,
                    StatConfigurationGUID = statConfiguration.GUID
                });
            }
            else
            {
                existingSelection.StatConfigurationGUID = statConfiguration.GUID;
            }
        }

        public void AddNpc(StaticCreature npc)
        {
            if (!AddCreature(Npcs, NpcGUIDs, npc))
            {
                return;
            }

            foreach (var session in Sessions)
            {
                session.AddNpcFromCombat(npc);
            }
        }

        public void RemoveNpc(StaticCreature npc)
        {
            if (!RemoveCreature(Npcs, NpcGUIDs, npc))
            {
                return;
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemoveNpcFromCombat(this, npc);
            }
        }

        public void AddEnemy(StaticCreature enemy)
        {
            if (!AddCreature(Enemies, EnemyGUIDs, enemy))
            {
                return;
            }

            foreach (var session in Sessions)
            {
                session.AddEnemyFromCombat(enemy);
            }
        }

        public void RemoveEnemy(StaticCreature enemy)
        {
            if (!RemoveCreature(Enemies, EnemyGUIDs, enemy))
            {
                return;
            }

            foreach (var session in Sessions.ToList())
            {
                session.RemoveEnemyFromCombat(this, enemy);
            }
        }

        internal void AddSessionFromSession(Session session)
        {
            AddSessionDirect(session);
        }

        internal void RemoveSessionFromSession(Session session)
        {
            RemoveSessionDirect(session);
        }

        internal void ClearReferences()
        {
            Sessions.Clear();
            SessionGUIDs.Clear();
            PlayerCharacters.Clear();
            PlayerCharacterGUIDs.Clear();
            PlayerCharacterStatConfigurations.Clear();
            Npcs.Clear();
            NpcGUIDs.Clear();
            Enemies.Clear();
            EnemyGUIDs.Clear();
        }

        private void AddSessionDirect(Session session)
        {
            if (!Sessions.Any(existing => existing.GUID == session.GUID))
            {
                Sessions.Add(session);
            }

            if (!SessionGUIDs.Contains(session.GUID))
            {
                SessionGUIDs.Add(session.GUID);
            }
        }

        private bool RemoveSessionDirect(Session session)
        {
            var removed = Sessions.RemoveAll(existing => existing.GUID == session.GUID) > 0;
            removed = SessionGUIDs.Remove(session.GUID) || removed;

            return removed;
        }

        private void EnsurePlayerCharacterStatConfiguration(PlayerCharacter playerCharacter)
        {
            if (!PlayerCharacters.Any(existing => existing.GUID == playerCharacter.GUID)
                || PlayerCharacterStatConfigurations.Any(selection => selection.PlayerCharacterGUID == playerCharacter.GUID))
            {
                return;
            }

            var defaultStatConfiguration = playerCharacter.StatConfigurations
                .OrderByDescending(statConfiguration => statConfiguration.HP)
                .FirstOrDefault();

            if (defaultStatConfiguration is null)
            {
                return;
            }

            PlayerCharacterStatConfigurations.Add(new CombatPlayerCharacterStatConfiguration
            {
                PlayerCharacterGUID = playerCharacter.GUID,
                StatConfigurationGUID = defaultStatConfiguration.GUID
            });
        }

        private static bool AddCreature<TCreature>(List<TCreature> creatures, List<Guid> creatureGUIDs, TCreature creature)
            where TCreature : Creature
        {
            var added = false;

            if (!creatures.Any(existing => existing.GUID == creature.GUID))
            {
                creatures.Add(creature);
                added = true;
            }

            if (!creatureGUIDs.Contains(creature.GUID))
            {
                creatureGUIDs.Add(creature.GUID);
                added = true;
            }

            return added;
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
