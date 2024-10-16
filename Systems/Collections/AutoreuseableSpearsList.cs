using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of all Vanilla Spears to make them autoreusable.
    /// </summary>
    public sealed class AutoreusableSpearsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ItemID.AdamantiteGlaive,
                ItemID.ChlorophytePartisan,
                ItemID.CobaltNaginata,
                ItemID.DarkLance,
                ItemID.MonkStaffT2,
                ItemID.Gungnir,
                ItemID.MushroomSpear,
                ItemID.MythrilHalberd,
                ItemID.NorthPole,
                ItemID.ObsidianSwordfish,
                ItemID.OrichalcumHalberd,
                ItemID.PalladiumPike,
                ItemID.Spear,
                ItemID.Swordfish,
                ItemID.TheRottedFork,
                ItemID.TitaniumTrident,
                ItemID.Trident,
                ItemID.ThunderSpear
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not an item is an autoreusable spear.
        /// </summary>
        public static bool IsAutoreuseableSpear(Item item) => List.Contains(item.type);
    }
}
