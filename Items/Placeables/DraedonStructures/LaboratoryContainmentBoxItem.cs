using CalamityMod.Items.Materials;
using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class LaboratoryContainmentBoxItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LaboratoryContainmentBox>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LaboratoryPlating>(10).
                AddIngredient<DubiousPlating>().
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
