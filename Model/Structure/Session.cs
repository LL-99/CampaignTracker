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
        public List<SessionCombatReference> CombatReferences { get; private set; } = [];
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

        [JsonIgnore]
        public IReadOnlyCollection<Combat> OrderedCombats => Combats
            .OrderBy(GetCombatIndex)
            .ThenBy(combat => combat.Name)
            .ToList();

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

            ReindexCombats();

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
            AddCombat(combat, null);
        }

        public void AddCombat(Combat combat, int? combatIndex)
        {
            AddCombatDirect(combat);
            SetCombatIndexDirect(combat, combatIndex ?? GetCombatIndex(combat));
            ReindexCombats();
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
            ReindexCombats();
        }

        public int GetCombatIndex(Combat combat)
        {
            var combatReference = CombatReferences.FirstOrDefault(reference => reference.CombatGUID == combat.GUID);

            if (combatReference is not null)
            {
                return combatReference.CombatIndex;
            }

            var existingIndex = CombatGUIDs.FindIndex(combatGuid => combatGuid == combat.GUID);

            return existingIndex < 0
                ? GetNextCombatIndex()
                : existingIndex + 1;
        }

        public void SetCombatIndex(Combat combat, int combatIndex)
        {
            if (!Combats.Any(existing => existing.GUID == combat.GUID))
            {
                return;
            }

            SetCombatIndexDirect(combat, combatIndex);
            ReindexCombats();
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
            SetCombatIndexDirect(combat, GetNextCombatIndex());
            ReindexCombats();
            AddCombatCreatures(combat);
        }

        internal void RemoveCombatFromCombat(Combat combat)
        {
            if (RemoveCombatDirect(combat))
            {
                RemoveCombatCreaturesIfUnused(combat);
                ReindexCombats();
            }
        }

        internal void ClearReferences()
        {
            Campaign = null!;
            PlayerCharacters.Clear();
            PlayerCharacterGUIDs.Clear();
            Npcs.Clear();
            NpcGUIDs.Clear();
            Enemies.Clear();
            EnemyGUIDs.Clear();
            Combats.Clear();
            CombatGUIDs.Clear();
            CombatReferences.Clear();
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

            if (!CombatReferences.Any(reference => reference.CombatGUID == combat.GUID))
            {
                CombatReferences.Add(new SessionCombatReference
                {
                    CombatGUID = combat.GUID,
                    CombatIndex = CombatReferences.Count == 0
                        ? CombatGUIDs.Count
                        : CombatReferences.Max(reference => reference.CombatIndex) + 1
                });
            }
        }

        private bool RemoveCombatDirect(Combat combat)
        {
            var removed = Combats.RemoveAll(existing => existing.GUID == combat.GUID) > 0;
            removed = CombatGUIDs.Remove(combat.GUID) || removed;
            removed = CombatReferences.RemoveAll(reference => reference.CombatGUID == combat.GUID) > 0 || removed;

            return removed;
        }

        private int GetNextCombatIndex()
        {
            return CombatReferences.Count == 0
                ? 1
                : CombatReferences.Max(reference => reference.CombatIndex) + 1;
        }

        private void SetCombatIndexDirect(Combat combat, int combatIndex)
        {
            var targetIndex = Math.Clamp(combatIndex, 1, Math.Max(Combats.Count, 1));
            var orderedCombats = OrderedCombats
                .Where(existing => existing.GUID != combat.GUID)
                .ToList();

            orderedCombats.Insert(Math.Min(targetIndex - 1, orderedCombats.Count), combat);

            for (var index = 0; index < orderedCombats.Count; index++)
            {
                EnsureCombatReference(orderedCombats[index]).CombatIndex = index + 1;
            }

            CombatGUIDs.Clear();
            CombatGUIDs.AddRange(orderedCombats.Select(orderedCombat => orderedCombat.GUID));
        }

        private void ReindexCombats()
        {
            var orderedCombats = OrderedCombats.ToList();
            CombatReferences.RemoveAll(reference => !Combats.Any(combat => combat.GUID == reference.CombatGUID));

            for (var index = 0; index < orderedCombats.Count; index++)
            {
                EnsureCombatReference(orderedCombats[index]).CombatIndex = index + 1;
            }

            CombatGUIDs.Clear();
            CombatGUIDs.AddRange(orderedCombats.Select(orderedCombat => orderedCombat.GUID));
        }

        private SessionCombatReference EnsureCombatReference(Combat combat)
        {
            var combatReference = CombatReferences.FirstOrDefault(reference => reference.CombatGUID == combat.GUID);

            if (combatReference is not null)
            {
                return combatReference;
            }

            combatReference = new SessionCombatReference
            {
                CombatGUID = combat.GUID,
                CombatIndex = GetNextCombatIndex()
            };

            CombatReferences.Add(combatReference);
            return combatReference;
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
