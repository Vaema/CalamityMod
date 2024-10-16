using System.Collections.Generic;
using CalamityMod.Items.Fishing.FishingRods;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class HighTestFishingPoleList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ItemID.GoldenFishingRod,
                ItemType<EarlyBloomRod>(),
                ItemType<TheDevourerofCods>()
            ];
        }

        public override void Unload() => List = null;

        public static bool IsHighTestFishingPole(Item item) => List.Contains(item.type);
    }
}
