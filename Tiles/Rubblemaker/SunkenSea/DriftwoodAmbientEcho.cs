using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class DriftwoodAmbient1Echo : DriftwoodAmbient1
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient1";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            // TODO: Most of these currently don't work
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }

    public class DriftwoodAmbient2Echo : DriftwoodAmbient2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
    public class DriftwoodAmbient3Echo : DriftwoodAmbient3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
    public class DriftwoodAmbient4Echo : DriftwoodAmbient4
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient4";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
    public class DriftwoodAmbient5Echo : DriftwoodAmbient5
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient5";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
    public class DriftwoodAmbient6Echo : DriftwoodAmbient6
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodAmbient6";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
}
