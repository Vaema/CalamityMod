using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GreenWater : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";

        private const int TimeLeft = 300;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = TimeLeft;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0f)
            {
                if (Projectile.Center.Y > Projectile.ai[2])
                    Projectile.tileCollide = true;
                else
                    Projectile.tileCollide = false;
            }
            else
            {
                if (Projectile.Center.Y < Projectile.ai[2])
                    Projectile.tileCollide = true;
                else
                    Projectile.tileCollide = false;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0f);

            Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + MathHelper.PiOver4;

            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
                Projectile.localAI[0] += 1f;
            }

            int blood = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, Main.rand.NextFloat(1.6f, 2.4f));
            Main.dust[blood].noGravity = true;
            Main.dust[blood].velocity *= 0.5f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > TimeLeft - 5)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath12, Projectile.Center);

            for (int i = 4; i < 31; i++)
            {
                float oldXPos = Projectile.oldVelocity.X * (30f / (float)i);
                float oldYPos = Projectile.oldVelocity.Y * (30f / (float)i);
                int killDust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXPos, Projectile.oldPosition.Y - oldYPos), 8, 8, DustID.Blood, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.8f);
                Main.dust[killDust].noGravity = true;
                Dust dust = Main.dust[killDust];
                dust.velocity *= 0.5f;
                killDust = Dust.NewDust(new Vector2(Projectile.oldPosition.X - oldXPos, Projectile.oldPosition.Y - oldYPos), 8, 8, DustID.Blood, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.4f);
                dust = Main.dust[killDust];
                dust.velocity *= 0.05f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Venom, 90);
    }
}
