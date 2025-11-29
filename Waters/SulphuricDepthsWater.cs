using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems.Graphic.LiquidSystem;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.Waters
{
    public class SulphuricDepthsWaterflow : ModWaterfallStyle, IPaintableWaterfallStyle
    {
        public void ModifyDrawColor(in Tile tile, int x, int y, ref VertexColors liquidColor) => CalamityUtils.SulphuricWaterColor(x, y, ref liquidColor, false);
    }

    public class SulphuricDepthsWater : ModWaterStyle, IPaintableWaterStyle, IEmittableWaterStyle
    {
        public static int Type { get; private set; }
        public static ModWaterStyle Instance { get; private set; }
        public static ModWaterfallStyle WaterfallStyle { get; private set; }
        public static int SplashDust { get; private set; }
        public static int DropletGore { get; private set; }

        public override void SetStaticDefaults()
        {
            Type = Slot;
            Instance = this;
            WaterfallStyle = ModContent.Find<ModWaterfallStyle>("CalamityMod/SulphuricDepthsWaterflow");
            SplashDust = ModContent.DustType<SulphuricDepthsSplash>();
            DropletGore = ModContent.GoreType<SulphuricDepthsWaterDroplet>();
        }

        public override void Unload()
        {
            Type = -1;
            Instance = null;
            WaterfallStyle = null;
            SplashDust = 0;
            DropletGore = 0;
        }

        public override int ChooseWaterfallStyle() => WaterfallStyle.Slot;
        public override int GetSplashDust() => SplashDust;
        public override int GetDropletGore() => DropletGore;
        public override Color BiomeHairColor() => new Color(35, 117, 89);
        public void ModifyDrawColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope) => CalamityUtils.SulphuricWaterColor(x, y, ref liquidColor, isSlope);
        public void ModifyLight(in Tile tile, int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 outputColor = new Vector3(r, g, b);

            if (tile.TileType != RustyChestTile.TileType)
            {
                outputColor = Vector3.Lerp(outputColor, Color.MediumSeaGreen.ToVector3(), 0.18f);
            }
            r = outputColor.X;
            g = outputColor.Y;
            b = outputColor.Z;
        }
    }
}
