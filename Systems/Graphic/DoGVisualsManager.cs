using CalamityMod.Effects;
using CalamityMod.Graphics;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Skies;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using System;
using CalamityMod.Graphics.Metaballs;
using System.Linq;

namespace CalamityMod.Systems.Graphic
{
    public class DoGVisualsManager : ModSystem
    {
        public class DistortionDebris
        {
            public Vector2 Position;

            public Vector2 Velocity;

            public float Scale;

            public float Opacity;

            public float Rotation;

            public float Depth;

            public int FrameX;

            public int FrameY;

            public int Time;

            public int Lifetime;

            public void Update()
            {
                if (Time <= 30)
                    Opacity += 0.05f;
                if (Time >= Lifetime - 30)
                    Opacity -= 0.05f;

                Position += Velocity;
                Opacity = MathHelper.Clamp(Opacity, 0f, 1f);
                Rotation += Velocity.X * 0.03f;
                Time++;
            }
        }

        public class DistortionStar
        {
            public Vector2 Position;

            public float Scale;

            public float StoredScale;

            public float Depth;

            public Color Color;

            public int Time;

            public int Lifetime;

            public void Update()
            {
                Scale = MathHelper.Lerp(0f, StoredScale, MathF.Sin(Time / (float)Lifetime) * 0.5f + 0.5f);
                Time++;
            }
        }

        public float FillProgress = 0;

        public List<DistortionDebris> ForegroundDebris;

        public List<DistortionStar> ForegroundStars;

        /// <summary>
        /// Whether or not the background render targets should have their contents drawn to them.
        /// </summary>
        public bool ShouldDrawToBackgroundTargets => DoGSky.SkyIntensity > 0f;

        /// <summary>
        /// Whether or not the foreground render target should have its contents drawn to it.
        /// </summary>
        public bool ShouldDrawToForegroundTarget => MetaballManager.metaballs.Any(m => m.AnythingToDraw && m is DoGDistortionMetaball);

        /// <summary>
        /// The actual effects and contents seen inside the rift in the background of DoG's fight.
        /// </summary>
        public static ManagedRenderTarget DistortionRiftBackgroundContentsTarget { get; private set; }

        /// <summary>
        /// The primitives used to make up the shape of the rift seen in the background of DoG's fight.
        /// </summary>
        public static ManagedRenderTarget DistortionRiftPrimitivesTarget { get; private set; }

        /// <summary>
        /// The actual effects and contents seen inside of certain attacks used by DoG. Also used for DoG's distortion metaballs.
        /// </summary>
        public static ManagedRenderTarget DistortionForegroundContentsTarget { get; private set; }

        public override void OnModLoad()
        {
            if (Main.dedServ)
                return;

            ForegroundDebris = [];
            ForegroundStars = [];
            On_Main.DrawSunAndMoon += DiscardCelestialObjects;
        }

