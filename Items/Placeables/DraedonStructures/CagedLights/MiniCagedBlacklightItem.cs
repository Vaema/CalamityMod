using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Plates;
using CalamityMod.Tiles.DraedonStructures.CagedLights;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures.CagedLights;

public class MiniCagedBlacklightItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MiniAgedBlacklightItem>();

        Item.DefaultToPlaceableTile(ModContent.TileType<MiniCagedBlacklight>());
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe(20).
            AddIngredient<DubiousPlating>().
            AddIngredient<MysteriousCircuitry>(2).
            AddIngredient<Onyxplate>().
            AddTile(TileID.Anvils).
            Register();
    }
}
