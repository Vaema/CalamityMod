using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems.Graphic.LiquidSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class MiddleAbyssWaterflow : ModWaterfallStyle, IWaterfallStyleModifyColor
    {
        public void ModifyColor(in Tile tile, int x, int y, ref VertexColors liquidColor) => CalamityUtils.ModifySulphuricWaterColor(x, y, ref liquidColor, false);
    }

    public class MiddleAbyssWater : ModWaterStyle, IWaterStyleModifyColor
    {
        public static ModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/MiddleAbyssWaterflow");
            SplashDust = ModContent.DustType<MiddleAbyssSplash>();
            DropletGore = ModContent.GoreType<MiddleAbyssWaterDroplet>();
        }

        public override void Unload()
        {
            Instance = null;
            WaterfallStyle = null;
            SplashDust = 0;
            DropletGore = 0;
        }

        public override int ChooseWaterfallStyle() => WaterfallStyle.Slot;
        public override int GetSplashDust() => SplashDust;
        public override int GetDropletGore() => DropletGore;
        public override Color BiomeHairColor() => new Color(36, 23, 19);

        public void ModifyColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope) => CalamityUtils.ModifySulphuricWaterColor(x, y, ref liquidColor, isSlope);
    }
}
