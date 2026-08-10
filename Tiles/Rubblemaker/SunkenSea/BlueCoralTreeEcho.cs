using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class BlueCoralTreeEcho : BlueCoralTree
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/BlueCoralTree";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<CyanCoral>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<CyanCoral>(), Type, 0);
    }
}
