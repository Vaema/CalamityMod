using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class BlacklistedWeaponsWithToolPowerList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            // This list intentionally does not contain Grax.
            List =
            [
                ItemID.ButchersChainsaw,
                ItemID.LucyTheAxe,
                ItemID.Rockfish,
                ItemType<AxeofPurity>(),
                ItemType<HydraulicVoltCrasher>(),
                ItemType<InfernaCutter>(),
                ItemType<PhotonRipper>(),
                ItemType<Respiteblock>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if this Item is Blacklisted.
        /// </summary>
        public static bool IsBlacklistedItem(Item item) => List.Contains(item.type);
    }
}
