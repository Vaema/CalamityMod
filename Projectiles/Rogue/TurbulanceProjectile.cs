using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.Audio;
using CalamityMod.Items.Weapons.Rogue;
using Terraria.DataStructures;

namespace CalamityMod.Projectiles.Rogue
{
    public class TurbulanceProjectile : ModProjectile, ILocalizedModType
    {
        NPC npcToStickTo = null;
        Vector2 stickOffset;

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
            Projectile.penetrate = 3;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            stickOffset = Vector2.Zero;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target);
        }

        public override void AI()
        {
            if (npcToStickTo != null)
            {
                Projectile.position = npcToStickTo.Center + (stickOffset * npcToStickTo.scale) + (Projectile.Size / 2);
            }

            if (!Projectile.Calamity().stealthStrike)
            {
                Projectile.velocity.Y += 0.05f;
                Projectile.velocity.X *= 0.995f;
            }
            else
            {
                Projectile.velocity *= 1.05f;

                if (Projectile.velocity == Vector2.Zero)
                {
                    if (Projectile.ai[2] % 6 == 0)
                    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Projectile.Center, CalamityUtils.RandomVelocity(1, 10, 20), Color.LightSkyBlue, 100, 2f, 1f, MathHelper.ToRadians(Main.rand.NextFloat(-3f, 3f))));
                    Projectile.ai[2]++;
                }
            }

            if (Projectile.velocity != Vector2.Zero)
            {
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
            }

            Projectile.ai[0]++;
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
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.9f), Projectile.Center);

                npcToStickTo = target;
                stickOffset = target.DirectionTo(Projectile.Center) * ((target.width + target.height) / 4);
                Projectile.velocity = Vector2.Zero;
            }
            OnHitEffects(hit.Crit);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects(false);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnHitEffects(false);
            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.7f), Projectile.Center);;
                Projectile.velocity = Vector2.Zero;
                stickOffset = oldVelocity * 1.7f;
            }
            else
            {
                return true;
            }
            return false;
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
