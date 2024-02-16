using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class NeedlerProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 2;
            Projectile.aiStyle = ProjAIStyleID.Nail;
            AIType = ProjectileID.NailFriendly;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 1;
        }

        public override void AI()
        {
            Projectile.alpha -= 10;
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            Projectile.localAI[1] += 1f;
            if (Projectile.localAI[1] > 6f && Projectile.numHits < 1)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 207 : 256, -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f));
                dust.scale = Main.rand.NextFloat(0.6f, 0.9f);
                dust.noGravity = true;
            }
            if (Projectile.localAI[1] == 4f)
            {
                for (int i = 0; i <= 8; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 40 : 207, (Projectile.velocity * Main.rand.NextFloat(0.1f, 0.65f)).RotatedByRandom(0.4f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.9f, 1.6f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 90);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Venom, 90);

        public override void OnKill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 48;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int j = 0; j < 4; j++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 46, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.3f));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.8f, 1.8f);
                if (Main.rand.NextBool())
                    dust.fadeIn = 0.5f;
            }
            for (int k = 0; k < 9; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(6) ? 44 : 39, new Vector2(3, 3).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.3f));
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(0.8f, 1.8f);
                dust.fadeIn = 0.5f;
            }
        }
    }
}
