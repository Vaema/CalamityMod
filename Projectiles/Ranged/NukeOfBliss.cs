using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Projectiles.Ranged.BlissfulBombardierHoldout;
using static System.Net.Mime.MediaTypeNames;

namespace CalamityMod.Projectiles.Ranged
{
    public class NukeOfBliss : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private ref float RocketID => ref Projectile.ai[0];
        public ref float time => ref Projectile.ai[2];
        public int reachedPeakTime = 120;
        public int rainDownTimer = 150;
        public NPC targeted;
        public float fade = 1;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = 700;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (Projectile.Center.Y > Owner.Calamity().mouseWorld.Y && rainDownTimer <= 0)
                Projectile.tileCollide = true;

            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(90) * Projectile.direction;

            if (time > reachedPeakTime)
            {
                if (rainDownTimer > 1)
                {
                    Projectile.Center = new Vector2((targeted != null ? targeted.Center.X : Owner.Calamity().mouseWorld.X), Owner.Center.Y) + new Vector2(0, -600);
                }

                if (targeted == null)
                    targeted = (rainDownTimer == 0 ? Projectile.Center : Owner.Calamity().mouseWorld).ClosestNPCAt(rainDownTimer == 0 ? 600 : 250);
                if (targeted != null && Projectile.Center.Y > targeted.Center.Y)
                    targeted = null;

                if (rainDownTimer >= 80)
                {
                    bool isClusterRocket = (RocketID == ItemID.ClusterRocketI || RocketID == ItemID.ClusterRocketII);
                    if (rainDownTimer % 15 == 0)
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Custom/Providence/ProvidenceHolyBlastShoot");
                        SoundEngine.PlaySound(fire with { Volume = 0.45f, Pitch = (0 - 0.3f * Utils.GetLerpValue(0, 300, rainDownTimer, true)) }, Projectile.Center);
                        for (int i = 0; i < (isClusterRocket ? 4 : 2); i++)
                        {
                            Vector2 variance = new Vector2(80 * (isClusterRocket ? 3 : 1), 0) * Main.rand.NextFloat(-1f, 1f);
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + variance, (((targeted != null ? targeted.Center : Owner.Calamity().mouseWorld) - Projectile.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(8, 12)) + variance * 0.008f, ModContent.ProjectileType<BlissfulBombardierSplitProjectile>(), (int)(Projectile.damage * (isClusterRocket ? 0.15f : 0.3f)), Projectile.knockBack, Projectile.owner, Projectile.ai[0], 0f);
                        }
                    }
                }
                if (rainDownTimer > 0)
                    rainDownTimer--;
                if (rainDownTimer == 65)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        SoundStyle sound = new("CalamityMod/Sounds/Item/MissileNearing");
                        SoundEngine.PlaySound(sound with { Volume = 0.6f, Pitch = 0.4f, MaxInstances = 2 }, Projectile.Center);
                    }
                }
                if (rainDownTimer == 1)
                {
                    targeted = null;
                    Projectile.extraUpdates = 10;
                    Projectile.penetrate = 1;
                    Projectile.velocity = ((targeted != null ? targeted.Center : Owner.Calamity().mouseWorld) - Projectile.Center).SafeNormalize(Vector2.UnitX) * 15;
                }
                if (rainDownTimer == 0)
                {
                    Particle spark2 = new SparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, false, 19, 1.7f, Color.Goldenrod);
                    GeneralParticleHandler.SpawnParticle(spark2);
                    if (targeted != null && targeted.Center.Y > Projectile.Center.Y)
                    {
                        Vector2 moveToTrackingPos = ((targeted != null ? targeted.Center : Owner.Calamity().mouseWorld) - Projectile.Center).SafeNormalize(Vector2.UnitX);
                        if (Projectile.velocity.Length() < 15)
                            Projectile.velocity += moveToTrackingPos * 2.5f;
                        else
                            Projectile.velocity *= 0.9f;
                    }
                }
            }
            else
                Projectile.velocity *= 0.995f;

            fade = rainDownTimer > 0 ? Utils.GetLerpValue(reachedPeakTime, reachedPeakTime * 0.7f, time, true) : 1;
            if (fade > 0.2f)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.6f), staticEffectsColor, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.3f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            if (RocketID == ItemID.DryRocket || RocketID == ItemID.WetRocket || RocketID == ItemID.LavaRocket || RocketID == ItemID.HoneyRocket)
            {
                Projectile.ignoreWater = false;
                if (Projectile.wet)
                    Projectile.timeLeft = 1;
            }

            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);
        }
        public override void OnKill(int timeLeft)
        {
            // Only do rocket effects for the owner client side
            if (Projectile.owner != Main.myPlayer)
                return;

            var info = new CalamityUtils.RocketBehaviorInfo((int)RocketID)
            {
                // Since we use our own spawning method for the cluster rockets, we don't need them to shoot anything,
                // we'll do it ourselves.
                clusterProjectileID = ProjectileID.None,
                destructiveClusterProjectileID = ProjectileID.None,
            };

            bool isClusterRocket = (RocketID == ItemID.ClusterRocketI || RocketID == ItemID.ClusterRocketII);
            SoundStyle fire = new("CalamityMod/Sounds/Item/MineralMortarExplode");
            SoundEngine.PlaySound(fire with { Volume = 0.9f, Pitch = 0.4f }, Projectile.Center);
            SoundStyle fire2 = new("CalamityMod/Sounds/Item/BlazingCoreParry");
            SoundEngine.PlaySound(fire2 with { Volume = 0.9f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

            int blastRadius = (int)(MathHelper.Clamp(Projectile.RocketBehavior(info), 3, 100));
            Projectile.ExpandHitboxBy((float)blastRadius);
            Projectile.damage = (int)(Projectile.damage * 0.5f);
            Projectile.penetrate = -1;
            Projectile.Damage();

            float blastRadiusVisual = blastRadius * 0.5f;

            Particle orb4 = new CustomPulse(Projectile.Center, Vector2.Zero, staticEffectsColor, "CalamityMod/Particles/SoftRoundExplosion", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.07f * blastRadiusVisual, 19);
            GeneralParticleHandler.SpawnParticle(orb4);
            Particle orb5 = new CustomPulse(Projectile.Center, Vector2.Zero, effectsColor, "CalamityMod/Particles/SoftRoundExplosion", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.05f * blastRadiusVisual, 19);
            GeneralParticleHandler.SpawnParticle(orb5);

            for (int i = 0; i < 3; i++)
            {
                Particle orb3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/SmallBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0, 0.45f * blastRadiusVisual, 15, true);
                GeneralParticleHandler.SpawnParticle(orb3);
            }

            for (int i = 0; i < 40; i++)
            {
                Dust c = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>());
                c.velocity = (MathHelper.TwoPi * i / 40f).ToRotationVector2() * 8.5f * (i % 2 == 0 ? 0.88f : 1f) * blastRadiusVisual;
                c.scale = Main.rand.NextFloat(0.3f, 0.6f) * blastRadius * 0.3f * (i % 2 == 0 ? 2.2f : 1.8f);
                c.noGravity = true;
                c.color = effectsColor;
            }
            for (int i = 0; i < 25; i++)
            {
                Dust l = Dust.NewDustPerfect(Projectile.Center, 278);
                l.velocity = new Vector2(5, 5).RotatedByRandom(100) * blastRadiusVisual * Main.rand.NextFloat(0.4f, 1f);
                l.scale = Main.rand.NextFloat(0.6f, 0.8f) * blastRadiusVisual * 0.2f * (i % 2 == 0 ? 2.2f : 1.8f);
                l.noGravity = false;
                l.color = staticEffectsColor;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = new Vector2(8, 8).RotatedByRandom(100) * blastRadiusVisual * Main.rand.NextFloat(0.4f, 1f);
                Particle spark = new CustomSpark(Projectile.Center, velocity, "CalamityMod/Projectiles/Boss/ProvidenceCrystal", false, 12, 0.15f * blastRadius, Color.White, new Vector2(1.5f, 0.4f), true, false, 0, false, false, 0.7f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.35f;
            int hitsToMinMult = 10;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult * (rainDownTimer <= 0 ? 1 : 0.2f);
        }
        public override bool? CanDamage() => (fade <= 0.2f) ? false : null;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/NukeOfBliss").Value;

            float fade2 = rainDownTimer > 0 ? Utils.GetLerpValue(reachedPeakTime, reachedPeakTime * 0.7f, time) : 1;
            Projectile.DrawProjectileWithBackglow(staticEffectsColor with { A = 0 } * fade2, Color.Lerp(lightColor, Color.Goldenrod with { A = 0 }, Utils.GetLerpValue(0, reachedPeakTime, time, true)) * fade2, 6f * Utils.GetLerpValue(0, reachedPeakTime, time, true), texture);
            return false;
        }
    }
}
