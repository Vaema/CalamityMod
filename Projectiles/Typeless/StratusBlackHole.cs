using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class StratusBlackHole : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 314;
            Projectile.height = 198;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.MaxUpdates = 1;
            Projectile.timeLeft = 3600 * Projectile.MaxUpdates;
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.Opacity = 0;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.975f;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
            {
                Projectile.frame = 0;
            }
            if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity = Projectile.timeLeft / 30f;
            }
            else if (Projectile.Opacity < 1)
            {
                Projectile.Opacity += 0.05f;
            }
            Projectile.rotation = MathHelper.Lerp(0, Projectile.velocity.SafeNormalize(Vector2.Zero).X, Projectile.velocity.Length() / 20f);
            if (float.IsNaN(Projectile.rotation))
                Projectile.rotation = 0;
            foreach (var player in Main.ActivePlayers)
            {
                if (player.Distance(Projectile.Center) <= 600 && player.miscCounter % 30 == 15)
                {
                    player.Calamity().StratusStarburst++;
                    if (player.Calamity().StratusStarburst <= CalamityPlayer.MaxStratusStarburst)
                        player.Calamity().StarburstEntities.Add(new DataStructures.StarburstEntity(Projectile.Center));
                    player.Calamity().HasStratusItemCooldown = (int)MathHelper.Max(player.Calamity().HasStratusItemCooldown, 180);
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, 600, 0, MathHelper.TwoPi);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();
            Texture2D telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseOpacity(0.2f * Projectile.Opacity);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseColor(Color.SkyBlue);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSecondaryColor(Color.Lerp(Color.LightSkyBlue, Color.White, 0.5f));
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSaturation(1);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].Apply();

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, lightColor, 0, telegraphBase.Size() / 2f, 1170f, 0, 0);
            Main.spriteBatch.ExitShaderRegion();

            lightColor = Color.Lerp(Color.White, Color.SkyBlue, 0.75f);
            return true;
        }
    }
}
