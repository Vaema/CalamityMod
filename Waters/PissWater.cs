using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class PissWaterflow : ModWaterfallStyle { }

    public class PissWater : ModWaterStyle
    {
        public static ModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/PissWaterflow");
            SplashDust = DustID.Water_Desert;
            DropletGore = GoreID.WaterDripDesert;
        }

        public override void Unload()
        {
            Instance = null;
            WaterfallStyle = null;
            SplashDust = 0;
            DropletGore = 0;
        }

        public override int ChooseWaterfallStyle() => WaterfallStyle.Slot;
        public override int GetSplashDust() => DustID.Water_Desert;
        public override int GetDropletGore() => GoreID.WaterDripDesert;
        public override Color BiomeHairColor() => Color.Yellow;
    }
}
