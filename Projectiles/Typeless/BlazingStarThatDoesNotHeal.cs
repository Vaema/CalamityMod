using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.Providence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class BlazingStarThatDoesNotHeal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Particles/Sparkle";

        public static Asset<Texture2D> Bloom;
        public override void Load() => Bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft % 4 == 0) //only once per 4 frames
                Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0f);

            if (Projectile.velocity.Length() < 18f)
                Projectile.velocity *= 1.015f;
        }

        internal float WidthFunction(float completionRatio) => (1f - completionRatio) * Projectile.scale * 16f;
        internal Color ColorFunction(float completionRatio)
        {
            float hue = 0.04f * (Projectile.ai[0] % 4f) + 0.1f * completionRatio * CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly * 0.25f) % 1f);
            Color trailColor = Main.hslToRgb(hue, 0.8f, 0.6f);
            return trailColor * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 8);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D sparkleTex = TextureAssets.Projectile[Type].Value;
            Texture2D bloomTex = Bloom.Value;
            float bloomScale = (float)sparkleTex.Height / (float)bloomTex.Height;
            float sparkleScale = 0.7f + CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly % 2f) / 2f) * 0.2f;

            Color color = ColorFunction(0f);
            float rotation = Projectile.rotation + Main.GlobalTimeWrappedHourly * 8f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(bloomTex, drawPos, null, color * 0.5f, 0, bloomTex.Size() * 0.5f, 5f * bloomScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, Color.Lerp(color, Color.White, 0.7f), rotation, sparkleTex.Size() * 0.5f, 2.2f * sparkleScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, color, rotation + MathHelper.PiOver4, sparkleTex.Size() * 0.5f, 1.6f * sparkleScale, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), ((new Vector2(7, 7) * Projectile.scale).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.75f, 1.05f);
                dust.color = ColorFunction(0f);
                dust.noLightEmittence = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<HolyFlames>(), 60 * 5);
    }
}
