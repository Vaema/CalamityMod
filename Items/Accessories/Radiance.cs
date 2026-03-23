using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("AstralArcanum", "Purity")]
    public class Radiance : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int MaxLifeBoost = 70;
        public static int DebuffedRegenBoost = 4; // Added on top of the baseline regen boost
        public static int DebuffedDefenseBoost = 9;
        public static int ExtraDebuffDefenseBoost = 4; // Per additional debuff
        public static int FramesToDecayDefense = 15;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifeBoost);
        public LocalizedText TooltipExtensionText => this.GetLocalization("HoldShiftTooltip").WithFormatArgs(LivingDew.RegenTimeBoost.ToPercent(), (HoneyDew.NaturalRegenPower - 1f).ToPercent(),
        RadiantOoze.MinRegenBoost.ToRegenPerSecond(), RadiantOoze.MaxRegenBoost.ToRegenPerSecond(), DebuffedDefenseBoost, DebuffedRegenBoost.ToRegenPerSecond(), ExtraDebuffDefenseBoost);
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 7));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 44;
            Item.defense = 3;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            player.statLifeMax2 += MaxLifeBoost;

            // Abyss light, debuff near-immunity, and life regen effects
            modPlayer.purity = true;

            // Inherits effects from Honey Dew and Living Dew
            modPlayer.honeyDew = true;
            modPlayer.livingDew = true;

            // Add light if the other accessories aren't equipped and visibility is turned on
            if (!(modPlayer.rOoze || modPlayer.aAmpoule) && !hideVisual)
                Lighting.AddLight(player.Center, new Vector3(1.32f, 1.32f, 1.82f));
        }


        public override void ModifyTooltips(List<TooltipLine> list)
        {
            var player = Main.LocalPlayer;
            if (player != null)
            {
                list.FindAndReplace("[REGEN]", player.Calamity().purityRegen.ToString("0.##"));
                list.FindAndReplace("[DEFENSE]", player.Calamity().jewelBonusDefense.ToString());
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AmbrosialAmpoule>().
                AddIngredient<InfectedJewel>().
                AddIngredient<AuricBar>(5).
                AddIngredient<AscendantSpiritEssence>(4).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
