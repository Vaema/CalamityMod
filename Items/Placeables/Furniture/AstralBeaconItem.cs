using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Tiles.Astral;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture;

public class AstralBeaconItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AstralBeacon>());
        Item.value = Item.sellPrice(gold: 2);
        Item.rare = ItemRarityID.Cyan;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Items.Placeables.Astral.AstralStone>(30).
            AddIngredient<AureusCell>(5).
            AddIngredient<StarblightSoot>(20).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}
