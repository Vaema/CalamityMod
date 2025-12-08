using CalamityMod.Tiles.Abyss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture
{
    public class RustyChest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<RustyChestTile>());
            Item.value = Item.sellPrice(silver: 10); // Special: generated chest price
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Items.Placeables.Abyss.HardenedSulphurousSandstone>(8).
                AddRecipeGroup("IronBar", 2).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
