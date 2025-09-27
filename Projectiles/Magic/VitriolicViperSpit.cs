using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class VitriolicViperSpit : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public int direction = 0;
        public Vector2 lastPos;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 13;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.timeLeft = 300 * Projectile.MaxUpdates;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 4.5f * MathHelper.Clamp(Projectile.ai[2], 0.25f, 1f);

            if (time == 0)
            {
                direction = Main.rand.NextBool() ? 1 : -1;
                lastPos = Projectile.Center;
            }

            if (time > 3)
                Projectile.Center += offset * direction * Utils.GetLerpValue(3, 10, time, true);

            if (time > 3 && targetDist < 1400)
            {
                Particle spark = new GlowSparkParticle(Projectile.Center, Utils.DirectionTo(Projectile.Center, lastPos), false, (int)(15 * MathHelper.Clamp(Projectile.ai[2], 0.5f, 1f)), 0.07f * MathHelper.Clamp(Projectile.ai[2], 0.25f, 1f), Color.Chartreuse, new Vector2(1, 1.4f), false, true);
                GeneralParticleHandler.SpawnParticle(spark);

                if (Main.rand.NextBool((int)(MathHelper.Clamp((1 - Projectile.ai[2]) * 5, 1, 15))))
                {
                    int area = (int)(25 * MathHelper.Clamp(Projectile.ai[2], 0.5f, 1f));
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(area, area), (int)CalamityDusts.SulphurousSeaAcid);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                    dust.velocity = Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 0.7f);
                }
            }
            time++;
            lastPos = Projectile.Center;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 230);

            SoundStyle fire = new("CalamityMod/Sounds/NPCHit/NuclearTerrorHit");
            SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = 0.7f }, Projectile.Center);

            for (int i = 0; i < (MathHelper.Clamp(6 - Projectile.numHits * 2, 1, 10)); i++)
            {
                DirectionalPulseRing pulse = new DirectionalPulseRing(target.Center, Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.2f, 1), Main.rand.NextBool() ? Color.Chartreuse : Color.GreenYellow, new Vector2(1f, 1), 0, Main.rand.NextFloat(0.28f, 0.44f) * MathHelper.Clamp(Projectile.ai[2], 0.5f, 1f), 0f, 50);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.9f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 60 * MathHelper.Clamp(Projectile.ai[2], 0.25f, 1f), targetHitbox);
    }
}
