using Terraria.ModLoader;
using Terraria.GameContent;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class SmallMossyStonePileEcho : SmallMossyStonePile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallMossyStonePile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<MossyStone>());
            FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<MossyStone>(), Type, 0, 1, 2, 3, 4, 5);
        }
    }
    public class LargeMossyStonePileEcho : LargeMossyStonePile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/LargeMossyStonePile";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<MossyStone>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<MossyStone>(), Type, 0, 1, 2);
        }
    }
}
