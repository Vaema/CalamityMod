using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Dusts
{
    public class VoidDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.scale *= Main.rand.NextFloat(0.8f, 1f);
        }

        public override bool Update(Dust dust)
        {
            dust.rotation += MathF.Sign(dust.velocity.X);
            dust.velocity *= 0.98f;
            dust.scale += 0.02f;

            float light = MathHelper.Clamp(dust.scale * 0.8f, 0f, 1f);
            if (!dust.noLightEmittence)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * light);

            return true;
        }
        public override bool PreDraw(Dust dust)
        {
            Vector2 dustCenter = dust.position + new Vector2(0.25f, 0.25f);
            Texture2D rechargeTexture2 = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;
            if (!dust.noLight)
            {
                Texture2D rechargeTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
                // Glow Orb
                Main.EntitySpriteDraw(rechargeTexture, dustCenter - Main.screenPosition, null, dust.color with { A = 0 }, dust.rotation, rechargeTexture.Size() * 0.5f, dust.scale * 0.1f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(rechargeTexture, dustCenter - Main.screenPosition, null, dust.color with { A = 0 } * 0.85f, dust.rotation, rechargeTexture.Size() * 0.5f, dust.scale * 0.04f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(rechargeTexture2, dustCenter - Main.screenPosition, null, Color.Black, dust.rotation, rechargeTexture2.Size() * 0.5f, dust.scale * 0.075f, SpriteEffects.None, 0);
            return false;
        }
    }
}
