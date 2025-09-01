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
        public override int swingTime { get; set; } = 15;
        public override bool AlternateSwings { get => base.AlternateSwings; set => base.AlternateSwings = value; }
        public override float lineCollisionLength => 82;

        // Boolshit
        public bool helixFired = false;
        public bool gotEnergyThisSwing = false;
        public bool trailFXTriggered = false;
        public bool particlesSpawned = false;
        public bool sparkTriggered = false;
        private bool IsThrusting => Projectile.localAI[0] == 1;

        public bool IsAlternateThrust { get; set; }
        public int thrustPullbackTime = 26;
        public int thrustForwardTime = 13;
        public int thrustCooldownTime = 20;

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

            if (player.altFunctionUse == 2)
            {
                // Check for energy before the projectile exists
                if (calamityPlayer.lucreciaEnergy < 100)
                {
                    Projectile.Kill();
                    return;
                }

                IsAlternateThrust = true;
                StartupTime = thrustPullbackTime;
                swingTime = thrustForwardTime;
                CooldownTime = thrustCooldownTime;
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
                StartupTime = 8;
                CooldownTime = 5;
                swingTime = 15;
                OffsetDistance = 36;
                RotateInStartup = 0.8f;
                RotateInCooldown = 0;
            }

            if (swingTime > 0)
                swingTime = (int)(swingTime / player.GetAttackSpeed(DamageClass.Melee));

            // rest of the original Spawn method
            if (AlternateSwings)
                modplayer.swingNum = (modplayer.swingNum + 1) % 2;
        }

        public override void AdditionalAI()
        {
            if (IsAlternateThrust)
            {
                if (inStartup)
                {
                    trailFXTriggered = false; // Reset bool just in case
                    Projectile.Opacity += 0.01f; // Fade in

                    // Pullback
                    float eased = MathF.Pow(StartupCompletion, 0.3f);
                    OffsetDistance = (int)MathHelper.Lerp(44, 34, eased);
                    Projectile.scale = baseScale * MathHelper.Lerp(1f, 0.85f, eased);


                    if (!sparkTriggered && Projectile.scale < 0.96f) // Right before thrusting forward
                    {
                        // Won't happen again this right-click
                        sparkTriggered = true;

                        GenericSparkle sparker = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.AntiqueWhite, Color.Lerp(Color.MediumPurple, Color.CornflowerBlue, 0.5f) * 0.3f, 2f, 8, Projectile.velocity.ToRotation() + (Main.rand.NextBool() ? -0.25f : 0.25f), 1f);
                        GeneralParticleHandler.SpawnParticle(sparker);
                    }
                }

                else if (timer < StartupTime + swingTime)
                {
                    Projectile.extraUpdates = 18;

                    // Thrust forward
                    OffsetDistance = (int)MathHelper.Lerp(34, 60, SwingCompletion);
                    Main.player[Projectile.owner].Calamity().GeneralScreenShakePower = 3.5f;

                    // Scaling adjustments
                    var t = MathHelper.Clamp(SwingCompletion, 0f, 1f);
                    var eased = MathF.Pow(t, 0.125f);
                    Projectile.scale = baseScale * MathHelper.Lerp(0.9f, 1.6f, eased);

                    // VFX vars
                    Player player = Main.player[Projectile.owner];
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 fireDirection = Vector2.Normalize(mousePosition - player.Center);

                    // Fire the bigass projectile
                    if (!trailFXTriggered)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, fireDirection * 5, ModContent.ProjectileType<LucreciaDNATrailCreator>(), Projectile.damage * 27, Projectile.knockBack, Projectile.owner, 0f, 0f);
                        
                        SoundStyle swish = new("CalamityMod/Sounds/Custom/MeatySlash");
                        SoundEngine.PlaySound(swish with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);

                        SoundStyle projectile = new("CalamityMod/Sounds/Item/OmicronBeam");
                        SoundEngine.PlaySound(projectile with { Volume = 0.75f, Pitch = Main.rand.NextFloat(0.15f, 0.2f) }, Projectile.Center);
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

                else
                {
                    Projectile.extraUpdates = 7;
                    Projectile.Opacity -= 0.012f;

                    // During cooldown, pull back slightly
                    Projectile.scale = baseScale * MathHelper.Lerp(1.6f, 1.55f, CooldownCompletion);
                    OffsetDistance = (int)MathHelper.Lerp(60, 52, CooldownCompletion);
                }
            }

            else // Primary
            {
                Projectile.Opacity += 0.03f; //fade in effect to look smoother for repeated use

                if (inStartup)
                {
                    gotEnergyThisSwing = false;
                    helixFired = false;
                    AfterImageLength = 0;
                    Projectile.scale = baseScale * MathHelper.Lerp(0.625f, 0.8f, StartupCompletion);
                }

                else if (inCooldown)
                {
                    helixFired = false;
                    Projectile.Opacity -= 0.09f;
                    AfterImageLength = 0;
                    Projectile.extraUpdates = 6;
                    Projectile.scale = baseScale * MathHelper.Lerp(0.85f, 0.625f, CooldownCompletion);
                }

                else if (inSwing)
                {
                    if (!helixFired)
                    {
                        helixFired = true;
                        var player = Main.player[Projectile.owner];
                        var mousePosition = Main.MouseWorld;
                        var fireDirection = Vector2.Normalize(mousePosition - player.Center);
                        var helixSpeed = 12f;
                        var helixVelocity = fireDirection * helixSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center + fireDirection * 3, helixVelocity, ModContent.ProjectileType<LucreciaSmallProjectile>(), Projectile.damage * 3, Projectile.knockBack, Projectile.owner);
                        
                        SoundStyle projectile = new("CalamityMod/Sounds/Item/LucreciaBoltFire");
                        SoundEngine.PlaySound(projectile with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.06f, 0.1f) }, Projectile.Center);
                    }
                    AfterImageLength = 32;
                    Projectile.extraUpdates = 10;
                    var t = MathHelper.Clamp(SwingCompletion, 0f, 1f);
                    var parabola = 1f - MathF.Pow(t - 0.5f, 2f) * 4f;
                    OffsetDistance = (int)MathHelper.Lerp(36 * 1f, 36 * 1.435f, parabola);
                    var upPhase = t <= 0.5f ? t / 0.5f : (1f - t) / 0.5f;
                    var eased = MathF.Pow(upPhase, 2.6f);
                    Projectile.scale = baseScale * MathHelper.Lerp(0.8f, 1.4f, eased);

                }
            }
            base.AdditionalAI();
        }

        public override float SwingFunction()
        {
            var player = Main.player[Projectile.owner];

            if (IsAlternateThrust)
            {
                var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
                float swingDirection = Projectile.spriteDirection;

                if (inStartup)
                {
                    return 0f;
                }

                if (timer < StartupTime + swingTime)
                {
                    return 0f;
                }

                else
                    return 0f; // Just retract and fade out without moving the angle
            }

            else // Primary swing
            {
                var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
                float swingDirection = Projectile.spriteDirection;
                if (modplayer.swingNum == 1)
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
                Color useColor = modplayer.swingNum == 1 ? Color.CornflowerBlue : Color.MediumPurple; // Alternate each swing

                for (int k = 0; k < points; k++)
                {
                    Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f);
                    Particle spark = new GlowSparkParticle((target.Center + velocity * 7.5f), velocity * 0.5f, false, 9, 0.05f, useColor, new Vector2(0.5f, 0.4f), true, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }

        // Draw fullbright
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            return base.PreDraw(ref lightColor);
        }
    }
}
