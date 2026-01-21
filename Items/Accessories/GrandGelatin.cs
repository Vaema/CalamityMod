using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class GrandGelatin : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float MoveSpeedBoost = 0.12f;
        public static float JumpSpeedBoost = 0.6f; // Both 12% so we only need just one in the tooltip
        public static int AuraLifetime = 1800;
        public static int AuraRegenBoost = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBoost.ToPercent(), AuraLifetime.FramesToSeconds(), AuraRegenBoost.ToRegenPerSecond());

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 52;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.GrandGelatin = true;
            player.moveSpeed += MoveSpeedBoost;
            player.jumpSpeedBoost += JumpSpeedBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CleansingJelly>().
                AddIngredient<LifeJelly>().
                AddIngredient<VitalJelly>().
                AddIngredient(ItemID.SoulofLight, 2).
                AddIngredient(ItemID.SoulofNight, 2).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
