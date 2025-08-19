using CalamityMod.Items.Placeables.Astral;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureMonolith
{
    public class MonolithLantern : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureMonolith.MonolithLantern>());
            Item.value = Item.sellPrice(copper: 30);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralMonolith>(6).
                AddIngredient(ItemID.Torch).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
