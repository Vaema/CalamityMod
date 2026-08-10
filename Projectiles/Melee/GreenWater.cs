using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee;

public class GreenWater : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Melee";

    private const int TimeLeft = 300;
    public Vector2 storedVel;

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.alpha = 255;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = 1;
        Projectile.extraUpdates = 2;
        Projectile.timeLeft = TimeLeft;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        Player Owner = Main.player[Projectile.owner];

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        if (Projectile.ai[0] == 0)
        {
            Projectile.ArmorPenetration = 40;
            if (Projectile.timeLeft == TimeLeft)
            {
                storedVel = Projectile.velocity;
                Projectile.velocity = -Projectile.velocity;
            }
            if (Projectile.timeLeft > TimeLeft - 60)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation = storedVel.ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                if (Projectile.timeLeft == TimeLeft - 60)
                    Projectile.velocity = storedVel;

                NPC target = Owner.Calamity().mouseWorld.ClosestNPCAt(500);
                CalamityUtils.HomeInOnSelectedNPC(Projectile, target, false, 0.3f, 10, 0.98f);

                if (Main.rand.NextBool(5))
                {
                    Particle spark = new GlowOrbParticle(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.6f), true, 13, Main.rand.NextFloat(0.55f, 0.8f), Color.DarkRed * 0.8f, false, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }
        else if (Projectile.ai[0] == 1)
        {
            NPC target = Projectile.Center.ClosestNPCAt(155);
            CalamityUtils.HomeInOnSelectedNPC(Projectile, target, false, 0.15f, 9, 0.98f, accelerate: true);
        }
        else
        {
            Projectile.scale = 1.2f;
            // Spawn in a helix-style pattern
            float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 8f;

            float fade = Utils.GetLerpValue(255, 0, Projectile.alpha);
            SparkParticle orb = new(Projectile.Center + offset, -Projectile.velocity * 0.05f, false, 7, 0.7f, Color.Aqua * 0.6f * fade);
            GeneralParticleHandler.SpawnParticle(orb);

            SparkParticle orb2 = new(Projectile.Center - offset, -Projectile.velocity * 0.05f, false, 7, 0.7f, Color.Aqua * 0.6f * fade);
            GeneralParticleHandler.SpawnParticle(orb2);

            Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), DustID.Water_Desert, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 9).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 180, default, Main.rand.NextFloat(0.8f, 1.4f));
            dust.noGravity = true;
            dust.alpha = (int)MathHelper.Clamp(Projectile.alpha, 0, 180);

            if (Projectile.timeLeft > 130)
            {
                Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 150, 130, 255, 0));
            }
        }

        if (Main.rand.NextBool())
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 3 + Main.rand.NextVector2Circular(6, 6), DustID.Blood, (-Projectile.velocity.SafeNormalize(Vector2.UnitX) * 4).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.8f), 100, default, Main.rand.NextFloat(0.8f, 1.4f));
            dust.noGravity = true;
            dust.alpha = (int)MathHelper.Clamp(Projectile.alpha, 0, 100);
        }
        if (Projectile.timeLeft <= 60)
        {
            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 0, 60, 255, 0));
        }
        if (Projectile.timeLeft > TimeLeft - 20)
        {
            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, TimeLeft, TimeLeft - 20, 255, 0));
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.ai[0] == 2) // Water tooth
        {
            target.AddBuff(BuffID.Wet, 300);
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 300);
        }
        if (Projectile.ai[0] == 0) // "Jaw" teeth
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 180);
        }
        for (int i = 0; i <= 4; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, (Projectile.velocity * 2.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(0.9f, 1.8f));
            dust.noGravity = false;
        }
        for (int i = 0; i <= 2; i++)
        {
            Particle spark = new AltSparkParticle(Projectile.Center, (Projectile.velocity * 4.5f).RotatedByRandom(0.7) * Main.rand.NextFloat(0.1f, 0.8f) + new Vector2(0, -2), true, 20, 0.5f, Color.DarkRed * 0.7f);
            GeneralParticleHandler.SpawnParticle(spark);
        }
        SoundStyle sound = new("CalamityMod/Sounds/NPCHit/PerfSmallHit", 3);
        SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.2f, -0.3f) }, Projectile.Center);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        float minMult = 0.25f;
        int hitsToMinMult = 6;
        float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
        modifiers.SourceDamage *= damageMult;
    }
}
