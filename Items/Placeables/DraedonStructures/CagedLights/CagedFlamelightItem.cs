using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Plates;
using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights
{
    public class CagedFlamelightItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AgedFlamelightItem>();

            Item.DefaultToPlaceableTile(ModContent.TileType<CagedFlamelight>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient<DubiousPlating>(2).
                AddIngredient<MysteriousCircuitry>().
                AddIngredient<Havocplate>().
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
