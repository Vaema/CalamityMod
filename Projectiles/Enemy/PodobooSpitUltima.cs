using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using CalamityMod.Graphics.Primitives;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityMod.Projectiles.Enemy
{
    public class PodobooSpitUltima : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.01f;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * Main.rand.NextFloat(0f, 1f);
                Vector2 angleVec = angle.ToRotationVector2();
                float distance = Main.rand.NextFloat(7f, 18f);
                Vector2 off = angleVec * distance;
                off.Y *= (float)Projectile.height / Projectile.width;
                Vector2 pos = Projectile.Center + off;
                Dust d = Dust.NewDustPerfect(pos, DustID.InfernoFork, angleVec * Main.rand.NextFloat(2f, 4f));
                d.customData = true;
            }
        }



        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            Vector2 overallOffset = Projectile.Size / 2;
            int numPoints = 92;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);

            int animSped = 10;
            Vector2 scale = Vector2.One + new Vector2(MathF.Cos(Main.GlobalTimeWrappedHourly * animSped), MathF.Sin(Main.GlobalTimeWrappedHourly * animSped)) * 0.25f;
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, scale, 0);
            return false;
        }
        private float PrimitiveWidthFunction(float completionRatio)
        {
            return MathHelper.Lerp(12, 1, completionRatio);
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.Orange, Color.DarkOrange, completionRatio);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 60);
        }
    }
}
