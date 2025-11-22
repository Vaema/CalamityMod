using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class AmalgamatedBrain : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float DamageBoost = 0.05f;
        public static int NimbusDamage => CalamityUtils.ScaleWithDifficulty(18);
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.rBrain = true; // Handles shaderain cloud spawning on hit
            modPlayer.aBrain = true;
            player.brainOfConfusionItem = Item;
            player.GetDamage<GenericDamageClass>() += DamageBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RottenBrain>().
                AddIngredient(ItemID.BrainOfConfusion).
                AddIngredient(ItemID.SoulofNight, 3).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
