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
                Projectile.frame--;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame < 0)
            {
                Projectile.frame = Main.projFrames[Type] - 1;
            }
            if (Projectile.timeLeft < 30)
            {
                Projectile.Opacity = Projectile.timeLeft / 30f;
            }
            else if (Projectile.Opacity < 1)
            {
                Projectile.Opacity += 0.05f;
            }
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
        static Texture2D _TransparentBloomTex;

        public static Texture2D GetTransparentBloomTex()
        {
            if (_TransparentBloomTex == null)
            {
                _TransparentBloomTex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
                var BaseArray = new Color[_TransparentBloomTex.Width * _TransparentBloomTex.Height];
                var ColorArray = new Color[_TransparentBloomTex.Width * _TransparentBloomTex.Height];
                _TransparentBloomTex.GetData(BaseArray);
                for (var i = 0; i < BaseArray.Length; i++)
                {
                    ColorArray[i] = new Color((int)BaseArray[i].R, (int)BaseArray[i].R, (int)BaseArray[i].R, (int)BaseArray[i].R);
                }
                _TransparentBloomTex.SetData(ColorArray);
            }
            return _TransparentBloomTex;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            //Draw the bloom circle

            Texture2D telegraphBase = GetTransparentBloomTex();
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.DarkSlateBlue * 0.75f * Projectile.Opacity, 0, telegraphBase.Size() / 2f, 1200f * 1.25f * Projectile.Opacity / telegraphBase.Width, 0, 0);

            //Draw the inner particles
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseOpacity(10f);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseSaturation(0.1f);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/FrozenCrust"), 1);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].Apply();
            telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing").Value;
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.SkyBlue * 0.75f * Projectile.Opacity, 0, telegraphBase.Size() / 2f, 1200f * Projectile.Opacity / telegraphBase.Width, 0, 0);
            Main.spriteBatch.ExitShaderRegion();

            //Draw the outer particles
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseOpacity(0.2f);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseSaturation(0.1f);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/FrozenCrust"), 1);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].Apply();
            telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Particles/HighResFoggyCircleHardEdge").Value;
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.SkyBlue * Projectile.Opacity * 0.75f, 0, telegraphBase.Size() / 2f, 1200f * Projectile.Opacity / telegraphBase.Width, 0, 0);
            Main.spriteBatch.ExitShaderRegion();
            
            lightColor = Color.Lerp(Color.White, Color.SkyBlue, 0.75f);
            return true;
        }
    }
}
