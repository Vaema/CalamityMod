using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class MossHornetList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.MossHornet,
                NPCID.TinyMossHornet,
                NPCID.LittleMossHornet,
                NPCID.BigMossHornet,
                NPCID.GiantMossHornet
            ];
        }

        public override void Unload() => List = null;

        public static bool IsMossHornet(NPC npc) => List.Contains(npc.type);
    }
}
