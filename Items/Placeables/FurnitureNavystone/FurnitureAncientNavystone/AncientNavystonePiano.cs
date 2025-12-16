using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureNavystone.FurnitureAncientNavystone
{
    [LegacyName("EutrophicPiano")]
    public class AncientNavystonePiano : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureNavystone.FurnitureAncientNavystone.AncientNavystonePiano>());
            Item.value = Item.sellPrice(copper: 60);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AncientSmoothNavystone>(15).
                AddIngredient(ItemID.Bone, 4).
                AddIngredient(ItemID.Book).
                AddTile(TileID.Sawmill).
                Register();
        }
    }
}
