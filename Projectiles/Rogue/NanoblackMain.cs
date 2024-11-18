using System.IO;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackMain : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/NanoblackReaper";

        internal const int UpdatesPerFrame = 3;
        private const int Lifetime = 240;
        private static int InternalLifetime => Lifetime * UpdatesPerFrame;
        private const float BoomerangReturnTime = 16f;

        private const int BaseTesselationDelay = 6;
        private static int InternalTesselationDelay => BaseTesselationDelay * UpdatesPerFrame;
        private const float TesselationSpawnSpeed = 24f;

        internal const float RotationIncrement = 0.22f;

        private Player Owner => Main.player[Projectile.owner];
        internal ref float RealFrameCounter => ref Projectile.ai[0];
        internal ref float TesselationSpawnCooldown => ref Projectile.ai[1];
        internal ref float LightspeedCarveState => ref Projectile.ai[2];
        internal bool Returning
        {
            get => Projectile.localAI[0] != 0f;
            set => Projectile.localAI[0] = (value ? 1f : 0f);
        }

        public override void SetStaticDefaults()
        {
            // Nanoblack Reaper does not spin exactly on the center of its sprite.
            DrawOffsetX = -11;
            DrawOriginOffsetY = -4;
            DrawOriginOffsetX = 0;

            ProjectileID.Sets.TrailCacheLength[Type] = 7;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.MaxUpdates = UpdatesPerFrame;
            Projectile.timeLeft = InternalLifetime;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6 * UpdatesPerFrame;

            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Returning);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bool r = reader.ReadBoolean();
            Returning = r;
        }

        // Nanoblack Reaper's AI has been converted into a trenchcoat function due to the needed expansion of the sub-functions.
        public override void AI()
        {
            if (Projectile.timeLeft == InternalLifetime)
                FrameOneEffects();

            InFlightVisualEffects();
            UpdateAIVariables();

            // On the frame the scythe begins returning, send a net update.
            if (RealFrameCounter >= BoomerangReturnTime && RealFrameCounter < BoomerangReturnTime + 1f)
            {
                Returning = true;
                Projectile.netUpdate = true;
            }

            // The scythe runs its returning AI if the frame counter is greater than ReboundTime.
            if (Returning)
                BoomerangMovement();

            // Spawn Nanoblack Tesselations at a consistent and overwhelming rate while in flight.
            if (TesselationSpawnCooldown <= 0f)
            {
                SpawnTesselation();
                TesselationSpawnCooldown = InternalTesselationDelay;
            }

            RotateScytheInFlight();
        }

        private void FrameOneEffects()
        {
            // If you set these values, you were a fool. Nanoblack Reaper does not care.
            RealFrameCounter = 0f;
            TesselationSpawnCooldown = InternalTesselationDelay;
            LightspeedCarveState = 0f;
        }

        // Produces electricity and green firework sparks constantly while in flight.
        private void InFlightVisualEffects()
        {
            if (!Main.rand.NextBool(UpdatesPerFrame))
                return;

            int dustType = Main.rand.NextBool(5) ? DustID.Electric : 220 /* no DustID entry */;

            Vector2 position = Main.rand.NextVector2FromRectangle(Projectile.Hitbox);
            float scale = Main.rand.NextFloat(0.8f, 1.1f);
            float velocityMult = Main.rand.NextFloat(0.3f, 0.6f);

            Dust d = Dust.NewDustPerfect(position, dustType, Vector2.Zero, Scale: scale);
            if (d is null || d.dustIndex == Main.maxDust)
                return;

            d.noGravity = true;
            d.velocity = velocityMult * Projectile.velocity;
        }

        private void UpdateAIVariables()
        {
            // Only increment the real frame counter once per frame, on the final extra update of that frame.
            if (Projectile.FinalExtraUpdate())
            {
                ++RealFrameCounter;
            }

            // Tesselation spawn cooldown decrements every update so that it may be out of sync with gameplay frames if needed.
            --TesselationSpawnCooldown;
        }

        private void BoomerangMovement()
        {
            Player owner = Owner;
            Vector2 toOwner = Projectile.SafeDirectionTo(owner.Center, -Vector2.UnitY);
            float baseReturnSpeed = NanoblackReaper.Speed;
            float currentReturnSpeed = baseReturnSpeed;

            // Nanoblack Reaper's return speed increases sharply if it remains in flight for too long.
            float returnSpeedIncreaseTime = BoomerangReturnTime * 2f;
            if (RealFrameCounter >= returnSpeedIncreaseTime)
                currentReturnSpeed *= 1f + 0.05f * (RealFrameCounter - returnSpeedIncreaseTime);

            // Lerp into the desired velocity every update.
            Vector2 desiredVelocity = currentReturnSpeed * toOwner;
            float returnSharpness = 0.04f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, returnSharpness);

            // Delete the projectile if it touches its owner.
            if (Main.myPlayer == Projectile.owner)
                if (Projectile.Hitbox.Intersects(owner.Hitbox))
                    Projectile.Kill();
        }

        // Spawns an individual Nanoblack Tesselation.
        // Tesselations emit from the blade of the scythe and fly directly away, rapidly coming to a halt.
        private void SpawnTesselation()
        {
            // Each tesselation randomly chooses which of its four zero-point energy strikes to fire first.
            // For consistent RNG across clients, this randomness is executed even if the result is not used.
            float zeroPointStrikeIndex = Main.rand.Next(4); // 0f, 1f, 2f or 3f

            if (Main.myPlayer != Projectile.owner)
                return;

            int tessID = ModContent.ProjectileType<NanoblackTesselation>();
            int tessDamage = (int)(NanoblackReaper.TesselationDamageRatio * Projectile.damage);
            float tessKB = 1.5f;

            // The blade of Nanoblack Reaper is close enough to straight-right +X that using the rotation directly is fine.
            float scytheBladeRotation = Projectile.rotation;
            Vector2 spawnOffsetDir = scytheBladeRotation.ToRotationVector2();
            Vector2 tessPos = Projectile.Center + spawnOffsetDir * 14f;
            Vector2 tessVelDir = spawnOffsetDir.RotatedBy(-MathHelper.PiOver4); // close enough to a blade-egress vector
            Vector2 tessVel = tessVelDir * TesselationSpawnSpeed;

            var source = Projectile.GetSource_FromThis();
            int tessIdx = Projectile.NewProjectile(source, tessPos, tessVel, tessID, tessDamage, tessKB, Projectile.owner, ai1: zeroPointStrikeIndex);

            // The spin direction of the scythe transfers to the tesselations.
            if (tessIdx.WithinBounds(Main.maxProjectiles))
                Main.projectile[tessIdx].direction = Projectile.direction;
        }

        private void RotateScytheInFlight()
        {
            float spin = Projectile.direction <= 0 ? -1f : 1f;
            Projectile.rotation += spin * RotationIncrement;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }
    }
}
