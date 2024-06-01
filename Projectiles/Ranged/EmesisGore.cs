using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
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
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (setStats)
            {
                rotDirection = (Main.rand.NextBool() ? -1 : 1);
                Projectile.scale = Main.rand.NextFloat(0.75f, 1.2f);
                setStats = false;
            }
            Projectile.rotation += 0.009f * rotDirection * Projectile.scale * Utils.GetLerpValue(0, 300, Projectile.timeLeft);
            Projectile.velocity *= 0.995f;
            Projectile.alpha = (int)(Utils.Remap(Projectile.timeLeft, 70, 0, 0, 255, true));
            if (Projectile.timeLeft > 70)
            {
                if (Projectile.timeLeft % 2 == 0)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center - Projectile.velocity * 2f, -Projectile.velocity * 0.05f, false, 7, Projectile.scale, Color.Lime * 0.135f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }
    }
}
