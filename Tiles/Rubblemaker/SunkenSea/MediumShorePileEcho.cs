using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class MediumShorePileEcho : MediumShorePile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumShorePile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<Runestone>(), Type);
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Runestone>(), Type, 0);
    }
}
