using System;
using System.IO;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

// Adopts a lot of code from Last Prism (aka Yharim's Crystal) for beam calculations,
// but delivers damage completely differently.
public class NanoblackPiercingStrike : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    internal const float MaxBeamLength = 1800f;
    private const float BeamTileCollisionWidth = 1f;
    private const float BeamHitboxCollisionWidth = 40;
    // Sample points is much higher due to this beam only being cast once instead of every frame.
    private const int NumSamplePoints = 9;
    // no BeamLengthChangeFactor due to this beam never moving; it only gets cast once

    private const float BeamLightBrightness = 0.5f;

    internal Vector2 TargetPos => new(Projectile.ai[1], Projectile.ai[2]);
    internal ref float VisibleBeamLength => ref Projectile.localAI[0];

    // 0f = Beam not calculated yet
    // 1f = Beam calculated but no damage has been done
    // 2f = This projectile has had at least 1 frame of active hitbox and has hit N enemies, it no longer does damage
    internal ref float ProgressiveState => ref Projectile.localAI[1];
    internal const float ProgressiveState_Initial = 0f;
    internal const float ProgressiveState_BeamCalculated = 1f;
    internal const float ProgressiveState_HasDealtDamage = 2f;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 36;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.penetrate = 1;
        Projectile.extraUpdates = 0;
        // Completely ignores tiles and is not stopped by them, unlike Last Prism.
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.timeLeft = 6;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(VisibleBeamLength);
    public override void ReceiveExtraAI(BinaryReader reader) => VisibleBeamLength = reader.ReadSingle();

    public override void AI()
    {
        if (Projectile.numHits > 0)
            ProgressiveState = ProgressiveState_HasDealtDamage;
        
        // This projectile does not move and its velocity is always zero.
        // Destination X/Y is stored in AI variables and is computed into a faux velocity.
        Vector2 fakeVel = (TargetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY);

        // Supercharged Nanoblack Strikes ignore tiles but still perform a Last Prism laser scan to disturb water and cut tiles.
        if (ProgressiveState == ProgressiveState_Initial)
        {
            Vector2 samplingPoint = Projectile.Center;
            float[] laserScanResults = new float[NumSamplePoints];
            Collision.LaserScan(samplingPoint, fakeVel, BeamTileCollisionWidth, MaxBeamLength, laserScanResults);

            float averageSampledLength = 0f;
            for (int i = 0; i < laserScanResults.Length; ++i)
                averageSampledLength += laserScanResults[i];
            averageSampledLength /= NumSamplePoints;
            // There is no lerping of visible beam length because the beam is only sampled once.
            VisibleBeamLength = averageSampledLength;

            float distToTarget = Projectile.Center.Distance(TargetPos);
            float minOvershoot = 224f;
            float maxOvershoot = 320f;

            // If the beam is too short (it got stuck in a wall), force it to be an appropriate length. This item ignores walls.
            // Similarly, if the beam is far too long, clip it down so it doesn't go ridiculously far past its target.
            if (VisibleBeamLength < distToTarget || VisibleBeamLength > distToTarget + maxOvershoot)
            {
                float randomOvershoot = Main.rand.NextFloat(minOvershoot, maxOvershoot);
                VisibleBeamLength = distToTarget + randomOvershoot;
            }

            ProgressiveState = ProgressiveState_BeamCalculated;
        }

        // X = beam length. Y = beam width.
        Vector2 beamDims = new(VisibleBeamLength, Projectile.width * Projectile.scale);

        // Cause visual effects based on computed data
        PiercingStrikeEffects();

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
        Vector2 lightDest = Projectile.Center + fakeVel * VisibleBeamLength;
        DelegateMethods.v3_1 = NanoblackReaper.PiercingStrikeColor.ToVector3() * BeamLightBrightness;
        Utils.PlotTileLine(Projectile.Center, lightDest, beamDims.Y, DelegateMethods.CastLight);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host crystal), that's good enough.
        if (projHitbox.Intersects(targetHitbox))
            return true;

        // Otherwise, perform an AABB line collision check to check the whole beam.
        float _ = float.NaN;
        Vector2 fakeVel = (TargetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY);
        Vector2 endPoint = Projectile.Center + fakeVel * VisibleBeamLength;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, endPoint, BeamHitboxCollisionWidth * Projectile.scale, ref _);
    }

    public override bool? CanHitNPC(NPC target) => ProgressiveState == ProgressiveState_HasDealtDamage ? false : null;

    // This projectile does not die upon landing a hit so that it may persist for visual reasons.
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.penetrate++;

    private void PiercingStrikeEffects()
    {
        Vector2 fakeVel = (TargetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY);
        Vector2 endPoint = Projectile.Center + fakeVel * VisibleBeamLength;

        // 0 to 5
        int iter = 5 - Projectile.timeLeft;

        float startLerp = iter * 0.15f;
        float endLerp = 0.6f + iter * 0.15f;
        Vector2 segmentStart = Vector2.Lerp(Projectile.Center, endPoint, startLerp);
        Vector2 segmentEnd = Vector2.Lerp(Projectile.Center, endPoint, endLerp);
        float particleSpeed = Main.rand.NextFloat(34f, 40f);
        Vector2 particleVel = fakeVel.RotatedByRandom(MathHelper.Pi / 30f) * particleSpeed;

        int lifetime = 5;
        float xScale = 0.017f;
        float xShrink = 0.66f;

        Particle carveParticle = new StaticGlowLine(segmentStart, segmentEnd, particleVel, lifetime, xScale, xShrink, NanoblackReaper.PiercingStrikeColor, true);
        GeneralParticleHandler.SpawnParticle(carveParticle);
    }
}
