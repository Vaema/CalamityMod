using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class WideScarletSeagrassEcho : WideScarletSeagrass
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WideScarletSeagrass";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>(), Type, 0, 1, 2, 3);
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0, 1, 2, 3);
        }
    }
}
