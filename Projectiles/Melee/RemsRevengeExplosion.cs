using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Melee
{
    public class RemsRevengeExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
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
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Time == 0f)
            {
                // Makeshift metal-blood explosion sound that is good enough ig
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/Ravager/RavagerStomp", 2) { Pitch = -0.75f, PitchVariance = 0.5f }, Projectile.Center);

                for (int i = 0; i < 5; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.DarkRed, Color.Red, Utils.GetLerpValue(0, 5, i, true)), "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.5f + 0.03f * i, (int)(20 - i * 1.5f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }
                for (int i = 0; i < 3; i++)
                {
                    Particle explosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Lerp(Color.DarkRed, Color.Red, Utils.GetLerpValue(0, 3, i, true)), "CalamityMod/Projectiles/FireProj", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 2f + 0.22f * i, (int)(20 - i * 2f));
                    GeneralParticleHandler.SpawnParticle(explosion);
                }

                Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.RosyBrown, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.1f, 2f, 24, true);
                GeneralParticleHandler.SpawnParticle(outerGlow);

                for (int p = 0; p < 16; p++)
                {
                    Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(8f, 12f);
                    float scale = Main.rand.NextFloat(0.6f, 2f);
                    Particle blood = new BloodParticle(Projectile.Center, velocity, 30, scale, Color.DarkRed);
                    GeneralParticleHandler.SpawnParticle(blood);
                }
            }

            Projectile.scale = MathHelper.Lerp(0f, 1f, PiecewiseAnimation(Time / 20f, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) }));
            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Laceration>(), 60);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale, targetHitbox);
    }
}
