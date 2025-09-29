using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using System;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace CalamityMod.Projectiles.Magic
{
    public class AquaSigilWaterdroplet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = false; // This is VFX
            Projectile.ignoreWater = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.damage = 0; // This is VFX
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.01f / 255f, (255 - Projectile.alpha) * 0.15f / 255f, (255 - Projectile.alpha) * 0.05f / 255f);
            Projectile.scale -= 0.01f;

            if (Projectile.scale <= 0f)
            {
                Projectile.Kill();
            }

            if (Projectile.ai[0] <= 3f)
            {
                Projectile.ai[0] += 1f;
                return;
            }

            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, Projectile.velocity, false, 2, Projectile.scale * 1.525f, Color.LightSkyBlue));

            // Gravity!
            Projectile.velocity.Y = Projectile.velocity.Y + 0.35f;
        }

        private float WidthFunction(float completionRatio) => MathHelper.Lerp(0f, MathHelper.Lerp(Projectile.scale * 48f, 0f, completionRatio), MathF.Pow(completionRatio, 1f / 2.5f));
        private Color ColorFunction(float completionRatio)
        {
            float offsetTime = Main.GlobalTimeWrappedHourly;
            float fadeOpacity = Utils.GetLerpValue(0.5f, 0f, completionRatio, true) * Projectile.Opacity;
            Color endColor = Color.LightSkyBlue;
            return Color.Lerp(endColor, Color.White, completionRatio) * fadeOpacity;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, pixelate: false, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 32);


            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, Color.White, Projectile.rotation, sourceRect.Size() * 0.5f, 1f, 0);

            return false;
        }
    }
}
