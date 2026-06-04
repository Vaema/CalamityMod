using System;
using System.Linq;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/StarProj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 32;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.localAI[1] = Main.rand.NextFloat(30f);
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.MaxUpdates = 4;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[2] == 0)
            {
                SoundStyle soundStyle = SoundID.DD2_WitherBeastCrystalImpact;
                soundStyle.MaxInstances = 10;
                SoundEngine.PlaySound(soundStyle, Projectile.Center);

                Color col = new Color(54, 209, 54);
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, col, "CalamityMod/Particles/BlastCone", new Vector2(Main.rand.NextFloat(4f, 7f), 1.5f), Vector2.Zero.AngleTo(Projectile.velocity), 1f, 0f, 30));
            }
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.6f, 0f);
            if (Projectile.FinalExtraUpdate())
            {
                if (Projectile.ai[0] < 240f)
                {
                    Projectile.ai[0] += 1f;

                    if (Projectile.timeLeft < 160)
                        Projectile.timeLeft = 160;
                }

                if (Projectile.velocity.Length() < 16f)
                    Projectile.velocity *= 1.01f;
            }

            Projectile.position -= (Projectile.velocity / Projectile.MaxUpdates) * (Projectile.MaxUpdates - 1);

            int index = Player.FindClosest(Projectile.position, Projectile.width, Projectile.height);
            Player player = Main.player[index];
            if (player is null)
                return;

            float playerDist = Vector2.Distance(player.Center, Projectile.Center);
            if (!player.immune && playerDist < 50f && !player.dead && Projectile.position.X < player.position.X + player.width && Projectile.position.X + Projectile.width > player.position.X && Projectile.position.Y < player.position.Y + player.height && Projectile.position.Y + Projectile.height > player.position.Y)
            {
                int healAmt = (int)Projectile.ai[1];
                player.HealPlayer(healAmt, HealTextType.Local);
                NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, index, healAmt);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End(out var ss);
            var device = Main.instance.GraphicsDevice;

            using (ProvUtils.SecondaryLease.Scope(clearColor: Color.Transparent))
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                var tex = GeneralParticleHandler.GetTexture(GeneralParticleHandler.particleIDsByTypes[typeof(BloomParticle)]);
                var len = Math.Min(Projectile.oldPos.Length, 32);
                for (var i = 0; i < len; i++)
                {
                    Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    Color color = FireCoreColorFunction(i / (float)len, Projectile.oldPos[i]);
                    var color2 = new Color(0, 25, 0);
                    float width = FireWidthFunction(i / (float)len, Projectile.oldPos[i]);
                    Main.spriteBatch.Draw(tex, drawPos, null, (color2 with { A = 0 }) * (1 - i / (float)len), 0f, tex.Size() * 0.5f, new Vector2(width) / tex.Width, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(tex, drawPos, null, (color with { A = 0 }) * (1 - i / (float)len), 0f, tex.Size() * 0.5f, new Vector2(width) / tex.Width * 0.25f, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.End();
            }

            using (ProvUtils.PrimaryLease.Scope(clearColor: Color.Transparent))
            {

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                Main.spriteBatch.Draw(ProvUtils.SecondaryLease.Target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);


                float vel = Projectile.velocity.Length() / 8;
                Projectile.localAI[1] += vel;

                Texture2D drawTexture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
                Color brightGreen = new Color(54, 209, 54, 0);
                Vector2 projDirection = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Vector2 halfTextureSize = drawTexture.Size() / 2f;
                Color halfBrightGreen = brightGreen * 0.5f;
                float timeLeftColorScale = MathHelper.Lerp(0.5f, 1.5f, Math.Abs(MathF.Sin(Projectile.localAI[1] / 10f)));
                Projectile.rotation += MathHelper.ToRadians(timeLeftColorScale * 2f);
                Vector2 timeLeftDrawEffect = new Vector2(0.5f, 1f) * timeLeftColorScale;
                Vector2 timeLeftDrawEffect2 = new Vector2(0.5f, 1f) * timeLeftColorScale;
                brightGreen *= timeLeftColorScale;
                halfBrightGreen *= timeLeftColorScale;

                Vector2 position3 = projDirection + Projectile.velocity.SafeNormalize(Vector2.Zero) * Utils.GetLerpValue(0.5f, 1f, Projectile.localAI[0] / 60f, clamped: true) * 0;

                SpriteEffects spriteEffects = SpriteEffects.None;
                if (Projectile.spriteDirection == -1)
                    spriteEffects = SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(drawTexture, position3, null, brightGreen, MathHelper.PiOver2 - Projectile.rotation, halfTextureSize, timeLeftDrawEffect, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, brightGreen, 0f - Projectile.rotation, halfTextureSize, timeLeftDrawEffect2, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, halfBrightGreen, MathHelper.PiOver2 - Projectile.rotation, halfTextureSize, timeLeftDrawEffect * 0.6f, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, halfBrightGreen, 0f - Projectile.rotation, halfTextureSize, timeLeftDrawEffect2 * 0.6f, spriteEffects, 0);

                Main.EntitySpriteDraw(drawTexture, position3, null, brightGreen, MathHelper.PiOver4 + Projectile.rotation, halfTextureSize, timeLeftDrawEffect * 0.6f, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, brightGreen, MathHelper.PiOver4 * 3f + Projectile.rotation, halfTextureSize, timeLeftDrawEffect2 * 0.6f, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, halfBrightGreen, MathHelper.PiOver4 + Projectile.rotation, halfTextureSize, timeLeftDrawEffect * 0.36f, spriteEffects, 0);
                Main.EntitySpriteDraw(drawTexture, position3, null, halfBrightGreen, MathHelper.PiOver4 * 3f + Projectile.rotation, halfTextureSize, timeLeftDrawEffect2 * 0.36f, spriteEffects, 0);
                var scale = new Vector2(1f, 1f);
                var texture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowOrbParticle").Value;
                using (Main.spriteBatch.Scope())
                {

                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
                    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(54, 209, 54), 0, texture.Size() * 0.5f, 1f, 0, 0f);
                    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, 0, texture.Size() * 0.5f, 0.5f, 0, 0f);
                    Main.spriteBatch.End();
                }
                Main.spriteBatch.End();
            }

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.spriteBatch.Draw(ProvUtils.PrimaryLease.Target, Vector2.Zero, null, Color.White * Projectile.Opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(ss);
            return false;
        }
        public float FireWidthFunction(float completion, Vector2 pos)
        {
            float width;
            float maxBodyWidth = 32f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            // Crop the tip of the trail into a conic shape.
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            // Pulse inwards and outwards over time.
            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion, Vector2 pos)
        {
            Color mainColor = new Color(54, 209, 54) * 1.3f;
            Color endColor = Color.Lerp(mainColor, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(mainColor, endColor, completion) * Projectile.Opacity;
        }

        public float FireCoreWidthFunction(float completion, Vector2 pos)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 24;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion, Vector2 pos)
        {
            Color mainColor = new Color(54, 209, 54);
            Color tipColor = Color.Lerp(mainColor, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            Color fullBodyColor = Color.Lerp(mainColor, tipColor, completion);
            return Color.Lerp(fullBodyColor, Color.White, 0.175f) * Projectile.Opacity;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            SoundStyle fireHeal = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
            SoundEngine.PlaySound(fireHeal with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
            Color particleColor = new Color(54, 209, 54);
            Color smokeColor = Color.Lerp(particleColor, Color.DarkSlateGray, 0.5f);
            Particle pulse = new CustomPulse(Projectile.Center, Vector2.Zero, smokeColor, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.06f, 18);
            GeneralParticleHandler.SpawnParticle(pulse);
            for (int i = 0; i < 7; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(7f), smokeColor, 30, Main.rand.NextFloat(0.6f, 1f), 0.5f, Main.rand.NextFloat(-0.03f, 0.03f), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), (Vector2.UnitX).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(1.8f, 10f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
                dust.color = particleColor;
                dust.noLightEmittence = true;
            }
        }
    }
}
