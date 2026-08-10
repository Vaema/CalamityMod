using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAbyss;

[LegacyName("AbyssPiano")]
public class AbyssSynth : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureAbyss.AbyssSynth>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SmoothAbyssGravel>(15).
            AddIngredient(ItemID.Bone, 4).
            AddIngredient(ItemID.Book).
            AddTile<VoidCondenser>().
            Register();
    }
}
