using CalamityMod.Systems;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class FloralParadiseWaterflow : ModWaterfallStyle { }

    public class FloralParadiseWater : CalamityModWaterStyle
    {
        public override int ChooseWaterfallStyle()
        {
            return ModContent.Find<ModWaterfallStyle>("CalamityMod/FloralParadiseWaterflow").Slot;
        }

        public override int GetSplashDust() => DustID.Water;
        public override int GetDropletGore() => GoreID.WaterDripCavern;
        public override Color BiomeHairColor() => Color.PaleTurquoise;
    }
}
