using System.Collections.Generic;
using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class SkeletronPrimeIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.SkeletronPrime,
                NPCType<SkeletronPrime2>(),
                NPCID.PrimeCannon,
                NPCID.PrimeLaser,
                NPCID.PrimeSaw,
                NPCID.PrimeVice
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(NPC npc) => List.Contains(npc.type);
        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
