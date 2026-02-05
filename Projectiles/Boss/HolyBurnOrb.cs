using System;
using CalamityMod.Dusts;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyBurnOrb : ModProjectile, ILocalizedModType
    {
        bool started = false;
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetDefaults()
        {
            Projectile.localAI[1] = Main.rand.NextFloat(30f);
            Projectile.width = Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void AI()
        {
            ProvUtils.ApplyGFBDamage(Projectile, 120, 50);

            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0f);

            if (!started)
            {
                Color cl = ProvUtils.GetProjectileColor(255);
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, cl, "CalamityMod/Particles/BlastCone", new Vector2(Main.rand.NextFloat(4f, 7f), 1.5f), Vector2.Zero.AngleTo(Projectile.velocity), 1f, 0f, 30));
                started = true;
            }

            if (Projectile.ai[0] < 240f)
            {
                Projectile.ai[0] += 1f;

                if (Projectile.timeLeft < 160)
                    Projectile.timeLeft = 160;
            }

            if (Main.getGoodWorld)
            {
                if (Projectile.velocity.Length() < 12f && Projectile.ai[1] == 0f)
                {
                    Projectile.velocity *= 1.02f;
                }
                else
                {
                    Projectile.ai[1] += 0.05f;
                    Projectile.velocity *= MathHelper.Lerp(0.95f, 1.05f, (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                }
            }
            else if (Projectile.velocity.Length() < 16f)
                Projectile.velocity *= 1.01f;

            Projectile.localAI[1] += (Projectile.velocity.Length() / 20);

            Color col = ProvUtils.GetProjectileColor(255);
            float vel = MathHelper.Clamp(Projectile.velocity.Length() / 5, 0, 1.5f);
            GlowOrbParticle p = new GlowOrbParticle(Projectile.Center, Projectile.velocity + new Vector2(Main.rand.NextFloat(vel * 2), 0).RotatedByRandom(MathHelper.TwoPi), false, 4, 1f, col);
            GeneralParticleHandler.SpawnParticle(p);
            //Trail for visibilty
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center + Projectile.velocity, Vector2.Zero, col, "CalamityMod/Particles/BlastCone", new Vector2(3f, 2f), Vector2.Zero.AngleFrom(Projectile.velocity), 1f, 0f, 3));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float lerpMult = MathHelper.Lerp(0.5f, 1.5f, Math.Abs(MathF.Sin(Projectile.localAI[1] / 10f)));
            
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color baseColor = ProvUtils.GetProjectileColor(255, true) * 4;
            Color baseColor2 = ProvUtils.GetProjectileColor(255);
            baseColor.A = 0;
            baseColor *= lerpMult;
            baseColor2 *= lerpMult;
            Vector2 origin = texture.Size() / 2f;
            Vector2 scale = new Vector2(0.5f, 1f) * ((lerpMult-1)*0.5f + 1f);

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Projectile.rotation += MathHelper.ToRadians(lerpMult * 2f);

            float upRight = MathHelper.PiOver4;
            float up = MathHelper.PiOver2;
            float upLeft = 3f * MathHelper.PiOver4;
            float left = MathHelper.Pi;
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upLeft + Projectile.rotation, origin, scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upRight - Projectile.rotation, origin, scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upLeft + Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upRight - Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, up + Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, left - Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, up + Projectile.rotation, origin, scale * 0.36f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, left - Projectile.rotation, origin, scale * 0.36f, spriteEffects, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Color particleColor = ProvUtils.GetProjectileColor(0);
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

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) => Projectile.Kill();

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If the player is dodging, don't apply debuffs
            if (info.Damage <= 0 || target.creativeGodMode)
                return;

            ProvUtils.ApplyDebuffs(target, 120);
        }
    }
}
