using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class BlackGlassBand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public static int cooldown = 300;
        public static int damage = 32;
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 23;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.bGlassBand = true;
            modPlayer.bGlassBandVisual = !hideVisual;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddRecipeGroup("AnyGoldBar", 8).
                AddIngredient(ItemID.Diamond, 3).
                AddIngredient(ItemID.Obsidian, 35).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
