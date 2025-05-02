using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class DigitateCoralEcho : DigitateCoral
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DigitateCoral";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<LimeCoral>());
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
        }
    }

    public class DigitateCoral2Echo : DigitateCoral2
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DigitateCoral2";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<LimeCoral>());
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
        }
    }

    public class DigitateCoral3Echo : DigitateCoral3
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DigitateCoral3";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<LimeCoral>());
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<LimeCoral>(), Type, 0);
        }
    }
}
