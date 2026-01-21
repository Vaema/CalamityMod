using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Melee
{
    public class AndromedasStrideBoltSpawner : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        public NPC Target => Main.npc[(int)Projectile.ai[0]];
        public ref float ChargeLevel => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 70;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.hide = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            // Set time left based on charge level
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.timeLeft = 15 * (int)ChargeLevel;
                Projectile.localAI[0] = 1f;
            }

            // Constantly follow its target
            // Die if the target is no longer active
            if (!Target.active)
            {
                Projectile.Kill();
                return;
            }
            else
                Projectile.Center = Target.Center;

            // Spawn smoke
            if (Projectile.timeLeft < 59)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 flyDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi / 8f) * Main.rand.NextFloat(15f, 35f);

                    Particle smoke = new HeavySmokeParticle(Projectile.Center, flyDirection, Color.Lerp(Color.MidnightBlue, Color.Indigo, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)), 30, Main.rand.NextFloat(0.4f, 1.3f) * Projectile.scale, 0.8f, 0, false, 0, true);
                    GeneralParticleHandler.SpawnParticle(smoke);

                    if (Main.rand.NextBool(3))
                    {
                        Particle smokeGlow = new HeavySmokeParticle(Projectile.Center, flyDirection, Color.Red, 20, Main.rand.NextFloat(0.1f, 0.7f) * Projectile.scale, 0.8f, 0, true, 0.01f, true);
                        GeneralParticleHandler.SpawnParticle(smokeGlow);
                    }

                }
            }

            // Rain stars from the sky
            if (Projectile.timeLeft % 15 == 5)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                for (int i = 0; i < 2; i++)
                {
                    if (Owner.whoAmI == Main.myPlayer)
                    {
                        Vector2 starPos = new Vector2(Projectile.Center.X + Main.rand.NextFloat(-250f, 250f), Projectile.Center.Y - Main.rand.NextFloat(650f, 750f));
                        Vector2 starVel = (Projectile.Center - starPos).SafeNormalize(Vector2.UnitY) * 27f;
                        Projectile star = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), starPos, starVel, ProjectileType<GalaxiaBolt>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, 0.75f, MathHelper.Pi / 20f);
                        star.scale = 2f;
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 hitPositionDisplace = -Vector2.UnitY * Main.rand.NextFloat(10f);
                    Vector2 flyDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(5f, 15f);

                    Particle smoke = new HeavySmokeParticle(Projectile.Center + hitPositionDisplace, flyDirection, Color.Lerp(Color.MidnightBlue, Color.Indigo, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f)), 30, Main.rand.NextFloat(1f, 1.6f) * Projectile.scale, 0.8f, 0, false, 0, true);
                    GeneralParticleHandler.SpawnParticle(smoke);

                    if (Main.rand.NextBool(3))
                    {
                        Particle smokeGlow = new HeavySmokeParticle(Projectile.Center + hitPositionDisplace, flyDirection, Color.Red, 20, Main.rand.NextFloat(1.1f, 1.4f) * Projectile.scale, 0.8f, 0, true, 0.005f, true);
                        GeneralParticleHandler.SpawnParticle(smokeGlow);
                    }
                }
            }
        }
    }
}
