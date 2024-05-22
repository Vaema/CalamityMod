using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.Audio;
using CalamityMod.Items.Weapons.Rogue;

namespace CalamityMod.Projectiles.Rogue
{
    public class TurbulanceProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Turbulance";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!Projectile.Calamity().stealthStrike)
            {
                Projectile.velocity.Y += 0.05f;
                Projectile.velocity.X *= 0.995f;
            }
            else
            {
                Projectile.velocity *= 1.05f;
            }

            Projectile.ai[0]++;
            Projectile.tileCollide = Projectile.ai[0] >= 2f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 187, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, new Color(53, Main.DiscoG, 255));
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 16, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                dust.noGravity = true;
            }

            if (Projectile.Calamity().stealthStrike) //Stealth strike
            {
                if (Projectile.ai[0] % 4 == 1)
                {
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TurbulanceWindSlash>(), Projectile.damage, Projectile.knockBack / 2, Projectile.owner, 1f, 1f);
                    proj.Calamity().stealthStrike = true;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

            GeneralParticleHandler.SpawnParticle(new ImpactParticle(
                Projectile.Center, MathHelper.ToRadians(Main.rand.NextFloat(-2, 2)), 20, 2f, Color.SkyBlue));
            for (int i = 0; i < 3; i++)
            {
                GeneralParticleHandler.SpawnParticle(new ImpactParticle(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(20, 50), 0).RotatedByRandom(MathHelper.TwoPi), MathHelper.ToRadians(Main.rand.NextFloat(-2, 2)), 20, 0.5f, Color.SkyBlue));
            }    

            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(Turbulance.LightningStrike, Projectile.Center);

                for (int i = 0; i <= 10; i++)
                {
                    GeneralParticleHandler.SpawnParticle(
                        new CrackParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(20, 50), 0).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, Color.Gold, new Vector2(Main.rand.NextFloat(1f, 2f), Main.rand.NextFloat(2f)), Main.rand.NextFloat(MathHelper.TwoPi), 1f, 0.5f, 30)
                        );
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitEffects(hit.Crit);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects(false);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnHitEffects(false);
            return true;
        }

        private void OnHitEffects(bool homeIn)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int w = 0; w < 4; w++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<TurbulanceWindSlash>(), Projectile.damage / 3, Projectile.knockBack / 3, Main.myPlayer, 0f, homeIn ? 1f : 0f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
    }
}
