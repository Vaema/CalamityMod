using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class FloralParadiseWaterflow : ModWaterfallStyle { }

    public class FloralParadiseWater : ModWaterStyle
    {
        public static int Type { get; private set; }
        public static ModWaterStyle Instance { get; private set; }

        public override void SetStaticDefaults()
        {
            Type = Slot;
            Instance = this;
        }

        public override void Unload()
        {
            Type = -1;
            Instance = null;
        }

        public override int ChooseWaterfallStyle()
        {
            return ModContent.Find<ModWaterfallStyle>("CalamityMod/FloralParadiseWaterflow").Slot;
        }

        public override int GetSplashDust() => DustID.Water;
        public override int GetDropletGore() => GoreID.WaterDripCavern;
        public override Color BiomeHairColor() => Color.PaleTurquoise;
    }
}
