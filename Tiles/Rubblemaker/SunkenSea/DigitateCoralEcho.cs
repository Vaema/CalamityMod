using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

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
