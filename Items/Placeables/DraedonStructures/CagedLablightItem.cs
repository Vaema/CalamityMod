using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class CagedLablightItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables.DraedonStructures";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CagedLablight>());
            Item.value = Item.sellPrice(silver: 1);
        }

        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient<RustedPlating>(10).
        //        AddIngredient<MysteriousCircuitry>().
        //        AddIngredient<DraedonPowerCell>(8).
        //        AddTile(TileID.Anvils).
        //        Register();
        //}
    }
}
