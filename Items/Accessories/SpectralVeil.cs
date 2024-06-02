using System.Collections.Generic;
using CalamityMod.Balancing;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class SpectralVeil : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public const float TeleportRange = 845f;

        // Boosted by Cross Necklace.
        internal static readonly int VeilIFrames = BalancingConstants.VanillaDodgeIFrames;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 38;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.SpectralVeilHotKey);

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().spectralVeil = true;
            player.Calamity().stealthGenMoving += 0.15f;
            player.Calamity().stealthGenStandstill += 0.15f;
        }
    }
}
