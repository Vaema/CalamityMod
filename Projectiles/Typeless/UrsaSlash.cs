using System;
using CalamityMod.Dusts;
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
    public class UrsaSlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float radius = 50f;
        public bool visuals = true;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)radius;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (visuals && Owner.Calamity().ursaSergeantVisual)
            {
                Vector2 slashDir = new Vector2(13, 13).RotatedByRandom(100);
                Vector2 slashPos1 = Projectile.Center + slashDir.RotatedBy(MathHelper.ToRadians(90f));
                Vector2 slashPos2 = Projectile.Center + slashDir.RotatedBy(MathHelper.ToRadians(-90f));

                for (int i = 0; i < 3; i++)
                {
                    Particle bigSpark = new GlowSparkParticle(Projectile.Center - slashDir * 6, slashDir * 0.65f, false, 19, 0.07f * (1 - i * 0.25f), Color.Coral, new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(bigSpark);
                    Particle spark1 = new GlowSparkParticle(slashPos1 - slashDir * 6, slashDir * 0.65f, false, 19, 0.052f * (1 - i * 0.25f), Color.DarkTurquoise, new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark1);
                    Particle spark2 = new GlowSparkParticle(slashPos2 - slashDir * 6, slashDir * 0.65f, false, 19, 0.052f * (1 - i * 0.25f), Color.DarkTurquoise, new Vector2(1.9f, 1), true, false, 0.7f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int i = 0; i <= 9; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - slashDir * 6, dustStyle);
                    dust.scale = Main.rand.NextFloat(0.6f, 1.2f);
                    dust.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust.noGravity = true;
                    dust.color = Color.Coral;

                    Dust dust2 = Dust.NewDustPerfect(slashPos1 - slashDir * 6, dustStyle);
                    dust2.scale = Main.rand.NextFloat(0.6f, 1.2f);
                    dust2.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust2.noGravity = true;
                    dust2.color = Color.DarkTurquoise;

                    Dust dust3 = Dust.NewDustPerfect(slashPos2 - slashDir * 6, dustStyle);
                    dust3.scale = Main.rand.NextFloat(0.6f, 1.2f);
                    dust3.velocity = slashDir.RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 2);
                    dust3.noGravity = true;
                    dust3.color = Color.DarkTurquoise;
                }

                SoundStyle sound = new("CalamityMod/Sounds/Item/AstralSlash", 3);
                SoundEngine.PlaySound(sound with { Volume = 0.7f }, Projectile.Center);
                SoundStyle sound2 = new("CalamityMod/Sounds/NPCHit/PerfLargeHit", 3);
                SoundEngine.PlaySound(sound2 with { Volume = 1f, Pitch = Main.rand.NextFloat(0.4f, 0.5f) }, Projectile.Center);
            }
            visuals = false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Buffs.DamageOverTime.AstralInfectionDebuff>(), 300);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.7f);
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox);
        public override bool? CanDamage() => base.CanDamage();
        public override bool? CanCutTiles() => false;
    }
}
