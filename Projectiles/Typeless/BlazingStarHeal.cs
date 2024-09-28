using System;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
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
    public class BlazingStarHeal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Particles/Sparkle";

        public static Asset<Texture2D> Bloom;
        public override void Load() => Bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 200)
                Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.rotation += MathHelper.ToRadians(2f) * Utils.GetLerpValue(0f, 30f, Projectile.timeLeft, true);

            if (Projectile.timeLeft % 4 == 0) //only once per 4 frames
                Lighting.AddLight(Projectile.Center, 0f, 0.6f, 0f);
            if (Projectile.timeLeft > 190)
                Projectile.velocity *= 1.1f;
            else if (Projectile.timeLeft <= 190)
                Projectile.velocity *= 0.99f;
            if (Projectile.timeLeft <= 160)
                Projectile.velocity = Vector2.Zero;

            int index = Player.FindClosest(Projectile.position, Projectile.width, Projectile.height);
            Player player = Main.player[index];
            if (Projectile.timeLeft > 190 || player is null || Main.player[Projectile.owner].team != player.team)
                return;

            float playerDist = Vector2.Distance(player.Center, Projectile.Center);
            if (!player.immune && playerDist < 50f && !player.dead && Projectile.position.X < player.position.X + player.width && Projectile.position.X + Projectile.width > player.position.X && Projectile.position.Y < player.position.Y + player.height && Projectile.position.Y + Projectile.height > player.position.Y)
            {
                int healAmt = Utils.Clamp((200 - Projectile.timeLeft) / 10, 1, 10); //min heal is 5, max heal is 10, achievable after 2 seconds
                player.HealEffect(healAmt, false);
                player.statLife += healAmt;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;

                NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, index, healAmt);

                Projectile.Kill();
            }
        }

        internal float WidthFunction(float completionRatio) => (1f - completionRatio) * Projectile.scale * 16f;
        internal Color ColorFunction(float completionRatio)
        {
            float hue = 0.35f + 0.1f * completionRatio * CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly * 0.25f) % 1f);
            Color trailColor = Main.hslToRgb(hue, 0.6f, 0.5f);
            return trailColor * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 25);

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Texture2D sparkleTex = TextureAssets.Projectile[Type].Value;
            Texture2D bloomTex = Bloom.Value;
            float bloomScale = (float)sparkleTex.Height / (float)bloomTex.Height;
            float sparkleScale = 0.7f + CalamityUtils.Convert01To010((Main.GlobalTimeWrappedHourly % 2f) / 2f) * 0.2f;

            Color color = ColorFunction(0f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(bloomTex, drawPos, null, color * 0.5f, 0, bloomTex.Size() * 0.5f, 5f * bloomScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, Color.Lerp(color, Color.White, 0.7f), Projectile.rotation, sparkleTex.Size() * 0.5f, 2.2f * sparkleScale, SpriteEffects.None);
            Main.EntitySpriteDraw(sparkleTex, drawPos, null, color, Projectile.rotation + MathHelper.PiOver4, sparkleTex.Size() * 0.5f, 1.6f * sparkleScale, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            SoundStyle fireHeal = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
            SoundEngine.PlaySound(fireHeal with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);

            Particle pulse = new CustomPulse(Projectile.Center, Vector2.Zero, ColorFunction(0f), "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0f, 0.032f, 18);
            GeneralParticleHandler.SpawnParticle(pulse);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), ((new Vector2(7, 7) * Projectile.scale).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f)));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.75f, 1.05f);
                dust.color = ColorFunction(0f);
                dust.noLightEmittence = true;
            }
        }

        public override bool? CanDamage() => false;
    }
}
