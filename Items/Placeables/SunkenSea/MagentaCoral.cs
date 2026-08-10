using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea;

public class MagentaCoral : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<OrangeCoral>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.MagentaCoral>());
        Item.rare = ItemRarityID.Blue;
    }
}
