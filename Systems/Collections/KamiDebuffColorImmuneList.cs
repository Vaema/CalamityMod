using System.Collections.Generic;
using CalamityMod.NPCs.OldDuke;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class KamiDebuffColorImmuneList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            // Duke Fishron and Old Duke phase 3 becomes way too easy if you can make him stop being invisible with Yanmei's Knife.
            // This is a list so that other NPCs can be added as necessary.
            // IT DOES NOT make them immune to the debuff, just stops them from being recolored.
            List =
            [
                NPCID.DukeFishron,
                NPCType<OldDuke>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
