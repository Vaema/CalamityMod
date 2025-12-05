using System;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.Magic
{
    public class VisNeedle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
        }

        public override void AI()
        {
            // Face the right direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Leak a particle
            if (Main.rand.NextBool(6))
            {
                Vector2 velocity = Vector2.Normalize(Projectile.velocity).RotatedBy(Main.rand.NextFloat(-0.07f, 0.07f)) * 0.8f;
                float scale = Projectile.scale * 0.33f;

                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, velocity, false, 20, scale, Color.Magenta));
            }
        }


        // Handle the projectile's trail from here
        private float WidthFunction(float completionRatio) => MathHelper.Lerp(0f, MathHelper.Lerp(10f, 0f, completionRatio), MathF.Pow(completionRatio, 1f / 2.5f));
        private Color ColorFunction(float completionRatio)
        {
            float offsetTime = Main.GlobalTimeWrappedHourly;
            float fadeOpacity = Utils.GetLerpValue(0.5f, 0f, completionRatio, true) * (Projectile.Opacity * 0.75f);
            Color endColor = Color.Lerp(Color.Magenta, Color.Violet, (float)Math.Sin(completionRatio * MathHelper.Pi * 2f - offsetTime * 4f) * 0.5f + 0.5f);
            return Color.Lerp(endColor, Color.White, completionRatio) * fadeOpacity;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, pixelate: false, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);


            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, Color.White, Projectile.rotation, sourceRect.Size() * 0.5f, 1f, 0);

            return false;
        }
    }
}
