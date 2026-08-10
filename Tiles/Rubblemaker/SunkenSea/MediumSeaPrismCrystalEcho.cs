using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ambient.SunkenSea;

public class MediumSeaPrismCrystalEcho : MediumSeaPrismCrystal
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/MediumSeaPrismCrystal";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<PrismShard>(), Type, 0);
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<PrismShard>(), Type, 0);
    }

    // Don't drop items
    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
    }
}
