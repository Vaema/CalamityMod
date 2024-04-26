using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class StormSurgeTornado : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 66;
            Projectile.height = 66;
            Projectile.scale = 0.5f;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 1.25f, 1.25f);
            if (Projectile.scale <= 2f)
            {
                Projectile.scale *= 1.03f;
            }
            if (Projectile.scale >= 2f)
            {
                Projectile.Kill();
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 6)
            {
                Projectile.frame = 0;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
