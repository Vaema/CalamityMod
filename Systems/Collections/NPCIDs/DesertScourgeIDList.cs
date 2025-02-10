using System.Collections.Generic;
using CalamityMod.NPCs.DesertScourge;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class DesertScourgeIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<DesertScourgeHead>(),
                NPCType<DesertScourgeBody>(),
                NPCType<DesertScourgeTail>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
