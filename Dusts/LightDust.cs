using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class LightDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += MathF.Sign(dust.velocity.X);
            dust.velocity *= 0.98f;
            if (dust.noGravity)
                dust.scale += 0.02f;
            else
                dust.scale -= 0.01f;

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittence)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            return true;
        }
        public override bool PreDraw(Dust dust)
        {
            Vector2 dustCenter = dust.position + new Vector2(0.25f, 0.25f);
            Texture2D solidCenter = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            // Glow Orb
            Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, dust.scale * 0.1f, SpriteEffects.None, 0);
            if (dust.alpha < 1)
                Main.EntitySpriteDraw(bloom, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, bloom.Size() * 0.5f, dust.scale * 0.04f, SpriteEffects.None, 0);
            if (!dust.noLight)
            {
                Main.EntitySpriteDraw(solidCenter, dustCenter - Main.screenPosition, null, Color.Lerp(dust.color, Color.White, 0.3f) with { A = 0 } * Utils.GetLerpValue(255, 0, dust.alpha), dust.rotation, solidCenter.Size() * 0.5f, dust.scale * 0.075f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
