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
        public float FillProgress = 0;

        public float BackgroundLightningFill;

        public float BackgroundLightningMax;

        public float BackgroundLightningTimer;

        /// <summary>
        /// Whether or not the background render targets should have their contents drawn to them.
        /// </summary>
        public static bool ShouldDrawToBackgroundTargets => DoGSky.SkyIntensity > 0f;

        /// <summary>
        /// Whether or not the foreground render target should have its contents drawn to it.
        /// </summary>
        public static bool ShouldDrawToForegroundTarget => MetaballManager.metaballs.Any(m => m.AnythingToDraw && m is DoGDistortionMetaball);

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
            if (DoGSky.SkyIntensity > 0.1f)
            {
                // Disable ambient sky entities while DoG's background effects are active.
                if (SkyManager.Instance["Ambience"].IsActive())
                    SkyManager.Instance["Ambience"].Deactivate();

                if (CalamityClientConfig.Instance.FancyBackgroundVisuals)
                {
                    // Randomly flicker between different values to create a natural lightning effect in the background of the 
                    // Distortion rift's clouds.
                    if (Main.rand.NextBool(200) && DoGSky.SkyIntensity > 0f && BackgroundLightningTimer <= 0f)
                    {
                        BackgroundLightningTimer = Main.rand.NextBool(10) ? Main.rand.Next(30, 45) : Main.rand.Next(5, 20);
                        BackgroundLightningMax = Main.rand.NextFloat(0.7f, 0.9f);
                    }

                    if (BackgroundLightningTimer > 0f)
                    {
                        float minFill = BackgroundLightningMax * 0.5f;
                        BackgroundLightningFill = Main.rand.NextFloat(minFill, BackgroundLightningMax);
                        BackgroundLightningTimer--;
                    }
                    else
                    {
                        if (BackgroundLightningTimer < 0f)
                            BackgroundLightningTimer = 0f;
                        BackgroundLightningFill = MathHelper.Lerp(BackgroundLightningFill, 0f, 0.05f);
                    }
                }
            }
        }

        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            if (DoGSky.SkyIntensity > 0f)
                FillProgress += 0.05f;
            else
                FillProgress -= 0.05f;

            // Darken the lighting color of both the background and foreground.
            FillProgress = MathHelper.Clamp(FillProgress, 0, 1);
            if (FillProgress > 0)
            {
                Color colorToUse = DoGSky.DoGSkyColor;
                float tileBrightness = CalamityClientConfig.Instance.FancyBackgroundVisuals ? 35f : 190f;

                backgroundColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, Color.White.R * 0.035f, FillProgress);
                backgroundColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, Color.White.G * 0.035f, FillProgress);
                backgroundColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, Color.White.B * 0.035f, FillProgress);
                backgroundColor = new(backgroundColor.ToVector3() + colorToUse.ToVector3() * 0.025f * FillProgress);

                tileColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, tileBrightness, FillProgress);
                tileColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, tileBrightness, FillProgress);
                tileColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, tileBrightness, FillProgress);

                float tileColorAdditive = CalamityClientConfig.Instance.FancyBackgroundVisuals ? 0.075f : 0.4f;
                tileColor = new(tileColor.ToVector3() + colorToUse.ToVector3() * tileColorAdditive * FillProgress);
            }
        }

        private void DiscardCelestialObjects(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
        {
            // Fade out the sun and moon during DoG's fight.
            if (DoGSky.SkyIntensity > 0f)
            {
                tempMushroomInfluence = DoGSky.SkyIntensity;
                moonColor *= 1f - DoGSky.SkyIntensity;
            }

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

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

                // Draw the primitives which comprise the rift to another render target.
                DistortionRiftPrimitivesTarget.SwapTo();                
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
            DrawDistortionLightningBackdrop_Background();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the layer of rolling clouds in the background.
            DrawDistortionClouds_Background();

            Main.spriteBatch.End();
        }

        private void DrawDistortionLightningBackdrop_Background()
        {
            // Flicker between white and black for the lightning effect.
            Color backdropColor = Color.Lerp(Color.Lerp(Color.Black, Color.White, BackgroundLightningFill), DoGSky.DoGSkyColor, 0.25f);
            Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), backdropColor);
        }

        private void DrawDistortionClouds_Background()
        {
            Effect rollingCloudsShader = CalamityShaders.DoGBackgroundFogShader;
            Texture2D cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds").Value;
            Texture2D distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2").Value;
            Texture2D erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise").Value;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);           
            Color darkPixelColor = Color.Lerp(Color.Lerp(Color.Black, Color.DarkGray, 0.3f), Color.Black, BackgroundLightningFill * 0.8f);
            Color brightPixelColor = Color.Lerp(Color.Lerp(Color.Black, DoGSky.DoGSkyColor, 0.6f), Color.Black, BackgroundLightningFill * 0.8f);

            rollingCloudsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            rollingCloudsShader.Parameters["overallOpacity"].SetValue(1f);
            rollingCloudsShader.Parameters["distortionStrength"].SetValue(0.24f);
            rollingCloudsShader.Parameters["mainNoiseTextureScale"].SetValue(2f);
            rollingCloudsShader.Parameters["distortionTextureScale"].SetValue(0.8f);
            rollingCloudsShader.Parameters["erosionTextureScale"].SetValue(0.26f);
            rollingCloudsShader.Parameters["erosionMin"].SetValue(0.06f);
            rollingCloudsShader.Parameters["gradientPrecision"].SetValue(20f);

            rollingCloudsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            rollingCloudsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.001f);

            rollingCloudsShader.Parameters["darkerPixelColor"].SetValue(darkPixelColor.ToVector3());
            rollingCloudsShader.Parameters["brighterPixelColor"].SetValue(brightPixelColor.ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = distortionTexture;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            rollingCloudsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(cloudsTexture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }
        #endregion  

        #region Distortion Foreground Drawing
        private void DrawDistortionForeground()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw a black tile for the same reasoning explained above for the background drawing.
            Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
            
            // Draw a layer of rolling clouds in the background.
            DrawDistortionClouds_Foreground();

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
            windsShader.Parameters["mainNoiseTextureScale"].SetValue(1.6f);
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
            windsShader.Parameters["mainNoiseTextureScale"].SetValue(1.24f);
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
        #endregion
    }
}
