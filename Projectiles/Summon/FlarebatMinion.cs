using System;
using System.IO;
using CalamityMod.Buffs.Summon;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Summon
{
    public class FlarebatMinion : BaseMinionProjectile
    {
        public override int AssociatedProjectileTypeID => ProjectileType<FlarebatMinion>();

        public override int AssociatedBuffTypeID => BuffType<FlarebatBuff>();

        public override ref bool AssociatedMinionBool => ref ModdedOwner.FlarebatBool;

        public override bool PreHardmodeMinionTileVision => true;

        public override int AnimationFrames => 8;

        private Vector2 IdlePosition => Owner.MountedCenter +
            -Vector2.UnitY.RotatedBy(MathHelper.ToRadians(15f) * (Owner.ownedProjectileCounts[Type] + Owner.ownedProjectileCounts[ProjectileType<FrostbatMinion>()] - 1) - MathHelper.ToRadians(30f) * MinionIndex) * 80f;

        private Action CurrentBehavior;

        [Flags]
        private enum AttackBehaviorFlags : byte
        {
            None = 0,
            HasDashed = 1,
            HasHitTargetOnDash = 2,
            IsReturningToOwner = 4,
        }

        private AttackBehaviorFlags AttackState;

        public int MinionIndex;

        public override void SetDefaults()
        {
            base.SetDefaults();
            (Projectile.width, Projectile.height) = (32, 28);
        }

        #region AI

        public override void MinionAI()
        {
            if (Projectile.ai[0] == 0f)
            {
                CurrentBehavior = IdleBehavior;
                FramesUntilNextAnimationFrame = 3;

                int minionIndex = 0;
                foreach (var proj in Main.ActiveProjectiles)
                {
                    if ((proj.type != Type && proj.type != ProjectileType<FrostbatMinion>()) || proj.owner != Projectile.owner)
                        continue;

                    if (proj.type == Type)
                        MinionIndex = minionIndex;
                    else
                        proj.ModProjectile<FrostbatMinion>().MinionIndex = minionIndex;

                    minionIndex++;
                }

                Projectile.ai[0]++;
            }

            // Teleport to the owner if abnormally far away
            if (!Projectile.WithinRange(Owner.Center, 2000f))
            {
                Projectile.position = Owner.Center;
                Projectile.velocity *= 0.3f;
                Projectile.netUpdate = true;
            }

            CurrentBehavior.Invoke();
            Projectile.rotation = Projectile.rotation.AngleTowards(MathHelper.ToRadians(Projectile.velocity.X * 3f), 0.2f);
            Projectile.spriteDirection = MathF.Sign(Projectile.velocity.X);

            if (!Main.dedServ)
            {
                if (CurrentBehavior == AttackBehavior || (CurrentBehavior == IdleBehavior && Main.rand.NextBool(10)))
                {
                    Dust ambientDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
                    ambientDust.noGravity = Main.rand.NextBool();
                    ambientDust.noLight = true;
                    ambientDust.noLightEmittence = true;
                }

                Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.5f);
            }
        }

        private void IdleBehavior()
        {
            if (Target is not null)
            {
                CurrentBehavior = AttackBehavior;
                Projectile.velocity = Projectile.DirectionTo(Target.Center).RotatedByRandom(MathHelper.PiOver2 * 0.7f) * 10f;
                return;
            }

            Projectile.velocity = Projectile.DirectionTo(IdlePosition) * Utils.Remap(Projectile.DistanceSQ(IdlePosition), 25600f, 0f, 6f, 0f);
            Projectile.MinionAntiClump();
        }

        private void AttackBehavior()
        {
            if (Target is null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            // The first stage is redirecting towards the target.
            if (!AttackState.HasFlag(AttackBehaviorFlags.HasDashed) && Vector2.Dot(Projectile.DirectionTo(Target.Center), Projectile.velocity.SafeNormalize(-Vector2.UnitY)) < 0.96f)
            {
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(Target.Center).ToRotation(), 0.095f).ToRotationVector2() *
                    Utils.Remap(Projectile.DistanceSQ(Target.Center), 6400f, 0f, 14f, 0f);
            }

            // When it's whithin a distnce and it hasn't dashed yet, it'll dash.
            else if (!AttackState.HasFlag(AttackBehaviorFlags.HasDashed) && Projectile.DistanceSQ(Target.Center) < 57600f)
            {
                Projectile.velocity = Projectile.DirectionTo(Target.Center) * 19f;
                AttackState |= AttackBehaviorFlags.HasDashed;
                Projectile.netUpdate = true;

                if (!Main.dedServ)
                {
                    Particle dashRing = new DirectionalPulseRing(
                        Projectile.Center,
                        Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 5f,
                        Color.OrangeRed,
                        new Vector2(0.5f, 1f),
                        Projectile.velocity.ToRotation(),
                        0.04f,
                        0.4f,
                        30);
                    GeneralParticleHandler.SpawnParticle(dashRing);

                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, PitchVariance = 0.1f, Volume = 0.6f }, Projectile.Center);
                }
            }

            // The second stage is after the dash.
            // If it has hit the target is was supposed to hit or he missed:
            // Return to the owner.
            if (AttackState.HasFlag(AttackBehaviorFlags.HasDashed) && !AttackState.HasFlag(AttackBehaviorFlags.IsReturningToOwner))
            {
                if (Vector2.Dot(Projectile.DirectionTo(Target.Center), Projectile.velocity.SafeNormalize(-Vector2.UnitY)) < 0f || AttackState.HasFlag(AttackBehaviorFlags.HasHitTargetOnDash))
                {
                    AttackState |= AttackBehaviorFlags.IsReturningToOwner;
                    Projectile.netUpdate = true;
                }
            }

            // The minion will redirect towards its idle position.
            // When it has arrived to it at a distance, reset the state and start over.
            if (AttackState.HasFlag(AttackBehaviorFlags.IsReturningToOwner))
            {
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.DirectionTo(IdlePosition).ToRotation(), 0.095f).ToRotationVector2() *
                    Utils.Remap(Projectile.DistanceSQ(IdlePosition), 6400f, 0f, 14f, 0f);

                if (Projectile.DistanceSQ(IdlePosition) < 6400f)
                {
                    AttackState = AttackBehaviorFlags.None;
                    Projectile.netUpdate = true;
                }
            }
        }

        public override bool MinionContactDamage() => true;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (AttackState.HasFlag(AttackBehaviorFlags.HasDashed) && !AttackState.HasFlag(AttackBehaviorFlags.HasHitTargetOnDash) && Target == target)
                AttackState |= AttackBehaviorFlags.HasHitTargetOnDash;

            target.AddBuff(BuffID.OnFire, 60);
        }

        #endregion

        #region Drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White * 0.6f,
                Projectile.rotation,
                frame.Size() * 0.5f,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }

        #endregion

        #region Syncing

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(MinionIndex);
            writer.Write((byte)AttackState);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            MinionIndex = reader.Read7BitEncodedInt();
            AttackState = (AttackBehaviorFlags)reader.ReadByte();
        }

        #endregion
    }
}
