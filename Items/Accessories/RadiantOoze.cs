using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class RadiantOoze : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int MinRegenBoost = 4;
        public static int MaxRegenBoost = 10;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinRegenBoost.ToRegenPerSecond(), MaxRegenBoost.ToRegenPerSecond());

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            // bool left in for abyss light purposes and life regen effects
            modPlayer.rOoze = true;

            // Add light if the other accessories aren't equipped and visibility is turned on
            if (!(modPlayer.aAmpoule || modPlayer.purity) && !hideVisual)
                Lighting.AddLight(player.Center, new Vector3(1f, 1f, 0.6f));
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            var player = Main.LocalPlayer;
            if (player != null)
            {
                list.FindAndReplace("[REGEN]", (player.Calamity().radiantOozeRegen).ToString("0.##"));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BlightedGel>(45).
                AddIngredient<PurifiedGel>(15).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
