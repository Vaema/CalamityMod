using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Effects;
using CalamityMod.Events;
using CalamityMod.Graphics;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Placeables.FurnitureVoid;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Systems.Graphic;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
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
            /// A parallax effect applied to the arm which affects how far back it appears in the background.
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

        public List<DistortionRiftArm> MainRiftCracks;

        public List<DistortionRiftArm> OuterRiftCracks;

        public static bool CanSkyBeActive { get; private set; }

        public static float SkyIntensity { get; private set; }

        public static Color DoGSkyColor { get; private set; }

        public List<RealityCrack> RealityCracks { get; private set; } = [];

        public List<DistortionRiftArm> DistortionRiftArms { get; private set; } = [];

        public override void Update(GameTime gameTime)
        {
            DoGIndex = NPC.FindFirstNPC(ModContent.NPCType<DevourerofGodsHead>());
            GetCorrectDoGColor();

            // Initialize the rift arms and background cracks.
            if (!Initialized)
            {
                GenerateRift();
                Initialized = true;
            }

            if (CanSkyBeActive)
                SkyIntensity = MathHelper.Clamp(SkyIntensity + 0.05f, 0f, 1f);
            else
                SkyIntensity = MathHelper.Clamp(SkyIntensity - 0.05f, 0f, 1f);
        }

        private void GenerateRift()
        {
            Vector2 riftSpawnLocation = Main.LocalPlayer.Center - Vector2.UnitY * 2000f + Main.rand.NextVector2Circular(450f, 450f);
            for (int i = 0; i < 2; i++)
            {
                RealityCrack realityCrack = new RealityCrack()
                {
                    Position = riftSpawnLocation,
                    Depth = 30f,
                    Scale = 2.25f + i,
                    Rotation = i == 0 ? 0f : MathHelper.Pi,
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
                    ArmStartingPosition = riftSpawnLocation,
                    Depth = 30f,
                    TotalPoints = 8,

                    MaxWidth = Main.rand.NextFloat(120f, 140f),
                    MinDistanceBetweenPoints = 80f,
                    MaxDistanceBetweenPoints = 120f,
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
                    ArmStartingPosition = riftSpawnLocation,
                    Depth = 30f,
                    TotalPoints = Main.rand.Next(6, 10),

                    MaxWidth = Main.rand.NextFloat(6f, 9f),
                    MinDistanceBetweenPoints = 500f,
                    MaxDistanceBetweenPoints = 600f,
                    MinPointAngularVariance = Main.rand.Next(-10, -5),
                    MaxPointAngularVariance = Main.rand.Next(5, 10),

                    ArmPoints = [],
                };
                OuterRiftCracks.Add(distortionRiftArm);
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
                    MainRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(MainRiftCracks[i].ArmStartingPosition, MainRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);
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
                    OuterRiftCracks[i].ArmPoints = GenerateDistortionRiftArmPoints(OuterRiftCracks[i].ArmStartingPosition, OuterRiftCracks[i].TotalPoints, minDistance, maxDistance, minAngularVariance, maxAngularVariance, placementAngle);
                DistortionRiftArms.Add(OuterRiftCracks[i]);
            }
        }

        private static List<Vector2> GenerateDistortionRiftArmPoints(Vector2 startingPosition, int totalPoints, float minDistanceBetweenPoints, float maxDistanceBetweenPoints, float minAngularVariance, float maxAngularVariance, float? optionalPointPlacementAngle = null)
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

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (Main.gameMenu)
                return;

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
            Color twlight = new(109, 21, 150);
            Color lightBlue = new(0, 221, 250);

            if (DoGIndex != -1)
            {
                var DoG = Main.npc[DoGIndex].ModNPC<DevourerofGodsHead>();
                Color goalSkyColor = Color.Black;
                if (DoG.isInAgressiveState)
                    goalSkyColor = Color.Fuchsia;
                if (DoG.isInPassiveState)
                    goalSkyColor = lightBlue;
                if (DoG.isInLaserWallState || DoG.isInPostWallState || DoG.postTeleportTimer > 0 || DoG.teleportTimer > 0 || DoG.NPC.localAI[2] < 180 && DoG.NPC.localAI[2] > 60)
                    goalSkyColor = twlight;

                //if (DoG.isInPostWallState || DoG.postTeleportTimer > 0 || DoG.teleportTimer > 0 || DoG.NPC.localAI[2] < 180 && DoG.NPC.localAI[2] > 60)
                //{
                //    if (DoG.Phase2Started)
                //        goalSkyColor = Color.Black;
                //    else
                //        goalSkyColor = new Color(117, 21, 161);
                //}

                DoGSkyColor = Color.Lerp(DoGSkyColor, goalSkyColor, 0.1f);
            }
            else
            {
                // Adopt the twlight color during laser walls if both monoliths are active simultaneously.
                // Otherwise, use each phase's individual color depending on the monolith's color.
                var goalSkyColor = Color.Black;
                if (Main.LocalPlayer.Calamity().monolithDevourerPShader > 0)
                    goalSkyColor = Color.Fuchsia;
                if (Main.LocalPlayer.Calamity().monolithDevourerBShader > 0)
                    goalSkyColor = lightBlue;

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
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity * 0.6f);
            shader.Parameters["distortionStrength"].SetValue(0.3f);
            shader.Parameters["mainNoiseTextureScale"].SetValue(0.8f);
            shader.Parameters["distortionTextureScale"].SetValue(0.6f);
            shader.Parameters["erosionTextureScale"].SetValue(2f);
            shader.Parameters["erosionMin"].SetValue(0.38f * SkyIntensity);
            shader.Parameters["gradientPrecision"].SetValue(20f);

            shader.Parameters["pixelationFactor"].SetValue(screenSize * 0.5f);
            shader.Parameters["worldOffset"].SetValue(Main.screenPosition / windsTexture.Size() * 0.025f);

            shader.Parameters["darkerPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.64f).ToVector3());
            shader.Parameters["brighterPixelColor"].SetValue(Color.Lerp(Color.DarkGray, Color.Black, 0.32f).ToVector3());
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
            shader.Parameters["overallOpacity"].SetValue(SkyIntensity * 0.25f);
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

            shader.Parameters["opacityCutoffValue"].SetValue(0.8f * SkyIntensity);
            shader.Parameters["fadeoutPower"].SetValue(1f);
            shader.Parameters["overallOpacity"].SetValue(0.2f);
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
            Texture2D distortionRiftContents = DoGVisualsManager.DistortionRiftBackgroundContentsTarget;
            Texture2D distortionRift = DoGVisualsManager.DistortionRiftPrimitivesTarget;

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

        public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, DoGSkyColor, SkyIntensity);

        public override float GetCloudAlpha() => 1f;

        public override void Activate(Vector2 position, params object[] args) => CanSkyBeActive = true;

        public override bool IsActive() => CanSkyBeActive || SkyIntensity > 0f;

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
    }
}
