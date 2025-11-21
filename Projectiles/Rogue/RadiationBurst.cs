using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityMod.Projectiles.Rogue
{
    public class RadiationBurst : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public NPC targetedNPC;
        public int time = 0;
        public int boomTime = 50;

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = boomTime + 2;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (time < boomTime)
            {
                float fade = Utils.GetLerpValue(boomTime + 3, 0, time);
                float numberOfDusts = 2f;
                float rotFactor = 360f / numberOfDusts;
                for (int i = 0; i < numberOfDusts; i++)
                {
                    float rot = MathHelper.ToRadians(i * rotFactor);
                    Vector2 velOffset = CalamityUtils.RandomVelocity(100f, 70f, 250f, 0.04f);
                    velOffset *= Main.rand.NextFloat(25, 45) * fade;
                    Particle energy = new GlowOrbParticle(Projectile.Center + velOffset * 2.5f, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, false, (int)(14 - (5 * fade)), Main.rand.NextFloat(1.1f, 1.25f) - 0.5f * fade, Color.Chartreuse);
                    GeneralParticleHandler.SpawnParticle(energy);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + velOffset * 2.5f, 278, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                    dust.noGravity = true;
                    dust.color = Color.Chartreuse;
                }
            }
            if (time == boomTime && targetDist < 1400)
            {
                for (int i = 0; i <= 30; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 278 : (int)CalamityDusts.SulphurousSeaAcid, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(0.9f, 1.3f));
                    dust.noGravity = true;
                    if (dust.type == 278)
                    {
                        dust.noGravity = false;
                        dust.color = Color.Chartreuse;
                    }
                    else
                        dust.scale *= 1.5f;

                }
                for (int i = 0; i < 3; i++)
                {
                    Particle Smear = new CustomPulse(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 18, Projectile.velocity, Color.Chartreuse * 0.7f, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-5, 5), 0, 0.25f + i * 0.11f, 18 - i * 3);
                    GeneralParticleHandler.SpawnParticle(Smear);
                }
                Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Chartreuse, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 1.5f, 0.5f, 32);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 1f, 0.2f, 32);
                GeneralParticleHandler.SpawnParticle(orb2);

                Owner.SetScreenshake(3.5f);
            }
            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.75f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => time >= boomTime ? CalamityUtils.CircularHitboxCollision(Projectile.Center, 350, targetHitbox) : false;
    }
}
