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
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type, 0);
            // TODO: Most of these currently don't work
            //FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0);
        }
    }
}
