using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.DataStructures;
using CalamityMod.Dusts;
using CalamityMod.Effects;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Packets.Entities;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace CalamityMod.Projectiles.Ranged
{
    public class AbyssalFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public Vector2[] ControlPoints = new Vector2[32];
        public Player Owner => Main.player[Projectile.owner];
        public Projectile VoidragonHoldout => Main.projectile[(int)Projectile.ai[0]];
        public ref float LaserLength => ref Projectile.ai[1];

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public const float MaxLaserLength = 3330f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 75;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            // If the owner is no longer able to cast the beam, kill it.
            if (!Owner.channel || Owner.noItems || Owner.CCed || VoidragonHoldout is null)
            {
                Projectile.Kill();
                return;
            }

            // Set the control points for the primitive drawing.
            for (int i = 0; i < ControlPoints.Length; i++)
                ControlPoints[i] = Projectile.Center + Projectile.velocity * i / (ControlPoints.Length - 1f) * LaserLength;

            // Grow and shrink depending on how long left the laser has to remain active.
            int beamTimer = VoidragonHoldout.ModProjectile<VoidragonHoldout>().beamTimer;
            if (beamTimer <= 500 && beamTimer >= 425)
                Projectile.scale += 0.1f;
            else if (beamTimer <= 25 && beamTimer >= 0 && Projectile.scale > 0f)
                Projectile.scale -= 0.1f;
            Projectile.scale = MathHelper.Clamp(Projectile.scale, 0f, 2f);

            // Decide where to position the laserbeam.
            Vector2 circlePointDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction);
            Projectile.Center = VoidragonHoldout.ModProjectile<VoidragonHoldout>().GunTipPosition - ((Projectile.velocity * 55).RotatedBy(-0.02f * Projectile.direction));

            LaserLength = MaxLaserLength;

            // Update aim.
            UpdateAim();

            // Spawn a bunch of particles along the length of the laser.
            if (Projectile.scale >= 0.25f)
            {
                BezierCurve curve = new(ControlPoints);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 fireSpawnPosition = curve.Evaluate(Main.rand.NextFloat(0.05f, 1f));
                    Vector2 fireVelocity = Projectile.velocity + Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(10f, 15f);

                    Color fireColor = Color.Lerp(Color.DarkOrchid, Color.DarkMagenta, Main.rand.NextFloat());
                    int fireLifetime = Main.rand.Next(45, 60);
                    float fireScale = Main.rand.NextFloat(0.45f, 1f) * Projectile.scale;
                    float fireOpacity = Main.rand.NextFloat(0.65f, 0.95f);

                    HeavySmokeParticle abyssalFlames = new(fireSpawnPosition, fireVelocity, fireColor, fireLifetime, fireScale, fireOpacity, 0.03f, true);
                    GeneralParticleHandler.SpawnParticle(abyssalFlames);
                }

                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkSpawnPosition = curve.Evaluate(Main.rand.NextFloat(0.05f, 1f));
                    Vector2 sparkVelocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2 * Main.rand.NextBool().ToDirectionInt()) * Main.rand.NextFloat(9f, 18f) * Main.rand.NextVector2Circular(1f, 1f);

                    Dust dust = Dust.NewDustDirect(sparkSpawnPosition, 1, 1, ModContent.DustType<LightDust>());
                    dust.noGravity = true;
                    dust.velocity = sparkVelocity;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f) * Projectile.scale;
                    dust.color = Main.rand.NextBool() ? Color.MediumOrchid : Color.BlueViolet;
                    dust.noLightEmittence = true;
                }
            }

            // Shake the screen slightly.
            if (Owner.Calamity().GeneralScreenShakePower < 1.15f)
                Owner.Calamity().GeneralScreenShakePower = MathHelper.Lerp(0f, 1.15f, Projectile.scale / 2f);

            // Make the beam cast light along its length. The brightness of the light is reliant on the scale of the beam.
            DelegateMethods.v3_1 = Color.DarkViolet.ToVector3() * Projectile.scale * 0.4f;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Projectile.width * Projectile.scale, DelegateMethods.CastLight);
        }

        public void UpdateAim()
        {
            // Only execute the aiming code for the owner.
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 newAimDirection = VoidragonHoldout.velocity.SafeNormalize(Vector2.UnitY);

            // Sync if the direction is different from the old one.
            // Spam caps are ignored due to the frequency of this happening.
            if (newAimDirection != Projectile.velocity)
            {
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            Projectile.velocity = newAimDirection;
        }
        private float PrimitiveWidthFunction(float completionRatio)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 75f;
            float shrinkRatio = 0.275f;

            if (completionRatio < shrinkRatio)
                width = MathF.Sin(completionRatio / shrinkRatio * MathHelper.PiOver2) * maxBodyWidth + shrinkRatio;
            else
                width = Utils.Remap(completionRatio, shrinkRatio, 1f, maxBodyWidth, 0f);

            return width;
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            Color vibrantColor = Color.Lerp(Color.Indigo, Color.MediumPurple, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.67f - completionRatio / LaserLength * 29f) * 0.5f + 0.5f);          
            float opacity = Projectile.Opacity * Utils.GetLerpValue(0.97f, 0.9f, completionRatio, true) *
                Utils.GetLerpValue(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true) *
                MathF.Pow(Utils.GetLerpValue(60f, 270f, LaserLength, true), 3f);

            return Color.Lerp(vibrantColor, Color.Black, 0.5f) * opacity * 2f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> mainStreakTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2");
            Asset<Texture2D> secondaryStreakTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2");

            MiscShaderData shader = GameShaders.Misc["CalamityMod:AbyssalFire"];
            shader.Shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            shader.Shader.Parameters["secondaryColor"].SetValue(Color.Lerp(Color.MediumPurple, Color.White, 0.65f).ToVector4());
            shader.SetShaderTexture(mainStreakTexture);
            Main.instance.GraphicsDevice.Textures[2] = secondaryStreakTexture.Value;

            PrimitiveRenderer.RenderTrail(ControlPoints, new(PrimitiveWidthFunction, PrimitiveColorFunction, shader: shader), ControlPoints.Length + 12);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overWiresUI.Add(index);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player Owner = Main.player[Projectile.owner];
            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 180);
            int bonusDamage = 200 * Owner.Calamity().sharkGunDamageScaling;
            if (target.Calamity().demonicFlamesBonusDamage <= bonusDamage)
            {
                target.Calamity().demonicFlamesBonusDamage = bonusDamage;
                target.AddBuff(ModContent.BuffType<DemonicFlames>(), 180);
                // Demonic Flames damage must be synced, because OnHitNPC is only run for the client that hit the NPC
                if (Main.netMode != NetmodeID.SinglePlayer)
                    DemonicFlamesSyncPacket.Send(target);
            }
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
