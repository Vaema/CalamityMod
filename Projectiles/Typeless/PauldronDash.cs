using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class PauldronDash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float ExplosionRadius = 75f;

        public override void SetDefaults()
        {
            //These shouldn't matter because its circular
            Projectile.width = 75;
            Projectile.height = 75;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 4;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 22;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            target.AddBuff(ModContent.BuffType<Buffs.StatDebuffs.ArmorCrunch>(), 300);

            SoundStyle sound = new("CalamityMod/Sounds/Item/HolyColliderProjectileHit");
            SoundEngine.PlaySound(sound with { Volume = 0.4f, Pitch = 0.15f }, Projectile.Center);
            SoundStyle sound2 = new("CalamityMod/Sounds/Item/MagicRockImpact");
            SoundEngine.PlaySound(sound2 with { Volume = 0.6f, Pitch = 0.35f }, Projectile.Center);

            for (int i = 0; i <= 12; i++)
            {
                float variance = Main.rand.NextFloat(-0.7f, 0.7f);
                int dustStyle = ModContent.DustType<LightDust>();
                Dust dust2 = Dust.NewDustPerfect(target.Center, dustStyle, Projectile.velocity);
                dust2.scale = (Main.rand.NextFloat(1.1f, 1.3f) - Math.Abs(variance)) * 2.5f;
                dust2.velocity = (Vector2.UnitY * -18).RotatedBy(variance) * Main.rand.NextFloat(0.7f, 1f) * (1 - Math.Abs(variance) * 1.3f);
                dust2.color = Main.rand.NextBool() ? Color.Orange : Color.OrangeRed;
                dust2.noGravity = false;

                Particle spark = new SparkParticle(Projectile.Center, new Vector2(17, 17).RotatedByRandom(100) * Main.rand.NextFloat(0.4f, 1f), true, 55, 0.85f, Main.rand.NextBool() ? Color.Orange : Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            float baseRot = Main.rand.NextFloat(-9f, 9f);
            for (int i = 0; i < 5; i++)
            {
                float rot = Main.rand.NextFloat(-0.3f, 0.3f);
                for (int b = 0; b < 2; b++)
                {
                    Vector2 pulseVel = new Vector2(0, Main.rand.NextFloat(-7, -9)).RotatedBy(i * (MathHelper.ToRadians(360f) / 5)).RotatedBy(rot + baseRot + 0.9f);
                    Particle orb = new CustomPulse(target.Center, pulseVel, (Main.rand.NextBool() ? Color.Orange : Color.OrangeRed) * 0.9f, "CalamityMod/Projectiles/Summon/RustyBeaconPulse", Vector2.One, pulseVel.ToRotation(), 0.2f, Main.rand.NextFloat(0.55f, 0.85f) * 3f, Main.rand.Next(14, 19 + 1));
                    GeneralParticleHandler.SpawnParticle(orb);
                }
                Particle orb2 = new CustomSpark(target.Center, new Vector2(0, -7).RotatedBy(i * (MathHelper.ToRadians(360f) / 5)).RotatedBy(rot + baseRot), "CalamityMod/Particles/BloomLineFade", false, 13, 0.095f, Main.rand.NextBool() ? Color.Orange : Color.OrangeRed, new Vector2(2.9f, 0.5f), shrinkSpeed: 0.9f);
                GeneralParticleHandler.SpawnParticle(orb2);
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PauldronExplosion>(), Projectile.damage / 2, 0, Projectile.owner);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
        public override bool? CanDamage() => base.CanDamage();
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Owner.Calamity().GeneralScreenShakePower = 4f;
            for (int i = 0; i < 2; i++)
            {
                Particle bloom = new CustomSpark(target.Center, Vector2.Zero, "CalamityMod/Particles/BloomCircle", false, 20, 1.2f, Color.OrangeRed, new Vector2(1, 1), true, true, glowCenterScale: 0.7f, glowOpacity: 0.8f);
                GeneralParticleHandler.SpawnParticle(bloom);
            }
        }
        public override bool? CanCutTiles() => false;
    }
}
