using System.Collections.Generic;
using CalamityMod.NPCs.SlimeGod;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class SlimeGodIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<EbonianPaladin>(),
                NPCType<CrimulanPaladin>(),
                NPCType<SplitEbonianPaladin>(),
                NPCType<SplitCrimulanPaladin>(),
                NPCType<SlimeGodCore>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
