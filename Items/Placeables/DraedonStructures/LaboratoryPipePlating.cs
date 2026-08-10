using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures;

public class LaboratoryPipePlating : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.LaboratoryPipePlating>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(2).
            AddIngredient<LaboratoryPlating>().
            AddIngredient<RustedPipes>().
            AddTile(TileID.Anvils).
            Register();
    }
}
