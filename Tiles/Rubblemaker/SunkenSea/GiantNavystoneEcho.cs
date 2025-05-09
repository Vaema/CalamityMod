using Terraria.ModLoader;
using CalamityMod.Tiles.SunkenSea.Ambient;
using CalamityMod.Items.Placeables.SunkenSea;
using Terraria.GameContent;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class GiantNavystone1Echo : GiantNavystone1
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/GiantNavystone1";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }

    public class GiantNavystone2Echo : GiantNavystone2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/GiantNavystone2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Navystone>(), Type, 0);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Navystone>(), Type, 0);
        }
    }
}
