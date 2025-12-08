using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class SmallPolypForestPileEcho : SmallPolypForestPile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallPolypForestPile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>());
            FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0, 1, 2);
        }
    }
    public class PolypForestPilesEcho : MediumPolypForestPile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumPolypForestPile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>());
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        }
    }
    public class LargePolypForestPileEcho : LargePolypForestPile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/LargePolypForestPile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<PolypSand>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<PolypSand>(), Type, 0, 1, 2);
        }
    }
}
