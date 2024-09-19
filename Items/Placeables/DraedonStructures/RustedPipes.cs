using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class RustedPipes : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.RustedPipes>());
        }

        public override void AddRecipes()
        {
            CreateRecipe(5).
                AddRecipeGroup("IronBar").
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
