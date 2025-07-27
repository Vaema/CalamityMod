using CalamityMod.Items.Placeables.FurnitureSilva;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureSilva
{
    [LegacyName("EffulgentManipulator")]
    public class SilvaBasin : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureSilva.SilvaBasin>());
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SilvaCrystal>(10).
                AddRecipeGroup("AnyGoldBar", 5).
                AddTile(TileID.GlassKiln).
                Register();
        }
    }
}
