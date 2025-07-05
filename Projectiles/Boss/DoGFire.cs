using System;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Boss
{
    public class DoGFire : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                    Vector2 dustVelocity = Projectile.velocity * -1.2f;
                    float dustScale = Main.rand.NextFloat(0.6f, 0.8f);
                    Dust dust = Dust.NewDustDirect(dustSpawnPosition, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, Color.Purple, dustScale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.noLightEmittence = false;
                }
            }
            if (Main.rand.NextBool())
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 smokeVelocity = Main.rand.NextVector2Circular(1f, 1f) * 0.65f;
                    int smokeLifetime = 12;
                    float smokeScale = Main.rand.NextFloat(0.15f, 0.3f);
                    float smokeOpacity = Main.rand.NextFloat(0.75f, 0.9f);

                    HeavySmokeParticle ghastlySmoke = new(Projectile.Center, smokeVelocity, Projectile.ai[0] == 2f ? Color.SkyBlue : Color.Purple, smokeLifetime, smokeScale, smokeOpacity, 0.02f, true);
                    GeneralParticleHandler.SpawnParticle(ghastlySmoke);
                }
            }
            if (Projectile.ai[0] != 2f)
            {
                float offset = 40f;
                Vector2 spawnPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * offset;
                Particle spark = new VoidSparkParticle(spawnPos, Projectile.velocity, false, 10, 0.12f, Color.Purple, 0.45f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), CalamityUtils.SecondsToFrames(6));
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn a bunch of dust along the length of the trail a soul is killed.
            BezierCurve curve = new(Projectile.oldPos);
            for (int i = 0; i < 35; i++)
            {
                Vector2 dustSpawnPosition = curve.Evaluate(Main.rand.NextFloat());
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 3f;
                float dustScale = Main.rand.NextFloat(1.2f, 1.8f);
                Dust dust = Dust.NewDustDirect(dustSpawnPosition, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, Color.Purple, dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittence = false;
            }

            // Spawn a burst of dust at the center.
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                float dustScale = Main.rand.NextFloat(1.8f, 2.4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, Color.Purple, dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittence = false;
            }
            Projectile.Damage();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.ai[0] == 2f)
            {
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            }
        }

        public float FireWidthFunction(float completion)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 120f;
            float curveRatio = 0.5f;

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width;
        }

        public Color FireColorFunction(float completion)
        {
            Color tipColor = Color.Transparent;
            return Color.Lerp(Projectile.ai[0] == 2f ? Color.Cyan : Color.Purple * 1.3f, tipColor, completion);
        }

        public float FireCoreWidthFunction(float completion)
        {
            float width;
            float maxBodyWidth = Projectile.scale * Projectile.ai[0] == 2f ? 64f : 78f;
            float curveRatio = 0.3f;

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width;
        }

        public Color FireCoreColorFunction(float completion)
        {
            Color tipColor = Color.Lerp(Projectile.ai[0] == 2f ? Color.BlueViolet : Color.Purple, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(Color.BlueViolet, tipColor, completion);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, PixelationPrimitiveLayer layer)
        {
            // Render the main trail for the body for the soul.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(FireWidthFunction, FireColorFunction, (_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length * Projectile.ai[0] == 2f ? 29 : 58);

            // Render a smaller, pure white trail in the same position to represent the glowing white core of the soul.
            Vector2[] fireCoreLength = Projectile.oldPos.Take(8).ToArray();
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(fireCoreLength, new(FireCoreWidthFunction, FireCoreColorFunction, (_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), fireCoreLength.Length * 29);
        }
    }
}
