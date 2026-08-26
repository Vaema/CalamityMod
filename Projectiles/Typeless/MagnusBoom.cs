using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Typeless;

public class MagnusBoom : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public Player Owner => Main.player[Projectile.owner];
    public ref float Time => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 80;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 20;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        if (Time == 0f)
        {
            for (int i = 0; i < 5; i++)
            {
                Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Magenta, Color.LightPink, Utils.GetLerpValue(0, 5, i, true)), "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.09f + 0.005f * i, (int)(20 - i * 1.5f));
                GeneralParticleHandler.SpawnParticle(explosion);
            }
            for (int i = 0; i < 3; i++)
            {
                Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.Magenta, Color.LightPink, Utils.GetLerpValue(0, 3, i, true)), "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.09f + 0.01f * i, (int)(20 - i * 2f));
                GeneralParticleHandler.SpawnParticle(explosion);
            }

            Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.PowderBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.1f, 2f, 24, true);
            GeneralParticleHandler.SpawnParticle(outerGlow);
            Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.05f, 1f, 24, true);
            GeneralParticleHandler.SpawnParticle(innerGlow);

            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2();
                Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 12, 0.4f, Color.Magenta, new Vector2(1f, 0.1f), true);
                GeneralParticleHandler.SpawnParticle(cross);
            }
        }

        Projectile.scale = MathHelper.Lerp(0f, 1f, PiecewiseAnimation(Time / 20f, new CurveSegment[] { new(EasingType.PolyOut, 0f, 0f, 1f, 4) }));
        if (Time < 10f)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(71, 73 + 1), Main.rand.NextVector2CircularEdge(6f, 6f) * (Main.rand.NextFloat(1f, 1.2f) + Projectile.scale));
                dust.noGravity = true;
                dust.noLight = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f) + Projectile.scale;
                dust.alpha = Main.rand.Next(120, 180 + 1);
            }
        }

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale, targetHitbox);
}


