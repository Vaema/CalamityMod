using System;
using System.Linq;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;

namespace CalamityMod.Waters;

internal static class WaterStyleCommon
{
    public static void ModifySulphuricWaterColor(int x, int y, ref VertexColors initialColor, bool isSlope)
    {
        if (SulphuricWaterSafeZoneSystem.NearbySafeTiles.Count >= 1)
        {
            Color cleanWaterColor = new(10, 62, 193);
            var closestSafeZone = SulphuricWaterSafeZoneSystem.NearbySafeTiles.OrderBy(t => t.Key.ToVector2().DistanceSQ(new(x, y))).First();
            Point closestSafeZonePoint = closestSafeZone.Key;
            float closestSafeZoneAmt = closestSafeZone.Value;
            float lerpAmt = (1f - closestSafeZoneAmt) * 21f;

            void ModifyColor(Vector2 point, ref Color vertexColor)
            {
                float distanceToClosest = point.Distance(closestSafeZonePoint.ToVector2());
                float acidicWaterInterpolant = Utils.GetLerpValue(12f, 20.5f, distanceToClosest + lerpAmt, true);
                vertexColor = Color.Lerp(cleanWaterColor, vertexColor, acidicWaterInterpolant);
            }

            ModifyColor(new(x - 0.5f, y - 0.5f), ref initialColor.TopLeftColor);
            ModifyColor(new(x + 0.5f, y - 0.5f), ref initialColor.TopRightColor);
            ModifyColor(new(x - 0.5f, y + 0.5f), ref initialColor.BottomLeftColor);
            ModifyColor(new(x + 0.5f, y + 0.5f), ref initialColor.BottomRightColor);
        }

        ModifyTransparentWaterColor(x, y, ref initialColor, isSlope);
    }

    public static void ModifyTransparentWaterColor(int x, int y, ref VertexColors initialColor, bool isSlope)
    {
        if (isSlope)
        {
            initialColor.TopLeftColor *= 1f / 3;
            initialColor.TopRightColor *= 1f / 3;
            initialColor.BottomLeftColor *= 1f / 3;
            initialColor.BottomRightColor *= 1f / 3;
        }
        else
        {
            initialColor.TopLeftColor *= 0.4f;
            initialColor.TopRightColor *= 0.4f;
            initialColor.BottomLeftColor *= 0.4f;
            initialColor.BottomRightColor *= 0.4f;
        }
    }

    public static void ModifySunkenSeaWaterLight(int x, int y, Vector3 waterColor, ref float r, ref float g, ref float b)
    {
        float tick = (float)Main.timeForVisualEffects;
        float brightness = MathHelper.Clamp(0.07f, 0f, 0.07f);
        float waveScale1 = tick * 0.028f;
        float waveScale2 = tick * 0.1f;
        int yScale = -y / 2;
        int xScale = x / 15;
        float wave1 = tick * 0.024f * -50 + ((-x / 30) + (y / 30)) * 25;
        float wave2 = waveScale2 * -10 + ((-xScale) + yScale) * 45;
        float wave3 = waveScale1 * -100 + ((x / 7) + (y / 50)) * 25;
        float wave4 = tick * 0.15f * 10 + ((x / 3) + yScale) * 45;
        float wave5 = waveScale1 * -70 + ((-x / 25) + (-y / 25)) * 20;
        float wave6 = waveScale2 * -10 + (xScale + yScale) * 45;
        float bigwave = tick * 0.01f * -70 + ((-x / 2) + (-y / 40)) * 5;
        float wave1angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave1));
        float wave2angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave2));
        float wave3angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave3));
        float wave4angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave4));
        float wave5angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave5));
        float wave6angle = 0.55f + 0.45f * MathF.Sin(MathHelper.ToRadians(wave6));
        float bigwaveangle = 0.55f + 0.80f * MathF.Sin(MathHelper.ToRadians(bigwave));
        float sumofwave = 0.07f + wave1angle + wave2angle + wave3angle + wave4angle + wave5angle + wave6angle + bigwaveangle;

        r = MathHelper.Lerp(r, waterColor.X, sumofwave) * brightness;
        g = MathHelper.Lerp(g, waterColor.Y, sumofwave) * brightness;
        b = MathHelper.Lerp(b, waterColor.Z, sumofwave) * brightness;
    }
}
