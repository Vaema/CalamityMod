using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class StalkCoralEcho : StalkCoral
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/StalkCoral";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Limestone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Limestone>(), Type, 0);
        }
    }

    public class StalkCoral2Echo : StalkCoral2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/StalkCoral2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Limestone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Limestone>(), Type, 0);
        }
    }
    public class StalkCoral3Echo : StalkCoral3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/StalkCoral3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Limestone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Limestone>(), Type, 0);
        }
    }

}
