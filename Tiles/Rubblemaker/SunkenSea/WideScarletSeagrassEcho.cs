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
            RegisterItemDrop(ModContent.ItemType<PolypSand>(), Type, 0);
            // TODO: Currently none of these work
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0);
        }
    }

    public class WideScarletSeagrass2Echo : WideScarletSeagrass2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WideScarletSeagrass2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>(), Type, 0);
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0);
        }
    }
    public class WideScarletSeagrass3Echo : WideScarletSeagrass3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WideScarletSeagrass3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0);
        }
    }
    public class WideScarletSeagrass4Echo : WideScarletSeagrass4
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WideScarletSeagrass4";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0);
        }
    }
}
