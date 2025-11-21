using CalamityMod.Items.Placeables.FurnitureDriftwood;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class MediumDriftwoodWreckageEcho : MediumDriftwoodWreckage
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumDriftwoodWreckage";
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            RegisterItemDrop(ModContent.ItemType<Driftwood>(), Type);
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Driftwood>(), Type, 0, 1);
        }
    }
}
