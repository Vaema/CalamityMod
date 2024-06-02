using CalamityMod.Items.DraedonMisc;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAuric;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAuric
{
    public class AuricServer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<AuricServerTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AuricPanel>(12).
                AddIngredient<DraedonPowerCell>(15).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
