using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSacrilegious;

public class RitualCandle : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<RitualCandleTile>());
        Item.value = Item.sellPrice(copper: 60);
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed)
        {
            Item.placeStyle = 1;
        }
        else
        {
            Item.placeStyle = 0;
        }
        return base.CanUseItem(player);
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OccultBrickItem>(5).
            AddIngredient(ItemID.Torch).
            AddTile<SCalAltar>().
            Register();
    }
}
