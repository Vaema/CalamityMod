using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class PissWater : ModWaterStyle
    {
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/PissWaterflow").Slot;

        public override int GetSplashDust() => 102;

        public override int GetDropletGore() => 711;

        public override Color BiomeHairColor() => Color.Yellow;
    }
}
