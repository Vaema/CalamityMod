using System.Collections.Generic;
using CalamityMod.NPCs.StormWeaver;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class StormWeaverIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<StormWeaverHead>(),
                NPCType<StormWeaverBody>(),
                NPCType<StormWeaverTail>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
