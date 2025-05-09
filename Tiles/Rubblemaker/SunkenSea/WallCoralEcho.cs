using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class WallCoral1Echo : WallCoral1
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoral1";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            // TODO: None of these work
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }

    public class WallCoral2Echo : WallCoral2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoral2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }

    public class WallCoral3Echo : WallCoral3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoral3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }

    public class WallCoral4Echo : WallCoral4
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/WallCoral4";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Shellstone>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Shellstone>(), Type, 0);
        }
    }
}
