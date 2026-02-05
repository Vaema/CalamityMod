using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace CalamityMod.Projectiles.Enemy
{
    public class PodobooSpitSplitting : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Projectiles/Ranged/DrizzlefishFire2";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.01f;
            Lighting.AddLight(Projectile.Center, 0.4f, 0, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * Main.rand.NextFloat(0f, 1f);
                Vector2 angleVec = angle.ToRotationVector2();
                float distance = Main.rand.NextFloat(4f, 8f);
                Vector2 off = angleVec * distance;
                off.Y *= (float)Projectile.height / Projectile.width;
                Vector2 pos = Projectile.Center + off;
                Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork, angleVec * Main.rand.NextFloat(1f, 2f));
                d.customData = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 1)
            {
                if (Projectile.penetrate > 1)
                {
                    if (Projectile.velocity.X != oldVelocity.X)
                        Projectile.velocity.X = -oldVelocity.X;
                    if (Projectile.velocity.Y != oldVelocity.Y)
                        Projectile.velocity.Y = -oldVelocity.Y;
                    Projectile.penetrate--;
                    return false;
                }
            }            
            else if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.PiOver2), Type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, ai0: 1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(-MathHelper.PiOver2), Type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, ai0: 1);
            }
            return true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Projectile.ai[0] == 1)
            {
                if (Projectile.penetrate > 1)
                {
                    Projectile.velocity *= -1;
                }
            }
            else if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.PiOver2), Type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, ai0: 1);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(-MathHelper.PiOver2), Type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, ai0: 1);
            }

            if (info.Damage <= 0)
                return;

            target.AddBuff(BuffID.OnFire, 60);        
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], Color.Magenta, 1, TextureAssets.Projectile[Type].Value);
            int animSped = 10;
            Vector2 scale = Vector2.One + new Vector2(MathF.Cos(Main.GlobalTimeWrappedHourly * animSped), MathF.Sin(Main.GlobalTimeWrappedHourly * animSped)) * 0.25f;
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, scale, 0);
            return false;
        }
    }
}
