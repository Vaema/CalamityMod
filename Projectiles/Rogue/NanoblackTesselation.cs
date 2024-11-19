using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
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
        internal static Asset<Texture2D> Glow;

        // As Nanoblack Tesselations are not square, this is required for the glowmask to be rendered properly.
        private const int SpriteWidth = 52;

        private const int Lifetime = 60;
        private const int VanishTime = 12;
        internal const int MinDelay = 15;
        internal const int MaxDelay = 45;

        private const float TargetingRange = 600f;
        private const float ZeroPointFiringRange = 2000f;

        // Rotation speed is inherited directly from the scythe.
        private const float StartingRotationIncrement = NanoblackMain.RotationIncrement * NanoblackMain.UpdatesPerFrame;
        private const float DriftSpeed = 0.8f;

        private Player Owner => Main.player[Projectile.owner];
        internal ref float ZeroPointStrikeDelay => ref Projectile.ai[0];
        internal ref float CurrentSpin => ref Projectile.ai[1];
        private bool IsVanishing => Projectile.timeLeft < VanishTime;

        public override void Load() => Glow = ModContent.Request<Texture2D>(Texture + "Glow");

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
            Projectile.scale = 0.5f;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == Lifetime)
                FrameOneEffects();

            // Tesselations enable their owner's mouse listener so that the mouse state is synced.
            // This is necessary for their targeting algorithm.
            Owner.Calamity().mouseWorldListener = true;

            // If the Tesselation cannot attack, then it immediately transitions to vanishing.
            bool shouldShutdown = false;
            if (!IsVanishing)
                shouldShutdown = AttemptZeroPointEnergyStrikeThisFrame();
            if (shouldShutdown)
                Projectile.timeLeft = VanishTime - 1;

            ProcessSpin();

            // Tesselations always screech to a near-halt during spindown, but drift gently afterwards to form a mesmerizing pattern.
            if (!IsVanishing && Projectile.velocity.LengthSquared() > DriftSpeed * DriftSpeed)
                Projectile.velocity *= 0.75f;

            // If the tesselation is vanishing, shrink by 12% every frame
            if (IsVanishing)
                Projectile.scale *= 0.88f;
        }

        private void FrameOneEffects()
        {
            // Sanity check the firing delay.
            if (ZeroPointStrikeDelay < MinDelay)
                ZeroPointStrikeDelay = MinDelay;
            else if (ZeroPointStrikeDelay > MaxDelay)
                ZeroPointStrikeDelay = MaxDelay;

            CurrentSpin = StartingRotationIncrement;

            // Create particles to visually flavor the spawning of the projectile.
            CreationDestructionVFX(false);
        }

        private void ProcessSpin()
        {
            // Visually spins the tesselation.
            float rotationIncrement = CurrentSpin * Projectile.spriteDirection;
            Projectile.rotation += rotationIncrement;

            // Spin slows down exponentially over time.
            CurrentSpin *= 0.93f;
        }

        // Returns whether or not the Tesselation should shut down.
        private bool AttemptZeroPointEnergyStrikeThisFrame()
        {
            // If the attack isn't ready yet, don't perform it.
            if (ZeroPointStrikeDelay > 0f)
            {
                --ZeroPointStrikeDelay;
                return false;
            }

            // As is Nanoblack tradition, Tesselations prefer to target bosses whenever possible.
            bool inFiringRange = false;
            NPC target = Owner.ClampedMouseWorld().ClosestNPCAt(TargetingRange, bossPriority: true);

            // If the first cursor-based targeting attempt fails, try again near the Tesselation itself.
            if (target is null || !target.active)
                target = Projectile.Center.ClosestNPCAt(TargetingRange, bossPriority: true);

            // Check firing range. Tesselations can fire across a whole 1080p screen, but it's not infinite distance.
            if (target is not null && target.active)
                inFiringRange = target.DistanceSQ(Projectile.Center) < ZeroPointFiringRange * ZeroPointFiringRange;
            if (!inFiringRange)
                return true;

            // At this point, the targeting is confirmed. Emit the strike.
            EmitZeroPointEnergyStrike(target);
            return true;
        }

        private void EmitZeroPointEnergyStrike(NPC target)
        {
            // Exact visual offset of the zero-point energy strike is randomly chosen.
            // For consistent RNG across clients, this randomness is executed even if the result is not used.
            float xInterp = Main.rand.NextFloat();
            float yInterp = Main.rand.NextFloat();

            if (Main.myPlayer != Projectile.owner)
                return;

            // The "dartboard" is the majority, but not all, of the NPC's hitbox.
            Vector2 c = target.Center;
            float dartboardScale = 0.4f; // 0.5f would be the entire hitbox of the NPC
            Vector2 topLeft = c - dartboardScale * target.Size;
            Vector2 bottomRight = c + dartboardScale * target.Size;
            float dartboardX = MathHelper.Lerp(topLeft.X, bottomRight.X, xInterp);
            float dartboardY = MathHelper.Lerp(topLeft.Y, bottomRight.Y, yInterp);
            Vector2 strikeDest = new(dartboardX, dartboardY);
            Vector2 offset = strikeDest - c;

            int zpeID = ModContent.ProjectileType<NanoblackStrike>();
            int zpeDamage = Projectile.damage; // same damage ratio as the tesselation itself
            float zpeKB = 0f;

            var source = Projectile.GetSource_FromThis();
            int zpeIdx = Projectile.NewProjectile(source, strikeDest, Vector2.Zero, zpeID, zpeDamage, zpeKB, Projectile.owner, ai0: target.whoAmI, ai1: offset.X, ai2: offset.Y);
            if (zpeIdx.WithinBounds(Main.maxProjectiles))
            {
                Projectile zpe = Main.projectile[zpeIdx];
                zpe.ArmorPenetration += NanoblackReaper.ZeroPointArmorPenetration; // Add excessive armor penetration.

                // This consistently orients the visuals of the hitscan attack for flair.
                zpe.direction = zpe.spriteDirection = Projectile.spriteDirection;
            }

            // Draw a bright line of energy between the Tesselation and the spawned strike.
            Vector2 lineVel = 3f * Projectile.velocity;
            float xScale = 0.009f;
            float xShrink = 0.88f;
            Color lineColor = NanoblackReaper.ZeroPointLineColor;
            Particle energyLine = new StaticGlowLine(Projectile.Center, strikeDest, lineVel, 7, xScale, xShrink, lineColor, true);
            GeneralParticleHandler.SpawnParticle(energyLine);

            // Draw four stacked glow orbs right at the start of the line.
            // One glow orb was not glowy enough.
            float orbScale = 1.5f;
            for (int i = 0; i < 4; ++i)
            {
                Particle energyOrb = new GlowOrbParticle(Projectile.Center, lineVel, false, 15, orbScale, lineColor);
                GeneralParticleHandler.SpawnParticle(energyOrb);
            }
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

        public override void OnKill(int timeLeft) => CreationDestructionVFX(true);

        private void CreationDestructionVFX(bool killed = false)
        {
            float sparkSpeed = 2f;
            float baseRot = MathHelper.PiOver2;
            float scale = 0.018f;
            Color color = NanoblackReaper.TesselationParticleColor;
            for (int i = 0; i < 6; ++i)
            {
                float rot = baseRot + i * NanoblackReaper.PiOver3;
                Vector2 sparkVel = sparkSpeed * rot.ToRotationVector2();
                Vector2 squashStretch = new(1f, 0.3f);
                Particle p = new GlowSparkParticle(Projectile.Center, sparkVel, killed, 15, scale, color, squashStretch, true, true, 1f);
            }
        }
    }
}
