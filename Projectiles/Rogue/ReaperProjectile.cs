using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Rogue
{
    public class ReaperProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public int time = 0;
        public int ChargeupTime = 56;
        public int Lifetime = 500;
        public bool spinning = false;
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/TheOldReaper";
        public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
        public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
        public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);
        public Player Owner => Main.player[Projectile.owner];
        public SlotId SpinSoundSlot;

        Vector2 squash = Vector2.One;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = Lifetime + ChargeupTime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15 * Projectile.MaxUpdates;
            Projectile.DamageType = RogueDamageClass.Instance;
        }
        public override bool ShouldUpdatePosition()
        {
            return ChargeProgress >= 1;
        }

        public override bool? CanDamage()
        {
            //We don't want the anticipation to deal damage.
            if (ChargeProgress < 1)
                return false;

            return base.CanDamage();
        }
        //Swing animation keys
        public CurveSegment pullback = new CurveSegment(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
        public CurveSegment throwout = new CurveSegment(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
        internal float ArmAnticipationMovement() => PiecewiseAnimation(ChargeProgress, new CurveSegment[] { pullback, throwout });
        public override void AI()
        {
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Projectile.spriteDirection = Projectile.direction;

            if (SoundEngine.TryGetActiveSound(SpinSoundSlot, out var SpinSound) && SpinSound.IsPlaying)
                SpinSound.Position = Projectile.Center;

            //Anticipation animation. Make the player look like theyre holding the weapon
            if (ChargeProgress < 1)
            {
                Owner.ChangeDir(MathF.Sign(Main.MouseWorld.X - Owner.Center.X));

                float armRotation = ArmAnticipationMovement() * Owner.direction;

                Owner.heldProj = Projectile.whoAmI;
                Projectile.spriteDirection = Owner.direction;
                Projectile.direction = Owner.direction;

                Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -70f * Owner.gravDir + new Vector2(14 * Owner.direction, 0);
                Projectile.rotation = (-MathHelper.PiOver4 * Projectile.direction + armRotation) * Owner.gravDir;

                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

                time++;
                return;
            }

            //Play the throw sound when the throw ACTUALLY BEGINS.
            //Additionally, make the projectile collide and set its speed and velocity
            if (Projectile.timeLeft == Lifetime)
            {
                Projectile.netUpdate = true;

                SoundStyle fire = new("CalamityMod/Sounds/Item/SwingMid");
                SoundEngine.PlaySound(fire with { Volume = 0.8f, Pitch = Main.rand.NextFloat(0.2f, 0.3f) }, Projectile.Center);

                Projectile.Center = Owner.MountedCenter + Projectile.velocity * 4f;
                if (Projectile.Calamity().stealthStrike)
                {
                    Projectile.velocity = new Vector2(0.5f * Owner.direction, -1) * 18;
                }
                else
                {
                    Projectile.velocity = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction) * 15;
                }
                Projectile.spriteDirection = Projectile.direction;
                SpinSoundSlot = SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/SpinningWoosh") with { Pitch = -0.3f, Volume = 0.6f }, Projectile.Center);
                
                time = 0;
                spinning = true;
            }

            if (Projectile.velocity.X > 0)
                Projectile.direction = 1;
            else
                Projectile.direction = -1;

            if (spinning)
            {
                if (Projectile.velocity.Length() < 15)
                    Projectile.velocity *= 1.01f;
                if (time == 0)
                    Projectile.rotation += Main.rand.NextFloat(0, 10) * Projectile.direction;
                squash = new Vector2(1.3f, 0.8f);
                Projectile.rotation += 0.2f * Projectile.direction;

                if (targetDist < 1400)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float rot = Main.rand.NextFloat(-5.5f, 5.5f);
                        float scale = Main.rand.NextFloat(1f, 1.15f);
                        Particle Smear = new CustomPulse(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 18, Projectile.velocity, Color.Chartreuse * Main.rand.NextFloat(0.68f, 0.75f), "CalamityMod/Particles/CircularSmearSmokey", squash.RotatedBy(rot), Projectile.velocity.ToRotation() + MathHelper.ToRadians(150f) - rot, scale, scale, 3);
                        GeneralParticleHandler.SpawnParticle(Smear);
                    }

                    if (time % 7 == 0)
                    {
                        Particle trail = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(80, 80), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), false, 60, Main.rand.NextFloat(0.9f, 1.5f), Color.Chartreuse);
                        GeneralParticleHandler.SpawnParticle(trail);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        float rot2 = (Projectile.rotation * Projectile.direction);
                        Vector2 dustPos = Projectile.Center + (i * MathHelper.Pi + Projectile.rotation * 0.3f + MathHelper.PiOver2).ToRotationVector2() * 70f;
                        Dust dust = Dust.NewDustPerfect(dustPos, 267);
                        dust.noGravity = true;
                        dust.scale = 0.8f;
                        dust.color = Color.Chartreuse;
                        dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.45f, 0.6f);
                    }
                    if (Main.rand.NextBool())
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(23, 23), Main.rand.NextBool(7) ? 28 : 215);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                        dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.7f);
                    }
                }

                if (Projectile.Calamity().stealthStrike)
                {
                    if (time > 70)
                    {
                        NPC target = Owner.Center.ClosestNPCAt(3000);

                        if (target != null)
                        {
                            Projectile.extraUpdates = 4;
                            if (Projectile.Center.Y < target.Center.Y && Projectile.velocity.Length() < 15)
                                Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 0.9f;
                            else if (Projectile.Center.Y < target.Center.Y)
                                Projectile.velocity *= 0.9f;
                        }
                        else
                        {
                            Projectile.velocity = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction) * 15;
                            Projectile.extraUpdates = 4;
                        }
                    }
                }
            }

            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.Calamity().stealthStrike && time <= 70)
                Projectile.numHits--;
            if (Projectile.numHits == 0)
            {
                if (Projectile.Calamity().stealthStrike) // Rainstorm
                {
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SupernovaStealthBoom>(), (int)(Projectile.damage * 0.5), 0f, Projectile.owner, 0, 0, 0);
                }
                else // Radiation Burst
                {
                    SoundStyle fire = new("CalamityMod/Sounds/Item/RadiationBurst");
                    SoundEngine.PlaySound(fire with { Volume = 1f, Pitch = 0, MaxInstances = -1 }, Projectile.Center);
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RadiationBurst>(), (int)(Projectile.damage * 0.5), 0f, Projectile.owner, 0, 0, 0);
                }
            }
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(SpinSoundSlot, out var SpinSound))
                SpinSound?.Stop();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Main.EntitySpriteDraw(tex.Value, drawPos, null, spinning ? Color.White : Color.Lerp(lightColor, Color.White, Utils.GetLerpValue(0, ChargeupTime, time, true)), Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            Asset<Texture2D> p = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmear");
            
            if (spinning)
            {
                Main.EntitySpriteDraw(p.Value, drawPos + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4, null, Color.Chartreuse with { A = 0 } * 0.45f, Projectile.velocity.ToRotation() + MathHelper.PiOver2, p.Size() * 0.5f, new Vector2(0.9f - 0.3f * Utils.GetLerpValue(25, 0, time, true), 1 + 0.6f * Utils.GetLerpValue(25, 0, time, true)) * Main.rand.NextFloat(1.25f, 1.4f), SpriteEffects.None);
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Chartreuse * 0.5f, 1);
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 70, targetHitbox);
    }
}
