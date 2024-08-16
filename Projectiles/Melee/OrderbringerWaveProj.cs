using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using CalamityMod.Particles;
using Terraria.ModLoader;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityMod.Projectiles.Melee
{
    public class OrderbringerWaveProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Melee/JudgementProj";
        public ref float time => ref Projectile.ai[0];
        public float hitboxSize = 10;
        public Color mainColor;
        public float fade = 1;
        public float damageMult = 1;
        public float fadeOut = 1;
        public override void SetDefaults()
        {
            Projectile.width = 336;
            Projectile.height = 274;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 450;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Vector2 topCorner = Projectile.Center + (Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(100f)) * Projectile.scale) * 157;
            Vector2 bottomCorner = Projectile.Center + (Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(-100f)) * Projectile.scale) * 157;

            if (time == 0)
            {
                Projectile.scale = 0.0875f;
                Projectile.velocity *= 0.9f;
                mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (time < 200)
            {
                Projectile.scale += 0.0026f;
                hitboxSize += 0.4525f;
                Projectile.velocity *= 0.995f;

                if (time % 50 == 0 && time > 10)
                {
                    //Projectile stars = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(hitboxSize * 0.8f, hitboxSize * 0.8f) - Projectile.velocity * 2, (-Projectile.velocity * 3).RotatedByRandom(0.9f), ModContent.ProjectileType<StarofJudgement>(), (int)(Projectile.damage * 0.2f), 3f, Projectile.owner, 0f);
                    //stars.penetrate = 1;
                }
            }
            else
            {
                Projectile.velocity *= 0.975f;
            }
            if (time > 300 && fade > 0)
                fade -= 0.0065f;

            if (time < 250 && time > 20)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(hitboxSize, hitboxSize) - Projectile.velocity * 2, 66);
                    trailDust.scale = Main.rand.NextFloat(0.7f, 0.85f) - (time < 150 ? 0 : time * 0.001f);
                    trailDust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.85f, 1.5f);
                    trailDust.color = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
                    trailDust.noGravity = true;
                }

                if (time % 9 == 0)
                {
                    Particle orb = new GlowSparkParticle(topCorner, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(170f)), false, 15, Main.rand.NextFloat(0.03f, 0.055f) * Utils.GetLerpValue(250, 150, time, true), mainColor, new Vector2(1, 0.8f), true, false, 0.3f);
                    GeneralParticleHandler.SpawnParticle(orb);
                    Particle orb2 = new GlowSparkParticle(bottomCorner, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(-170f)), false, 15, Main.rand.NextFloat(0.03f, 0.055f) * Utils.GetLerpValue(250, 150, time, true), mainColor, new Vector2(1, 0.8f), true, false, 0.3f);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }
                if (Main.rand.NextBool(12))
                {
                    Particle orb3 = new SparkParticle(topCorner, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(170f)) * Main.rand.NextFloat(5f, 20f), false, 15, Main.rand.NextFloat(0.8f, 1.35f), mainColor);
                    GeneralParticleHandler.SpawnParticle(orb3);
                    Particle orb4 = new SparkParticle(bottomCorner, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(-170f)) * Main.rand.NextFloat(5f, 20f), false, 15, Main.rand.NextFloat(0.8f, 1.35f), mainColor);
                    GeneralParticleHandler.SpawnParticle(orb4);
                }
            }

            fadeOut = Utils.GetLerpValue(0, 180, Projectile.timeLeft, true);

            time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            float waveFade = Utils.GetLerpValue(0, 300, Projectile.timeLeft);
            for (int i = 1; i < 6; i++) // Weird for loop because of squash code
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, mainColor with { A = 0 } * fadeOut, Projectile.rotation, tex.Size() / 2f, new Vector2(1 - (i * 0.2f * waveFade), 1 + (i * 0.35f  * waveFade)) * Projectile.scale * 1.1f, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 300);
            if (damageMult < 1.3f && damageDone > 2)
                damageMult += 0.325f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Deal more damage on pierce
            modifiers.SourceDamage *= damageMult;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, hitboxSize, targetHitbox);
    }
}
