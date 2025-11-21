using CalamityMod.CalPlayer;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PhoenixsPrideFirewall : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public const int MaxTornadoHeight = 270;

        public float Scale; // Size of the firewall, scales with NPC width
        public float fadeOut = 0f; // Used to control the firewall fading in and out at the start and end of its lifespan
        public NPC Target => Main.npc[(int)Projectile.ai[0]];
        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

        public override void SetDefaults()
        {
            Projectile.width = 408;
            Projectile.height = 102;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.scale = Scale;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = FourSeasonsGalaxia.PhoenixAttunement_FlamePillarLocalIFrames;
        }

        public override void AI()
        {
            // Constantly follow its target
            // Immediately start fading away if the target is no longer active
            if (!Target.active)
            {
                Projectile.velocity *= 0.975f;
                if (Projectile.timeLeft > 15)
                    Projectile.timeLeft = 15;
            }
            else
                Projectile.velocity = (Projectile.velocity * 13f + (Target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 24f) / 14f;

            // Fade in and out at the start and end of its lifespan
            if (Projectile.timeLeft > 170)
                fadeOut += 0.1f;
            if (Projectile.timeLeft < 15)
                fadeOut -= 0.05f;

            // Some smoke on the base
            if (Projectile.timeLeft % 3 == 0)
            {
                MediumMistParticle smoke = new(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), Color.Orange, Color.DarkOrange, 1.1f, 170f);
                GeneralParticleHandler.SpawnParticle(smoke);
                SquareParticle spark = new(Projectile.Center, -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * 8f, true, 20, 0.9f, Color.White);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Bottom,
                Projectile.Bottom - Vector2.UnitY * MaxTornadoHeight,
                72,
                ref _);
        }

        public float WidthFunction(float completionRatio) => MathF.Max(50f * Scale * (MathF.Pow(1f - completionRatio, 2) * 2.75f), 0.35f);
        public Color ColorFunction(float completionRatio) => Color.Orange * (completionRatio > 0.1f ? 1f : completionRatio * 10f);
        public void RenderPixelatedPrimitives(SpriteBatch spritebatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:Bordernado"].UseSaturation(-0.2f);
            GameShaders.Misc["CalamityMod:Bordernado"].UseOpacity(fadeOut);
            GameShaders.Misc["CalamityMod:Bordernado"].SetShaderTexture(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"));
            Vector2[] drawPoints = new Vector2[7];
            Vector2 upwardAscent = Vector2.UnitY * MaxTornadoHeight * fadeOut;

            Vector2 bottom = Projectile.Center;
            Vector2 top = bottom - upwardAscent;
            // Offset is intentionally applied here instead of with an offset function in PrimitiveSettings to make it a bit choppier
            for (int i = 0; i < drawPoints.Length - 1; i++)
                drawPoints[i] = Vector2.Lerp(top + Vector2.UnitX * (MathF.Sin(Main.GameUpdateCount * 0.15f + i) * i * 6f), bottom, i / (float)(drawPoints.Length - 1));

            drawPoints[drawPoints.Length - 1] = bottom;
            PrimitiveRenderer.RenderTrail(drawPoints, new(WidthFunction, ColorFunction, pixelate: true, shader: GameShaders.Misc["CalamityMod:Bordernado"]), 80);
        }
    }
}
