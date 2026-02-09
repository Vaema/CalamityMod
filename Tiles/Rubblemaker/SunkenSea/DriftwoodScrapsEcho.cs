using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class DriftwoodScrapsEcho : DriftwoodScraps
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodScraps";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type);
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0, 1, 2, 3, 4, 5);
        }
    }
}
