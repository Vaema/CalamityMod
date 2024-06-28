using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMarnite
{
    public class MarniteBookcase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.value = 0;
            Item.createTile = ModContent.TileType<Tiles.FurnitureMarnite.MarniteBookcase>();
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PolishedMarniteBlock>(4).
                AddIngredient(ItemID.Book, 10).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
