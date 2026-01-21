using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class BansheeHookBoom : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int TimeLeft = 5;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TimeLeft;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.5f, 0f, 0.15f);

            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= TimeLeft)
            {
                Projectile.position = Projectile.Center;
                Projectile.width = Projectile.height = (int)(52f * Projectile.scale);
                Projectile.Center = Projectile.position;

                for (int i = 0; i < 2; i++)
                {
                    int bansheeDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                    Main.dust[bansheeDust].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(Math.PI) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                }

                for (int j = 0; j < 5; j++)
                {
                    int bansheeDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 200, default, 2.7f);
                    Main.dust[bansheeDust2].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(Math.PI) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                    Main.dust[bansheeDust2].noGravity = true;
                    Main.dust[bansheeDust2].velocity *= 3f;
                    bansheeDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                    Main.dust[bansheeDust2].position = Projectile.Center + Vector2.UnitY.RotatedByRandom(Math.PI) * (float)Main.rand.NextDouble() * (float)Projectile.width / 2f;
                    Main.dust[bansheeDust2].velocity *= 2f;
                    Main.dust[bansheeDust2].noGravity = true;
                    Main.dust[bansheeDust2].fadeIn = 2.5f;
                }

                for (int k = 0; k < 2; k++)
                {
                    int bansheeDust3 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 0, default, 2.7f);
                    Main.dust[bansheeDust3].position = Projectile.Center + Vector2.UnitX.RotatedByRandom(Math.PI).RotatedBy((double)Projectile.velocity.ToRotation(), default) * (float)Projectile.width / 2f;
                    Main.dust[bansheeDust3].noGravity = true;
                    Main.dust[bansheeDust3].velocity *= 3f;
                }

                for (int l = 0; l < 5; l++)
                {
                    int spiritDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0f, 0f, 0, default, 1.5f);
                    Main.dust[spiritDust].position = Projectile.Center + Vector2.UnitX.RotatedByRandom(Math.PI).RotatedBy((double)Projectile.velocity.ToRotation(), default) * (float)Projectile.width / 2f;
                    Main.dust[spiritDust].noGravity = true;
                    Main.dust[spiritDust].velocity *= 3f;
                }
            }
        }
    }
}
