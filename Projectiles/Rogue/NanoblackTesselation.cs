using System.IO;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackTesselation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        internal static Asset<Texture2D> Glow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Rogue/NanoblackTesselationGlow");

        // As Nanoblack Tesselations are not square, this is required for the glowmask to be rendered properly.
        private const int SpriteWidth = 52;

        private const int Lifetime = 60;
        private const int VanishTime = 12;

        private const float TargetingRange = 600f;
        private const float ZeroPointFiringRange = 2000f;
        private const int SuccessiveStrikeCooldown = 4;

        // Rotation speed is inherited directly from the scythe.
        private const float StartingRotationIncrement = NanoblackMain.RotationIncrement * NanoblackMain.UpdatesPerFrame;
        private const float DriftSpeed = 0.8f;

        private Player Owner => Main.player[Projectile.owner];

        // Tesselations do not re-target unless their current target is out of their extensive range or is dead.
        internal ref float TargetIndexPlusOne => ref Projectile.ai[0];
        internal ref float ZeroPointStrikeIndex => ref Projectile.ai[1];
        // 0f = Spindown, 1f = Zero-Point Strikes, 2f = Vanishing
        internal ref float OverallState => ref Projectile.ai[2];
        internal ref float ZeroPointStrikeDelay => ref Projectile.localAI[0];
        internal ref float CurrentSpin => ref Projectile.localAI[1];

        // Tesselations can fire a maximum of four zero-point energy strikes.
        private int zeroPointStrikesPerformed = 0;

        public override void SetStaticDefaults()
        {
            // Nanoblack Tesselations are not perfectly square and need assistance to spin properly.
            DrawOffsetX = -10;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            // Spin is deterministic and not necessary to sync
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            float lai0 = reader.ReadSingle();
            Projectile.localAI[0] = lai0;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == Lifetime)
                FrameOneEffects();

            UpdateOverallState();
            ProcessSpin();

            // Tesselations always screech to a near-halt during spindown, but drift gently afterwards to form a mesmerizing pattern.
            if (OverallState == 0f)
                Projectile.velocity *= 0.75f;

            if (OverallState == 1f)
            {
                // If the Tesselation cannot attack, then it immediately transitions to vanishing.
                bool shutdown = PerformZeroPointStrikesIfAble();
                if (shutdown)
                    Projectile.timeLeft = VanishTime - 1;
            }

            // If the tesselation is vanishing, shrink by 12% every frame
            if (OverallState == 2f)
                Projectile.scale *= 0.88f;
        }

        private void FrameOneEffects()
        {
            TargetIndexPlusOne = 0f; // no target
            OverallState = 0f; // Spindown
            ZeroPointStrikeDelay = 0f; // can fire instantly when primed
            CurrentSpin = StartingRotationIncrement;

            // Dust is produced to visually flavor the spawning of the projectile.
            SpawnDust();
        }

        private void UpdateOverallState()
        {
            // Tesselations enable their owner's mouse listener so that the mouse state is synced.
            // This is necessary for their targeting algorithm.
            Owner.Calamity().mouseWorldListener = true;
            
            // When there is little time left, tesselations vanish even if they could still fire.
            if (OverallState == 1f && Projectile.timeLeft < VanishTime)
                OverallState = 2f;

            // Otherwise, tesselations are primed to fire once they are not moving quickly.
            else if (OverallState == 0f && Projectile.velocity.LengthSquared() < DriftSpeed * DriftSpeed)
            {
                // TODO -- visual effects on first frame of being armed
                OverallState = 1f;
            }
        }

        private void ProcessSpin()
        {
            // Visually spins the tesselation.
            float rotationIncrement = CurrentSpin * Projectile.direction;
            Projectile.rotation += rotationIncrement;

            // Spin slows down exponentially over time.
            CurrentSpin *= 0.93f;

            // Always update current orientation to reflect current spin direction
            Projectile.spriteDirection = Projectile.direction;
        }

        // Returns whether or not the Tesselation cannot attack and should shut down.
        private bool PerformZeroPointStrikesIfAble()
        {
            // If the maximum number of zero-point strikes has been delivered, shut down the Tesselation.
            if (zeroPointStrikesPerformed >= 4)
                return true;
            
            // Reduce the cooldown of the zero-point strikes, if applicable.
            if (ZeroPointStrikeDelay > 0f)
            {
                --ZeroPointStrikeDelay;
                return false;
            }
            
            // Get the index of the NPC to target.
            int targetIdx = EvaluateTargeting();

            // If there's no target, shut down the Tesselation.
            if (targetIdx == -2)
                return true;

            // At this point, the cooldown and targeting are confirmed.
            EmitZeroPointEnergyStrike(targetIdx);
            return false;
        }

        // As is Nanoblack tradition, Tesselations prefer to target bosses whenever possible.
        // Returns the index of the current target for convenience.
        // If it returns -2, targeting has failed and the Tesselation will shut down.
        private int EvaluateTargeting()
        {
            // Check whether the current target is valid, alive, and in firing range.
            // If so, do nothing.
            int currentTargetIdx = (int)(TargetIndexPlusOne - 1);
            if (currentTargetIdx.WithinBounds(Main.maxNPCs))
            {
                NPC target = Main.npc[currentTargetIdx];
                if (target is not null && target.active && target.DistanceSQ(Projectile.Center) <= ZeroPointFiringRange * ZeroPointFiringRange)
                    return currentTargetIdx;
            }

            // Otherwise, choose a new target based on the player's current cursor position.
            NPC newTarget = Owner.ClampedMouseWorld().ClosestNPCAt(TargetingRange, bossPriority: true);
            if (newTarget is not null && newTarget.active)
            {
                // Store the target index.
                TargetIndexPlusOne = newTarget.whoAmI + 1;
                return newTarget.whoAmI;
            }

            return -2;
        }

        private void EmitZeroPointEnergyStrike(int targetIdx)
        {
            // Exact positioning of the zero-point energy strike is randomly chosen.
            // For consistent RNG across clients, this randomness is executed even if the result is not used.
            float xInterp = Main.rand.NextFloat();
            float yInterp = Main.rand.NextFloat();

            if (Main.myPlayer != Projectile.owner)
                return;

            NPC target = Main.npc[targetIdx];
            Vector2 c = target.Center;

            float dartboardScale = 0.4f; // 0.5f would be the entire hitbox of the NPC
            Vector2 topLeft = c - dartboardScale * target.Size;
            Vector2 bottomRight = c + dartboardScale * target.Size;

            Vector2 directStrikeDest = new(MathHelper.Lerp(topLeft.X, bottomRight.X, xInterp), MathHelper.Lerp(topLeft.Y, bottomRight.Y, yInterp));

            // zero-point energy strikes should probably be their own subclass of DirectStrike but not yet
            int zpeID = ModContent.ProjectileType<DirectStrike>();
            int zpeDamage = Projectile.damage; // same ratio as the tesselation itself
            float zpeKB = 0f;
            var source = Projectile.GetSource_FromThis();
            int zpeIdx = Projectile.NewProjectile(source, directStrikeDest, Vector2.Zero, zpeID, zpeDamage, zpeKB, Projectile.owner, targetIdx);
            if (zpeIdx.WithinBounds(Main.maxProjectiles))
            {
                Projectile zpe = Main.projectile[zpeIdx];
                zpe.ArmorPenetration += NanoblackReaper.ZeroPointArmorPenetration;
            }

            // TODO -- visuals for zero-point energy strikes. This is where the visual strike index is used
            Dust.NewDustPerfect(directStrikeDest, 173);

            ++zeroPointStrikesPerformed;

            // Set the cooldown between strikes.
            ZeroPointStrikeDelay = SuccessiveStrikeCooldown + 6;

            // Swap visual strike index. If it overflows, reset it back to zero.
            ZeroPointStrikeIndex += 1f;
            if (ZeroPointStrikeIndex > 3f)
                ZeroPointStrikeIndex = 0f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            return false;
        }

        // Draws the tesselation's glowmask.
        public override void PostDraw(Color lightColor)
        {
            float fWidthOverTwo = SpriteWidth / 2f;
            float fHeightOverTwo = Projectile.height / 2f;

            // Make sure the glowmask matches the tesselation's own orientation
            SpriteEffects eff = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                eff = SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(fWidthOverTwo, fHeightOverTwo);
            Main.EntitySpriteDraw(Glow.Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, eff, 0);
        }

        // Spawns a tiny bit of dust when the tesselation vanishes.
        public override void OnKill(int timeLeft)
        {
            SpawnDust();
        }

        // Spawns a small bit of Luminite themed dust.
        private void SpawnDust()
        {
            int dustCount = Main.rand.Next(3, 6);
            Vector2 corner = Projectile.position;
            for (int i = 0; i < dustCount; ++i)
            {
                int dustType = 229;
                float scale = 0.6f + Main.rand.NextFloat(0.4f);
                int idx = Dust.NewDust(corner, Projectile.width, Projectile.height, dustType);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].velocity *= 3f;
                Main.dust[idx].scale = scale;
            }
        }
    }
}
