using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class SmallGleamingBurrowsPileEcho : SmallGleamingBurrowsPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallGleamingBurrowsPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<HardenedEutrophicSand>());
        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<HardenedEutrophicSand>(), Type, 0, 1, 2);
    }
}
public class MediumGleamingBurrowsPileEcho : MediumGleamingBurrowsPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumGleamingBurrowsPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<HardenedEutrophicSand>());
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<HardenedEutrophicSand>(), Type, 0, 1, 2, 3, 4, 5);
    }
}
public class LargeGleamingBurrowsPileEcho : LargeGleamingBurrowsPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/LargeGleamingBurrowsPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<HardenedEutrophicSand>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<HardenedEutrophicSand>(), Type, 0, 1, 2, 3, 4, 5);
    }
}
