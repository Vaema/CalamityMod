using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Plates;
using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights
{
    public class MiniCagedCinderlightItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MiniAgedCinderlightItem>();

            Item.DefaultToPlaceableTile(ModContent.TileType<MiniCagedCinderlight>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe(20).
                AddIngredient<DubiousPlating>().
                AddIngredient<MysteriousCircuitry>(2).
                AddIngredient<Cinderplate>().
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
