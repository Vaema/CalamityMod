using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class OrderbringerBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        private int whiteLightTimer = 5;
        public int time = 0;
        public Color mainColor = Color.White;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 74;
            Projectile.height = 74;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 6;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (time == 0)
            {
                mainColor = Main.rand.NextBool() ? Color.MediumPurple : Color.MediumOrchid;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center); //used for some drawing prevention for when it's offscreen since it makes a fuck load of particles
            if (Projectile.timeLeft % 3 == 0 && targetDist < 1400f)
            {
                Particle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 3, -Projectile.velocity * 0.05f, false, 17, 1.5f, mainColor * 0.7f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            time++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Lerp(mainColor, Color.White, 0.5f), 1, null, true, true);
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundStyle fire = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit", 2);
            SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = 0.4f }, Projectile.Center);
            if (Projectile.numHits < 1)
            {
                Particle orb = new GlowSparkParticle(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY), false, 12, 0.07f, mainColor, new Vector2(1.5f, 0.8f), true);
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }
    }
}
