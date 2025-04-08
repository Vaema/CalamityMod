using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class UnstableDust : ModDust
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
            dust.rotation = Main.rand.NextFloat(-5, 5);
        }

        public override bool Update(Dust dust)
        {
            float rotDir = Math.Sign(dust.rotation);
            dust.rotation += 0.5f * dust.scale * rotDir;
            dust.velocity *= 0.96f;
            if (dust.noGravity)
                dust.scale -= 0.045f;
            else
            {
                dust.scale -= 0.03f;
                dust.velocity.Y += Main.rand.NextFloat(0.1f, 0.35f);
            }

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittence)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            if (dust.scale <= 0)
                dust.active = false;

            dust.position += dust.velocity;
            // dust.fadeIn is used to change the shake intensity, -2 will negate it
            dust.position += Main.rand.NextVector2Circular(dust.fadeIn + 2, dust.fadeIn + 2);

            return false;
        }
        public override bool PreDraw(Dust dust)
        {
            Vector2 dustCenter = dust.position + new Vector2(0.25f, 0.25f);

            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/FullStar").Value;

            Vector2 squash = new Vector2(0.4f, 1);
            float texScaling = 6;

            Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.1f * texScaling, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.04f * texScaling, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, squash * dust.scale * 0.025f * texScaling, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
