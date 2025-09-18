using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureBasalt
{
    public class BasaltSlab : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 200;
        }

        public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureBasalt.BasaltSlab>());

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Basalt>(1).
                AddTile(TileID.WorkBenches).
                DisableDecraft().
                Register();
        }

    }
}
