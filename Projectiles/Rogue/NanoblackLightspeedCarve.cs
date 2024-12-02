using System;
using System.IO;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    // Adopts a lot of code from Last Prism (aka Yharim's Crystal) beams for visuals,
    // but delivers damage completely differently.
    public class NanoblackLightspeedCarve : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override LocalizedText DisplayName => this.GetLocalization(IsPerfect ? "Perfect" : "Standard");

        internal const float MaxBeamLength = 1800f;
        private const float BeamTileCollisionWidth = 1f;
        private const float BeamHitboxCollisionWidth = 40;
        // Sample points is much higher due to this beam only being cast once instead of every frame.
        private const int NumSamplePoints = 9;
        // no BeamLengthChangeFactor due to this beam never moving; it only gets cast once

        private const float BeamLightBrightness = 0.5f;

        internal bool IsPerfect => Projectile.ai[0] == 1f;
        internal Vector2 TargetPos => new(Projectile.ai[1], Projectile.ai[2]);
        internal ref float VisibleCarveLength => ref Projectile.localAI[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 0;
            // Lightspeed Carves completely ignore tiles and are not stopped by them, unlike Last Prism.
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 12;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(VisibleCarveLength);
        public override void ReceiveExtraAI(BinaryReader reader) => VisibleCarveLength = reader.ReadSingle();

        public override void AI()
        {
            // This projectile does not move and its velocity is always zero.
            // Destination X/Y is stored in AI variables and is computed into a faux velocity.
            Vector2 fakeVel = (TargetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY);
            
            // Lightspeed Carves ignore tiles but still perform a Last Prism laser scan to disturb water and cut tiles.
            Vector2 samplingPoint = Projectile.Center;
            float[] laserScanResults = new float[NumSamplePoints];
            Collision.LaserScan(samplingPoint, fakeVel, BeamTileCollisionWidth, MaxBeamLength, laserScanResults);

            float averageSampledLength = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
                averageSampledLength += laserScanResults[i];
            averageSampledLength /= NumSamplePoints;
            // There is no lerping of visible beam length because the beam is only sampled once.
            VisibleCarveLength = averageSampledLength;

            // X = beam length. Y = beam width.
            Vector2 beamDims = new(averageSampledLength, Projectile.width * Projectile.scale);

            // Cause visual effects based on computed data
            CarveEffects();

            // If the game is rendering (i.e. isn't a dedicated server), make the beam disturb water.
            if (Main.netMode != NetmodeID.Server)
            {
                WaterShaderData wsd = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();
                // A universal time-based sinusoid which updates extremely rapidly. GlobalTimeWrappedHourly is 0 to 3600, measured in seconds.
                float waveSine = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
                Vector2 ripplePos = Projectile.position + new Vector2(beamDims.X * 0.5f, 0f).RotatedBy(Projectile.rotation);
                // WaveData is encoded as a Color. Not sure why, considering Vector3 exists.
                Color waveData = new Color(0.5f, 0.1f * Math.Sign(waveSine) + 0.5f, 0f, 1f) * Math.Abs(waveSine);
                wsd.QueueRipple(ripplePos, waveData, beamDims, RippleShape.Square, Projectile.rotation);
            }

            // Make the beam cast light along its length.
            // v3_1 is an unnamed decompiled variable which is the color of the light cast by DelegateMethods.CastLight
            Vector2 lightDest = Projectile.Center + fakeVel * averageSampledLength;
            DelegateMethods.v3_1 = NanoblackReaper.LightspeedCarveColor.ToVector3() * BeamLightBrightness;
            Utils.PlotTileLine(Projectile.Center, lightDest, beamDims.Y, DelegateMethods.CastLight);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host crystal), that's good enough.
            if (projHitbox.Intersects(targetHitbox))
                return true;
            // Otherwise, perform an AABB line collision check to check the whole beam.
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], BeamHitboxCollisionWidth * Projectile.scale, ref _);
        }

        private void CarveEffects()
        {

        }
    }
}
