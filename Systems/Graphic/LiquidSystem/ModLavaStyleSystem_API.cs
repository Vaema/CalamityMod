using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Graphic.LiquidSystem;

[Autoload(Side = ModSide.Client)]
public sealed partial class ModLavaStyleSystem : ModSystem
{
    public static ModLavaStyle CurrentLavaStyle => LavaStyles[LavaStyle];

    public static void ModifyLightBlended(int i, int j, ref float r, ref float g, ref float b)
    {
        Vector3 vanillaLavaLight = new Vector3(0.55f, 0.33f, 0.11f);
        Vector3 lavaEmit = LavaStyle == 0 ? vanillaLavaLight : Vector3.Zero;
        ModifyLightSetup(i, j, LavaStyle, ref lavaEmit.X, ref lavaEmit.Y, ref lavaEmit.Z);

        for (int styleIndex = 0; styleIndex < ModLavaStyleLoader.TotalCount; styleIndex++)
        {
            if (LavaAlpha[styleIndex] > 0f && styleIndex != LavaStyle)
            {
                Vector3 propagatingColor = (styleIndex == 0) ? vanillaLavaLight : Vector3.Zero;
                ModifyLightSetup(i, j, styleIndex, ref propagatingColor.X, ref propagatingColor.Y, ref propagatingColor.Z);

                Vector3 activeColor = (LavaStyle == 0) ? vanillaLavaLight : Vector3.Zero;
                ModifyLightSetup(i, j, LavaStyle, ref activeColor.Z, ref activeColor.Y, ref activeColor.Z);

                lavaEmit = Vector3.Lerp(propagatingColor, activeColor, LavaAlpha[LavaStyle]);
            }
        }

        if (lavaEmit.X != 0.0f || lavaEmit.Y != 0.0f || lavaEmit.Z != 0.0f)
        {
            float colorManipulator = (270 - Main.mouseTextColor) / 900f;
            lavaEmit += Vector3.One * colorManipulator;
        }

        r = Math.Max(r, lavaEmit.X);
        g = Math.Max(g, lavaEmit.Y);
        b = Math.Max(b, lavaEmit.Z);
    }

    public static void ModifyColorBlended(int i, int j, ref VertexColors initialColor, bool isSlope)
    {
        VertexColors color = initialColor;
        DrawColorSetup(i, j, LavaStyle, ref color, isSlope);
        for (int styleIndex = 0; styleIndex < ModLavaStyleLoader.TotalCount; styleIndex++)
        {
            if (LavaAlpha[styleIndex] > 0f && styleIndex != LavaStyle)
            {
                VertexColors propagatingColor = initialColor;
                DrawColorSetup(i, j, styleIndex, ref propagatingColor, isSlope);

                VertexColors activeColor = initialColor;
                DrawColorSetup(i, j, LavaStyle, ref activeColor, isSlope);

                color = LerpColors(propagatingColor, activeColor, LavaAlpha[LavaStyle]);
            }
        }

        initialColor = color;

        static VertexColors LerpColors(VertexColors a, VertexColors b, float amt)
        {
            return new VertexColors()
            {
                TopLeftColor = Color.Lerp(a.TopLeftColor, b.TopLeftColor, amt),
                TopRightColor = Color.Lerp(a.TopRightColor, b.TopRightColor, amt),
                BottomLeftColor = Color.Lerp(a.BottomLeftColor, b.BottomLeftColor, amt),
                BottomRightColor = Color.Lerp(a.BottomRightColor, b.BottomRightColor, amt),
            };
        }
    }

    public static void ModifyLightSetup(int i, int j, int style, ref float r, ref float g, ref float b)
    {
        if (LavaStyles[style] is ModLavaStyle styles)
        {
            styles.ModifyLight(i, j, ref r, ref g, ref b);
        }
    }

    public static void DrawColorSetup(int x, int y, int style, ref VertexColors liquidColor, bool isSlope = false)
    {
        if (LavaStyles[style] is ModLavaStyle styles)
        {
            styles.DrawColor(x, y, ref liquidColor, isSlope);
        }
    }

    public static int GetDropletGoreID(int oldID = -1)
    {
        if (CurrentLavaStyle is ModLavaStyle lavaStyle)
        {
            return lavaStyle.GetDropletGore();
        }

        return (oldID >= 0) ? oldID : GoreID.LavaDrip;
    }

    public static int GetSplashDustID(int oldID = -1)
    {
        if (CurrentLavaStyle is ModLavaStyle lavaStyle)
        {
            return lavaStyle.GetSplashDust();
        }

        return (oldID >= 0) ? oldID : DustID.Lava;
    }

    public static void InflictDebuff(Player player, int onFireTime)
    {
        if (CurrentLavaStyle is ModLavaStyle lavaStyle)
        {
            lavaStyle.InflictDebuff(player, onFireTime);
        }
    }
}
