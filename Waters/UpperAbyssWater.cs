using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class UpperAbyssWaterflow : ModWaterfallStyle { }

    public class UpperAbyssWater : CalamityModWaterStyle
    {
        public static CalamityModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        public override void SetStaticDefaults()
        {
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/UpperAbyssWaterflow");
            SplashDust = ModContent.DustType<SunkenSeaSplash>();
            DropletGore = ModContent.GoreType<SunkenSeaWaterDroplet>();
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
        public override Color BiomeHairColor() => new Color(9, 69, 82);
        public override void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope) => ILEditing.ILChanges.SelectSulphuricWaterColor(x, y, ref liquidColor, isSlope);
    }
}
