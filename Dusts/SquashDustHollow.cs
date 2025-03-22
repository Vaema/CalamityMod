using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class SquashDustHollow : ModDust
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            float fadeSpeed = (dust.fadeIn + 1);
            dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;
            dust.velocity *= 0.96f * fadeSpeed;
            if (dust.noGravity)
                dust.scale -= 0.045f * fadeSpeed;
            else
            {
                dust.scale -= 0.03f * fadeSpeed;
                dust.velocity.Y += Main.rand.NextFloat(0.1f, 0.35f) * fadeSpeed;
            }

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittence)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            if (dust.scale <= 0)
                dust.active = false;

            dust.position += dust.velocity;

            return false;
        }
        public override bool PreDraw(Dust dust)
        {
            Vector2 dustCenter = dust.position + new Vector2(0.25f, 0.25f);

            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing").Value;

            Vector2 squash = new Vector2(Utils.Remap(dust.velocity.Length(), 2, 7, 1, 0.5f), Utils.Remap(dust.velocity.Length(), 2, 7, 1, 2.5f));

            // Glow Orb
            Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.2f) with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.09f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                for (int i = 0; i < 2; i++)
                Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.4f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.075f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
