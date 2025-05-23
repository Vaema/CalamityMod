using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.MainMenu
{
    public class SunkenFishBoid
    {
        // -- GENERAL VARIABLES FOR DEFINING GENERAL BEHAVIOR AND FUNCTIONALITY FOR THE FISH ENTITIES -- 

        public enum FishType
        {
            SeaMinnow,
            PolypPanasea,
            PrismaticGuppy1,
            PrismaticGuppy2,
            PrismaticGuppy3,
        }

        public string[] SeaMinnowTextureNames =
        [
            "SeaMinnow",
            "AlphaSeaMinnow",
        ];

        public string[] PolypPanaseaTextureNames =
        [
            "PolypPanasea",
            "PolypPanaseaGreen",
            "PolypPanaseaPurple",
            "PolypPanaseaRadiant",
            "PolypPanaseaTurquoise",
            "PolypPanaseaGreenCoated",
            "PolypPanaseaPurpleCoated",
            "PolypPanaseaRadiantCoated",
            "PolypPanaseaRedCoated",
            "PolypPanaseaTurquoiseCoated",
        ];

        public string[] PrismaticGuppyTextureNames_1 =
        [
            "PrismaticGuppy",
            "PrismaticGuppyGreen",
            "PrismaticGuppyPink",
            "PrismaticGuppyRadiant",
        ];

        public string[] PrismaticGuppyTextureNames_2 =
        [
            "PrismaticGuppy2",
            "PrismaticGuppyGreen2",
            "PrismaticGuppyPink2",
            "PrismaticGuppyRadiant2",
        ];

        public string[] PrismaticGuppyTextureNames_3 =
        [
            "PrismaticGuppy3",
            "PrismaticGuppyGreen3",
            "PrismaticGuppyPink3",
            "PrismaticGuppyRadiant3",
        ];

        public int Time;

        public int Lifetime;

        public int Depth;

        public int CurrentFrame;

        public int MaxFrames;

        public Vector2 Position;

        public Vector2 Velocity;

        public float Scale;

        public float StoredScale;

        public float Rotation;

        public float Opacity;

        public float PassiveMovementTimer;

        public Vector2 PassiveMovementVector;

        public float PassiveMovementSpeed;

        public float MaxSpeed = 2f;

        public FishType SelectedFishType;

        public int FishTextureIndex = -1;

        public bool Initialized = false;

        // -- BOID ALOGORITHM SPECIFIC VARIABLES -- 

        // 19MAY2025: fryzahh:
        // Coefficients to control how strongly each of the Boid Algos affect the movement of each boid.
        // Higher values will lead to sharper flocking AI, but movement might be a little choppier.
        // Please beware of this if you ever change these values.
        public float CohesionCoefficient = 0.015f;

        public float AlignmentCoefficient = 0.035f;

        public float SeparationCoefficient = 0.055f;

        public float MaxRadiusFromOtherFish = 75f;

        public float MaxDetectionRadius = 125f;

        /// <summary>
        /// The ratio of how much time a fish has left to live before despawning.
        /// </summary>
        public float LifetimeInterpolant => Time / (float)Lifetime;

        /// <summary>
        /// Whether or not this fish boid is any of the Prismatic Guppy variants. Used for allowing allow Prismatic Guppies to school with each other.
        /// </summary>
        public bool IsAPrismaticGuppy => SelectedFishType == FishType.PrismaticGuppy1 || SelectedFishType == FishType.PrismaticGuppy2 || SelectedFishType == FishType.PrismaticGuppy3;

        public SunkenFishBoid(Vector2 position, float scale, int lifetime, int depth)
        {
            Position = position;
            StoredScale = scale;
            Lifetime = lifetime;
            Depth = depth;

            SelectedFishType = (FishType)Main.rand.Next(0, 5);
        }

        public void Update()
        {
            if (!Initialized)
            {
                // Select the correct texture index for the different types of fish textures.
                if (SelectedFishType == FishType.SeaMinnow)
                    FishTextureIndex = Main.rand.Next(SeaMinnowTextureNames.Length);
                if (SelectedFishType == FishType.PolypPanasea)
                    FishTextureIndex = Main.rand.Next(PolypPanaseaTextureNames.Length);
                if (SelectedFishType == FishType.PrismaticGuppy1 || SelectedFishType == FishType.PrismaticGuppy2 || SelectedFishType == FishType.PrismaticGuppy3)
                    FishTextureIndex = Main.rand.Next(PrismaticGuppyTextureNames_1.Length);

                // Select the correct amount of animation frames each texture has based on the fish type.
                if (SelectedFishType == FishType.SeaMinnow)
                    MaxFrames = 8;
                if (SelectedFishType == FishType.PolypPanasea)
                    MaxFrames = 6;
                if (SelectedFishType == FishType.PrismaticGuppy1)
                    MaxFrames = 6;
                if (SelectedFishType == FishType.PrismaticGuppy2)
                    MaxFrames = 4;
                if (SelectedFishType == FishType.PrismaticGuppy3)
                    MaxFrames = 5;

                Initialized = true;
            }

            // Don't run anything if the previous two variables haven't been intialized properly.
            if (!Initialized)
                return;

            // Scale up and out accordingly depending on the remaining lifetime of the fish.
            if (LifetimeInterpolant < 0.07f)
            {
                Scale = MathHelper.Lerp(0f, StoredScale, LifetimeInterpolant / 0.07f);
                Opacity = MathHelper.Lerp(0f, 1f, LifetimeInterpolant / 0.07f);
            }
            else if (LifetimeInterpolant > 0.9f)
            {
                Scale = MathHelper.Lerp(StoredScale, 0f, (LifetimeInterpolant - 0.9f) / 0.1f);
                Opacity = MathHelper.Lerp(1f, 0f, (LifetimeInterpolant - 0.9f) / 0.1f);
            }

            // Rotate towards the movement direction.
            Rotation = Velocity.ToRotation();

            // Animate.
            if (Time % 6 == 0)
                CurrentFrame = (CurrentFrame + 1) % MaxFrames;

            // Perform all Boids Behavior.
            DoBoidsBehavior();

            // Move and swim around idly.
            PassiveMovementTimer--;
            if (PassiveMovementTimer <= 0f)
            {
                PassiveMovementSpeed = Main.rand.NextFloat(0.05f, 1.45f);
                PassiveMovementVector.X = Main.rand.NextFloat(-100f, 101f);
                PassiveMovementVector.Y = Main.rand.NextFloat(-100f, 101f);
                PassiveMovementTimer = Main.rand.Next(120, 180);
            }

            float moveSpeed = PassiveMovementSpeed / PassiveMovementVector.Length();
            Velocity = Vector2.Lerp(Velocity, PassiveMovementVector * moveSpeed, 0.01f);

            // Make fishes closest to the screen swim away from the mouse cursor if it's nearby.
            float distanceFromCursor = Vector2.Distance(Position, Main.MouseScreen);
            if (distanceFromCursor < 100f && Depth <= 1)
            {
                float distanceInterpolant = Utils.GetLerpValue(100f, 20f, distanceFromCursor, true);
                Velocity += Position.DirectionTo(Main.MouseScreen).SafeNormalize(Vector2.UnitY) * distanceInterpolant * -0.8f;
            }

            Velocity = Velocity.ClampMagnitude(0f, 1.45f);
            Position += Velocity;

            Time++;
        }
        
        public void DoBoidsBehavior()
        {
            Vector2 cohesion = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 separation = Vector2.Zero;
            int totalSchoolMembers = 0;

            float detetctionRadiusSquared = MathF.Pow(MaxDetectionRadius, 2f);
            float separationDistanceSquared = MathF.Pow(MaxRadiusFromOtherFish, 2f);
            List<SunkenFishBoid> allOtherFishes = CalamityMainMenu_Sunken.Fishes;
            foreach (SunkenFishBoid other in allOtherFishes)
            {
                // Only school with other fishes that are nearby, of the same type and within the same depth.
                bool isPrismaticGuppy = IsAPrismaticGuppy && other.IsAPrismaticGuppy;
                bool schoolByFishType = isPrismaticGuppy || SelectedFishType == other.SelectedFishType;
                float distance = Vector2.DistanceSquared(Position, other.Position);
                if (distance < detetctionRadiusSquared && other != this && schoolByFishType && other.Depth == Depth)
                {
                    float separationFactor = (separationDistanceSquared - distance) * Scale;
                    separation += (Position - other.Position) * separationFactor;

                    cohesion += other.Position;
                    alignment += other.Velocity;
                    totalSchoolMembers++;
                }
            }

            if (totalSchoolMembers == 0)
                return;

            // Add all boids algorithm factors into the velocity.
            cohesion /= totalSchoolMembers;
            Velocity += (cohesion - Position).SafeNormalize(Vector2.Zero) * CohesionCoefficient;

            alignment /= totalSchoolMembers;
            Velocity += (alignment - Velocity).SafeNormalize(Vector2.Zero) * AlignmentCoefficient;

            separation /= totalSchoolMembers;
            Velocity += separation.SafeNormalize(Vector2.Zero) * SeparationCoefficient;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Initialized)
                return;

            Rectangle screenBounds = new Rectangle(-50, -50, Main.screenWidth + 100, Main.screenHeight + 100);
            Vector2 depthFactor = new(1f / Depth, 1.1f / Depth);
            Vector2 parallaxedPosition = Position * depthFactor;

            // Don't draw anything if the fish is not within the screen bounds. No point in wasting resources on that.
            if (!screenBounds.Contains((int)parallaxedPosition.X, (int)parallaxedPosition.Y))
                return;

            Texture2D fishTexture = GetCorrectFishTexture();
            Rectangle frame = fishTexture.Frame(1, MaxFrames, 0, CurrentFrame);
            Vector2 origin = frame.Size() * 0.5f;

            float opacityByDepth = Utils.Remap(Depth, 1, 4, 0.8f, 0.6f, true) * Opacity;
            float drawRotaton = Rotation + MathHelper.Pi;
            float scaleByDepth = Scale / Depth;

            SpriteEffects effects = (Velocity.X > 0f) ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(fishTexture, parallaxedPosition, frame, Color.White * opacityByDepth, drawRotaton, origin, scaleByDepth, effects, 0f);
        }

        private Texture2D GetCorrectFishTexture()
        {
            string sunkenSeaPath = "CalamityMod/NPCs/SunkenSea/";
            Texture2D returnTexture = SelectedFishType switch
            {
                FishType.PolypPanasea => ModContent.Request<Texture2D>(sunkenSeaPath + PolypPanaseaTextureNames[FishTextureIndex]).Value,
                FishType.PrismaticGuppy1 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_1[FishTextureIndex]).Value,
                FishType.PrismaticGuppy2 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_2[FishTextureIndex]).Value,
                FishType.PrismaticGuppy3 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_3[FishTextureIndex]).Value,
                _ => ModContent.Request<Texture2D>(sunkenSeaPath + SeaMinnowTextureNames[FishTextureIndex]).Value,
            };
            return returnTexture;
        }
    }
}
