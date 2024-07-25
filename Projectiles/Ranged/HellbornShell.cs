using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;

namespace CalamityMod.Projectiles.Ranged
{
    public class HellbornShell : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/HellbornShell";
        public int Time = 0;
        public bool TouchedGrass = false;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void SetStaticDefaults()
        {
            // Unsure if it will look good with these
            //ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            //ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1, ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/DragonsBreathMag").Value);
            return true;
        }
        public override void AI()
        {
            Projectile.scale = 0.8f;
            Projectile.extraUpdates = 1;
            Time++;
            Player Owner = Main.player[Projectile.owner];
            if (!TouchedGrass)
            {
                if (Projectile.timeLeft <= 240)
                    Projectile.rotation += 0.04f * Projectile.direction;
                else
                    Projectile.rotation = (new Vector2(0, -5 * Projectile.direction) + Projectile.velocity * -1.2f).ToRotation();

                Projectile.velocity.Y += 0.087f;
                Projectile.velocity.X *= 0.99f;

                if (Main.rand.NextBool(4))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity, 303, -Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.2f, 1f), 180, default, Main.rand.NextFloat(0.4f, 1.3f));
                    dust.noGravity = false;
                    dust.color = Color.White;
                }
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }
            Projectile.alpha = (int)(255 * Utils.GetLerpValue(60, 0, Projectile.timeLeft, true));
            if (Collision.SolidCollision(Projectile.Center - Projectile.velocity * 1.5f, 2, 2))
                    Projectile.tileCollide = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.damage = 0;
            TouchedGrass = true;
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            /*
            if (Main.zenithWorld)
            {
                for (int i = 0; i <= 30; i++)
                {
                    SparkParticle spark1 = new SparkParticle(Projectile.Center, new Vector2(4, 4).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 2.8f), false, 45, 1.4f, Main.rand.NextBool(4) ? Color.Orange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark1);
                }
                Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.DarkOrange, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 2.8f, 33);
                GeneralParticleHandler.SpawnParticle(explosion);
                Particle explosion2 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.OrangeRed, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 3f, 33, false);
                GeneralParticleHandler.SpawnParticle(explosion2);
            }
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 20);
            */
        }
    }
}
