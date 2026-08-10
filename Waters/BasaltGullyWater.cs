using CalamityMod.Dusts.WaterSplash;
using CalamityMod.Gores.WaterDroplet;
using CalamityMod.Systems.Graphic.LiquidSystem;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.Waters;

public class BasaltGullyWaterflow : ModWaterfallStyle, IWaterfallStyleModifyColor
{
    public void ModifyColor(in Tile tile, int x, int y, ref VertexColors liquidColor) => WaterStyleCommon.ModifyTransparentWaterColor(x, y, ref liquidColor, false);
}

public class BasaltGullyWater : ModWaterStyle, IWaterStyleModifyColor, IWaterStyleModifyLight
{
    public static int Type;

    private readonly Vector3 WaterGlowColor = new Color(144, 174, 200).ToVector3();

    public override void SetStaticDefaults() => Type = Slot;

    public void ModifyLight(in Tile tile, int i, int j, ref float r, ref float g, ref float b)
    {
        Vector3 outputColor = new Vector3(r, g, b);
        if (outputColor == Vector3.One || outputColor == new Vector3(0.25f, 0.25f, 0.25f) || outputColor == new Vector3(0.5f, 0.5f, 0.5f))
            return;

        if (tile.TileType != RustyChestTile.TileType)
        {
            WaterStyleCommon.ModifySunkenSeaWaterLight(i, j, WaterGlowColor, ref outputColor.X, ref outputColor.Y, ref outputColor.Z);
        }

        r = outputColor.X;
        g = outputColor.Y;
        b = outputColor.Z;
    }

    public void ModifyColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope) => WaterStyleCommon.ModifyTransparentWaterColor(x, y, ref liquidColor, isSlope);

    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("CalamityMod/BasaltGullyWaterflow").Slot;
    public override int GetSplashDust() => ModContent.DustType<BasaltGullySplash>();
    public override int GetDropletGore() => ModContent.GoreType<BasaltGullyWaterDroplet>();
    public override Color BiomeHairColor() => Color.WhiteSmoke;
}
