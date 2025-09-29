using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class WallCoralEcho : WallCoral
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoral";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            // TODO: None of these work
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }

    public class WallCoralLeftEcho : WallCoralLeft
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoralLeft";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }
}
