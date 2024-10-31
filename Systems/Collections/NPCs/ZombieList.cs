using System.Collections.Generic;
using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class ZombieList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.Zombie,
                NPCID.ArmedZombie,
                NPCID.BaldZombie,
                NPCID.PincushionZombie,
                NPCID.ArmedZombiePincussion, // what is this spelling
                NPCID.SlimedZombie,
                NPCID.ArmedZombieSlimed,
                NPCID.SwampZombie,
                NPCID.ArmedZombieSwamp,
                NPCID.TwiggyZombie,
                NPCID.ArmedZombieTwiggy,
                NPCID.FemaleZombie,
                NPCID.ArmedZombieCenx,
                NPCID.ZombieRaincoat,
                NPCID.ZombieEskimo,
                NPCID.ArmedZombieEskimo,
                NPCID.BigRainZombie,
                NPCID.SmallRainZombie,
                NPCID.BigFemaleZombie,
                NPCID.SmallFemaleZombie,
                NPCID.BigTwiggyZombie,
                NPCID.SmallTwiggyZombie,
                NPCID.BigSwampZombie,
                NPCID.SmallSwampZombie,
                NPCID.BigSlimedZombie,
                NPCID.SmallSlimedZombie,
                NPCID.BigPincushionZombie,
                NPCID.SmallPincushionZombie,
                NPCID.BigBaldZombie,
                NPCID.SmallBaldZombie,
                NPCID.BigZombie,
                NPCID.SmallZombie,
                NPCID.MaggotZombie,
                NPCType<BucketZombie>()
                // halloween zombies not included because they don't drop shackles or zombie arms
            ];
        }

        public override void Unload() => List = null;

        public static bool IsZombie(NPC npc) => List.Contains(npc.type);
    }
}
