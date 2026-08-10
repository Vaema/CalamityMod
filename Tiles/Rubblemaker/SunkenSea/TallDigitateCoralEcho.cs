using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class TallDigitateCoralEcho : TallDigitateCoral
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/TallDigitateCoral";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<LimeCoral>(), Type, 0);
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
    }
}

public class TallDigitateCoral2Echo : TallDigitateCoral2
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/TallDigitateCoral2";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<LimeCoral>(), Type, 0);
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
    }
}
public class TallDigitateCoral3Echo : TallDigitateCoral3
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/TallDigitateCoral3";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<LimeCoral>(), Type, 0);
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
    }
}
public class TallDigitateCoral4Echo : TallDigitateCoral4
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/TallDigitateCoral4";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<LimeCoral>(), Type, 0);
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
    }
}
