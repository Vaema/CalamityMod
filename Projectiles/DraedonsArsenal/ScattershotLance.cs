using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class ScattershotLance : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public Vector2 startVel = Vector2.Zero;
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 35;
            Projectile.height = 35;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 11;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time == 0)
            {
                startVel = Projectile.velocity;
                for (int i = 0; i <= 9; i++)
                {
                    Particle spark = new SparkParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.2f, 1f), false, 55, Main.rand.NextFloat(0.2f, 1.8f), Color.Red);
                    GeneralParticleHandler.SpawnParticle(spark);

                    SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.2f, 1f), false, 25, Main.rand.NextFloat(0.1f, 0.8f), Color.White);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int i = 0; i <= 15; i++)
                {
                    Dust chargefull = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), 278);
                    chargefull.velocity = Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.2f, 2f);
                    chargefull.scale = Main.rand.NextFloat(0.4f, 0.8f);
                    chargefull.noGravity = true;
                    chargefull.color = Color.Red;
                }
            }
            Projectile.rotation = startVel.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity = Vector2.Zero;
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits == 0)
            {
                SoundStyle sound = new("CalamityMod/Sounds/Item/MeldShoot");
                SoundEngine.PlaySound(sound with { Volume = 1f }, Projectile.Center);
                //Particle spark = new GlowSparkParticle(target.Center, startVel.SafeNormalize(Vector2.UnitY) * 20, false, 9, 0.1f, Color.Red, new Vector2(1f, 1.1f), true, true, 0.8f);
                //GeneralParticleHandler.SpawnParticle(spark);
            }
            for (int i = 0; i < MathHelper.Clamp(25 - Projectile.numHits * 4, 1, 10); i++)
            {
                Vector2 velocity = startVel.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 1.4f);
                Particle nanoDust = new NanoParticle(target.Center, velocity, Main.rand.NextBool(3) ? Color.Crimson : Color.Red, Main.rand.NextFloat(0.4f, 1f), 35, Main.rand.NextBool(), true);
                GeneralParticleHandler.SpawnParticle(nanoDust);
            }
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.88f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (startVel == Vector2.Zero)
                return false;
            // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host crystal), that's good enough.
            if (projHitbox.Intersects(targetHitbox))
                return true;
            // Otherwise, perform an AABB line collision check to check the whole beam.
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + startVel * 12, Projectile.width * Projectile.scale, ref _);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 1)
                return false;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowSpark").Value;
            float fade = Utils.GetLerpValue(0, 5, Projectile.timeLeft, true);
            for (int i = 0; i < 5; i++)
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition + (startVel * (2 + (i * 2)) * fade), null, Color.Red with { A = 0 } * fade, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.4f, 0.9f) * (0.12f - (i * 0.0165f)), SpriteEffects.None);
            for (int i = 0; i < 5; i++)
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition + (startVel * (2 + (i * 2)) * fade), null, Color.White with { A = 0 } * 0.8f * fade, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.4f, 0.9f) * (0.12f - (i * 0.0165f)) * 0.7f, SpriteEffects.None);
            return false;
        }
    }
}
