using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class CinderArrowProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Items/Ammo/CinderArrow";
        public bool isShrapnel = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                isShrapnel = Projectile.ai[2] == 1;
                if (isShrapnel)
                {
                    Projectile.velocity *= 0.5f;
                    Projectile.timeLeft = 300;
                }
            }
            Projectile.ai[1]++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (isShrapnel)
            {
                Projectile.arrow = false;
                Projectile.extraUpdates = 3;

                if (Main.rand.NextBool(4))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * 2, 90, -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.55f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.65f, 0.85f);
                }
            }
            else
            {
                Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.3f);
                if (Projectile.ai[1] > 4 && Main.rand.NextBool(3))
                {
                    float velMulti = Main.rand.NextFloat(0.1f, 0.75f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * 2, 90, -Projectile.velocity.RotatedBy(0.45) * velMulti);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.45f, 0.75f);
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity * 2, 90, -Projectile.velocity.RotatedBy(-0.45) * velMulti);
                    dust2.noGravity = true;
                    dust2.scale = Main.rand.NextFloat(0.45f, 0.75f);
                    dust2.noLight = true;
                    dust2.noLightEmittence = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!isShrapnel)
            {
                int Dusts = 9;
                float radians = MathHelper.TwoPi / Dusts;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int i = 0; i < Dusts; i++)
                {
                    Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f),Color.Crimson, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                SoundEngine.PlaySound(SoundID.Item69 with { Volume = 0.35f, Pitch = 1f, PitchVariance = 0.15f }, Projectile.Center);

                for (int b = 0; b < 3; b++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -10).RotatedByRandom(0.8f) * Main.rand.NextFloat(0.9f, 1.1f), ModContent.ProjectileType<CinderArrowProj>(), (int)(Projectile.damage * 0.1f), 0f, Projectile.owner, 0f, 0f, 1f);
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.damage = (int)(Projectile.damage * 0.5f);
                    Projectile.penetrate = -1;
                    Projectile.ExpandHitboxBy(110);
                    Projectile.Damage();
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isShrapnel)
            {
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (isShrapnel)
            {
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLine").Value;
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Crimson * 0.35f, 1, texture);
                return false;
            }
            else
            {
                Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/BloodfireBulletProj").Value;
                if (Projectile.ai[1] > 6)
                    CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White * 0.3f, 1, texture);
                return true;
            }
        }

        public override bool? CanDamage() => isShrapnel && Projectile.ai[1] < 20 ? false : null;
    }
}
