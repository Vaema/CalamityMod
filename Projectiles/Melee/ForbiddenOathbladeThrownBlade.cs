using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Packets.Entities;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Melee
{
    public class ForbiddenOathbladeThrownBlade : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/ForbiddenOathblade";

        public int ChargeupTime => (int)Projectile.localAI[2];
        public int Lifetime = 120;
        public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
        public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
        public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);
        public Player Owner => Main.player[Projectile.owner];

        // Real variables here
        public ref float time => ref Projectile.ai[0];
        public ref float stabOrder => ref Projectile.ai[1];
        public ref NPC stabbedTarget => ref Main.npc[(int)Projectile.ai[2]];

        public bool thrown = false;
        public int fadeOutTime = 80;
        public bool stuckInTarget = false;
        public int stuckTimer = 0;
        public bool exitedTarget = false;
        public bool stuckInGround = false;
        public Vector2 impalePos;
        public int bounces = 0;
        public Vector2 tipPosition = Vector2.Zero;
        public bool fadingOut => Projectile.timeLeft <= (Lifetime - fadeOutTime);
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + ChargeupTime;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override bool ShouldUpdatePosition()
        {
            return (ChargeProgress >= 1 && !stuckInGround && !stuckInTarget);
        }

        //Swing animation keys
        public CurveSegment pullback = new CurveSegment(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
        public CurveSegment throwout = new CurveSegment(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
        internal float ArmAnticipationMovement() => PiecewiseAnimation(ChargeProgress, new CurveSegment[] { pullback, throwout });

        public override void AI()
        {
            float playerDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Projectile.spriteDirection = Projectile.direction;
            Vector3 Light = Color.MediumOrchid.ToVector3();
            Lighting.AddLight(Projectile.Center, Light * 0.5f);

            if (Projectile.timeLeft == Lifetime)
            {
                // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
                Vector2 toMouse = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX * Owner.direction);
                Projectile.velocity = toMouse * 14;
                Projectile.Center = Owner.MountedCenter + toMouse * 12f;
                Projectile.spriteDirection = Projectile.direction;
                thrown = true;
                time = 0;
                Projectile.extraUpdates = 1;
                Projectile.tileCollide = true;
                Projectile.netUpdate = true;
            }

            if (Projectile.velocity.X > 0)
                Projectile.direction = 1;
            else
                Projectile.direction = -1;

            if (thrown)
            {
                Projectile.spriteDirection = Projectile.direction;

                if (!stuckInGround && exitedTarget)
                    Projectile.rotation += (0.35f * (MathF.Abs(Projectile.velocity.Y) * 0.03f + 0.85f)) * Main.rand.NextFloat(0.7f, 1f) * Projectile.direction * Projectile.Opacity;
                else
                    Projectile.rotation = (Projectile.velocity.ToRotation() + MathHelper.PiOver4 * (Projectile.direction == 1 ? 1 : 3));

                if (time > fadeOutTime * 0.7f)
                    Projectile.velocity *= 0.93f;

                if (stuckInTarget)
                {
                    Projectile.tileCollide = false;
                    time--;
                    Projectile.timeLeft++;
                    if (stuckTimer > 0)
                        stuckTimer--;
                    else
                    {
                        Projectile.velocity *= 0.01f;
                        stabbedTarget.Calamity().demonSwordImpales--;
                        stuckInTarget = false;
                        fadeOutEffect();
                    }

                    Projectile.Center = stabbedTarget.Center + impalePos;
                    if ((stabbedTarget.life <= 0) || stabbedTarget == null || Projectile.localAI[0] == 5 || !stabbedTarget.active)
                    {
                        Projectile.timeLeft = Lifetime;
                        stabbedTarget.Calamity().demonSwordImpales--;
                        stuckInTarget = false;
                        exitedTarget = true;
                        Projectile.tileCollide = true;
                        Projectile.rotation += Main.rand.NextFloat(-1.5f, 1.5f);
                        for (int i = 0; i < Main.maxNPCs; i++)
                            Projectile.localNPCImmunity[i] = 0;
                        Projectile.numHits = 0;
                        SoundStyle unstuck = new("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3);
                        SoundEngine.PlaySound(unstuck with { Volume = 0.85f, Pitch = Main.rand.NextFloat(0.3f, 0.4f), MaxInstances = 3 }, Projectile.Center);
                    }
                }
                if (exitedTarget && !stuckInGround)
                {
                    if (Projectile.velocity.Y < 14 && Projectile.timeLeft < (bounces == 1 ? Lifetime - 20 : Lifetime))
                        Projectile.velocity.Y += 0.1f * (bounces * 3);
                }

                if (fadingOut)
                    fadeOutEffect();
            }
            if (!fadingOut && !stuckInTarget && !stuckInGround && ChargeProgress >= 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 safeVel = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    Vector2 dustVel = exitedTarget ? safeVel.RotatedBy(MathHelper.ToRadians(-90 * Projectile.direction) + Projectile.rotation) : safeVel.RotatedBy(MathHelper.ToRadians(90 * (i == 0 ? 1 : -1))).RotatedByRandom(0.1f) * Main.rand.NextFloat(3, 4);
                    if (!exitedTarget && Main.rand.NextBool(3))
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + safeVel * 60, ModContent.DustType<LightDust>(), dustVel, 0, default, Main.rand.NextFloat(0.9f, 1.1f));
                        dust.noGravity = true;
                        dust.color = Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet;
                        dust.noLight = true;
                        dust.noLightEmittence = true;
                        dust.alpha = 50;
                    }
                    if (exitedTarget && i == 0)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + safeVel.RotatedBy(Projectile.rotation - MathHelper.ToRadians(Projectile.direction * 45)) * 45, DustID.FireworksRGB, dustVel * Main.rand.NextFloat(1, 4), 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                        dust.noGravity = true;
                        dust.color = Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet;
                    }
                }
                
                if (Main.rand.NextBool(4))
                {
                    Particle spark2 = new CustomSpark(Projectile.Center, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.2f, 0.6f), "CalamityMod/Particles/DemonSigilParticle", false, 17, Main.rand.NextFloat(0.2f, 0.3f), Color.Lerp(Color.MediumOrchid, Color.BlueViolet, Main.rand.NextFloat(0, 0.7f)) * 0.6f, new Vector2(1, 1), true, false, Main.rand.NextFloat(-1f, 1f), false, false);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }

            //Anticipation animation. Make the player look like they're holding the item
            if (ChargeProgress < 1)
            {
                throwAnimation();
                if (ChargeProgress >= 0.6f && time == 0)
                {
                    SoundStyle swing = new("CalamityMod/Sounds/Item/DemonSwordSwing", 2);
                    SoundEngine.PlaySound(swing with { Volume = 0.65f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
                    time++;
                }
                return;
            }
            else
                time++;
        }
        public void fadeOutEffect()
        {
            Projectile.tileCollide = false;
            if (Projectile.timeLeft > Lifetime - fadeOutTime)
                Projectile.timeLeft = Lifetime - fadeOutTime;
            Projectile.Opacity = Utils.GetLerpValue(0, Lifetime - fadeOutTime, Projectile.timeLeft, true);

            Vector2 dustVel = new Vector2(10, 10).RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.2f, 1f) * Projectile.Opacity;
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), dustVel, 0, default, Main.rand.NextFloat(1.1f, 1.4f));
                dust.noGravity = true;
                dust.color = Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet;
                dust.noLight = true;
                dust.noLightEmittence = true;
                dust.alpha = 100;
                dust.velocity += Projectile.velocity;
            }
        }
        public void throwAnimation()
        {
            Owner.ChangeDir(MathF.Sign(Main.MouseWorld.X - Owner.Center.X));

            float armRotation = ArmAnticipationMovement() * Owner.direction;

            Owner.heldProj = Projectile.whoAmI;
            Projectile.spriteDirection = Owner.direction;
            Projectile.direction = Owner.direction;

            Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -55f * Owner.gravDir;
            Projectile.rotation = (-MathHelper.PiOver4 * Projectile.direction + armRotation) * Owner.gravDir;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int bonusDamage = 60;
            if (target.Calamity().demonicFlamesBonusDamage <= bonusDamage)
            {
                target.Calamity().demonicFlamesBonusDamage = bonusDamage;
                target.AddBuff(ModContent.BuffType<DemonicFlames>(), 120);
                // Demonic Flames damage must be synced, because OnHitNPC is only run for the client that hit the NPC
                if (Main.netMode != NetmodeID.SinglePlayer)
                    DemonicFlamesSyncPacket.Send(target);
            }

            if (!exitedTarget)
            {
                SoundStyle stuck = new("CalamityMod/Sounds/Item/DemonSwordImpact", 2);
                SoundEngine.PlaySound(stuck with { Volume = 0.75f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = 3 }, Projectile.Center);

                Projectile.ai[2] = target.whoAmI;

                if (target.Calamity().demonSwordImpales < 0)
                    target.Calamity().demonSwordImpales = 0;
                if (target.Calamity().demonSwordImpales >= 3)
                {
                    float bladeValue = -1;
                    Projectile ejectedBlade = null;
                    for (int x = 0; x < Main.maxProjectiles; x++)
                    {
                        Projectile projectile = Main.projectile[x];
                        if (projectile.owner == Projectile.owner && projectile.type == Projectile.type && Projectile.localAI[0] != 5 && projectile.ai[2] == Projectile.ai[2] && projectile.timeLeft > (Lifetime - fadeOutTime) && (bladeValue == -1 || bladeValue > projectile.ai[1]))
                        {
                            bladeValue = projectile.ai[1];
                            ejectedBlade = projectile;
                        }
                    }
                    ejectedBlade.localAI[0] = 5;
                    ejectedBlade.ai[1] += 1000;
                    ejectedBlade.velocity = Projectile.velocity.RotatedByRandom(0.12f);

                    for (int i = 0; i < 10; i++)
                    {
                        Particle spark2 = new SparkParticle(target.Center, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.2f, 1.5f), false, 45, Main.rand.NextFloat(0.3f, 1f), Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet);
                        GeneralParticleHandler.SpawnParticle(spark2);

                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.3f, 1f), 0, default, Main.rand.NextFloat(1.3f, 1.6f));
                        dust.noGravity = true;
                        dust.noLight = true;
                        dust.color = Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet;
                    }
                }
                target.Calamity().demonSwordImpales++;

                stuckInTarget = true;
                impalePos = Projectile.Center - stabbedTarget.Center;
                stuckTimer = 3600;
            }
            Projectile.netUpdate = true;
            if (Main.netMode != NetmodeID.SinglePlayer)
                DemonSwordImpalesSyncPacket.Send(target);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.4f;
            int hitsToMinMult = 8;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult * (exitedTarget ? 1.15f : 1f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (exitedTarget)
            {
                if (bounces >= 2)
                {
                    impaleGround(oldVelocity);
                }
                else
                {
                    if (Projectile.velocity.X != oldVelocity.X)
                    {
                        Projectile.velocity.X = -oldVelocity.X;
                    }
                    if (Projectile.velocity.Y != oldVelocity.Y)
                    {
                        Projectile.velocity.Y = -oldVelocity.Y;
                    }
                    if (Projectile.velocity.Length() < 8)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8;
                    Projectile.velocity.Y *= 0.85f;
                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/CeramicImpact", 2) { Volume = 0.35f, Pitch = Main.rand.NextFloat(-0.4f, -0.5f) }, Projectile.Center);
                    bounces++;
                    Projectile.timeLeft = Lifetime;
                }
            }
            else
            {
                impaleGround(oldVelocity);
            }
            Projectile.netUpdate = true;
            return false;
        }
        public void impaleGround(Vector2 oldVelocity)
        {
            Projectile.velocity = oldVelocity;
            stuckInGround = true;
            Projectile.timeLeft = (int)(Lifetime - fadeOutTime * 0.3f);
            Projectile.tileCollide = false;
            SoundStyle stuck = new("CalamityMod/Sounds/Item/DemonSwordImpact", 2);
            SoundEngine.PlaySound(stuck with { Volume = 0.75f, Pitch = Main.rand.NextFloat(0.2f, 0.3f), MaxInstances = 3 }, Projectile.Center);
        }
        public override bool? CanDamage()
        {
            if (ChargeProgress < 1 || fadingOut || stuckInGround || stuckInTarget || (Projectile.numHits > 0 && !exitedTarget))
                return false;

            return base.CanDamage();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => (Projectile.numHits > 0 && !exitedTarget) ? false : CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.width / (exitedTarget ? 0.5f : 1), targetHitbox); // After exiting a target, the hitbox is larger
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/ForbiddenOathblade").Value;
            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;

            float fadeScale = (1 - Projectile.Opacity);
            for (int i = 0; i < 16; i++)
            {
                Color auraColor = Color.MediumOrchid with { A = 0 } * 0.4f * fadeScale * Projectile.Opacity;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 16f).ToRotationVector2() * 9 * fadeScale;
                Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition + drawOffset, null, auraColor, Projectile.rotation, centerTexture.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.BlueViolet with { A = 0 }, lightColor, Projectile.Opacity) * Projectile.Opacity, Projectile.rotation, centerTexture.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
        /*
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.timeLeft);
            writer.Write(Projectile.rotation);
            writer.Write(Projectile.localAI[2]);
            writer.Write(Projectile.localAI[0]);

            writer.WriteFlags(stuckInTarget, thrown);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.timeLeft = reader.Read();
            Projectile.rotation = reader.ReadSingle();
            Projectile.localAI[2] = reader.ReadSingle();
            Projectile.localAI[0] = reader.ReadSingle();

            reader.ReadFlags(out stuckInTarget, out thrown);
        }
        */
    }
}
