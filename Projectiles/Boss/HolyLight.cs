using System;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Particles;
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

        public override void SetDefaults()
        {
            Projectile.localAI[1] = Main.rand.NextFloat(30f);
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
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

            if (Projectile.ai[0] < 240f)
            {
                Projectile.ai[0] += 1f;

                if (Projectile.timeLeft < 160)
                    Projectile.timeLeft = 160;
            }

            if (Projectile.velocity.Length() < 16f)
                Projectile.velocity *= 1.01f;

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

            Color col = new Color(54, 209, 54);
            Particle spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.8f, false, 5, 0.06f, col * 0.85f, new Vector2(1, 0.3f), true, false, 1.5f);
            GeneralParticleHandler.SpawnParticle(spark);
        }

        public override bool PreDraw(ref Color lightColor)
        {
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

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            SoundStyle fireHeal = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
            SoundEngine.PlaySound(fireHeal with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
            Color particleColor =  new Color(54, 209, 54);
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
