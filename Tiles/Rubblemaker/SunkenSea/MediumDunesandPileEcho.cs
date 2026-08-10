using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class MediumDunesandPileEcho : MediumDunesandPile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumDunesandPile";
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        RegisterItemDrop(ModContent.ItemType<Dunesand>(), Type);
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Dunesand>(), Type, 0);
    }
}