        public override void PostSetupContent()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() =>
            {
                DistortionRiftBackgroundContentsTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
                DistortionRiftPrimitivesTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
                DistortionForegroundContentsTarget = new(true, ManagedRenderTarget.CreateScreenSizedTarget);
            });
            RenderTargetManager.RenderTargetUpdateLoopEvent += PrepareTargets;
        }

        public override void OnModUnload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawSunAndMoon -= DiscardCelestialObjects;
            RenderTargetManager.RenderTargetUpdateLoopEvent -= PrepareTargets;
        }

        public override void PostUpdateEverything()
        {
            if (ForegroundDebris.Count < 100 && Main.rand.NextBool(12))
            {
                DistortionDebris debris = new DistortionDebris()
                {
                    Position = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(4500f, 4500f),
                    Velocity = Vector2.UnitX * Main.rand.NextFloat(4f, 12f),
                    Scale = Main.rand.NextFloat(0.4f, 1.2f),
                    Depth = Main.rand.NextFloat(3f, 10f),
                    FrameX = Main.rand.Next(3),
                    FrameY = Main.rand.Next(4)
                };
                ForegroundDebris.Add(debris);
            }

            if (ForegroundStars.Count < 100 && Main.rand.NextBool(12))
            {
                DistortionStar star = new DistortionStar()
                {
                    Position = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(4500f, 4500f),
                    StoredScale = Main.rand.NextFloat(0.75f, 1.25f),
                    Depth = Main.rand.NextFloat(8f, 12f),
                    Color = Utils.SelectRandom(Main.rand, Color.Fuchsia, new Color(0, 221, 250), new(117, 21, 161))
                };
                ForegroundStars.Add(star);
            }

            if (ForegroundDebris.Count > 0)
            {
                for (int i = 0; i < ForegroundDebris.Count; i++)
                    ForegroundDebris[i].Update();
            }

            if (ForegroundStars.Count > 0)
            {
                for (int i = 0; i < ForegroundStars.Count; i++)
                    ForegroundStars[i].Update();
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            var cplayer = Main.LocalPlayer.Calamity();
            if (cplayer.monolithDevourerBShader > 0 || cplayer.monolithDevourerPShader > 0 || NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()))
                FillProgress += 0.05f;
            else
                FillProgress -= 0.05f;

            // Darken the lighting color of both the background and foreground.
            FillProgress = MathHelper.Clamp(FillProgress, 0, 1);
            if (FillProgress > 0)
            {
                Color colorToUse = DoGSky.DoGSkyColor;
                backgroundColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, Color.White.R * 0.035f, FillProgress);
                backgroundColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, Color.White.G * 0.035f, FillProgress);
                backgroundColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, Color.White.B * 0.035f, FillProgress);
                backgroundColor = new(backgroundColor.ToVector3() + colorToUse.ToVector3() * 0.025f * FillProgress);

                tileColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, 35f, FillProgress);
                tileColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, 35f, FillProgress);
                tileColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, 35f, FillProgress);
                tileColor = new(tileColor.ToVector3() + colorToUse.ToVector3() * 0.075f * FillProgress);
            }
        }

        private void DiscardCelestialObjects(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
        {
            // Fade out the sun during DoG's fight.
            if (DoGSky.SkyIntensity > 0f)
                tempMushroomInfluence = DoGSky.SkyIntensity;

            orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
        }

        private void PrepareTargets()
        {
            // Determine when targets should have their contents drawn onto them in order to save on performance.
            if (ShouldDrawToBackgroundTargets)
            {
                // Draw the background which'll be displayed inside the rift onto a render target.
                DistortionRiftBackgroundContentsTarget.SwapTo();
                DrawDistortionRiftBackground();

                // Draw the primitives which compromise the rift to another render target.
                DistortionRiftPrimitivesTarget.SwapTo();

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
                if (SkyManager.Instance["CalamityMod:DevourerofGodsHead"].IsActive())
                    (SkyManager.Instance["CalamityMod:DevourerofGodsHead"] as DoGSky).DrawRiftToRenderTarget();
                Main.spriteBatch.End();
            }

            if (ShouldDrawToForegroundTarget)
            {
                // Draw the background which'll be displayed for all distortion-related effects done in the foreground (same layer as the player).
                DistortionForegroundContentsTarget.SwapTo();
                DrawDistortionForeground();
            }

            // Reset the current render target at the end of the method once any targets are being drawn to.
            if (ShouldDrawToForegroundTarget || ShouldDrawToBackgroundTargets)
                Main.instance.GraphicsDevice.SetRenderTarget(null);
        }

        #region Distortion Background Drawing
        private void DrawDistortionRiftBackground()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw a black tile behind everything since the shader used to generate the clouds will appear transparent with dark colors, thus revealing
            // the sky and any of its effects under the rift.
            Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the layer of rolling clouds in the background.
            DrawDistortionClouds_Background();

            Main.spriteBatch.End();
        }

        private void DrawDistortionClouds_Background()
        {
            Effect rollingCloudsShader = CalamityShaders.DoGBackgroundFogShader;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Asset<Texture2D> cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds");
            Asset<Texture2D> erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise");

            rollingCloudsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            rollingCloudsShader.Parameters["overallOpacity"].SetValue(0.5f);
            rollingCloudsShader.Parameters["distortionStrength"].SetValue(0.12f);
            rollingCloudsShader.Parameters["mainNoiseTextureScale"].SetValue(2f);
            rollingCloudsShader.Parameters["distortionTextureScale"].SetValue(0.8f);
            rollingCloudsShader.Parameters["erosionTextureScale"].SetValue(0.26f);
            rollingCloudsShader.Parameters["erosionMin"].SetValue(0.06f);
            rollingCloudsShader.Parameters["gradientPrecision"].SetValue(20f);

            rollingCloudsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            rollingCloudsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.001f);

            rollingCloudsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, Color.DarkGray, 0.6f).ToVector3());
            rollingCloudsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.Black, DoGSky.DoGSkyColor, 0.3f).ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = cloudsTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            rollingCloudsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(cloudsTexture.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }
        #endregion  

        #region Distortion Foreground Drawing
        private void DrawDistortionForeground()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw a black tile for the same reasoning explained above for the background drawing.
            Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
            
            // Draw a layer of rolling clouds in the background.
            DrawDistortionClouds_Foreground();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
            
            // Draw the stars in front of the clouds.
            DrawDistortionStars_Foreground();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the flying debris.
            DrawDistortionDebris_Foreground();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the winds effect in front of everything.
            DrawDistortionWinds_Foreground();

            Main.spriteBatch.End();
        }

        private void DrawDistortionWinds_Foreground()
        {
            Effect windsShader = CalamityShaders.DoGDistortionWindsShader;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Texture2D windsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2").Value;
            Texture2D highlightsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SharpNoise").Value;
            Texture2D distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/MeltyNoise").Value;
            Texture2D erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Pebbles").Value;

            windsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 1.075f);
            windsShader.Parameters["overallOpacity"].SetValue(0.8f);
            windsShader.Parameters["distortionStrength"].SetValue(0.3f);
            windsShader.Parameters["mainNoiseTextureScale"].SetValue(2.64f);
            windsShader.Parameters["distortionTextureScale"].SetValue(1.76f);
            windsShader.Parameters["erosionTextureScale"].SetValue(2f);
            windsShader.Parameters["erosionMin"].SetValue(0.5f);
            windsShader.Parameters["gradientPrecision"].SetValue(20f);

            windsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            windsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / windsTexture.Size() * 0.025f);

            windsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.64f).ToVector3());
            windsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.32f).ToVector3());
            windsShader.Parameters["highlightsColor"].SetValue(Color.Fuchsia.ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = highlightsTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

            Main.instance.GraphicsDevice.Textures[2] = distortionTexture;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[3] = erosionTexture;
            Main.instance.GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

            // Fuchsia highlights.
            windsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(windsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            windsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.8f);
            windsShader.Parameters["overallOpacity"].SetValue(0.7f);
            windsShader.Parameters["distortionStrength"].SetValue(0.6f);
            windsShader.Parameters["mainNoiseTextureScale"].SetValue(3.72f);
            windsShader.Parameters["distortionTextureScale"].SetValue(0.46f);
            windsShader.Parameters["erosionTextureScale"].SetValue(3f);
            windsShader.Parameters["erosionMin"].SetValue(0.75f);
            windsShader.Parameters["highlightsColor"].SetValue(new Color(0, 221, 250).ToVector3());

            // Cyan highlights.
            windsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(windsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawDistortionClouds_Foreground()
        {
            Effect rollingCloudsShader = CalamityShaders.DoGBackgroundFogShader;
            Texture2D cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds").Value;
            Texture2D erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise").Value;
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);

            rollingCloudsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 2f);
            rollingCloudsShader.Parameters["overallOpacity"].SetValue(1f);
            rollingCloudsShader.Parameters["distortionStrength"].SetValue(0.12f);
            rollingCloudsShader.Parameters["mainNoiseTextureScale"].SetValue(1f);
            rollingCloudsShader.Parameters["distortionTextureScale"].SetValue(0.8f);
            rollingCloudsShader.Parameters["erosionTextureScale"].SetValue(0.26f);
            rollingCloudsShader.Parameters["erosionMin"].SetValue(0.06f);
            rollingCloudsShader.Parameters["gradientPrecision"].SetValue(20f);

            rollingCloudsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            rollingCloudsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.025f);

            rollingCloudsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, new(0, 221, 250), 0.25f).ToVector3());
            rollingCloudsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.Black, Color.Fuchsia, 0.25f).ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = cloudsTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            rollingCloudsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(cloudsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawDistortionDebris_Foreground()
        {
            if (ForegroundDebris.Count <= 0)
                return;

            Texture2D rubbleTexture = TextureAssets.Projectile[ProjectileID.DeerclopsRangedProjectile].Value;
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            for (int i = 0; i < ForegroundDebris.Count; i++)
            {
                Vector2 depthFactor = new(1f / ForegroundDebris[i].Depth, 1.1f / ForegroundDebris[i].Depth);
                Vector2 drawPosition = (ForegroundDebris[i].Position - screenCenter) * depthFactor + screenCenter - Main.screenPosition;
                Rectangle frame = rubbleTexture.Frame(3, 4, ForegroundDebris[i].FrameX, ForegroundDebris[i].FrameY);
                Color debrisColor = Color.Lerp(Color.White, Color.Black, 0.8f) * ForegroundDebris[i].Opacity;
                Main.spriteBatch.Draw(rubbleTexture, drawPosition, frame, debrisColor, ForegroundDebris[i].Rotation, frame.Size() * 0.5f, ForegroundDebris[i].Scale / ForegroundDebris[i].Depth, 0, 0f);
            }
        }

        private void DrawDistortionStars_Foreground()
        {
            if (ForegroundStars.Count <= 0)
                return;

            Texture2D starTexture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            for (int i = 0; i < ForegroundStars.Count; i++)
            {
                Vector2 depthFactor = new(1f / ForegroundStars[i].Depth, 1.1f / ForegroundStars[i].Depth);
                Vector2 drawPosition = (ForegroundStars[i].Position - screenCenter) * depthFactor + screenCenter - Main.screenPosition;

                // Colored outer glow.
                Main.spriteBatch.Draw(starTexture, drawPosition, null, ForegroundStars[i].Color, 0f, starTexture.Size() * 0.5f, ForegroundStars[i].Scale, 0, 0f);
                Main.spriteBatch.Draw(starTexture, drawPosition, null, ForegroundStars[i].Color, MathHelper.PiOver2, starTexture.Size() * 0.5f, ForegroundStars[i].Scale / ForegroundStars[i].Depth, 0, 0f);

                // White inner core.
                Main.spriteBatch.Draw(starTexture, drawPosition, null, Color.White, 0f, starTexture.Size() * 0.5f, ForegroundStars[i].Scale * 0.6f, 0, 0f);
                Main.spriteBatch.Draw(starTexture, drawPosition, null, Color.White, MathHelper.PiOver2, starTexture.Size() * 0.5f, (ForegroundStars[i].Scale * 0.6f) / ForegroundStars[i].Depth, 0, 0f);
            }
        }
        #endregion
    }
}
