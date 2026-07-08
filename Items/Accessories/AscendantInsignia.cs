using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("YharimsInsignia")]
    public class AscendantInsignia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        // These values override Soaring Insignia's
        public static double FlightTimeBoost = 0.5D; 
        public static float MoveSpeedBoost = 0.15f;
        public static float JumpSpeedBoost = 0.75f; // NOTE: Tooltip shares this number with move speed % as they're equal
        public static float AccelerationBoost = 0.5f;
        public static int AbilityDuration = CalamityUtils.SecondsToFrames(4);
        public static int AbilityCooldown = CalamityUtils.SecondsToFrames(40);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlightTimeBoost.ToPercent(), MoveSpeedBoost.ToPercent(), (1f + AccelerationBoost).Round(), AbilityDuration.FramesToSeconds(), AbilityCooldown.FramesToSeconds());

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateDynamicHotkey(Item);
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 36;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.ascendantInsignia = true;
            player.empressBrooch = true;
            player.moveSpeed += MoveSpeedBoost;
            player.jumpSpeedBoost += JumpSpeedBoost - 0.5f;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.EmpressFlightBooster).
                AddIngredient<EffulgentFeather>(5).
                AddIngredient<DivineGeode>(5).
                AddIngredient(ItemID.SoulofFlight, 10).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
