using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonHoldout : BaseGunHoldoutProjectile
    {
        #region Fields & Properties

        private ref float Timer => ref Projectile.ai[0];

        private bool HasShotBeam
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value == true ? 1f : 0f;
        }

        private enum AIState { Level0, Level1, Level2, Level3 };

        private const float TimePerCharge = 180f;
        private float ChargeLV1 => TimePerCharge;
        private float ChargeLV2 => TimePerCharge * 2;
        private float ChargeLV3 => TimePerCharge * 3;

        private AIState State => Timer switch
        {
            >= TimePerCharge * 3 => AIState.Level3,
            >= TimePerCharge * 2 => AIState.Level2,
            >= TimePerCharge => AIState.Level1,
            _ => AIState.Level0,
        };

        private const int CoolingDownTime = 120;

        #region Overriden Base Holdout Members

        public override int AssociatedItemID => ItemType<TauCannon>();
        public override Vector2 GunTipPosition => base.GunTipPosition - Vector2.UnitY.RotatedBy(Projectile.rotation) * 3f * Projectile.spriteDirection;
        public override float WeaponTurnSpeed => Utils.Remap(Timer, 0f, TimePerCharge * 3f, base.WeaponTurnSpeed, 0.01f, true);
        public override float MaxOffsetLengthFromArm => 30f;
        public override float OffsetXUpwards => -15f;
        public override float OffsetXDownwards => 5f;
        public override float BaseOffsetY => -15f;
        public override float OffsetYUpwards => 10f;
        public override float OffsetYDownwards => 15f;

        #endregion

        #region SoundStyles & SlotsIDs

        public static readonly SoundStyle ChargeLV1Sound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV1") { Volume = 0.6f };
        public static readonly SoundStyle ChargeLV2Sound = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeLV2") { Volume = 0.6f };
        public static readonly SoundStyle OrbSound = new();
        public static readonly SoundStyle BoltShootSound = new("CalamityMod/Sounds/Custom/ExoMechs/ExoLaserShoot") { Volume = 0.1f, PitchVariance = 0.3f };
        public static readonly SoundStyle SmallBeamSound = new("CalamityMod/Sounds/Custom/AstrumDeus/AstrumDeusMine") { Volume = 0.2f, PitchVariance = 0.1f };
        public static readonly SoundStyle BigBeamSound = new();
        public static readonly SoundStyle CoolingDownSound = new("CalamityMod/Sounds/Custom/ExoMechs/ThanatosVent") { Volume = 0.025f };
        private SlotId OrbSoundSlot;
        private SlotId CoolingDownSoundSlot;

        #endregion 

        #endregion

        public override void KillHoldoutLogic()
        {
            // If the holdout hasn't reached any charge and the player's isn't holding the weapon anymore: kill it.
            if (State == AIState.Level0 && Owner.CantUseHoldout())
                Projectile.Kill();
        }

        public override void HoldoutAI()
        {
            
            if (Owner.CantUseHoldout() && KeepRefreshingLifetime == true)
            {
                KeepRefreshingLifetime = false;
                Projectile.timeLeft = State switch
                {
                    AIState.Level1 => 108 + CoolingDownTime + 1,
                    AIState.Level2 => 30 + CoolingDownTime + 1,
                    AIState.Level3 => 180 + CoolingDownTime + 1,
                    _ => 0,
                };
            }

            if (KeepRefreshingLifetime == false && Main.myPlayer == Projectile.owner)
            {
                switch (State)
                {
                    case AIState.Level1:

                        if (Projectile.timeLeft % 6 == 0 && Projectile.timeLeft > CoolingDownTime)
                        {
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                                GunTipPosition,
                                Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.5f) * HeldItem.shootSpeed,
                                ProjectileType<TauCannonBolt>(),
                                Projectile.damage,
                                Projectile.knockBack,
                                Projectile.owner);
                            OffsetLengthFromArm -= 5f;
                            SoundEngine.PlaySound(BoltShootSound, GunTipPosition);
                        }

                        break;

                    case AIState.Level2:

                        if (!HasShotBeam)
                        {
                            Projectile.NewProjectileDirect(
                                Projectile.GetSource_FromThis(),
                                GunTipPosition,
                                Projectile.velocity,
                                ProjectileType<TauCannonBeam>(),
                                Projectile.damage,
                                Projectile.knockBack,
                                Projectile.owner,
                                ai1: Projectile.whoAmI);
                            HasShotBeam = true;
                            OffsetLengthFromArm -= 25f;
                            SoundEngine.PlaySound(SmallBeamSound, GunTipPosition);
                        }

                        break;

                    case AIState.Level3:

                        if (!HasShotBeam)
                        {
                            if (Projectile.owner == Main.myPlayer)
                            {
                                Projectile.NewProjectileDirect(
                                    Projectile.GetSource_FromThis(),
                                    GunTipPosition + Projectile.velocity * 25f,
                                    Projectile.velocity,
                                    ProjectileType<TauCannonBeam>(),
                                    Projectile.damage,
                                    Projectile.knockBack,
                                    Projectile.owner,
                                    ai1: Projectile.whoAmI,
                                    ai2: 1f);

                                int randomPortalAmount = Main.rand.Next(4, 7);
                                for (int i = 0; i < randomPortalAmount; i++)
                                {
                                    Projectile.NewProjectileDirect(
                                        Projectile.GetSource_FromThis(),
                                        Owner.Center + Main.rand.NextVector2CircularEdge(480f, 480f) * Main.rand.NextFloat(0.8f, 1.2f),
                                        Vector2.Zero,
                                        ProjectileType<TauCannonPortal>(),
                                        Projectile.damage,
                                        Projectile.knockBack,
                                        Projectile.owner);
                                }
                            }

                            HasShotBeam = true;
                        }

                        if (Projectile.timeLeft > CoolingDownTime)
                        {
                            if (Projectile.timeLeft % 5f == 0f)
                            {
                                Particle ring = new DirectionalPulseRing(GunTipPosition - Projectile.velocity * 10f, Projectile.velocity * 5f, Color.Pink, new Vector2(0.5f, 1f), Projectile.velocity.ToRotation(), 0.1f, 0.6f, 10);
                                GeneralParticleHandler.SpawnParticle(ring);
                            }

                            if (Main.rand.NextBool(2))
                            {
                                Particle smoke = new HeavySmokeParticle(GunTipPosition, Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(1f, 3f), Color.Pink, 10, Main.rand.NextFloat(0.6f, 1.2f), 0.5f);
                                GeneralParticleHandler.SpawnParticle(smoke);
                            }
                        }

                        break;
                }
            }

            if (Timer == ChargeLV1 || Timer == ChargeLV2 || Timer == ChargeLV3)
                PerLevelChargeEffect(State);

            if (Projectile.timeLeft < CoolingDownTime && KeepRefreshingLifetime == false && Timer > TimePerCharge)
            {
                if (Main.rand.NextBool(8))
                {
                    Particle heat = new MediumMistParticle(
                        Projectile.Center - Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.4f,
                        -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(5f, 12f),
                        Color.DarkGray,
                        Color.Transparent,
                        Main.rand.NextFloat(0.6f, 0.9f),
                        Main.rand.NextFloat(300f, 400f));
                    GeneralParticleHandler.SpawnParticle(heat);
                }

                if (!SoundEngine.TryGetActiveSound(CoolingDownSoundSlot, out var sound))
                    CoolingDownSoundSlot = SoundEngine.PlaySound(CoolingDownSound with { Volume = 0.5f }, GunTipPosition);
                else
                    sound.Position = Projectile.Center;
            }

            if (KeepRefreshingLifetime == true)
            {
                if (Main.rand.NextBool(5))
                {
                    float randomRadius = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 5f, 15f);
                    Dust orbDust = Dust.NewDustPerfect(
                        GunTipPosition + Main.rand.NextVector2Circular(randomRadius, randomRadius),
                        72,
                        -Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(1f, 3f) * Utils.Remap(Timer, 0f, TimePerCharge * 3f, 1f, 5f),
                        Scale: Main.rand.NextFloat(0.4f, 1.3f));
                    orbDust.noGravity = true;
                    orbDust.noLight = true;
                    orbDust.noLightEmittence = true;
                }

                Particle orb = new GenericBloom(GunTipPosition, Projectile.velocity, Color.Fuchsia, Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0f, 1f), 2, false);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new GenericBloom(GunTipPosition, Projectile.velocity, Color.White, Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0f, 0.8f), 2, false);
                GeneralParticleHandler.SpawnParticle(orb2);

                float distanceFromTip = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 25f, 300f);
                float sizeIncrease = Utils.Remap(Timer, 0f, TimePerCharge * 3f, 0.05f, 0.4f);
                Particle lineCharge = new ManaDrainStreak(Owner, sizeIncrease, Main.rand.NextVector2Circular(distanceFromTip, distanceFromTip), Main.rand.NextFloat(5f, 10f), Color.White, Color.Fuchsia, Main.rand.Next(10, 21), GunTipPosition);
                GeneralParticleHandler.SpawnParticle(lineCharge);

                if (!SoundEngine.TryGetActiveSound(OrbSoundSlot, out var sound))
                    OrbSoundSlot = SoundEngine.PlaySound(OrbSound with { Volume = 0.5f }, GunTipPosition);
                else
                    sound.Position = Projectile.Center;
            }

            if (KeepRefreshingLifetime == true)
                Timer++;
        }

        private void PerLevelChargeEffect(AIState state)
        {
            int dustAmount = state switch
            {
                AIState.Level3 => 36,
                AIState.Level2 => 24,
                AIState.Level1 => 12,
            };

            for (int i = 0; i < dustAmount; i++)
            {
                float angle = MathHelper.TwoPi / dustAmount * i;
                Vector2 velocity = angle.ToRotationVector2() * 8f;
                Dust chargeDust = Dust.NewDustPerfect(GunTipPosition, 72, velocity, Scale: Utils.Remap(dustAmount, 10f, 30f, 1.5f, 2.2f));
                chargeDust.noGravity = true;
                chargeDust.noLight = true;
                chargeDust.noLightEmittence = true;
            }

            Particle chargeRing = new DirectionalPulseRing(
                GunTipPosition,
                Vector2.Zero,
                Color.Fuchsia,
                Vector2.One,
                0f,
                1f,
                0f,
                30);
            GeneralParticleHandler.SpawnParticle(chargeRing);

            SoundStyle chargeSound = state switch
            {
                AIState.Level3 => ChargeLV2Sound,
                AIState.Level2 => ChargeLV1Sound,
                AIState.Level1 => ChargeLV1Sound,
            };

            SoundEngine.PlaySound(chargeSound, GunTipPosition);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;
            Texture2D glowTexture = Request<Texture2D>(GlowTexture, AssetRequestMode.AsyncLoad).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color glowDrawColor = Color.Lerp(Color.Black, Color.White, Utils.GetLerpValue(0f, TimePerCharge * 3f, Timer, true));
            float drawRotation = Projectile.rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);
            Vector2 rotationPoint = texture.Size() * 0.5f;
            SpriteEffects flipSprite = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (Projectile.timeLeft > CoolingDownTime || KeepRefreshingLifetime == true)
            {
                float shake = KeepRefreshingLifetime ? Utils.Remap(Timer, TimePerCharge, TimePerCharge * 3f, 0f, 4f) : 3f;
                drawPosition += Main.rand.NextVector2Circular(shake, shake);
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, glowDrawColor, drawRotation, rotationPoint, Projectile.scale, flipSprite);

            return false;
        }
    }
}
