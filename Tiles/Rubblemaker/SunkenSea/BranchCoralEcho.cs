using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ambient.SunkenSea;

public class BranchCoralEcho : BranchCoral
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/BranchCoral";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<Limestone>(), Type, 0);
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Limestone>(), Type, 0);
    }
}
