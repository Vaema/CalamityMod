using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class SmallLimestoneCobblePileEcho : SmallLimestoneCobblePile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallLimestoneCobblePile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Limestone>(), Type);
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Limestone>(), Type, 0, 1, 2, 3, 4, 5);
        }
    }
}
