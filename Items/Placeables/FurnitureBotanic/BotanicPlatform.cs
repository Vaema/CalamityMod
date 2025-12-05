using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureBotanic
{
    public class BotanicPlatform : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureBotanic.BotanicPlatform>());

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<UelibloomBrick>().
                Register();
        }
    }
}
