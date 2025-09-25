using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class FryCoralEcho : FryCoral
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/FryCoral";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<OrangeCoral>(), Type, 0);
            // TODO: Currently none of these work
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<OrangeCoral>(), Type, 0);
        }
    }

    public class FryCoral2Echo : FryCoral2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/FryCoral2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<OrangeCoral>(), Type, 0);
            //FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<OrangeCoral>(), Type, 0);
        }
    }
    public class FryCoral3Echo : FryCoral3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/FryCoral3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<OrangeCoral>(), Type, 0);
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<OrangeCoral>(), Type, 0);
        }
    }
}
