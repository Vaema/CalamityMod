using System;
using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class LucreciaDNATrailCreator : ModProjectile
    {
        private List<Vector2> oldPositionsLeft = new List<Vector2>();
        private List<Vector2> oldPositionsRight = new List<Vector2>();
        private int trailTimer = 0;
        // The timer is initialized with a delay. It will reset to 6 after the first shot.
        private int middleStreakTimer = 40;
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 170;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.extraUpdates = 12;
            Projectile.timeLeft = 2400;
            Projectile.minion = true; // Scuffed but it prevents the projectile from being culled in a few situations
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            trailTimer++;

            // Very volatile values...
            float amplitude = 86f;
            float frequency = 0.039f;

            // Makes a perpendicular/mirrored trail, AKA a double helix
            Vector2 perpendicular = Vector2.Normalize(new Vector2(-Projectile.velocity.Y, Projectile.velocity.X));

            // Calculate the sine wave value.
            float sineWave = (float)Math.Cos(trailTimer * frequency);

            // Calculate offset based on the pattern of the sinewave and subtract the proj's dimensions to be accurate
            Vector2 offsetLeft = (perpendicular * amplitude * sineWave) - new Vector2(Projectile.width, Projectile.height);
            Vector2 offsetRight = (-perpendicular * amplitude * sineWave) - new Vector2(Projectile.width, Projectile.height);

            oldPositionsLeft.Add(Projectile.Center + offsetLeft);
            oldPositionsRight.Add(Projectile.Center + offsetRight);

            // The timer will now correctly decrease once per AI update.
            middleStreakTimer--;

            if (middleStreakTimer <= 0)
            {
                // Reset the timer for the next shot.
                middleStreakTimer = 6;

                // Spawn the main trail
                Particle spark = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(-170f)), "CalamityMod/Particles/BloomCircle", false, 18, 0.24f, Color.MediumPurple * 1.3f, new Vector2(1f, 2.5f), true, true, shrinkSpeed: 0.25f, glowOpacity: 0.4f);
                GeneralParticleHandler.SpawnParticle(spark);
                Particle spark2 = new CustomSpark(Projectile.Center + perpendicular, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(170f)), "CalamityMod/Particles/BloomCircle", false, 18, 0.24f, Color.CornflowerBlue * 1.3f, new Vector2(1f, 2.5f), true, true, shrinkSpeed: 0.25f, glowOpacity: 0.4f);
                GeneralParticleHandler.SpawnParticle(spark2);
            }


            // Remove old positions after 120
            int maxTrailLength = 120;
            if (oldPositionsLeft.Count > maxTrailLength)
                oldPositionsLeft.RemoveAt(0);
            if (oldPositionsRight.Count > maxTrailLength)
                oldPositionsRight.RemoveAt(0);
        }

        private float WidthFunction(float completionRatio)
        {
            return MathHelper.Lerp(12f, 0f, completionRatio);
        }


        private Color LeftColorFunction(float completionRatio)
        {
            Color baseColor = Color.MediumPurple * 1.3f;

            float alphaScaling = -4 * completionRatio * (completionRatio - 1);
            return baseColor * alphaScaling;
        }
        private Color RightColorFunction(float completionRatio)
        {
            Color baseColor = Color.CornflowerBlue * 1.3f;

            float alphaScaling = -4 * completionRatio * (completionRatio - 1);
            return baseColor * alphaScaling;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            MiscShaderData trailShader = GameShaders.Misc["CalamityMod:TrailStreak"];
            trailShader.SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            // Use the separate color functions for each trail
            PrimitiveRenderer.RenderTrail(oldPositionsLeft, new PrimitiveSettings(WidthFunction, LeftColorFunction, (_) => Projectile.Size * 1f, pixelate: false, shader: trailShader));
            PrimitiveRenderer.RenderTrail(oldPositionsRight, new PrimitiveSettings(WidthFunction, RightColorFunction, (_) => Projectile.Size * 1f, pixelate: false, shader: trailShader));
            return false;
        }
    }
}
