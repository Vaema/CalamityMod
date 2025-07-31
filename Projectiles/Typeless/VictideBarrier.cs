using CalamityMod.Cooldowns;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideBarrier : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float ExplodeTimer => ref Projectile.ai[0];
        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
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
            if (ExplodeTimer > 0f)
            {
                if (ExplodeTimer == 1f)
                {
                    Particle boom = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Cyan * 0.8f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.12f, 24, true);
                    GeneralParticleHandler.SpawnParticle(boom);

                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 velocity = (MathHelper.TwoPi * i / 5f).ToRotationVector2() * Main.rand.NextFloat(10f, 12f) * (i % 2 == 0 ? 0.88f : 1f);
                        Particle bub = new VelChangingSpark(Projectile.Center, velocity.RotatedByRandom(MathHelper.Pi / 10f), Vector2.UnitY * -6f, "CalamityMod/Particles/BloomRing", Main.rand.Next(36, 42 + 1), Main.rand.NextFloat(0.1f, 0.25f), Main.rand.NextBool(3) ? Color.HotPink : Color.Turquoise, Vector2.One, shrinkSpeed: 0.08f);
                        GeneralParticleHandler.SpawnParticle(bub);
                    }
                }

                if (ExplodeTimer == 6f)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(2.5f, 5.5f);
                        float smokeScale = Main.rand.NextFloat(1.6f, 2f);
                        Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVelocity, Color.PaleTurquoise, 18, smokeScale, 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }

                ExplodeTimer++;
            }
            else
            {
                // If the barrier set isn't on, unceremoniously disappear
                if (!Owner.Calamity().victideBarrierSet)
                {
                    Projectile.Kill();
                    return;
                }
                // Set to explode if cooldown is detected
                if (Owner.HasCooldown(WardingWave.ID))
                {
                    ExplodeTimer++;
                    Projectile.damage = (int)Owner.GetBestClassDamage().ApplyTo(VictideHeadBarrier.BarrierExplosionDamage);
                }

                Projectile.Center = Owner.MountedCenter;
                Projectile.timeLeft = 20;
            }
        }

        public override bool? CanDamage() => ExplodeTimer > 0f ? null : false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 112f, targetHitbox);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();
            modifiers.SourceDamage *= Utils.Remap(Projectile.numHits, 0, 10, 1f, 0.1f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Barrier soonTM
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
