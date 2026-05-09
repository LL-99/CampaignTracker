using CampaignTracker.Model.Combats;
using CampaignTracker.Model.Creatures;
using CampaignTracker.Model.Structure;
using Newtonsoft.Json;

namespace CampaignTracker.Services
{
    public class DataService
        : IDataService
    {
        public Campaign? Campaign { get; private set; }

        public DataService()
        {
            Campaign = CreateTestCampaign();
        }

        public void ClearCampaign()
        {
            Campaign = new();
            Campaign.EnsureDefaultEnvironmentals();
        }

        public bool TrySetCampaignString(string json)
        {
            Campaign? campaign;

            try
            {
                campaign = JsonConvert.DeserializeObject<Campaign>(json);
            }
            catch (JsonException)
            {
                return false;
            }

            if (campaign is null)
            {
                return false;
            }

            InitCampaignReferences(campaign);
            Campaign = campaign;

            return true;
        }

        private static void InitCampaignReferences(Campaign campaign)
        {
            campaign.EnsureDefaultEnvironmentals();

            foreach (var combat in campaign.Combats)
            {
                combat.PostInit(campaign);
            }

            foreach (var session in campaign.Sessions)
            {
                session.PostInit(campaign);
            }
        }

        private static Campaign CreateTestCampaign()
        {
            var campaign = new Campaign();

            var mira = Player("Mira Thorn", ("Ranger 6", 52), ("Ranger 5 / Rogue 1", 48));
            var owen = Player("Owen Ashmantle", ("Cleric 6", 45), ("Cleric 5", 39));
            var seraphine = Player("Seraphine Vale", ("Wizard 6", 32), ("Wizard 5", 27));
            var thalia = Player("Thalia Voss", ("Fighter 6", 61), ("Fighter 5", 54));

            foreach (var playerCharacter in new[] { mira, owen, seraphine, thalia })
            {
                campaign.AddPlayerCharacter(playerCharacter);
            }

            var captainIlyra = Static("Captain Ilyra Dawnwatch", 38, 2);
            var brotherCalem = Static("Brother Calem", 22, 1);
            var archivistNera = Static("Archivist Nera", 18, null);
            var quartermasterRusk = Static("Quartermaster Rusk", 31, 1);

            foreach (var npc in new[] { archivistNera, brotherCalem, captainIlyra, quartermasterRusk })
            {
                campaign.AddNpc(npc);
            }

            var ashWolf = Static("Ash Wolf", 11, 1);
            var bloodCultist = Static("Blood Cultist", 16, 2);
            var boneWarden = Static("Bone Warden", 44, 5);
            var emberDrake = Static("Ember Drake", 75, 7);
            var ghoulScout = Static("Ghoul Scout", 22, 2);
            var hobgoblinShield = Static("Hobgoblin Shield", 18, 2);
            var obsidianMyrmidon = Static("Obsidian Myrmidon", 58, 6);
            var plagueRatSwarm = Static("Plague Rat Swarm", 24, 2);

            foreach (var enemy in new[] { ashWolf, bloodCultist, boneWarden, emberDrake, ghoulScout, hobgoblinShield, obsidianMyrmidon, plagueRatSwarm })
            {
                campaign.AddEnemy(enemy);
            }

            var session1 = Session(campaign, "Smoke on the Trade Road", 0, mira, owen, seraphine, thalia);
            var session2 = Session(campaign, "Knives Beneath Eastmarket", 7, mira, owen, seraphine, thalia);
            var session3 = Session(campaign, "The Quarry Below", 14, mira, owen, seraphine, thalia);
            var session4 = Session(campaign, "Ash Abbey Revelations", 21, mira, owen, seraphine, thalia);
            var session5 = Session(campaign, "Blackgate Under Siege", 28, mira, owen, seraphine, thalia);
            var session6 = Session(campaign, "The Cinder Catacombs", 35, mira, owen, seraphine, thalia);

            session1.AddNpc(captainIlyra);
            session2.AddNpc(quartermasterRusk);
            session3.AddNpc(archivistNera);
            session4.AddNpc(brotherCalem);
            session5.AddNpc(captainIlyra);
            session6.AddNpc(archivistNera);

            Combat(campaign, "Roadside Pack", [(session1, 1)], [mira, thalia], [], [ashWolf, ashWolf]);
            Combat(campaign, "Burned Wagon Cultists", [(session1, 2)], [mira, owen, seraphine, thalia], [captainIlyra], [bloodCultist, bloodCultist]);
            Combat(campaign, "Market Knife Cell", [(session2, 1)], [mira, owen, seraphine, thalia], [quartermasterRusk], [bloodCultist, hobgoblinShield]);
            Combat(campaign, "The Long Hunt", [(session2, 2), (session3, 1)], [mira, thalia], [], [ashWolf, ghoulScout, ghoulScout]);
            Combat(campaign, "Quarry Gate", [(session3, 2)], [mira, owen, seraphine, thalia], [archivistNera], [hobgoblinShield, hobgoblinShield, ghoulScout]);
            Combat(campaign, "Bone Warden Awakening", [(session3, 3)], [owen, seraphine, thalia], [archivistNera], [boneWarden]);
            Combat(campaign, "Abbey Cloister Ambush", [(session4, 1)], [mira, owen, seraphine, thalia], [brotherCalem], [bloodCultist, plagueRatSwarm]);
            Combat(campaign, "Crypt Flame Trial", [(session4, 2)], [mira, owen, seraphine], [], [emberDrake]);
            Combat(campaign, "Blackgate Breach", [(session5, 1)], [mira, owen, seraphine, thalia], [captainIlyra], [hobgoblinShield, obsidianMyrmidon]);
            Combat(campaign, "Siege of Blackgate", [(session5, 2), (session6, 1)], [mira, owen, thalia], [captainIlyra], [obsidianMyrmidon, boneWarden]);
            Combat(campaign, "Rat Flooded Tunnels", [(session6, 2)], [mira, seraphine, thalia], [archivistNera], [plagueRatSwarm, plagueRatSwarm]);
            var cinderHeartGuardian = Combat(campaign, "Cinder Heart Guardian", [(session6, 3)], [mira, owen, seraphine, thalia], [archivistNera], [emberDrake, boneWarden]);
            AddExampleActionLogEntries(cinderHeartGuardian, mira, owen, seraphine, thalia, archivistNera, emberDrake, boneWarden);

            return campaign;
        }

        private static PlayerCharacter Player(string name, params (string ClassesAndLevels, float HP)[] statConfigurations)
        {
            return new PlayerCharacter
            {
                Name = name,
                StatConfigurations = statConfigurations
                    .Select(statConfiguration => new PlayerCharacterStatConfiguration
                    {
                        ClassesAndLevels = statConfiguration.ClassesAndLevels,
                        HP = statConfiguration.HP
                    })
                    .ToList()
            };
        }

        private static StaticCreature Static(string name, float hp, float? challengeRating)
        {
            return new StaticCreature
            {
                Name = name,
                Stats = new CreatureStats(hp, [], []),
                ChallengeRating = challengeRating
            };
        }

        private static Session Session(Campaign campaign, string name, int daysAfterStart, params PlayerCharacter[] players)
        {
            var session = new Session(campaign)
            {
                Name = name,
                DateUtc = new DateTime(2026, 1, 6, 19, 0, 0, DateTimeKind.Utc).AddDays(daysAfterStart)
            };

            foreach (var player in players)
            {
                session.AddPlayerCharacter(player);
            }

            return session;
        }

        private static Combat Combat(
            Campaign campaign,
            string name,
            IReadOnlyCollection<(Session Session, int CombatIndex)> sessions,
            IReadOnlyCollection<PlayerCharacter> players,
            IReadOnlyCollection<StaticCreature> npcs,
            IReadOnlyCollection<StaticCreature> enemies)
        {
            var combat = new Combat(campaign)
            {
                Name = name,
                ActionLog = new ActionLog()
            };

            foreach (var (session, combatIndex) in sessions)
            {
                session.AddCombat(combat, combatIndex);
            }

            foreach (var player in players)
            {
                combat.AddPlayerCharacter(player);
            }

            foreach (var npc in npcs)
            {
                combat.AddNpc(npc);
            }

            foreach (var enemy in enemies)
            {
                combat.AddEnemy(enemy);
            }

            return combat;
        }

        private static void AddExampleActionLogEntries(
            Combat combat,
            PlayerCharacter mira,
            PlayerCharacter owen,
            PlayerCharacter seraphine,
            PlayerCharacter thalia,
            StaticCreature archivistNera,
            StaticCreature emberDrake,
            StaticCreature boneWarden)
        {
            combat.ActionLog.AddEntry(
                combat.GUID,
                [emberDrake.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Ember Drake exhales fire.",
                    DamageInstances =
                    [
                        new() { Target = mira.GUID, DamageType = DamageType.Fire, BaseDamage = 22, DamageMultiplier = 0.5f, Note = "Successful save" },
                        new() { Target = seraphine.GUID, DamageType = DamageType.Fire, BaseDamage = 22, DamageMultiplier = 1 },
                        new() { Target = thalia.GUID, DamageType = DamageType.Fire, BaseDamage = 22, DamageMultiplier = 0.5f, Note = "Successful save" }
                    ]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [thalia.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Thalia lands a smite-backed longsword strike.",
                    DamageInstances =
                    [
                        new() { Target = boneWarden.GUID, DamageType = DamageType.Slashing, BaseDamage = 10, DamageMultiplier = 1 },
                        new() { Target = boneWarden.GUID, DamageType = DamageType.Radiant, BaseDamage = 13, DamageMultiplier = 1 }
                    ]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [seraphine.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Ice knife hits the Ember Drake and bursts over both guardians.",
                    DamageInstances =
                    [
                        new() { Target = emberDrake.GUID, DamageType = DamageType.Piercing, BaseDamage = 5, DamageMultiplier = 1 },
                        new() { Target = emberDrake.GUID, DamageType = DamageType.Cold, BaseDamage = 11, DamageMultiplier = 0.5f },
                        new() { Target = boneWarden.GUID, DamageType = DamageType.Cold, BaseDamage = 11, DamageMultiplier = 1 }
                    ]
                },
                new ActionEffect_Condition
                {
                    Type = EffectType.Condition,
                    Description = "Bone Warden is knocked prone by the icy blast.",
                    Condition = Condition.Prone,
                    Targets = [boneWarden.GUID]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [owen.GUID],
                new ActionEffect_Heal
                {
                    Type = EffectType.Heal,
                    Description = "Owen restores Mira with healing word.",
                    HealAmount = 10,
                    Targets = [mira.GUID]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [boneWarden.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Bone Warden sweeps its glaive through the front line.",
                    DamageInstances =
                    [
                        new() { Target = thalia.GUID, DamageType = DamageType.Slashing, BaseDamage = 12, DamageMultiplier = 1 },
                        new() { Target = owen.GUID, DamageType = DamageType.Slashing, BaseDamage = 12, DamageMultiplier = 1 }
                    ]
                },
                new ActionEffect_Condition
                {
                    Type = EffectType.Condition,
                    Description = "Owen fails the dread aura save.",
                    Condition = Condition.Frightened,
                    Targets = [owen.GUID]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [archivistNera.GUID],
                new ActionEffect_TemporaryHP
                {
                    Type = EffectType.TemporaryHP,
                    Description = "Archivist Nera reinforces the party with a warding chant.",
                    TemporaryHPAmount = 6,
                    Targets = [mira.GUID, owen.GUID, seraphine.GUID, thalia.GUID]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [mira.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Mira marks the Ember Drake and fires two arrows.",
                    DamageInstances =
                    [
                        new() { Target = emberDrake.GUID, DamageType = DamageType.Piercing, BaseDamage = 9, DamageMultiplier = 1 },
                        new() { Target = emberDrake.GUID, DamageType = DamageType.Piercing, BaseDamage = 8, DamageMultiplier = 1 }
                    ]
                },
                new ActionEffect_Buff
                {
                    Type = EffectType.Buff,
                    Description = "Hunter's mark remains on the Ember Drake.",
                    Targets = [emberDrake.GUID]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [emberDrake.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "The Ember Drake releases a sudden fire burst.",
                    DamageInstances =
                    [
                        new() { Target = seraphine.GUID, DamageType = DamageType.Fire, BaseDamage = 10, DamageMultiplier = 0, Note = "Successful save and evasion" },
                        new() { Target = boneWarden.GUID, DamageType = DamageType.Fire, BaseDamage = 10, DamageMultiplier = 0.5f, Note = "Fire resistance" }
                    ]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [owen.GUID],
                new ActionEffect_Damage
                {
                    Type = EffectType.Damage,
                    Description = "Bone Warden succeeds on the sacred flame save.",
                    DamageInstances =
                    [
                        new() { Target = boneWarden.GUID, DamageType = DamageType.Radiant, BaseDamage = 14, DamageMultiplier = 0, Note = "Successful save" }
                    ]
                });

            combat.ActionLog.AddEntry(
                combat.GUID,
                [thalia.GUID],
                new ActionEffect_Heal
                {
                    Type = EffectType.Heal,
                    Description = "Thalia uses second wind.",
                    HealAmount = 11,
                    Targets = [thalia.GUID]
                });
        }
    }
}
