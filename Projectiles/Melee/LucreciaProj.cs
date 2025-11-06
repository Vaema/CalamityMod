using System;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityMod.CalPlayer;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Projectiles.Melee.Shortswords;
using System.Runtime.InteropServices;
using CalamityMod.Sounds;

namespace CalamityMod.Projectiles.Melee
{
    public class LucreciaProj : BaseSwordHoldoutProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override bool useMeleeSpeed => true;
        public override bool useMeleeSize => false;
        public override int swingWidth => 310;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<Lucrecia>()).Item;
        public override string Texture => "CalamityMod/Items/Weapons/Melee/Lucrecia";
        public override SoundStyle? UseSound => SoundID.Item71 with { Volume = 0.85f };
        public override int StartupTime { get; set; }
        public override int CooldownTime { get; set; }
        public override int swingTime { get; set; }
        public override bool AlternateSwings { get => base.AlternateSwings; set => base.AlternateSwings = value; }
        public override float lineCollisionLength => 82;

        public bool helixFired = false;
        public bool gotEnergyThisSwing = false;
        public bool trailFXTriggered = false;
        public bool particlesSpawned = false;
        public bool sparkTriggered = false;

        public int standardStartupTime = 8;
        public int standardSwingTime = 14;
        public int standardCooldownTime = 7;
        private bool IsThrusting => Projectile.localAI[0] == 1;
        public bool IsAlternateThrust { get; set; }
        public int thrustStartupTime = 33;
        public int thrustForwardTime = 7;
        public int thrustCooldownTime = 17;
        public float thrustSpinRotation = 0f;
        public bool playedAlternateThrustStartupWhoosh = false;

        public override void Defaults()
        {
            Projectile.extraUpdates = 6;
            Projectile.noEnchantmentVisuals = true;
            Projectile.Opacity = 0.2f; // Starting point, fades in quickly upon startup
            Projectile.width = Projectile.height = 54;
        }

        public override void Spawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
            var calamityPlayer = player.Calamity();

            // Store the base use time from the item
            int baseUseTime = BaseItem.useTime;

            if (player.altFunctionUse == 2)
            {
                // Check for energy before the projectile exists
                if (calamityPlayer.lucreciaEnergy < 100)
                {
                    Projectile.Kill();
                    return;
                }

                IsAlternateThrust = true;

                // Scale all time vars correctly
                StartupTime = (int)(thrustStartupTime * (float)player.itemAnimationMax / baseUseTime);
                swingTime = (int)(thrustForwardTime * (float)player.itemAnimationMax / baseUseTime);
                CooldownTime = (int)(thrustCooldownTime * (float)player.itemAnimationMax / baseUseTime);

                Projectile.DamageType = DamageClass.Melee;
                Projectile.width = Projectile.height = 36;
                useMeleeSize = true;
                UseSound = SoundID.DD2_JavelinThrowersAttack;
                OffsetDistance = -30;
                RotateInStartup = 0.8f;
                RotateInCooldown = 1f;
                Projectile.knockBack = 15f;

                calamityPlayer.lucreciaEnergy = 0; // Remove energy for startup
            }
            else
            {
                IsAlternateThrust = false;

                // Scale all time vars correctly
                StartupTime = (int)(standardStartupTime * (float)player.itemAnimationMax / baseUseTime);
                CooldownTime = (int)(standardSwingTime * (float)player.itemAnimationMax / baseUseTime);
                swingTime = (int)(standardCooldownTime * (float)player.itemAnimationMax / baseUseTime);

                OffsetDistance = 36;
                RotateInStartup = 0.8f;
                RotateInCooldown = 0;
            }
        }

        public override void AdditionalAI()
        {
            Player player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();

            if (IsAlternateThrust)
            {
                if (inStartup)
                {
                    trailFXTriggered = false; // Reset bool just in case
                    Projectile.Opacity += 0.01f; // Fade in
                    Projectile.scale = baseScale * MathHelper.Lerp(1f, 0.85f, MathF.Pow(StartupCompletion, 0.3f));

                    if (StartupCompletion >= 0.12f && !playedAlternateThrustStartupWhoosh)
                    {
                        SoundEngine.PlaySound(CommonCalamitySounds.MeatySlashSound with { Volume = 0.55f, Pitch = -0.05f }, Projectile.Center);
                        playedAlternateThrustStartupWhoosh = true;
                    }

                    // Every 8 ticks during this part of startup, release a sparkle.
                    if (StartupCompletion > 0.12f && StartupCompletion < 0.44f)
                    {
                        if (timer % 8 == 0)
                        {
                            Vector2 pos = player.Center + Projectile.rotation.ToRotationVector2() * 60f;
                            Particle sparkle = new CritSpark(pos, new Vector2(7f, 0).RotatedBy(Projectile.rotation), Color.Lerp(Color.CornflowerBlue, Color.MediumPurple, Main.rand.NextFloat(1f)), Color.White * 0.33f, 1.2f, 12, 0.3f, 1.2f);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                        Particle smear = new CircularSmearVFX(player.Center, Color.CornflowerBlue * 0.25f, Projectile.rotation, Projectile.scale * 1.25f);
                        GeneralParticleHandler.SpawnParticle(smear);
                    }

                    // Easing on spin
                    float t = MathHelper.Clamp(StartupCompletion * 2f, 0f, 1f);
                    float eased = 0.5f - 0.5f * MathF.Cos(MathHelper.Pi * t);
                    thrustSpinRotation = eased * MathHelper.TwoPi; 

                    // Pull back
                    if (StartupCompletion >= 0.5f)
                    {
                        float easedPullback = MathF.Pow((StartupCompletion - 0.5f) * 2f, 0.3f);
                        OffsetDistance = (int)MathHelper.Lerp(44, 34, easedPullback);
                    }
                    else
                    {
                        OffsetDistance = 44;
                    }
                }

                else if (inCooldown)
                {
                    Projectile.Opacity -= 0.0135f;

                    // During cooldown, pull back slightly
                    Projectile.scale = baseScale * MathHelper.Lerp(1.4f, 1.25f, CooldownCompletion);
                    OffsetDistance = (int)MathHelper.Lerp(78, 38, CooldownCompletion);
                }

                else if (inSwing)
                {
                    // Thrust forward
                    OffsetDistance = (int)MathHelper.Lerp(34, 78, SwingCompletion);
                    Main.player[Projectile.owner].SetScreenshake(3.5f);

                    // Scaling adjustments
                    var t = MathHelper.Clamp(SwingCompletion, 0f, 1f);
                    var eased = MathF.Pow(t, 0.125f);
                    Projectile.scale = baseScale * MathHelper.Lerp(0.9f, 1.4f, eased);

                    // VFX vars
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 fireDirection = Vector2.Normalize(mousePosition - player.Center);

                    // Fire the bigass projectile
                    if (!trailFXTriggered)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, fireDirection * 5, ModContent.ProjectileType<LucreciaDNATrailCreator>(), Projectile.damage * 8, Projectile.knockBack, Projectile.owner, 0f, 0f);
                        
                        SoundStyle swish = new("CalamityMod/Sounds/Custom/MeatySlash");
                        SoundEngine.PlaySound(swish with { Volume = 0.3f, Pitch = Main.rand.NextFloat(0.1f, 0.2f) }, Projectile.Center);

                        SoundStyle projectile = new("CalamityMod/Sounds/Item/OmicronBeam");
                        SoundEngine.PlaySound(projectile with { Volume = 0.85f, Pitch = Main.rand.NextFloat(0.15f, 0.2f) }, Projectile.Center);
                    }

                    trailFXTriggered = true;

                    // Some sparks from the blade
                    if (!particlesSpawned)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Vector2 particleOrigin = Projectile.Center;
                            Vector2 particleSpeed = fireDirection.RotatedByRandom(MathHelper.ToRadians(46f)) * Main.rand.NextFloat(20f, 37f);
                            Particle sparks = new CritSpark(particleOrigin, particleSpeed, Color.Lerp(Color.CornflowerBlue, Color.MediumPurple, Main.rand.NextFloat(0f, 1f)), Color.NavajoWhite * 0.7f, Main.rand.NextFloat(0.9f, 2f), Main.rand.Next(38, 50), 0.1f, 1.5f, hueShift: 0.01f);
                            GeneralParticleHandler.SpawnParticle(sparks);
                        }
                        particlesSpawned = true;
                    }

                }
            }

            else // Primary
            {
                if (inStartup)
                {
                    Projectile.Opacity += 0.01f; // Fade in
                    gotEnergyThisSwing = false;
                    helixFired = false;
                    Projectile.scale = baseScale * MathHelper.Lerp(0.625f, 0.8f, StartupCompletion);
                }

                else if (inCooldown)
                {
                    helixFired = false;
                    Projectile.Opacity -= 0.09f;
                    Projectile.scale = baseScale * MathHelper.Lerp(0.85f, 0.625f, CooldownCompletion);
                }

                else if (inSwing)
                {
                    if (!helixFired)
                    {
                        helixFired = true;
                        var mousePosition = Main.MouseWorld;

                        // Smear fx on swing
                        Vector2 shootDir = player.Center.DirectionTo(mousePosition) * 10f;
                        int dir = -Math.Sign(mousePosition.X);
                        Particle swipe = new CustomSpark(player.Center - shootDir * 4, shootDir.RotatedBy(0.075f * (dir * (modplayer.swingNum % 2 == 0 ? 1 : -1))) * 1.22f, "CalamityMod/Particles/VerticalSmearLarge", false, (int)(14 / player.GetAttackSpeed(DamageClass.Melee)), 0.3f, modplayer.swingNum % 2 == 0 ? Color.CornflowerBlue * 0.85f : Color.MediumPurple * 0.8f, new Vector2(1.1f, 1.3f), true, false, 0, false, false);
                        GeneralParticleHandler.SpawnParticle(swipe);

                        var fireDirection = Vector2.Normalize(mousePosition - player.Center);
                        var helixSpeed = 12f;
                        var helixVelocity = fireDirection * helixSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center + fireDirection * 3, helixVelocity, ModContent.ProjectileType<LucreciaSmallProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        
                        SoundStyle projectile = new("CalamityMod/Sounds/Item/LucreciaBoltFire");
                        SoundEngine.PlaySound(projectile with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.06f, 0.1f) }, Projectile.Center);
                    }

                    var t = MathHelper.Clamp(SwingCompletion, 0f, 1f);
                    var parabola = 1f - MathF.Pow(t - 0.5f, 2f) * 4f;
                    OffsetDistance = (int)MathHelper.Lerp(36 * 1f, 36 * 1.435f, parabola);
                    var upPhase = t <= 0.5f ? t / 0.5f : (1f - t) / 0.5f;
                    var eased = MathF.Pow(upPhase, 2.6f);
                    Projectile.scale = baseScale * MathHelper.Lerp(0.8f, 1.4f, eased);

                    // Every 5 ticks during swing, release a particle
                    if (t > 0.05f && t < 0.5f)
                    {
                        if (timer % 5 == 0)
                        {
                            var fireDirection = Vector2.Normalize(Main.MouseWorld - player.Center);
                            var helixVelocity = fireDirection * 14f;

                            SparkParticle orb3 = new SparkParticle(player.Center + fireDirection * 3, helixVelocity.RotatedByRandom(1f), true, 16, 0.5f, Color.Lerp(Color.CornflowerBlue, Color.MediumPurple, Main.rand.NextFloat()) * 0.66f, true);
                            GeneralParticleHandler.SpawnParticle(orb3);
                        }
                    }
                }
            }
            base.AdditionalAI();
        }

        public override float SwingFunction()
        {
            var player = Main.player[Projectile.owner];

            if (IsAlternateThrust)
            {
                if (inStartup)
                    return thrustSpinRotation;
                else
                    return 0f; // Just retract and fade out without moving the angle
            }

            else // Primary swing
            {
                var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
                float swingDirection = Projectile.spriteDirection;
                if (modplayer.swingNum % 2 == 1)
                    swingDirection *= -1;

                var parabolicFactorPrimary = 1f - MathF.Pow(SwingCompletion - 0.5f, 2f) * 4f;
                parabolicFactorPrimary = MathHelper.Clamp(parabolicFactorPrimary, 0f, 1f);
                Projectile.localAI[0] = parabolicFactorPrimary;

                var startAnglePrimary = -swingWidth / 2.15f;
                var endAnglePrimary = swingWidth / 2.15f;
                var trueAnglePrimary = MathHelper.Lerp(startAnglePrimary, endAnglePrimary, SwingCompletion);

                return MathHelper.ToRadians(trueAnglePrimary * -swingDirection);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Add energy for the primary attack
            if (!IsAlternateThrust && !gotEnergyThisSwing)
            {
                SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.3f }, Projectile.Center);

                gotEnergyThisSwing = true;
                var player = Main.player[Projectile.owner];
                var modPlayer = player.Calamity();

                // +22 energy on hit
                modPlayer.lucreciaEnergy += 22;
                modPlayer.lucreciaEnergy = Math.Min(modPlayer.lucreciaEnergy, Lucrecia.MaxEnergy);

                // On-hit cut FX
                int points = 2;
                float radians = MathHelper.TwoPi / points;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)).RotatedByRandom(100);

                var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
                Color useColor = modplayer.swingNum % 2 == 0 ? Color.CornflowerBlue : Color.MediumPurple; // Alternate each swing

                for (int k = 0; k < points; k++)
                {
                    Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f);
                    Particle spark = new GlowSparkParticle((target.Center + velocity * 7.5f), velocity * 0.5f, false, 9, 0.05f, useColor, new Vector2(0.5f, 0.6f), true, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw fullbright
            lightColor = Color.White;
            return base.PreDraw(ref lightColor);
        }
    }
}
