using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Astral;

public class AstralClay : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.ClayBlock, 1);
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Astral.AstralClay>());
}
