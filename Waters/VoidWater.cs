using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class VoidWater : ModWaterStyle
    {
        public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/VoidWaterflow").Slot;

        public override int GetSplashDust() => 33;

        public override int GetDropletGore() => 713;

        public override Color BiomeHairColor() => Color.Blue;
    }
}
