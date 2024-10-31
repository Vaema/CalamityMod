using System.Collections.Generic;
using CalamityMod.NPCs.NormalNPCs;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class BossRushHPChangeDict : ModSystem
    {
        public static IDictionary<int, int> Dict { get; private set; }

        public override void OnModLoad()
        {
            Dict = new Dictionary<int, int>
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
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }
    }
}
