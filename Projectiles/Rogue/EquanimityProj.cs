using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using CalamityMod.Particles;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Projectiles.Typeless;

namespace CalamityMod.Projectiles.Rogue;

public class EquanimityProj : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Items/Weapons/Rogue/Equanimity";

    public static int ChargeupTime = 10;
    public static int Lifetime = 500;
    public float OverallProgress => 1 - Projectile.timeLeft / (float)Lifetime;
    public float ThrowProgress => 1 - Projectile.timeLeft / (float)(Lifetime);
    public float ChargeProgress => 1 - (Projectile.timeLeft - Lifetime) / (float)(ChargeupTime);

    public Player Owner => Main.player[Projectile.owner];
    public ref float Returning => ref Projectile.ai[0];
    public ref float Bouncing => ref Projectile.ai[2];

    public bool swapType = false;

    public override void SetDefaults()
    {
        Projectile.width = 46;
        Projectile.height = 46;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Lifetime + ChargeupTime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
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

        if (Returning == 1f)
            return false;

        return base.CanDamage();
    }

    //Swing animation keys
    public CurveSegment pullback = new(EasingType.PolyOut, 0f, 0f, MathHelper.PiOver4 * -1.2f, 2);
    public CurveSegment throwout = new(EasingType.PolyOut, 0.7f, MathHelper.PiOver4 * -1.2f, MathHelper.PiOver4 * 1.2f + MathHelper.PiOver2, 3);
    internal float ArmAnticipationMovement() => PiecewiseAnimation(ChargeProgress, new CurveSegment[] { pullback, throwout });

    public override void AI()
    {
        //Anticipation animation. Make the player look like theyre holding the boomerang
        if (ChargeProgress < 1)
        {
            float armRotation = ArmAnticipationMovement() * Owner.direction;

            Owner.heldProj = Projectile.whoAmI;

            Projectile.Center = Owner.MountedCenter + Vector2.UnitY.RotatedBy(armRotation * Owner.gravDir) * -40f * Owner.gravDir;
            Projectile.rotation = (-MathHelper.PiOver2 + armRotation) * Owner.gravDir;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi + armRotation);

            Projectile.tileCollide = false;
            return;
        }

        //Play the throw sound when the throw ACTUALLY BEGINS.
        //Additionally, make the projectile collide and set its speed and velocity
        if (Projectile.timeLeft == Lifetime)
        {
            SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, Projectile.Center);
            Projectile.Center = Owner.MountedCenter + Projectile.velocity * 12f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 17.5f;
        }

        Projectile.rotation += (MathHelper.PiOver4 / 4f + MathHelper.PiOver4 / 2f * Math.Clamp(ThrowProgress * 2f, 0, 1)) * Math.Sign(Projectile.velocity.X);

        //Boomerang spinny sounds
        if (Projectile.soundDelay <= 0)
        {
            SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
            Projectile.soundDelay = 8;
        }

        if (Projectile.velocity.Length() < 2f && Bouncing == 0f)
        {
            Returning = 1f;
            Projectile.numHits = 0;
        }

        if (Returning == 0f && Bouncing == 0f && Projectile.velocity.Length() > 2f && Projectile.timeLeft < (455 + ChargeupTime))
        {
            Projectile.velocity *= 0.88f;
        }

        if (Returning == 1f && Projectile.velocity.Length() < 20f)
        {
            Projectile.velocity *= 1.1f;
        }

        for (int i = 0; i < 2; i++)
        {
            Vector2 dustPos = Projectile.Center + (i * MathHelper.Pi + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 14f;
            Dust dust = Dust.NewDustPerfect(dustPos, Main.rand.NextBool() ? 91 : 109, (i * MathHelper.Pi + Projectile.rotation * Math.Sign(Projectile.velocity.X)).ToRotationVector2() * 3f);
            dust.noGravity = true;
        }

        if (Returning == 1f)
        {
            //Aim back at the player
            Projectile.velocity = Projectile.velocity.Length() * (Owner.MountedCenter - Projectile.Center).SafeNormalize(Vector2.One);

            if ((Projectile.Center - Owner.MountedCenter).Length() < 24f)
            {
                Projectile.Kill();
            }

            if (Projectile.numHits >= 7)
            {
                Projectile.Kill();
            }
        }

    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.Confused, 30);
        //If a non stealth strike, spawn no visuals and the regular versions of the shards
        if (!Projectile.Calamity().stealthStrike)
        {
            float baseDirectionRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 shootVelocity = new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.6f);
            if (!swapType)
            {
                for (int s = 0; s < 2; s++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity.RotatedByRandom(100), ModContent.ProjectileType<EquanimityLightShard>(), (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
            }
            if (swapType)
            {
                for (int s = 0; s < 2; s++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity.RotatedByRandom(100), ModContent.ProjectileType<EquanimityDarkShard>(), (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);
            }
            swapType = !swapType;
        }
        //If stealth strike, spawn the explosion visuals and enhance shards
        else
        {
            float baseDirectionRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 shootVelocity = new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.6f);
            if (!swapType)
            {
                #region Visuals and Sounds
                SoundEngine.PlaySound(DeadSunsWind.Explosion with { Volume = 0.4f }, Projectile.Center);
                for (int i = 0; i < 3; i++)
                {
                    Particle innerglow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Indigo, "CalamityMod/Particles/LargeBloom", Vector2.One, Main.rand.NextFloat(-10, 10), 0, 0.3f, 15);
                    GeneralParticleHandler.SpawnParticle(innerglow);
                }
                Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/FlameExplosion2", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.04f, 25, false, 0.85f);
                GeneralParticleHandler.SpawnParticle(explosion);
                Particle explosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/FlameExplosion2", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.07f, 25, false, 0.85f);
                GeneralParticleHandler.SpawnParticle(explosion2);
                Particle explosion3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Indigo * 0.55f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0.07f, 0.08f, 30);
                GeneralParticleHandler.SpawnParticle(explosion3);
                for (int s = 0; s < 13; s++)
                {
                    Vector2 randVel = new Vector2(7, 7).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.6f);
                    Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, Color.Black, Main.rand.Next(15, 25 + 1), Main.rand.NextFloat(0.3f, 0.5f), 0.8f);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
                #endregion
                // Create Blast
                float blastSize = 90;
                float minMultiplier = 0.25f;
                int hitsToMinMult = 8;
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.DamageType = RogueDamageClass.Instance;
                for (int s = 0; s < 2; s++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity.RotatedByRandom(100), ModContent.ProjectileType<EquanimityLightShard>(), (int)(Projectile.damage * 0.8f), 0f, Projectile.owner, 1f, 1f);
            }
            if (swapType)
            {
                #region Visuals and Sounds
                SoundEngine.PlaySound(LunicEye.UseSound with { Volume = 0.7f }, Projectile.Center);
                for (int i = 0; i < 3; i++)
                {
                    Particle innerglow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightPink, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0, 0.3f, 15);
                    GeneralParticleHandler.SpawnParticle(innerglow);
                }
                Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.WhiteSmoke, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.07f, 20, true);
                GeneralParticleHandler.SpawnParticle(explosion);
                Particle explosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumVioletRed, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.07f, 20, true);
                GeneralParticleHandler.SpawnParticle(explosion2);
                Particle explosion4 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightPink, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.3f, 20, true);
                GeneralParticleHandler.SpawnParticle(explosion4);
                Particle explosion3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.LightPink, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.04f, 20);
                GeneralParticleHandler.SpawnParticle(explosion3);
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/HighResHollowCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.09f, 25);
                GeneralParticleHandler.SpawnParticle(blastRing2);
                for (int i = 0; i < 10; i++)
                {
                    CritSpark spark2 = new CritSpark(Projectile.Center, new Vector2(12, 12).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1f), Color.White, Color.Orchid, 0.9f, 20, 2f, 2.2f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int k = 0; k < 10; k++)
                {
                    Vector2 velocity = new Vector2(10, 10).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.2f);
                    float colorRando = Main.rand.NextFloat(0, 1);
                    Particle spark = new GlowSparkParticle(Projectile.Center + velocity, velocity, false, 11, Main.rand.NextFloat(0.009f, 0.005f), Color.Lerp(Color.DarkOrchid, Color.IndianRed, colorRando), new Vector2(2.2f, 0.9f), true);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                #endregion
                // Create Blast
                float blastSize = 90;
                float minMultiplier = 0.25f;
                int hitsToMinMult = 8;
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.DamageType = RogueDamageClass.Instance;
                for (int s = 0; s < 2; s++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVelocity.RotatedByRandom(100), ModContent.ProjectileType<EquanimityDarkShard>(), (int)(Projectile.damage * 0.8f), 0f, Projectile.owner, 1f, 1f);
            }
            swapType = !swapType;
        }

        if (Projectile.numHits > 4)
        {
            Projectile.velocity *= 0.3f;
            Returning = 1f;
        }

        else
        {
            //Retarget
            NPC newTarget = null;
            float closestNPCDistance = 10000f;
            float targettingDistance = 900f;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.whoAmI == target.whoAmI)
                    continue;

                if (n.CanBeChasedBy(Projectile))
                {
                    float potentialNewDistance = (Projectile.Center - n.Center).Length();
                    if (potentialNewDistance < targettingDistance && potentialNewDistance < closestNPCDistance)
                    {
                        closestNPCDistance = potentialNewDistance;
                        newTarget = n;
                        Projectile.velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(Projectile.Center, newTarget, 30f, 3);
                    }
                }
            }

            if (newTarget == null)
            {
                Projectile.velocity *= 0.3f;
                Returning = 1f;
                return;
            }

            // The boomerang loses 15% damage per bounce.
            Projectile.damage = (int)(Projectile.damage * 0.85f);
            Bouncing = 2f;
        }
    }
}
