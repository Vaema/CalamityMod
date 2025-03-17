using System;
using CalamityMod.Buffs.DamageOverTime;
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
        public override string Texture => "CalamityMod/Projectiles/Melee/OrderbringerBeam";
        public int time = 0;
        public Color mainColor = Color.White;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
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
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (Projectile.timeLeft % 3 == 0 && targetDist < 1400f)
            {
                Particle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 3, -Projectile.velocity * 0.05f, false, 17, 1.5f, mainColor * 0.7f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Lerp(mainColor, Color.White, 0.3f) with { A = 0 }, 1, null, true, true);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 180);
            if (Projectile.numHits < 1)
            {
                Particle orb = new GlowSparkParticle(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY), false, 12, 0.07f, mainColor, new Vector2(1.5f, 0.8f), true);
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }
    }
}
