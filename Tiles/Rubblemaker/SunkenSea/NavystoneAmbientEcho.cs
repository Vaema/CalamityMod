using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class NavystoneAmbientEcho : NavystoneAmbient
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystoneAmbient";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            // TODO: Currently none of these work
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }

    public class NavystoneAmbient2Echo : NavystoneAmbient2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystoneAmbient2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }
    public class NavystoneAmbient3Echo : NavystoneAmbient3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystoneAmbient3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }
}
