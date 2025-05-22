using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class NavystonePile1Echo : NavystonePile1
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystonePile1";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }

    public class NavystonePile2Echo : NavystonePile2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystonePile2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }

    public class NavystonePile3Echo : NavystonePile3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystonePile3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }
}
