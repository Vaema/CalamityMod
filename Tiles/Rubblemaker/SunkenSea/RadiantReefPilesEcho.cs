using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class SmallRadiantReefPileEcho : SmallRadiantReefPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallRadiantReefPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0, 1, 2);
    }
}
public class RadiantReefPilesEcho : MediumRadiantReefPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumRadiantReefPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0, 1, 2, 3, 4, 5, 6, 7, 8);
    }
}
public class LargeRadiantReefPileEcho : LargeRadiantReefPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/LargeRadiantReefPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0, 1, 2);
    }
}
public class GiantRadiantReefPileEcho : GiantRadiantReefPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/GiantRadiantReefPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0, 1, 2);
    }
}
