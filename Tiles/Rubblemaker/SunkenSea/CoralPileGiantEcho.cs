using Terraria.ModLoader;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class CoralPileGiantEcho : CoralPileGiant
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/CoralPileGiant";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0);
    }
}
