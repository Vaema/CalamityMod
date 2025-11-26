using System;
using System.IO;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.Sounds;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class CrimulanSpike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";

        private const int TimeLeft = 600;
        private const int FadeOutTime = 30;
        private float MaxVelocity => Projectile.ai[0];
        private float Acceleration => Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TimeLeft;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < MaxVelocity)
            {
                Projectile.velocity *= Acceleration;
                if (Projectile.velocity.Length() > MaxVelocity)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= MaxVelocity;
                }
            }

            if (Projectile.timeLeft < FadeOutTime)
                Projectile.Opacity -= 1f / FadeOutTime;

            if (Main.rand.NextBool())
            {
                Color dustColor = Color.Crimson;
                dustColor.A = 150;
                int redSpiky = Dust.NewDust(Projectile.position - Projectile.velocity * 3f, Projectile.width, Projectile.height, DustID.TintableDust);
                Main.dust[redSpiky].velocity *= 0.3f;
                Main.dust[redSpiky].velocity += Projectile.velocity * 0.3f;
                Main.dust[redSpiky].color = dustColor;
                Main.dust[redSpiky].scale = 1.4f;
                Main.dust[redSpiky].noGravity = true;
            }
        }

        public override bool CanHitPlayer(Player target) => Projectile.Opacity == 1f;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Projectile.ai[2] > 0f ? ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/CrimulanSpike" + ((int)Projectile.ai[2] + 1)).Value : TextureAssets.Projectile[Projectile.type].Value;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1, texture);
            return false;
        }
    }
}
