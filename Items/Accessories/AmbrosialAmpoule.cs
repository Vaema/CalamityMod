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
    public class AmbrosialAmpoule : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int MaxLifeBoost = 50;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifeBoost, RadiantOoze.MinRegenBoost.ToRegenPerSecond(), RadiantOoze.MaxRegenBoost.ToRegenPerSecond());

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            player.statLifeMax2 += MaxLifeBoost;
            if (!player.HasBuff(BuffID.Honey))
                player.AddBuff(BuffID.Honey, 2);

            // bool left in for abyss light purposes and life regen effects
            modPlayer.aAmpoule = true;

            // Inherits all effects of Honey Dew and Living Dew
            modPlayer.honeyDewHalveDebuffs = true;
            modPlayer.livingDewHalveDebuffs = true;

            // Add light if the other accessories aren't equipped and visibility is turned on
            if (!(modPlayer.rOoze || modPlayer.purity) && !hideVisual)
                Lighting.AddLight(player.Center, new Vector3(1.2f, 1.2f, 0.72f));
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            var player = Main.LocalPlayer;
            if (player != null)
            {
                list.FindAndReplace("[REGEN]", player.Calamity().radiantOozeRegen.ToString("0.##"));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LivingDew>().
                AddIngredient<RadiantOoze>().
                AddIngredient<LifeAlloy>(3).
                AddIngredient(ItemID.FragmentSolar, 6).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
