using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Crags;

public class SpineSapling : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crags.Tree.SpineSapling>());
        Item.value = Item.sellPrice(copper: 50);
    }
}
