using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.DraedonStructures;
using CalamityMod.Tiles.DraedonSummoner;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.DraedonMisc;

public class CodebreakerBase : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.DraedonItems";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<CodebreakerTile>());
        Item.rare = ItemRarityID.Green;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ChargingStationItem>().
            AddIngredient<MysteriousCircuitry>(20).
            AddIngredient<DubiousPlating>(35).
            AddTile(TileID.Anvils).
            Register();
    }
}
