using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.MainMenu
{
    public class SunkenFishBoid
    {
        // -- GENERAL VARIABLES FOR DEFINING GENERAL BEHAVIOR AND FUNCTIONALITY FOR THE FISH ENTITIES -- 

        public enum FishType
        {
            SeaMinnow,
            AlphaSeaMinnow,
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

        public int Depth;

        public int CurrentFrame;

        public int MaxFrames;

        public int FrameSpeed;

        public Vector2 Position;

        public Vector2 Velocity;

        public Vector2 StretchFactor;

        public float Scale;

        public float StoredScale;

        public float Rotation;

        public float Opacity;

        public float PassiveMovementTimer;

        public Vector2 PassiveMovementVector;

        public float PassiveMovementSpeed;

        public float MaxSpeed_Default = 3f;

        public float MaxSpeed_SwimAwayFromCursor = 4.25f;

        public FishType SelectedFishType;

        public FishType? FishTypeOverride;

        public WeightedRandom<FishType> RandomFishType = new();

        public int FishTextureIndex = -1;

        public bool Initialized = false;

        public bool HasSpawnedSchool = false;

        public List<SunkenFishBoid> SeaMinnowSchoolMembers = [];

        public Asset<Texture2D> FishTexture;

        // -- BOID ALOGORITHM SPECIFIC VARIABLES -- 

        // 19MAY2025: fryzahh:
        // Coefficients to control how strongly each of the Boid Algos affect the movement of each boid.
        // Higher values will lead to sharper flocking AI, but movement might be a little choppier.
        // Please beware of this if you ever change these values.
        public float CohesionCoefficient = 0.075f;

        public float AlignmentCoefficient = 0.12f;

        public float SeparationCoefficient = 0.03f;

        public float MaxRadiusFromOtherFish = 125f;

        public float MaxDetectionRadius = 200f;

        /// <summary>
        /// Whether or not this fish boid is any of the Prismatic Guppy variants. Used for allowing Prismatic Guppies to school with each other.
        /// </summary>
        public bool IsAPrismaticGuppy => SelectedFishType == FishType.PrismaticGuppy1 || SelectedFishType == FishType.PrismaticGuppy2 || SelectedFishType == FishType.PrismaticGuppy3;

        /// <summary>
        /// Whether or not this fish boid is any one of the Sea Minnow variants. Used for allowing Sea Minnows to school with Alphas properly.
        /// </summary>
        public bool IsASeaMinnow => SelectedFishType == FishType.SeaMinnow || SelectedFishType == FishType.AlphaSeaMinnow;

        public SunkenFishBoid(Vector2 position, float scale, int depth, FishType? fishTypeOverride = null)
        {
            Position = position;
            StoredScale = scale;
            Depth = depth;
            FishTypeOverride = fishTypeOverride;

            FrameSpeed = Main.rand.Next(6, 11);
            StretchFactor = Vector2.One;
            RandomFishType.Add(FishType.AlphaSeaMinnow, 0.7f);
            RandomFishType.Add(FishType.PolypPanasea, 0.6f);
            RandomFishType.Add(FishType.PrismaticGuppy1, 0.1f);
            RandomFishType.Add(FishType.PrismaticGuppy2, 0.1f);
            RandomFishType.Add(FishType.PrismaticGuppy3, 0.1f);
        }

        public void Update()
        {
            if (!Initialized)
            {
                // Select the appropriate fish type.
                SelectedFishType = FishTypeOverride ?? RandomFishType;

                // Select the correct texture index for the different types of fish textures.
                if (SelectedFishType == FishType.SeaMinnow)
                    FishTextureIndex = 0;
                if (SelectedFishType == FishType.AlphaSeaMinnow)
                    FishTextureIndex = 1;
                if (SelectedFishType == FishType.PolypPanasea)
                    FishTextureIndex = Main.rand.Next(PolypPanaseaTextureNames.Length);
                if (IsAPrismaticGuppy)
                    FishTextureIndex = Main.rand.Next(PrismaticGuppyTextureNames_1.Length);

                // Select the correct amount of animation frames each texture has based on the fish type.
                if (IsASeaMinnow)
                    MaxFrames = 8;
                if (SelectedFishType == FishType.PolypPanasea)
                    MaxFrames = 6;
                if (SelectedFishType == FishType.PrismaticGuppy1)
                    MaxFrames = 6;
                if (SelectedFishType == FishType.PrismaticGuppy2)
                    MaxFrames = 4;
                if (SelectedFishType == FishType.PrismaticGuppy3)
                    MaxFrames = 5;

                CurrentFrame = Main.rand.Next(MaxFrames);
                FishTexture = GetCorrectFishTexture();
                Initialized = true;
            }

            if (!Initialized)
                return;

            if (Scale < StoredScale)
            {
                Scale = MathHelper.Clamp(Scale + 0.05f, 0f, StoredScale);
                Opacity = MathHelper.Clamp(Opacity + 0.05f, 0f, 1f);
            }

            // Perform all Boids Behavior.
            DoBoidsBehavior();

            // Move and swim around idly.
            PassiveMovementTimer--;
            if (PassiveMovementTimer <= 0f)
            {
                PassiveMovementSpeed = Main.rand.NextFloat(0.25f, MaxSpeed_Default);
                PassiveMovementVector.X = Main.rand.NextFloat(-100f, 101f);
                PassiveMovementVector.Y = Main.rand.NextFloat(-100f, 101f);
                PassiveMovementTimer = Main.rand.Next(120, 180);
            }

            var moveSpeed = PassiveMovementSpeed / PassiveMovementVector.Length();
            Velocity = Vector2.Lerp(Velocity, PassiveMovementVector * moveSpeed, 0.01f);

            // Make fishes closest to the screen swim away from the mouse cursor if it's nearby.
            var distanceFromCursor = Vector2.Distance(Position, Main.MouseScreen);
            var shouldSwimAway = distanceFromCursor < 100f && Depth <= 1;
            if (shouldSwimAway)
            {
                var distanceInterpolant = Utils.GetLerpValue(175f, 0f, distanceFromCursor, true);
                Velocity += Position.DirectionTo(Main.MouseScreen).SafeNormalize(Vector2.UnitY) * distanceInterpolant * -0.46f;

                // Stretch a little while swimming away from danger for additional silliness :D
                float stretch = MathHelper.Clamp(Velocity.Length() / MaxSpeed_SwimAwayFromCursor * 0.1f, 1f, 1.65f);
                Vector2 stretchedVector = new(Scale * stretch, Scale - Scale * stretch * 0.3f);
                StretchFactor = Vector2.Lerp(StretchFactor, stretchedVector, 0.15f);
            }
            else
            {
                if (Velocity.Length() > MaxSpeed_Default)
                    Velocity *= 0.7f;
                StretchFactor = Vector2.Lerp(StretchFactor, Vector2.One, 0.15f);
            }

            Velocity = Velocity.ClampMagnitude(0.25f, MaxSpeed_SwimAwayFromCursor);
            Position += Velocity;

            Rotation = Velocity.ToRotation();
            if (Time % FrameSpeed == 0)
                CurrentFrame = (CurrentFrame + 1) % MaxFrames;

            Time++;
        }

        public void DoBoidsBehavior()
        {
            // Polyp Panaseas do not travel in schools.
            if (SelectedFishType == FishType.PolypPanasea)
                return;

            var cohesion = Vector2.Zero;
            var alignment = Vector2.Zero;
            var separation = Vector2.Zero;
            var totalSchoolMembers = 0;

            var detetctionRadiusSquared = MathF.Pow(MaxDetectionRadius, 2f);
            var separationDistanceSquared = MathF.Pow(MaxRadiusFromOtherFish, 2f);
            var allOtherFishes = CalamityMainMenu_Sunken.Fishes;

            // Alpha Sea Minnows spawn a school of Sea Minnows manually and stick with it.
            if (IsASeaMinnow && SeaMinnowSchoolMembers.Count > 0)
            {
                foreach (var minnow in SeaMinnowSchoolMembers)
                {
                    var distance = Vector2.DistanceSquared(Position, minnow.Position);
                    var separationFactor = Utils.GetLerpValue(separationDistanceSquared, 0f, distance, true);
                    separation += (Position - minnow.Position) * separationFactor * 0.7f;

                    cohesion += minnow.Position;
                    alignment += minnow.Velocity;
                    totalSchoolMembers++;
                }
            }
            else
            {
                foreach (var other in allOtherFishes)
                {
                    // Only school with other fishes that are nearby, of the same type and within the same depth.
                    var isPrismaticGuppy = IsAPrismaticGuppy && other.IsAPrismaticGuppy;
                    var schoolByFishType = isPrismaticGuppy || SelectedFishType == other.SelectedFishType;

                    var distance = Vector2.DistanceSquared(Position, other.Position);
                    if (distance < detetctionRadiusSquared && other != this && schoolByFishType && other.Depth == Depth)
                    {
                        var separationFactor = Utils.GetLerpValue(separationDistanceSquared, 0f, distance, true);
                        separation += (Position - other.Position) * separationFactor * 0.7f;

                        cohesion += other.Position;
                        alignment += other.Velocity;
                        totalSchoolMembers++;
                    }
                }
            }
                
            if (totalSchoolMembers == 0)
                return;

            // Add all boids algorithm factors into the velocity.
            cohesion /= totalSchoolMembers;
            var schoolCenterInterpolant = Utils.GetLerpValue(0f, 80f, Position.Distance(cohesion), true);
            Velocity += (cohesion - Position).SafeNormalize(-Vector2.UnitY) * schoolCenterInterpolant * CohesionCoefficient;

            alignment /= totalSchoolMembers;
            Velocity += Velocity.ToRotation().AngleLerp(alignment.ToRotation(), 0.06f).ToRotationVector2() * Velocity.Length() * AlignmentCoefficient;

            separation /= totalSchoolMembers;
            Velocity += separation.SafeNormalize(-Vector2.UnitY) * SeparationCoefficient;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Initialized)
                return;

            Vector2 depthFactor = new(1f / Depth, 1.1f / Depth);
            Vector2 parallaxedPosition = Position * depthFactor;

            var frame = FishTexture.Frame(1, MaxFrames, 0, CurrentFrame);
            var origin = frame.Size() * 0.5f;

            var scaleByDepth = Scale / Depth;
            var drawRotaton = Rotation + MathHelper.Pi;
            var colorByDepth = Color.Lerp(Color.White, Color.Black, Utils.Remap(Depth, 1, 4, 0f, 0.8f, true));

            var effects = Velocity.X > 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
            spriteBatch.Draw(FishTexture.Value, parallaxedPosition, frame, colorByDepth * Opacity, drawRotaton, origin, StretchFactor * scaleByDepth, effects, 0f);
        }

        private Asset<Texture2D> GetCorrectFishTexture()
        {
            var sunkenSeaPath = "CalamityMod/NPCs/SunkenSea/";
            var returnTexture = SelectedFishType switch
            {
                FishType.PolypPanasea => ModContent.Request<Texture2D>(sunkenSeaPath + PolypPanaseaTextureNames[FishTextureIndex]),
                FishType.PrismaticGuppy1 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_1[FishTextureIndex]),
                FishType.PrismaticGuppy2 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_2[FishTextureIndex]),
                FishType.PrismaticGuppy3 => ModContent.Request<Texture2D>(sunkenSeaPath + PrismaticGuppyTextureNames_3[FishTextureIndex]),
                _ => ModContent.Request<Texture2D>(sunkenSeaPath + SeaMinnowTextureNames[FishTextureIndex]),
            };
            return returnTexture;
        }
    }
}
