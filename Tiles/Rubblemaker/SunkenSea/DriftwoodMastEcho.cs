using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class DriftwoodMastEcho : DriftwoodMast
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/DriftwoodMast";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type);
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0, 1, 2, 3, 4, 5);
        }
    }
}
