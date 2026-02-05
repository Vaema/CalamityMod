using CalamityMod.Items.DraedonMisc;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights
{
    public class MiniCagedLablightItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MiniAgedLablightItem>();

            Item.DefaultToPlaceableTile(ModContent.TileType<MiniCagedLablight>());
            Item.value = Item.sellPrice(silver: 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe(20).
                AddIngredient<DubiousPlating>().
                AddIngredient<MysteriousCircuitry>(2).
                AddIngredient<DraedonPowerCell>().
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
