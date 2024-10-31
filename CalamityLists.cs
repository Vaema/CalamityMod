using System;
using System.Collections.Generic;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod
{
    public sealed class CalamityLists : ModSystem
    {
        public static SortedDictionary<int, int> BossRushHPChanges;
        public static SortedDictionary<int, int> BossValues;
        public static SortedDictionary<int, int> bossTypes;

        public static Dictionary<int, int> EncryptedSchematicIDRelationship;

        public static List<int> DisabledSummonerNerfMinions;

        /// <summary>
        /// Each Sunken Sea subbiome has a correspoding spawn condition boolean value and a biome type.
        /// </summary>
        public static SortedDictionary<SunkenSeaBiomeFlags, (Func<NPCSpawnInfo, bool> SpawnCondition, int BiomeType)> SunkenSeaBiomeCorrespondentValues { get; private set; }

        public override void OnModLoad()
        {
            // Collections
            BossRushHPChanges = new SortedDictionary<int, int>
            {
                // Tier 1
                { NPCID.KingSlime, 300000 }, // 30 seconds
                { NPCID.BlueSlime, 3600 },
                { NPCID.SlimeSpiked, 7200 },
                { NPCID.GreenSlime, 2700 },
                { NPCID.RedSlime, 5400 },
                { NPCID.PurpleSlime, 7200 },
                { NPCID.YellowSlime, 6300 },
                { NPCID.IceSlime, 4500 },
                { NPCID.UmbrellaSlime, 5400 },
                { NPCID.RainbowSlime, 30000 },
                { NPCID.Pinky, 15000 },
                { NPCType<KingSlimeJewelRuby>(), 21000 },
                { NPCType<KingSlimeJewelSapphire>(), 18000 },
                { NPCType<KingSlimeJewelEmerald>(), 24000 },

                { NPCID.EyeofCthulhu, 450000 }, // 30 seconds
                { NPCID.ServantofCthulhu, 6000 },
                { NPCType<BloodlettingServant>(), 12000 },

                { NPCID.EaterofWorldsHead, 10000 }, // 30 seconds + immunity timer at start
                { NPCID.EaterofWorldsBody, 10000 },
                { NPCID.EaterofWorldsTail, 10000 },

                { NPCID.BrainofCthulhu, 100000 }, // 30 seconds with creepers
                { NPCID.Creeper, 10000 },

                { NPCID.QueenBee, 315000 }, // 30 seconds
                { NPCID.Bee, 3000 },
                { NPCID.BeeSmall, 2000 },
                { NPCID.BigHornetHoney, 10000 },
                { NPCID.HornetHoney, 7500 },
                { NPCID.LittleHornetHoney, 5000 },

                { NPCID.Deerclops, 315000 }, // 30 seconds

                { NPCID.SkeletronHead, 150000 }, // 30 seconds
                { NPCID.SkeletronHand, 60000 },

                { NPCID.WallofFlesh, 450000 }, // 30 seconds
                { NPCID.WallofFleshEye, 450000 },
                { NPCID.TheHungry, 10000 },
                { NPCID.TheHungryII, 5000 },
                { NPCID.LeechHead, 5000 },
                { NPCID.LeechBody, 5000 },
                { NPCID.LeechTail, 5000 },

                // Tier 2
                { NPCID.QueenSlimeBoss, 200000 }, // 30 seconds
                { NPCID.QueenSlimeMinionBlue, 6000 },
                { NPCID.QueenSlimeMinionPink, 6000 },
                { NPCID.QueenSlimeMinionPurple, 5000 },

                { NPCID.Spazmatism, 150000 }, // 30 seconds
                { NPCID.Retinazer, 125000 },
                { NPCType<Foveanator>(), 137500 },

                { NPCID.TheDestroyer, 250000 }, // 30 seconds + immunity timer at start
                { NPCID.TheDestroyerBody, 250000 },
                { NPCID.TheDestroyerTail, 250000 },
                { NPCID.Probe, 5000 },

                { NPCID.SkeletronPrime, 160000 }, // 30 seconds
                { NPCType<SkeletronPrime2>(), 160000 },
                { NPCID.PrimeVice, 54000 },
                { NPCID.PrimeCannon, 45000 },
                { NPCID.PrimeSaw, 45000 },
                { NPCID.PrimeLaser, 38000 },

                { NPCID.Plantera, 160000 }, // 30 seconds
                { NPCID.PlanterasTentacle, 5000 },
                { NPCType<PlanterasFreeTentacle>(), 5000 },

                // Tier 3
                { NPCID.Golem, 50000 }, // 30 seconds
                { NPCID.GolemHead, 30000 },
                { NPCID.GolemHeadFree, 30000 },
                { NPCID.GolemFistLeft, 25000 },
                { NPCID.GolemFistRight, 25000 },

                { NPCID.HallowBoss, 200000 }, // 30 seconds

                { NPCID.DukeFishron, 290000 }, // 30 seconds

                { NPCID.CultistBoss, 220000 }, // 30 seconds
                { NPCID.CultistDragonHead, 60000 },
                { NPCID.CultistDragonBody1, 60000 },
                { NPCID.CultistDragonBody2, 60000 },
                { NPCID.CultistDragonBody3, 60000 },
                { NPCID.CultistDragonBody4, 60000 },
                { NPCID.CultistDragonTail, 60000 },
                { NPCID.AncientCultistSquidhead, 50000 },

                { NPCID.MoonLordCore, 160000 }, // 1 minute
                { NPCID.MoonLordHand, 45000 },
                { NPCID.MoonLordHead, 60000 },
                { NPCID.MoonLordLeechBlob, 800 }

                // 9.5 minutes in total for vanilla Boss Rush bosses
            };

            // NOTE: This does not account for Calamity's base value increases
            BossValues = new SortedDictionary<int, int>
            {
                { NPCID.KingSlime, Item.buyPrice(0, 2) },
                { NPCID.EyeofCthulhu, Item.buyPrice(0, 2) },
                // Evil bosses drop 5 gold in vanilla; unmodified
                { NPCID.QueenBee, Item.buyPrice(0, 8) },
                { NPCID.Deerclops, Item.buyPrice(0, 8) },
                { NPCID.SkeletronHead, Item.buyPrice(0, 12) },
                { NPCID.WallofFlesh, Item.buyPrice(0, 12) },
                { NPCID.QueenSlimeBoss, Item.buyPrice(0, 16) },
                { NPCID.Spazmatism, Item.buyPrice(0, 16) },
                { NPCID.Retinazer, Item.buyPrice(0, 16) },
                { NPCID.TheDestroyer, Item.buyPrice(0, 16) },
                { NPCID.SkeletronPrime, Item.buyPrice(0, 16) },
                { NPCID.Plantera, Item.buyPrice(0, 20) },
                { NPCID.Golem, Item.buyPrice(0, 25) },
                { NPCID.HallowBoss, Item.buyPrice(0, 30) },
                { NPCID.DukeFishron, Item.buyPrice(0, 30) },
                { NPCID.CultistBoss, Item.buyPrice(0, 50) }
                // Moon Lord drops 1 plat in vanilla; unmodified
            };

            bossTypes = new SortedDictionary<int, int>()
            {
                { NPCID.KingSlime, 1 },
                { NPCType<DesertScourgeHead>(), 2 },
                { NPCID.EyeofCthulhu, 3 },
                { NPCType<Crabulon>(), 4 },
                { NPCID.EaterofWorldsHead, 5 },
                { NPCID.EaterofWorldsBody, 5 },
                { NPCID.EaterofWorldsTail, 5 },
                { NPCID.BrainofCthulhu, 6 },
                { NPCType<HiveMind>(), 7 },
                { NPCType<PerforatorHive>(), 8 },
                { NPCID.QueenBee, 9 },
                { NPCID.SkeletronHead, 10 },
                { NPCType<SlimeGodCore>(), 11 },
                { NPCType<SplitEbonianPaladin>(), 11 },
                { NPCType<SplitCrimulanPaladin>(), 11 },
                { NPCID.WallofFlesh, 12 },
                { NPCType<Cryogen>(), 13 },
                { NPCID.Retinazer, 14 },
                { NPCID.Spazmatism, 14 },
                { NPCType<AquaticScourgeHead>(), 15 },
                { NPCID.TheDestroyer, 16 },
                { NPCType<BrimstoneElemental>(), 17 },
                { NPCID.SkeletronPrime, 18 },
                { NPCType<CalamitasClone>(), 19 },
                { NPCID.Plantera, 20 },
                { NPCType<Leviathan>(), 21 },
                { NPCType<Anahita>(), 21 },
                { NPCType<AstrumAureus>(), 22 },
                { NPCID.Golem, 23 },
                { NPCType<PlaguebringerGoliath>(), 24 },
                { NPCID.DukeFishron, 25 },
                { NPCType<RavagerBody>(), 26 },
                { NPCID.CultistBoss, 27 },
                { NPCType<AstrumDeusHead>(), 28 },
                { NPCID.MoonLordCore, 29 },
                { NPCType<ProfanedGuardianCommander>(), 30 },
                { NPCType<Bumblefuck>(), 31 },
                { NPCType<Providence>(), 32 },
                { NPCType<CeaselessVoid>(), 33 },
                { NPCType<StormWeaverHead>(), 34 },
                { NPCType<Signus>(), 35 },
                { NPCType<Polterghast>(), 36 },
                { NPCType<OldDuke>(), 37 },
                { NPCType<DevourerofGodsHead>(), 38 },
                { NPCType<Yharon>(), 39 },
                { NPCType<SupremeCalamitas>(), 40 },
                { NPCType<AresBody>(), 41 },
                { NPCType<ThanatosHead>(), 41 },
                { NPCType<Artemis>(), 41 },
                { NPCType<Apollo>(), 41 },
                { NPCID.QueenSlimeBoss, 42 },
                { NPCID.HallowBoss, 43 },
                { NPCID.Deerclops, 44 }
            };

            EncryptedSchematicIDRelationship = new Dictionary<int, int>()
            {
                [1] = ItemType<EncryptedSchematicPlanetoid>(),
                [2] = ItemType<EncryptedSchematicJungle>(),
                [3] = ItemType<EncryptedSchematicHell>(),
                [4] = ItemType<EncryptedSchematicIce>(),
            };

            DisabledSummonerNerfMinions = new();

            SunkenSeaBiomeCorrespondentValues = new()
            {
                { SunkenSeaBiomeFlags.UndergroundDesert, (spawnInfo => spawnInfo.Player.ZoneDesert, -1 /* None needed. */) },
                { SunkenSeaBiomeFlags.TimelessShores, (spawnInfo => spawnInfo.Player.Calamity().ZoneTimelessShores, GetInstance<TimelessShoresBiome>().Type) },
                { SunkenSeaBiomeFlags.RadiantReefs, (spawnInfo => spawnInfo.Player.Calamity().ZoneRadiantReefs, GetInstance<RadiantReefsBiome>().Type) },
                { SunkenSeaBiomeFlags.PolypForest, (spawnInfo => spawnInfo.Player.Calamity().ZonePolypForest, GetInstance<PolypForestBiome>().Type) },
                { SunkenSeaBiomeFlags.GleamingBurrows, (spawnInfo => spawnInfo.Player.Calamity().ZoneGleamingBurrows, GetInstance<GleamingBurrowsBiome>().Type) },
                { SunkenSeaBiomeFlags.BasaltGully, (spawnInfo => spawnInfo.Player.Calamity().ZoneBasaltGully, GetInstance<BasaltGullyBiome>().Type) },
            };
        }

        public override void Unload()
        {

            BossRushHPChanges?.Clear();
            BossRushHPChanges = null;
            BossValues?.Clear();
            BossValues = null;
            bossTypes?.Clear();
            bossTypes = null;

            EncryptedSchematicIDRelationship = null;

            DisabledSummonerNerfMinions = null;

            SunkenSeaBiomeCorrespondentValues = null;
        }
    }
}
