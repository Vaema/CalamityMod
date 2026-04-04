using System;
using System.Linq;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EclipseSpark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public static int lifetime = 150;
        Color? color = null;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 32;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.localAI[0] = 20f;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.direction * Projectile.ai[0];

            if (Projectile.timeLeft < (lifetime - Projectile.ai[1]) && Projectile.localAI[0] >= 0)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= Projectile.localAI[0];
                Projectile.localAI[0]--;
                Projectile.timeLeft++;
            }

            if (Projectile.localAI[0] == 0)
            {
                Projectile.velocity = new(0, 1E-05f);
                Projectile.damage = 0;
                float dis = Projectile.position.Distance(Projectile.oldPos.Take(16).Last());
                if (dis < 0.1f)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            color ??= Color.Lerp(Color.OrangeRed, new Color(255, 191, 73), Main.rand.NextFloat(0.25f, 0.5f));

            Main.spriteBatch.End(out var ss);
            var device = Main.instance.GraphicsDevice;
            using var lease = RenderTargetPool.Shared.Rent(
                device,
                Main.screenWidth / 2,
                Main.screenHeight / 2,
                RenderTargetDescriptor.Default
            );
            using (lease.Scope(clearColor: Color.Transparent))
            {
                var list = Projectile.oldPos.Take(16);

                GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(list.ToArray(), new(FireWidthFunction, FireColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), 32);

                GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(list.ToArray(), new(FireCoreWidthFunction, FireCoreColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), 32);
            }
            float dis = Projectile.position.Distance(Projectile.oldPos.Take(16).Last()) / 128f - 0.1f;
            if (dis > 1)
            {
                dis = 1f;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * dis, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(ss);

            return false;
        }

        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = 24f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            // Pulse inwards and outwards over time.
            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = new Color(255, 191, 73);
            return Color.Lerp(mainColor, Color.Transparent, completion);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 16;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = color.Value;
            return mainColor;
        }
    }
}
