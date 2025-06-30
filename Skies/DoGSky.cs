using System.Collections.Generic;
using System.Linq;
using CalamityMod.Effects;
using CalamityMod.Events;
using CalamityMod.Graphics;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Skies
{
    public class DoGSky : CustomSky
    {
        public class RealityCrack
        {
            public Vector2 Position;

            public float Depth;

            public float Scale;

            public float Rotation;
        }

        public class DistortionRiftArm
        {
            /// <summary>
            /// The starting position of the arm.
            /// </summary>
            public Vector2 ArmStartingPosition;

            /// <summary>
            /// A list of the positions for each point which makes up the arm.
            /// </summary>
            public List<Vector2> ArmPoints;

            /// <summary>
            /// The depth of the rift arm.
            /// </summary>
            public float Depth;

            /// <summary>
            /// The maximum width of the rift arm.
            /// </summary>
            /// <remarks>
            /// Note that this width tapers off till the end.
            /// </remarks>
            public float MaxWidth;

            /// <summary>
            /// The maximum amount of distance which can be between two points.
            /// </summary>
            public float MinDistanceBetweenPoints;

            /// <summary>
            /// The maximum amount of distance which can be between two points.
            /// </summary>
            public float MaxDistanceBetweenPoints;

            /// <summary>
            /// The minimum angle at which the points along the arm can be placed at randomly.
            /// </summary>
            /// <remarks>
            /// Should ALWAYS be set as degrees.
            /// </remarks>
            public float MinPointAngularVariance;

            /// <summary>
            /// The minimum angle at which the points along the arm can be placed at randomly.
            /// </summary>
            /// <remarks>
            /// Should ALWAYS be set as degrees.
            /// </remarks>
            public float MaxPointAngularVariance;

            /// <summary>
            /// The total amount of points that make up this rift arm.
            /// </summary>
            public int TotalPoints;
        }

        public bool Initialized;

        public int DoGIndex;

        private List<DistortionRiftArm> MainRiftCracks;

        private List<DistortionRiftArm> OuterRiftCracks;

        public static bool CanSkyBeActive { get; private set; }

        public static float SkyIntensity { get; private set; }

        public static Color DoGSkyColor { get; private set; }

        public List<RealityCrack> RealityCracks { get; private set; } = [];

        public List<DistortionRiftArm> DistortionRiftArms { get; private set; } = [];

        public override void Update(GameTime gameTime)
        {
            DoGIndex = NPC.FindFirstNPC(ModContent.NPCType<DevourerofGodsHead>());
            GetCorrectDoGColor();

            // Initialize the reality cracks and distortion rift.
            if (!Initialized)
            {
                for (int i = 0; i < 3; i++)
                {
                    RealityCrack realityCrack = new RealityCrack()
                    {
                        Position = Main.LocalPlayer.Center,
                        Depth = 30f,
                        Scale = 2.75f,
                        Rotation = Main.rand.NextFloat(MathHelper.TwoPi)
                    };
                    RealityCracks.Add(realityCrack);
                }

                // Generate up to 30 arms for the rift.
                // The first 14 are larger cracks to form the main hole at the impact point.
                // The rest are longer, skinner cracks protruding outwards from it.
                MainRiftCracks = [];
                OuterRiftCracks = [];
                for (int i = 0; i < 14; i++)
                {
                    DistortionRiftArm distortionRiftArm = new DistortionRiftArm()
                    {
                        ArmStartingPosition = Main.LocalPlayer.Center,
                        Depth = 30f,
                        TotalPoints = 8,

                        MaxWidth = Main.rand.NextFloat(120f, 140f),
                        MinDistanceBetweenPoints = 40f,
                        MaxDistanceBetweenPoints = 80f,
                        MinPointAngularVariance = -6,
                        MaxPointAngularVariance = 6,

                        ArmPoints = [],
                    };
                    MainRiftCracks.Add(distortionRiftArm);
                }

                int outerCracksAmount = Main.rand.Next(12, 17);
                for (int i = 0; i < outerCracksAmount; i++)
                {
                    DistortionRiftArm distortionRiftArm = new DistortionRiftArm()
                    {
                        ArmStartingPosition = Main.LocalPlayer.Center,
                        Depth = 30f,
                        TotalPoints = Main.rand.Next(6, 10),

                        MaxWidth = Main.rand.NextFloat(4f, 7f),
                        MinDistanceBetweenPoints = 450f,
                        MaxDistanceBetweenPoints = 550f,
                        MinPointAngularVariance = Main.rand.Next(-10, -5),
                        MaxPointAngularVariance = Main.rand.Next(5, 10),

                        ArmPoints = [],
                    };
                    OuterRiftCracks.Add(distortionRiftArm);
                }

                Initialized = true;
            }

            // Generate the points for each arm and add them all to the global arm list.
            // Main cracks.
            for (int i = 0; i < MainRiftCracks.Count; i++)
            {
                float minDistance = MainRiftCracks[i].MinDistanceBetweenPoints * MainRiftCracks[i].Depth * 0.1f;
                float maxDistance = MainRiftCracks[i].MaxDistanceBetweenPoints * MainRiftCracks[i].Depth * 0.1f;
                float minAngularVariance = MainRiftCracks[i].MinPointAngularVariance;
                float maxAngularVariance = MainRiftCracks[i].MaxPointAngularVariance;
                float placementAngle = i * MathHelper.TwoPi / MainRiftCracks.Count;

                if (MainRiftCracks[i].ArmPoints.Count <= 0)
                    MainRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(Main.LocalPlayer.Center, MainRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);

                if (!DistortionRiftArms.Contains(MainRiftCracks[i]))
                    DistortionRiftArms.Add(MainRiftCracks[i]);
            }

            // Outer cracks.
            for (int i = 0; i < OuterRiftCracks.Count; i++)
            {
                float minDistance = OuterRiftCracks[i].MinDistanceBetweenPoints * OuterRiftCracks[i].Depth * 0.1f;
                float maxDistance = OuterRiftCracks[i].MaxDistanceBetweenPoints * OuterRiftCracks[i].Depth * 0.1f;
                float minAngularVariance = OuterRiftCracks[i].MinPointAngularVariance;
                float maxAngularVariance = OuterRiftCracks[i].MaxPointAngularVariance;
                float placementAngle = i * MathHelper.TwoPi / OuterRiftCracks.Count;

                if (OuterRiftCracks[i].ArmPoints.Count <= 0)
                    OuterRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(Main.LocalPlayer.Center, OuterRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);

                if (!DistortionRiftArms.Contains(OuterRiftCracks[i]))
                    DistortionRiftArms.Add(OuterRiftCracks[i]);
            }

            if (CanSkyBeActive)
                SkyIntensity = MathHelper.Clamp(SkyIntensity + 0.05f, 0f, 1f);
            else
                SkyIntensity = MathHelper.Clamp(SkyIntensity - 0.05f, 0f, 1f);
        }

        private List<Vector2> GenerateDistortionRiftArmPoints(Vector2 startingPosition, int totalPoints, float minDistanceBetweenPoints, float maxDistanceBetweenPoints, float minAngularVariance, float maxAngularVariance, float? optionalPointPlacementAngle = null)
        {
            // (Original code by Xyk; See TriactisHammerExplosion)
            // -fryzahh

            List<Vector2> points = [];
            for (int j = 0; j < totalPoints; j++)
            {
                // First point should always be the starting position.
                if (j == 0)
                {
                    points.Add(startingPosition);
                }
                // Next point should be a random location around the starting position.
                // If the placement angle has a value then rotate towards that with a very slight offset.
                else if (j == 1)
                {
                    float distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints);
                    Vector2 nextPointPosition = startingPosition + Main.rand.NextVector2Unit() * distanceFromLastPoint;
                    if (optionalPointPlacementAngle != null)
                        nextPointPosition = startingPosition + Vector2.UnitX.RotatedBy(optionalPointPlacementAngle.Value) * distanceFromLastPoint;
                    points.Add(nextPointPosition);
                }
                // All other points follow after the last one with the same slight offset to resemble cracks.
                else
                {
                    Vector2 previousPoint = points[j - 2];
                    float distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints);
                    if (j == totalPoints - 1)
                        distanceFromLastPoint = Main.rand.NextFloat(minDistanceBetweenPoints, maxDistanceBetweenPoints) * 0.5f;

                    Vector2 newPoint = points[j - 1] + (previousPoint.DirectionTo(points[j - 1]) * distanceFromLastPoint).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(minAngularVariance, maxAngularVariance) * j * 0.5f));
                    points.Add(newPoint);
                }
            }

            return points;
        }


        public override Color OnTileColor(Color inColor) 
        {
            return Color.Lerp(inColor, DoGSkyColor, SkyIntensity);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the wind effect.
            if (minDepth >= float.MinValue && maxDepth <= 5f)
                DrawDistortionWinds(spriteBatch);

            // Draw the rolling fog effect.
            if (minDepth >= 3f && maxDepth <= 10f)
                DrawRollingBackgroundFog(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the cracked glass textures.
            if (minDepth >= 6f && maxDepth <= float.MaxValue)
                DrawRealityCracks(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the rift to the Distortion over the cracked glass.
            if (minDepth >= 6f && maxDepth <= float.MaxValue)
                DrawDistortionRift(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
        }

        public void DrawRiftToRenderTarget()
        {
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);

            for (int i = 0; i < DistortionRiftArms.Count; i++)
            {
                Vector2 depthFactor = new(1f / DistortionRiftArms[i].Depth, 1.1f / DistortionRiftArms[i].Depth);
                List<Vector2> parallaxedPoints = [];
                for (int j = 0; j < DistortionRiftArms[i].ArmPoints.Count - 1; j++)
                    parallaxedPoints.Add((DistortionRiftArms[i].ArmPoints[j] - screenCenter) * depthFactor + screenCenter);

                PrimitiveRenderer.RenderTrail(parallaxedPoints, new((completion) => MathHelper.Lerp(0f, DistortionRiftArms[i].MaxWidth, 1f - completion) * SkyIntensity, (_) => Color.White, null, false, useUnscaledMatrices: true), DistortionRiftArms[i].TotalPoints + 16);
            }
        }

        private void GetCorrectDoGColor()
        {
            if (DoGIndex != -1)
            {
                var DoG = Main.npc[DoGIndex].ModNPC<DevourerofGodsHead>();
                var goalSkyColor = Color.Black;
                if (DoG.isInAgressiveState)
                    goalSkyColor = Color.Fuchsia;
                if (DoG.isInPassiveState)
                    goalSkyColor = Color.Cyan;
                if (DoG.isInLaserWallState)
                    goalSkyColor = new Color(117, 21, 161);

                if (DoG.isInPostWallState || DoG.postTeleportTimer > 0 || DoG.teleportTimer > 0 || DoG.NPC.localAI[2] < 180 && DoG.NPC.localAI[2] > 60)
                {
                    if (DoG.Phase2Started)
                        goalSkyColor = Color.Black;
                    else
                        goalSkyColor = new Color(117, 21, 161);
                }

                DoGSkyColor = Color.Lerp(DoGSkyColor, goalSkyColor, 0.1f);
            }
            else
            {
                var goalSkyColor = Color.Black;
                if (Main.LocalPlayer.Calamity().monolithDevourerPShader > 0)
                    goalSkyColor = Color.Fuchsia;
                if (Main.LocalPlayer.Calamity().monolithDevourerBShader > 0)
                    goalSkyColor = Color.Cyan;

                DoGSkyColor = Color.Lerp(DoGSkyColor, goalSkyColor, 0.1f);
            }
        }

        private void DrawDistortionWinds(SpriteBatch spriteBatch)
        {
            Effect shader = CalamityShaders.DoGDistortionWindsShader;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Asset<Texture2D> windsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2");
            Asset<Texture2D> highlightsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/SharpNoise");
            Asset<Texture2D> distortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/MeltyNoise");
            Asset<Texture2D> erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Pebbles");

            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity * 0.55f);
            shader.Parameters["distortionStrength"].SetValue(0.3f);
            shader.Parameters["mainNoiseTextureScale"].SetValue(0.8f);
            shader.Parameters["distortionTextureScale"].SetValue(0.6f);
            shader.Parameters["erosionTextureScale"].SetValue(2f);
            shader.Parameters["erosionMin"].SetValue(0.38f * SkyIntensity);
            shader.Parameters["gradientPrecision"].SetValue(20f);

            shader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            shader.Parameters["worldOffset"].SetValue(Main.screenPosition / windsTexture.Size() * 0.025f);

            shader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.8f).ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.6f).ToVector3());
            shader.Parameters["highlightsColor"].SetValue(DoGSkyColor.ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = highlightsTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointWrap;

            Main.instance.GraphicsDevice.Textures[2] = distortionTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[3] = erosionTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;

            shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(windsTexture.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawRollingBackgroundFog(SpriteBatch spriteBatch)
        {
            Effect shader = CalamityShaders.DoGBackgroundFogShader;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Asset<Texture2D> cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds");
            Asset<Texture2D> erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise");

            shader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 3f);
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity * 0.18f);
            shader.Parameters["distortionStrength"].SetValue(0.12f);
            shader.Parameters["mainNoiseTextureScale"].SetValue(0.6f);
            shader.Parameters["distortionTextureScale"].SetValue(0.8f);
            shader.Parameters["erosionTextureScale"].SetValue(0.26f);
            shader.Parameters["erosionMin"].SetValue(0.06f * SkyIntensity);
            shader.Parameters["gradientPrecision"].SetValue(20f);

            shader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            shader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.005f);

            shader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, Color.DarkGray, 0.75f).ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, DoGSkyColor, 0.8f).ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = cloudsTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            shader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(cloudsTexture.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private void DrawRealityCracks(SpriteBatch spriteBatch)
        {
            Effect shader = CalamityShaders.CircularOpacityShader;
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 screenCenter = Main.screenPosition + (screenSize * 0.5f);
            Asset<Texture2D> cracksTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/CrackedGlass_Glowing");

            shader.Parameters["opacityCutoffValue"].SetValue(0.75f * SkyIntensity);
            shader.Parameters["fadeoutPower"].SetValue(1.75f);
            shader.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < RealityCracks.Count; i++)
            {
                Vector2 depthFactor = new(1f / RealityCracks[i].Depth, 1.1f / RealityCracks[i].Depth);
                Vector2 drawPosition = (RealityCracks[i].Position - screenCenter) * depthFactor + screenCenter - Main.screenPosition;
                spriteBatch.Draw(cracksTexture.Value, drawPosition, new Rectangle(0, 0, cracksTexture.Width(), cracksTexture.Height()), Color.White, RealityCracks[i].Rotation, cracksTexture.Size() * 0.5f, RealityCracks[i].Scale * 0.5f, 0, 0f);
            }
        }

        private void DrawDistortionRift(SpriteBatch spriteBatch)
        {
            // Impose the distortion rift contents onto the primitives via shader.
            var metaballShader = CalamityShaders.MetaballEdgeShader;
            Texture2D distortionRiftContents = DoGWorldVisualChangesManager.DistortionRiftBackgroundContentsTarget;
            Texture2D distortionRift = DoGWorldVisualChangesManager.DistortionRiftPrimitivesTarget;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);

            metaballShader.Parameters["layerSize"]?.SetValue(distortionRiftContents.Size());
            metaballShader.Parameters["screenSize"]?.SetValue(screenSize);
            metaballShader.Parameters["layerOffset"]?.SetValue(Vector2.Zero);
            metaballShader.Parameters["edgeColor"]?.SetValue(Color.Lerp(DoGSkyColor, Color.Black, 0.85f).ToVector4());

            Main.instance.GraphicsDevice.Textures[1] = distortionRiftContents;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            metaballShader.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(distortionRift, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }
       
        public override float GetCloudAlpha()
        {
            return 1f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            CanSkyBeActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            CanSkyBeActive = false;
            RealityCracks.Clear();
            DistortionRiftArms.Clear();
            Initialized = false;
        }

        public override void Reset()
        {
            CanSkyBeActive = false;
            RealityCracks.Clear();
            DistortionRiftArms.Clear();
            Initialized = false;
        }

        public override bool IsActive()
        {
            return CanSkyBeActive || SkyIntensity > 0f;
        }
    }

    public class DoGWorldVisualChangesManager : ModSystem
    {
        public float FillProgress = 0;

        public static ManagedRenderTarget DistortionRiftBackgroundContentsTarget { get; private set; }

        public static ManagedRenderTarget DistortionRiftPrimitivesTarget { get; private set; }

        public override void OnModLoad()
        {
            if (!Main.dedServ)
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
                backgroundColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, Main.ColorOfTheSkies.R * 0.075f, FillProgress);
                backgroundColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, Main.ColorOfTheSkies.G * 0.075f, FillProgress);
                backgroundColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, Main.ColorOfTheSkies.B * 0.075f, FillProgress);
                backgroundColor = new(backgroundColor.ToVector3() + colorToUse.ToVector3() * 0.025f * FillProgress);

                tileColor.R = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.R, 100f, FillProgress);
                tileColor.G = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.G, 100f, FillProgress);
                tileColor.B = (byte)MathHelper.Lerp(Main.ColorOfTheSkies.B, 100f, FillProgress);
                tileColor = new(tileColor.ToVector3() + colorToUse.ToVector3() * 0.075f * FillProgress);
            }
        }

        private void DiscardCelestialObjects(On_Main.orig_DrawSunAndMoon orig, Main self, Main.SceneArea sceneArea, Color moonColor, Color sunColor, float tempMushroomInfluence)
        {
            if (DoGSky.SkyIntensity > 0f)
                tempMushroomInfluence = DoGSky.SkyIntensity;

            orig(self, sceneArea, moonColor, sunColor, tempMushroomInfluence);
        }

        private void PrepareTargets()
        {
            DistortionRiftBackgroundContentsTarget.SwapTo();

            // Draw the background which'll be displayed inside the rift onto a render target.
            DrawDistortionRiftBackground();

            DistortionRiftPrimitivesTarget.SwapTo();

            // Draw the primitives which compromise the rift to another render target.
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);
            if (SkyManager.Instance["CalamityMod:DevourerofGodsHead"].IsActive())
                (SkyManager.Instance["CalamityMod:DevourerofGodsHead"] as DoGSky).DrawRiftToRenderTarget();
            Main.spriteBatch.End();

            Main.instance.GraphicsDevice.SetRenderTarget(null);
        }

        private static void DrawDistortionRiftBackground()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw a black tile behind everything since the shader used to generate the clouds will appear transparent with dark colors.
            Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the layer of rolling clouds in the background.
            DrawDistortionBackgroundClouds();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the small stars that twinkle and fade out quickly.
            DrawDistortionBackgroundStars();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, CalamityUtils.BackgroundMatrix);

            // Draw the purple lightning bolts
            DrawDistortionBackgroundLighting();

            Main.spriteBatch.End();
        }

        private static void DrawDistortionBackgroundClouds()
        {
            Effect rollingCloudsShader = CalamityShaders.DoGBackgroundFogShader;

            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Asset<Texture2D> cloudsTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/RealisticClouds");
            Asset<Texture2D> erosionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/HarshNoise");

            rollingCloudsShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            rollingCloudsShader.Parameters["overallOpacity"].SetValue(0.75f);
            rollingCloudsShader.Parameters["distortionStrength"].SetValue(0.12f);
            rollingCloudsShader.Parameters["mainNoiseTextureScale"].SetValue(2f);
            rollingCloudsShader.Parameters["distortionTextureScale"].SetValue(0.8f);
            rollingCloudsShader.Parameters["erosionTextureScale"].SetValue(0.26f);
            rollingCloudsShader.Parameters["erosionMin"].SetValue(0.06f);
            rollingCloudsShader.Parameters["gradientPrecision"].SetValue(20f);

            rollingCloudsShader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            rollingCloudsShader.Parameters["worldOffset"].SetValue(Main.screenPosition / cloudsTexture.Size() * 0.001f);

            rollingCloudsShader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.Black, Color.DarkGray, 0.25f).ToVector3());
            rollingCloudsShader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.Black, DoGSky.DoGSkyColor, 0.4f).ToVector3());

            Main.instance.GraphicsDevice.Textures[1] = cloudsTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            Main.instance.GraphicsDevice.Textures[2] = erosionTexture.Value;
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            rollingCloudsShader.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(cloudsTexture.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
        }

        private static void DrawDistortionBackgroundLighting()
        {
            // TODO
        }

        private static void DrawDistortionBackgroundStars()
        {
            // TODO
        }
    }
}
