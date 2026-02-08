using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts.WaterSplash
{
    public abstract class LavaSplashDust : ModDust
    {
        public abstract Vector3 LightColor { get; }

        public override void OnSpawn(Dust dust)
        {
            dust.velocity *= 0.1f;
            dust.velocity.Y = -0.5f;
        }

        public override bool Update(Dust dust)
        {
            if (dust.scale > 10f)
            {
                dust.active = false;
            }
            Dust.lavaBubbles++;
            dust.position += dust.velocity;
            if (!dust.noGravity)
            {
                dust.velocity.Y += 0.1f;
            }
            if (dust.noGravity)
            {
                dust.scale += 0.03f;
                if (dust.scale < 1f)
                {
                    dust.velocity.Y += 0.075f;
                }
                dust.velocity.X *= 1.08f;
                dust.rotation += (dust.velocity.X > 0f) ? 0.01f : -0.01f;

                float intensity = Math.Min(dust.scale * 0.6f, 1.0f);

                int tileX = (int)(dust.position.X / 16f);
                int tileY = (int)(dust.position.Y / 16f + 1f);
                Lighting.AddLight(tileX, tileY, intensity * LightColor.X, intensity * LightColor.Y, intensity * LightColor.Z);
            }
            else
            {
                if (!Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y - 8f), 4, 4))
                {
                    dust.scale = 0f;
                }
                else
                {
                    dust.alpha += Main.rand.Next(2);
                    if (dust.alpha > 255)
                    {
                        dust.scale = 0f;
                    }
                    dust.velocity.Y = -0.5f;
                    dust.alpha++;
                    dust.scale -= 0.01f;
                    dust.velocity.Y = -0.2f;
                    dust.velocity.X += Main.rand.Next(-10, 10) * 0.002f;
                    if (dust.velocity.X < -0.25f)
                    {
                        dust.velocity.X = -0.25f;
                    }
                    if (dust.velocity.X > 0.25f)
                    {
                        dust.velocity.X = 0.25f;
                    }
                }

                float intensity = dust.scale * 0.3f + 0.4f;
                if (intensity > 1f)
                {
                    intensity = 1f;
                }

                int tileX = (int)(dust.position.X / 16f);
                int tileY = (int)(dust.position.Y / 16f);
                Lighting.AddLight(tileX, tileY, intensity * LightColor.X, intensity * LightColor.Y, intensity * LightColor.Z);
            }
            dust.rotation += dust.velocity.X * 0.5f;
            if (dust.fadeIn > 0f && dust.fadeIn < 100f)
            {
                dust.scale += 0.03f;
                if (dust.scale > dust.fadeIn)
                {
                    dust.fadeIn = 0f;
                }
            }
            dust.scale -= 0.01f;
            if (dust.noGravity)
            {
                dust.velocity *= 0.92f;
                if (dust.fadeIn == 0f)
                {
                    dust.scale -= 0.04f;
                }
            }
            if (dust.position.Y > Main.screenPosition.Y + Main.screenHeight)
            {
                dust.active = false;
            }

            dust.scale -= Dust.dCount switch
            {
                0.5f => 0.001f,
                0.6f => 0.0025f,
                0.7f => 0.005f,
                0.8f => 0.01f,
                0.9f => 0.02f,
                _ => 0.0f,
            };

            float despawnScaleThreshold = Dust.dCount switch
            {
                0.5f => 0.11f,
                0.6f => 0.13f,
                0.7f => 0.16f,
                0.8f => 0.22f,
                0.9f => 0.25f,
                _ => 0.1f
            };

            if (dust.scale < despawnScaleThreshold)
            {
                dust.active = false;
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            float colorIntensity = (255 - dust.alpha) / 255f;
            colorIntensity = (colorIntensity + 3f) / 4f;

            int R = (int)(lightColor.R * colorIntensity);
            int G = (int)(lightColor.G * colorIntensity);
            int B = (int)(lightColor.B * colorIntensity);
            int alpha = lightColor.A - dust.alpha;
            return new Color(R, G, B, alpha);
        }
    }
}
