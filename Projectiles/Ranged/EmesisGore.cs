using System;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class EmesisGore : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public bool setStats = true;
        public int rotDirection = 1;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 4;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (setStats)
            {
                rotDirection = (Main.rand.NextBool() ? -1 : 1);
                Projectile.scale = Main.rand.NextFloat(0.7f, 1f);
                Projectile.rotation = Main.rand.NextFloat(-20, 20);
                setStats = false;
            }
            Projectile.rotation += 0.01f * rotDirection * Projectile.scale * Utils.GetLerpValue(0, 600, Projectile.timeLeft);
            Projectile.velocity *= 0.9975f;
            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 70, 0, 0, 255, true));
            if (targetDist < 1400f && Projectile.timeLeft > 70)
            {
                if (Projectile.timeLeft % 6 == 0)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity + Main.rand.NextVector2Circular(20, 20), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), false, Main.rand.Next(9, 20 + 1), Projectile.scale * Main.rand.NextFloat(0.8f, 1.2f), Color.Chartreuse * Main.rand.NextFloat(0.15f, 0.5f));
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Main.rand.NextBool(6))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 75, -Projectile.velocity.RotatedByRandom(0.1) * Main.rand.NextFloat(0.1f, 0.3f), 0, default, Main.rand.NextFloat(0.5f, 1.2f));
                    dust.noGravity = true;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 420);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 420);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/OldDukeGore").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Color drawColor = Projectile.GetAlpha(lightColor);
            float drawRotation = Projectile.rotation;
            Vector2 rotationPoint = texture.Size() * 0.5f;

            
            Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, drawRotation, rotationPoint, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 45, targetHitbox);
    }
}
