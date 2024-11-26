using System.Collections.Generic;
using CalamityMod.NPCs.AstrumDeus;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class AstrumDeusIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<AstrumDeusHead>(),
                NPCType<AstrumDeusBody>(),
                NPCType<AstrumDeusTail>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
