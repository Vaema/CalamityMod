using System.Collections.Generic;
using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class NeedsDebuffIconDisplayList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.TargetDummy,
                NPCID.WallofFleshEye,
                NPCType<SuperDummyNPC>()
            ];
        }

        public override void Unload() => List = null;

        public static bool IsNPCNeedDebuffIconDisplay(NPC npc) => List.Contains(npc.type);
    }
}
