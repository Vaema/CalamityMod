using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Primitives
{
    /// <summary>
    /// This manages rendering primitives in via the provided <see cref="RenderTrail"/> method.<br/><br/>
    /// <b>To use normally, in the draw method of an NPC or Projectile call <see cref="RenderTrail"/>.</b><br/>
    /// <list type="bullet">
    /// <item>The first param should be the positions to use for the trail.</item>
    /// <item>The second is a struct that allows you to choose a desired configuration of settings to use with the trail, to allow for customization.</item>
    /// <item>The third controls how many points are created for the trail, and the fourth determines whether the primitive is subsequently rendered.</item>
    /// </list>
    /// If you wish to use pixelation, you <b>MUST</b> make the NPC/Projectile inherit <see cref="IPixelatedPrimitiveRenderer"/> and use <see cref="IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives"/> instead of predraw..<br/>
    /// You can also optionally specify a render layer with <see cref="IPixelatedPrimitiveRenderer.LayerToRenderTo"/>. It is <see cref="Enums.GeneralDrawLayer.BeforeNPCs"/> by default.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class PrimitiveRenderer : ModSystem
    {
        #region Static Members
        private static DynamicVertexBuffer VertexBuffer;

        private static DynamicIndexBuffer IndexBuffer;

        private static PrimitiveSettings MainSettings;

        private static Vector2[] MainPositions;

        private static VertexPosition2DColorTexture[] MainVertices;

        private static short[] MainIndices;

        private static VertexPositionColor[] WireframeVertices;

        private static int WireframeVertexCount;

        private static BasicEffect WireframeEffect;

        private static int[] NonSmoothIndexScratch;

        private const short MaxPositions = 1000;

        private const short MaxVertices = 3072;

        private const short MaxIndices = 8192;

        private static readonly List<Vector2> ControlPointsCache = new(MaxPositions);

        private static short PositionsIndex;

        private static float[] MainCompletionRatios;

        private static float TotalTrailLength;

        public const float Epsilon = 1e-6f;

        private static short VerticesIndex;

        private static short IndicesIndex;

        public override void OnModLoad()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainPositions = new Vector2[MaxPositions];
                MainVertices = new VertexPosition2DColorTexture[MaxVertices];
                MainIndices = new short[MaxIndices];
                MainCompletionRatios = new float[MaxPositions];
                WireframeVertices = new VertexPositionColor[MaxPositions * 8];
                NonSmoothIndexScratch = new int[MaxPositions];
                VertexBuffer ??= new DynamicVertexBuffer(Main.instance.GraphicsDevice, VertexPosition2DColorTexture.VertexDeclaration2D, MaxVertices, BufferUsage.WriteOnly);
                IndexBuffer ??= new DynamicIndexBuffer(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
                WireframeEffect ??= new BasicEffect(Main.instance.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = false,
                    LightingEnabled = false
                };
            });
        }

        public override void OnModUnload()
        {
            WireframeEffect?.Dispose();
            WireframeEffect = null;
        }

        private static void PerformPixelationSafetyChecks(PrimitiveSettings settings)
        {
            // Don't allow accidental screw ups with these.
            if (settings.Pixelate && !PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives using pixelation MUST be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method, did you forget to use the interface?");
            else if (!settings.Pixelate && PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives not using pixelation MUST NOT be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method.");
        }

        /// <summary>
        /// Renders a primitive trail.
        /// </summary>
        /// <param name="positions">The list of positions to use. Keep in mind that these are expected to be in <b>world position</b>, and <see cref="Main.screenPosition"/> is automatically subtracted from them all.<br/>At least 4 points are required to use smoothing.</param>
        /// <param name="settings">The primitive draw settings to use.</param>
        /// <param name="pointsToCreate">The amount of points to use. More is higher detailed, but less performant. By default, is the number of positions provided. <b>Going above 100 is NOT recommended.</b></param>
        public static void RenderTrail(List<Vector2> positions, PrimitiveSettings settings, int? pointsToCreate = null) => RenderTrail(positions.ToArray(), settings, pointsToCreate);

        /// <summary>
        /// Renders a primitive trail.
        /// </summary>
        /// <param name="positions">The list of positions to use. Keep in mind that these are expected to be in <b>world position</b>, and <see cref="Main.screenPosition"/> is automatically subtracted from them all.<br/>At least 4 points are required to use smoothing.</param>
        /// <param name="settings">The primitive draw settings to use.</param>
        /// <param name="pointsToCreate">The amount of points to use. More is higher detailed, but less performant. By default, is the number of positions provided. <b>Going above 100 is NOT recommended.</b></param>
        public static void RenderTrail(Vector2[] positions, PrimitiveSettings settings, int? pointsToCreate = null)
        {
            PerformPixelationSafetyChecks(settings);

            // Return if not enough to draw anything.
            if (positions.Length <= 2)
                return;

            // Return if too many to draw anything,
            if (positions.Length > MaxPositions)
                return;

            int desiredPointCount = pointsToCreate ?? positions.Length;
            desiredPointCount = Math.Clamp(desiredPointCount, 2, MaxPositions);

            // IF this is false, a correct position trail could not be made and rendering should not continue.
            if (!AssignPointsRectangleTrail(positions, settings, desiredPointCount))
                return;

            // A trail with only one point or less has nothing to connect to, and therefore, can't make a trail.
            MainSettings = settings;
            AssignCompletionData();

            if (PositionsIndex <= 2)
                return;

            AssignVerticesRectangleTrail();
            AssignIndicesRectangleTrail();

            // Else render without wasting resources creating a set.
            PrivateRender();
            return;
        }

        private static void PrivateRender()
        {
            if (IndicesIndex % 6 != 0 || VerticesIndex <= 3)
                return;

            // Perform screen culling, for performance reasons.
            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            Matrix view;
            Matrix projection;
            if (MainSettings.Pixelate || MainSettings.UseUnscaledMatrices)
                CalcuatePixelatedPerspectiveMatrices(out view, out projection);
            else
                CalamityUtils.CalculatePerspectiveMatricies(out view, out projection);

            var shaderToUse = MainSettings.Shader ?? GameShaders.Misc["CalamityMod:StandardPrimitiveShader"];
            shaderToUse.Shader.Parameters["uWorldViewProjection"].SetValue(view * projection);
            shaderToUse.Apply();

            VertexBuffer.SetData(MainVertices, 0, VerticesIndex, SetDataOptions.Discard);
            IndexBuffer.SetData(MainIndices, 0, IndicesIndex, SetDataOptions.Discard);

            Main.instance.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Main.instance.GraphicsDevice.Indices = IndexBuffer;
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VerticesIndex, 0, IndicesIndex / 3);

            if (MainSettings.DebugWireframe && WireframeEffect != null && TryBuildWireframeGeometry(MainSettings.WireframeColor, out int lineCount))
                DrawWireframe(view, projection, lineCount);
        }
        #endregion

        #region Set Preperation
        private static bool AssignPointsRectangleTrail(Vector2[] positions, PrimitiveSettings settings, int pointsToCreate)
        {
            // Don't smoothen the points unless explicitly told do so.
            if (!settings.Smoothen)
            {
                PositionsIndex = 0;

                int validCount = 0;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (positions[i] == Vector2.Zero)
                        continue;

                    NonSmoothIndexScratch[validCount++] = i;
                }

                if (validCount <= 2)
                    return false;

                int lastIndex = validCount - 1;
                float inversePointCount = 1f / (pointsToCreate - 1);
                float lastIndexFloat = lastIndex;

                // Remap the original positions across a certain length without additional allocations.
                for (int i = 0; i < pointsToCreate; i++)
                {
                    float completionRatio = i * inversePointCount;
                    float scaledIndex = completionRatio * lastIndexFloat;
                    int currentIndex = (int)scaledIndex;
                    int nextIndex = Math.Min(currentIndex + 1, lastIndex);
                    float localInterpolant = scaledIndex - currentIndex;

                    Vector2 currentPoint = positions[NonSmoothIndexScratch[currentIndex]];
                    Vector2 nextPoint = positions[NonSmoothIndexScratch[nextIndex]];
                    Vector2 interpolatedWorld = Vector2.Lerp(currentPoint, nextPoint, localInterpolant);
                    Vector2 finalPos = interpolatedWorld - Main.screenPosition;
                    if (settings.OffsetFunction != null)
                        finalPos += settings.OffsetFunction(completionRatio, interpolatedWorld);

                    MainPositions[PositionsIndex++] = finalPos;
                }
                return true;
            }

            // Due to the first point being manually added, points should be added starting at the second position instead of the first.
            PositionsIndex = 1;

            // Create the control points for the spline.
            List<Vector2> controlPoints = ControlPointsCache;
            controlPoints.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                // Don't incorporate points that are zeroed out.
                // They are almost certainly a result of incomplete oldPos arrays.
                if (positions[i] == Vector2.Zero)
                    continue;

                float completionRatio = i / (float)positions.Length;
                Vector2 offset = -Main.screenPosition;
                if (settings.OffsetFunction != null)
                    offset += settings.OffsetFunction(completionRatio, positions[i]);
                controlPoints.Add(positions[i] + offset);
            }

            // Avoid stupid index errors.
            if (controlPoints.Count <= 4)
                return false;

            for (int j = 0; j < pointsToCreate; j++)
            {
                float splineInterpolant = j / (float)pointsToCreate;
                float localSplineInterpolant = splineInterpolant * (controlPoints.Count - 1f) % 1f;
                int localSplineIndex = (int)(splineInterpolant * (controlPoints.Count - 1f));

                Vector2 farLeft;
                Vector2 left = controlPoints[localSplineIndex];
                Vector2 right = controlPoints[localSplineIndex + 1];
                Vector2 farRight;

                // Special case: If the spline attempts to access the previous/next index but the index is already at the very beginning/end, simply
                // cheat a little bit by creating a phantom point that's mirrored from the previous one.
                if (localSplineIndex <= 0)
                {
                    Vector2 mirrored = left * 2f - right;
                    farLeft = mirrored;
                }
                else
                    farLeft = controlPoints[localSplineIndex - 1];

                if (localSplineIndex >= controlPoints.Count - 2)
                {
                    Vector2 mirrored = right * 2f - left;
                    farRight = mirrored;
                }
                else
                    farRight = controlPoints[localSplineIndex + 2];

                MainPositions[PositionsIndex] = Vector2.CatmullRom(farLeft, left, right, farRight, localSplineInterpolant);
                PositionsIndex++;
            }

            // Manually insert the front and end points.
            MainPositions[0] = controlPoints[0];
            MainPositions[PositionsIndex] = controlPoints[controlPoints.Count - 1];
            PositionsIndex++;
            return true;
        }

        private static void AssignCompletionData()
        {
            TotalTrailLength = 0f;

            if (PositionsIndex <= 0)
                return;

            MainCompletionRatios[0] = 0f;

            for (int i = 1; i < PositionsIndex; i++)
            {
                float segmentLength = Vector2.Distance(MainPositions[i], MainPositions[i - 1]);
                TotalTrailLength += segmentLength;
                MainCompletionRatios[i] = TotalTrailLength;
            }

            if (PositionsIndex <= 0)
                return;

            if (TotalTrailLength > Epsilon)
            {
                float inverseTotal = 1f / TotalTrailLength;
                for (int i = 1; i < PositionsIndex; i++)
                    MainCompletionRatios[i] *= inverseTotal;

                MainCompletionRatios[PositionsIndex - 1] = 1f;
            }
            else
            {
                for (int i = 1; i < PositionsIndex; i++)
                    MainCompletionRatios[i] = 0f;
            }
        }

        private static void AssignVerticesRectangleTrail()
        {
            VerticesIndex = 0;
            for (int i = 0; i < PositionsIndex; i++)
            {
                float completionRatio = GetCompletionRatioForIndex(i);
                float widthAtVertex = Math.Max(MainSettings.WidthFunction(completionRatio, MainPositions[i]), 0f);
                Color vertexColor = MainSettings.ColorFunction(completionRatio, MainPositions[i]);
                float textureU = ComputeTextureCoordinateForIndex(i, completionRatio);

                ComputeEdgePositions(i, widthAtVertex, out Vector2 left, out Vector2 right, out float effectiveHalfWidth);

                // Override the initial vertex positions if requested.
                if (i == 0 && MainSettings.InitialVertexPositionsOverride.HasValue && MainSettings.InitialVertexPositionsOverride.Value.Item1 != Vector2.Zero && MainSettings.InitialVertexPositionsOverride.Value.Item2 != Vector2.Zero)
                {
                    left = MainSettings.InitialVertexPositionsOverride.Value.Item1;
                    right = MainSettings.InitialVertexPositionsOverride.Value.Item2;
                    effectiveHalfWidth = Math.Max(Vector2.Distance(left, right) * 0.5f, Epsilon);
                }

                // Guard against degenerate width
                effectiveHalfWidth = Math.Max(effectiveHalfWidth, Epsilon);

                Vector2 leftCurrentTextureCoord = new Vector2(textureU, 0.5f - effectiveHalfWidth * 0.5f);
                Vector2 rightCurrentTextureCoord = new Vector2(textureU, 0.5f + effectiveHalfWidth * 0.5f);

                MainVertices[VerticesIndex] = new VertexPosition2DColorTexture(left, vertexColor, leftCurrentTextureCoord, effectiveHalfWidth);
                VerticesIndex++;
                MainVertices[VerticesIndex] = new VertexPosition2DColorTexture(right, vertexColor, rightCurrentTextureCoord, effectiveHalfWidth);
                VerticesIndex++;
            }
        }

        private static float GetCompletionRatioForIndex(int index)
        {
            if (PositionsIndex <= 0)
                return 0f;

            if (index <= 0)
                return MainCompletionRatios[0];

            if (index >= PositionsIndex)
                return MainCompletionRatios[PositionsIndex - 1];

            return MainCompletionRatios[index];
        }

        private static float ComputeTextureCoordinateForIndex(int index, float completionRatio)
        {
            float clampedCompletion = MathHelper.Clamp(completionRatio, 0f, 1f);

            if (MainSettings.TextureCoordinateFunction != null)
                return MainSettings.TextureCoordinateFunction(clampedCompletion);

            float cycleLength = MainSettings.TextureCycleLength;
            if (Math.Abs(cycleLength) <= Epsilon)
                cycleLength = cycleLength >= 0f ? 1f : -1f;

            switch (MainSettings.TextureCoordinateMode)
            {
                case PrimitiveTextureMode.Distance:
                    float distance = clampedCompletion * TotalTrailLength + MainSettings.TextureScrollOffset;
                    return distance / cycleLength;

                default:
                    return clampedCompletion * cycleLength + MainSettings.TextureScrollOffset;
            }
        }

        private static void ComputeEdgePositions(int index, float halfWidth, out Vector2 left, out Vector2 right, out float effectiveHalfWidth)
        {
            Vector2 currentPosition = MainPositions[index];

            if (halfWidth <= 0f)
            {
                left = currentPosition;
                right = currentPosition;
                effectiveHalfWidth = Epsilon;
                return;
            }

            Vector2 forward = index >= PositionsIndex - 1 ? MainPositions[index] - MainPositions[index - 1] : MainPositions[index + 1] - MainPositions[index];
            Vector2 backward = index <= 0 ? forward : MainPositions[index] - MainPositions[index - 1];

            Vector2 forwardDir = forward.SafeNormalize(Vector2.Zero);
            if (forwardDir == Vector2.Zero)
                forwardDir = backward.SafeNormalize(Vector2.UnitX);
            if (forwardDir == Vector2.Zero)
                forwardDir = Vector2.UnitX;

            Vector2 defaultNormal = new Vector2(-forwardDir.Y, forwardDir.X).SafeNormalize(Vector2.UnitY);

            if (MainSettings.JoinStyle == PrimitiveJoinStyle.Flat || PositionsIndex <= 2 || index == 0 || index == PositionsIndex - 1)
            {
                Vector2 offset = defaultNormal * halfWidth;
                left = currentPosition - offset;
                right = currentPosition + offset;
                effectiveHalfWidth = halfWidth;
                return;
            }

            Vector2 backwardDir = backward.SafeNormalize(forwardDir);
            Vector2 averageTangent = backwardDir + forwardDir;
            if (averageTangent.LengthSquared() <= Epsilon)
                averageTangent = forwardDir;
            Vector2 averageNormal = new Vector2(-averageTangent.Y, averageTangent.X).SafeNormalize(defaultNormal);

            switch (MainSettings.JoinStyle)
            {
                case PrimitiveJoinStyle.Smooth:
                {
                    Vector2 offset = averageNormal * halfWidth;
                    left = currentPosition - offset;
                    right = currentPosition + offset;
                    effectiveHalfWidth = halfWidth;
                    return;
                }

                case PrimitiveJoinStyle.Miter:
                {
                    Vector2 prevNormal = new Vector2(-backwardDir.Y, backwardDir.X).SafeNormalize(defaultNormal);
                    Vector2 nextNormal = new Vector2(-forwardDir.Y, forwardDir.X).SafeNormalize(defaultNormal);
                    Vector2 miter = prevNormal + nextNormal;
                    if (miter.LengthSquared() <= Epsilon)
                        miter = averageNormal;

                    miter = miter.SafeNormalize(averageNormal);
                    float denom = Vector2.Dot(miter, nextNormal);
                    if (Math.Abs(denom) < Epsilon)
                        denom = denom >= 0f ? Epsilon : -Epsilon;

                    float miterLength = halfWidth / denom;
                    float maxLength = halfWidth * MainSettings.JoinMiterLimit;
                    miterLength = MathHelper.Clamp(miterLength, -maxLength, maxLength);
                    Vector2 offset = miter * miterLength;
                    left = currentPosition - offset;
                    right = currentPosition + offset;
                    effectiveHalfWidth = Math.Max(Math.Abs(miterLength), Epsilon);
                    return;
                }

                default:
                {
                    Vector2 offset = defaultNormal * halfWidth;
                    left = currentPosition - offset;
                    right = currentPosition + offset;
                    effectiveHalfWidth = halfWidth;
                    return;
                }
            }
        }

        private static bool TryBuildWireframeGeometry(Color lineColor, out int lineCount)
        {
            lineCount = 0;

            if (WireframeVertices == null)
                return false;

            int segments = PositionsIndex - 1;
            if (segments <= 0)
                return false;

            WireframeVertexCount = 0;

            for (int i = 0; i < segments; i++)
            {
                int currentLeft = i * 2;
                int currentRight = currentLeft + 1;
                int nextLeft = currentLeft + 2;
                int nextRight = nextLeft + 1;

                AddWireframeLineFromIndices(currentLeft, nextLeft, lineColor);
                AddWireframeLineFromIndices(currentRight, nextRight, lineColor);
            }

            for (int i = 0; i < PositionsIndex; i++)
            {
                int leftIndex = i * 2;
                AddWireframeLineFromIndices(leftIndex, leftIndex + 1, lineColor);
            }

            lineCount = WireframeVertexCount / 2;
            return lineCount > 0;
        }

        private static void AddWireframeLineFromIndices(int startVertexIndex, int endVertexIndex, Color color)
        {
            if (WireframeVertexCount + 1 >= WireframeVertices.Length)
                return;

            int vertexCount = VerticesIndex;
            if (startVertexIndex >= vertexCount || endVertexIndex >= vertexCount)
                return;

            ref readonly VertexPosition2DColorTexture start = ref MainVertices[startVertexIndex];
            ref readonly VertexPosition2DColorTexture end = ref MainVertices[endVertexIndex];
            WireframeVertices[WireframeVertexCount++] = new VertexPositionColor(new Vector3(start.Position, 0f), color);
            WireframeVertices[WireframeVertexCount++] = new VertexPositionColor(new Vector3(end.Position, 0f), color);
        }

        private static void DrawWireframe(Matrix view, Matrix projection, int lineCount)
        {
            if (lineCount <= 0 || WireframeEffect == null)
                return;

            WireframeEffect.World = Matrix.Identity;
            WireframeEffect.View = view;
            WireframeEffect.Projection = projection;

            var device = Main.instance.GraphicsDevice;
            foreach (EffectPass pass in WireframeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.LineList, WireframeVertices, 0, lineCount);
            }
        }

        private static void AssignIndicesRectangleTrail()
        {
            // What this is doing is basically representing each point on the vertices list as
            // indices. These indices should come together to create a tiny rectangle that acts
            // as a segment on the trail. This is achieved here by splitting the indices (or rather, points)
            // into 2 triangles, which requires 6 points.
            // The logic here basically determines which indices are connected together.
            IndicesIndex = 0;
            for (short i = 0; i < PositionsIndex - 2; i++)
            {
                short connectToIndex = (short)(i * 2);
                MainIndices[IndicesIndex] = connectToIndex;
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 1);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 2);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 2);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 1);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 3);
                IndicesIndex++;
            }
        }

        private static void CalcuatePixelatedPerspectiveMatrices(out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            // Due to the scaling, the normal transformation calcuations do not work with pixelated primitives.
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            viewMatrix = Matrix.Identity;
        }
        #endregion
    }
}
