#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public static class PrimitiveShapeBuilder
{
    public static PrimitiveMesh BuildRectangularQuad(
        Vector3 center,
        Vector2 size,
        Color color,
        Vector3 normal,
        Vector3 upHint,
        bool textured = false)
    {
        BuildFrame(normal, upHint, out var right, out var up);

        var halfRight = right * (size.X * 0.5f);
        var halfUp = up * (size.Y * 0.5f);

        if (textured)
        {
            var vertices = new[]
            {
                new VertexPositionColorTexture(center - halfRight - halfUp, color, new Vector2(0f, 1f)),
                new VertexPositionColorTexture(center + halfRight - halfUp, color, new Vector2(1f, 1f)),
                new VertexPositionColorTexture(center + halfRight + halfUp, color, new Vector2(1f, 0f)),
                new VertexPositionColorTexture(center - halfRight + halfUp, color, new Vector2(0f, 0f))
            };
            var indices = new short[] { 0, 1, 2, 0, 2, 3 };
            return new PrimitiveMesh(vertices, indices, PrimitiveType.TriangleList);
        }

        var colorVertices = new[]
        {
            new VertexPositionColor(center - halfRight - halfUp, color),
            new VertexPositionColor(center + halfRight - halfUp, color),
            new VertexPositionColor(center + halfRight + halfUp, color),
            new VertexPositionColor(center - halfRight + halfUp, color)
        };
        var colorIndices = new short[] { 0, 1, 2, 0, 2, 3 };
        return new PrimitiveMesh(colorVertices, colorIndices, PrimitiveType.TriangleList);
    }

    /// <summary>
    /// Builds a regular polygon with the specified number of sides, centered at the given position, and oriented according to the normal and up hint.
    /// The polygon is constructed in a single triangle fan, with the center vertex as the first element and subsequent vertices arranged in a clockwise order around the normal.
    /// If you need a different vertex order or a more complex polygon, consider <see cref="BuildArbitraryPolygon(IReadOnlyList{Vector3}, Color, bool)"/>
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="sides"></param>
    /// <param name="color"></param>
    /// <param name="normal"></param>
    /// <param name="upHint"></param>
    /// <param name="textured"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static PrimitiveMesh BuildRegularPolygon(
        Vector3 center,
        float radius,
        int sides,
        Color color,
        Vector3 normal,
        Vector3 upHint,
        bool textured = false)
    {
        if (sides < 3)
            throw new ArgumentOutOfRangeException(nameof(sides), "Polygon requires at least three sides.");

        BuildFrame(normal, upHint, out var right, out var up);

        if (textured)
        {
            var texturedVertices = new VertexPositionColorTexture[sides + 1];
            texturedVertices[0] = new VertexPositionColorTexture(center, color, new Vector2(0.5f, 0.5f));

            float safeRadius = Math.Max(radius, 1e-6f);

            for (int i = 0; i < sides; i++)
            {
                var direction = PolarToCartesian(i, sides, right, up) * radius;
                var point = center + direction;
                float u = 0.5f + 0.5f * MathHelper.Clamp(Vector3.Dot(direction, right) / safeRadius, -1f, 1f);
                float v = 0.5f - 0.5f * MathHelper.Clamp(Vector3.Dot(direction, up) / safeRadius, -1f, 1f);
                texturedVertices[i + 1] = new VertexPositionColorTexture(point, color, new Vector2(u, v));
            }

            var indices = new short[sides * 3];
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                int baseIndex = i * 3;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(next + 1);
            }

            return new PrimitiveMesh(texturedVertices, indices, PrimitiveType.TriangleList);
        }

        var colorVertices = new VertexPositionColor[sides + 1];
        colorVertices[0] = new VertexPositionColor(center, color);

        for (int i = 0; i < sides; i++)
        {
            var direction = PolarToCartesian(i, sides, right, up) * radius;
            colorVertices[i + 1] = new VertexPositionColor(center + direction, color);
        }

        var colorIndices = new short[sides * 3];
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            int baseIndex = i * 3;
            colorIndices[baseIndex] = 0;
            colorIndices[baseIndex + 1] = (short)(i + 1);
            colorIndices[baseIndex + 2] = (short)(next + 1);
        }

        return new PrimitiveMesh(colorVertices, colorIndices, PrimitiveType.TriangleList);
    }

    public static PrimitiveMesh BuildArbitraryPolygon(IReadOnlyList<Vector3> points, Color color, bool textured = false)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));
        if (points.Count < 3)
            throw new ArgumentOutOfRangeException(nameof(points), "Polygon requires at least three points.");

        if (textured)
        {
            float minX = points[0].X, maxX = points[0].X;
            float minY = points[0].Y, maxY = points[0].Y;
            for (int i = 1; i < points.Count; i++)
            {
                var p = points[i];
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }
            float width = Math.Max(maxX - minX, 1e-6f);
            float height = Math.Max(maxY - minY, 1e-6f);

            var texturedVertices = new VertexPositionColorTexture[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                float u = (point.X - minX) / width;
                float v = (point.Y - minY) / height;
                texturedVertices[i] = new VertexPositionColorTexture(point, color, new Vector2(u, v));
            }

            var indices = new short[(points.Count - 2) * 3];
            for (int i = 0; i < points.Count - 2; i++)
            {
                indices[i * 3] = 0;
                indices[i * 3 + 1] = (short)(i + 1);
                indices[i * 3 + 2] = (short)(i + 2);
            }

            return new PrimitiveMesh(texturedVertices, indices, PrimitiveType.TriangleList);
        }

        var colorVertices = new VertexPositionColor[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            colorVertices[i] = new VertexPositionColor(point, color);
        }

        var colorIndices = new short[(points.Count - 2) * 3];
        for (int i = 0; i < points.Count - 2; i++)
        {
            colorIndices[i * 3] = 0;
            colorIndices[i * 3 + 1] = (short)(i + 1);
            colorIndices[i * 3 + 2] = (short)(i + 2);
        }

        return new PrimitiveMesh(colorVertices, colorIndices, PrimitiveType.TriangleList);
    }

    public static PrimitiveMesh BuildEllipse(
        Vector3 center,
        Vector2 radii,
        int segments,
        Color color,
        Vector3 normal,
        Vector3 upHint,
        bool textured = false)
    {
        if (segments < 3)
            throw new ArgumentOutOfRangeException(nameof(segments), "Ellipse requires at least three segments.");

        BuildFrame(normal, upHint, out var right, out var up);

        if (textured)
        {
            var vertices = new VertexPositionColorTexture[segments + 1];
            vertices[0] = new VertexPositionColorTexture(center, color, new Vector2(0.5f, 0.5f));

            float safeX = Math.Max(radii.X, 1e-6f);
            float safeY = Math.Max(radii.Y, 1e-6f);

            for (int i = 0; i < segments; i++)
            {
                var offset = EllipseDirection(i, segments, right, up, radii);
                var point = center + offset;

                float u = 0.5f + 0.5f * MathHelper.Clamp(Vector3.Dot(offset, right) / safeX, -1f, 1f);
                float v = 0.5f - 0.5f * MathHelper.Clamp(Vector3.Dot(offset, up) / safeY, -1f, 1f);

                vertices[i + 1] = new VertexPositionColorTexture(point, color, new Vector2(u, v));
            }

            var indices = new short[segments * 3];
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int baseIndex = i * 3;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(next + 1);
            }

            return new PrimitiveMesh(vertices, indices, PrimitiveType.TriangleList);
        }

        var colorVertices = new VertexPositionColor[segments + 1];
        colorVertices[0] = new VertexPositionColor(center, color);

        for (int i = 0; i < segments; i++)
        {
            colorVertices[i + 1] = new VertexPositionColor(center + EllipseDirection(i, segments, right, up, radii), color);
        }

        var colorIndices = new short[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int baseIndex = i * 3;
            colorIndices[baseIndex] = 0;
            colorIndices[baseIndex + 1] = (short)(i + 1);
            colorIndices[baseIndex + 2] = (short)(next + 1);
        }

        return new PrimitiveMesh(colorVertices, colorIndices, PrimitiveType.TriangleList);
    }

    public static PrimitiveMesh BuildSphere(
        Vector3 center,
        float radius,
        int latitudeSegments,
        int longitudeSegments,
        Func<Vector3, Color>? colorFunc = null,
        bool textured = false)
    {
        if (latitudeSegments < 2)
            throw new ArgumentOutOfRangeException(nameof(latitudeSegments), "Sphere requires at least two latitude segments.");
        if (longitudeSegments < 3)
            throw new ArgumentOutOfRangeException(nameof(longitudeSegments), "Sphere requires at least three longitude segments.");

        colorFunc ??= static _ => Color.White;

        int vertexRows = latitudeSegments + 1;
        int vertexCols = longitudeSegments + 1;
        int vertexCount = vertexRows * vertexCols;

        if (vertexCount > short.MaxValue)
            throw new InvalidOperationException("Sphere produced more vertices than supported by the index buffer.");

        if (textured)
        {
            var vertices = new VertexPositionColorTexture[vertexCount];
            int v = 0;

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float phi = MathF.PI * lat / latitudeSegments;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float vCoord = 1f - (float)lat / latitudeSegments;

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float theta = MathHelper.TwoPi * lon / longitudeSegments;
                    var normal = SphericalDirection(sinPhi, cosPhi, theta);
                    float uCoord = (float)lon / longitudeSegments;

                    vertices[v++] = new VertexPositionColorTexture(center + normal * radius, colorFunc(normal), new Vector2(uCoord, vCoord));
                }
            }

            var indices = new List<short>(latitudeSegments * longitudeSegments * 6);
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int current = lat * vertexCols + lon;
                    int next = current + vertexCols;

                    indices.Add((short)current);
                    indices.Add((short)(current + 1));
                    indices.Add((short)next);

                    indices.Add((short)(current + 1));
                    indices.Add((short)(next + 1));
                    indices.Add((short)next);
                }
            }

            return new PrimitiveMesh(vertices, [.. indices], PrimitiveType.TriangleList);
        }

        var colorVertices = new VertexPositionColor[vertexCount];
        int vIndex = 0;

        for (int lat = 0; lat <= latitudeSegments; lat++)
        {
            float phi = MathF.PI * lat / latitudeSegments;
            float sinPhi = MathF.Sin(phi);
            float cosPhi = MathF.Cos(phi);

            for (int lon = 0; lon <= longitudeSegments; lon++)
            {
                float theta = MathHelper.TwoPi * lon / longitudeSegments;
                var normal = SphericalDirection(sinPhi, cosPhi, theta);
                colorVertices[vIndex++] = new VertexPositionColor(center + normal * radius, colorFunc(normal));
            }
        }

        var colorIndices = new List<short>(latitudeSegments * longitudeSegments * 6);
        for (int lat = 0; lat < latitudeSegments; lat++)
        {
            for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int current = lat * vertexCols + lon;
                int next = current + vertexCols;

                colorIndices.Add((short)current);
                colorIndices.Add((short)(current + 1));
                colorIndices.Add((short)next);

                colorIndices.Add((short)(current + 1));
                colorIndices.Add((short)(next + 1));
                colorIndices.Add((short)next);
            }
        }

        return new PrimitiveMesh(colorVertices, [.. colorIndices], PrimitiveType.TriangleList);
    }

    // Heres your vertice, sir. You probably don't need to call this directly.
    public static PrimitiveMesh BuildTriangle(Vector3 a, Vector3 b, Vector3 c, Color color, bool textured = false)
    {
        if (textured)
        {
            var vertices = new[]
            {
                new VertexPositionColorTexture(a, color, new Vector2(0f, 1f)),
                new VertexPositionColorTexture(b, color, new Vector2(1f, 1f)),
                new VertexPositionColorTexture(c, color, new Vector2(0.5f, 0f))
            };
            return new PrimitiveMesh(vertices, new short[] { 0, 1, 2 }, PrimitiveType.TriangleList);
        }

        var colorVertices = new[]
        {
            new VertexPositionColor(a, color),
            new VertexPositionColor(b, color),
            new VertexPositionColor(c, color)
        };
        return new PrimitiveMesh(colorVertices, new short[] { 0, 1, 2 }, PrimitiveType.TriangleList);
    }

    public static PrimitiveMesh BuildSemiCircle(
        Vector3 center,
        float radius,
        int segments,
        Color color,
        Vector3 normal,
        Vector3 forward,
        bool textured = false)
    {
        if (segments < 2)
            throw new ArgumentOutOfRangeException(nameof(segments), "Semi-circle requires at least two segments.");

        var n = normal.LengthSquared() < 1e-6f ? Vector3.Backward : Vector3.Normalize(normal);
        var f = forward.LengthSquared() < 1e-6f ? Vector3.Forward : Vector3.Normalize(forward);

        if (MathF.Abs(Vector3.Dot(f, n)) > 0.999f)
            f = Vector3.Normalize(Vector3.Cross(n, Vector3.Right));

        var right = Vector3.Normalize(Vector3.Cross(n, f));
        var tangent = Vector3.Normalize(Vector3.Cross(right, n));

        if (textured)
        {
            var texturedVertices = new VertexPositionColorTexture[segments + 2];
            texturedVertices[0] = new VertexPositionColorTexture(center, color, new Vector2(0.5f, 1f));

            float safeRadius = Math.Max(radius, 1e-6f); // todo: remove
            float newStep = MathF.PI / segments;

            for (int i = 0; i <= segments; i++)
            {
                float angle = newStep * i;
                Vector3 offset = right * MathF.Cos(angle) + tangent * MathF.Sin(angle);
                Vector3 point = center + offset * radius;

                float x = MathHelper.Clamp(Vector3.Dot(offset, right), -1f, 1f);
                float y = MathHelper.Clamp(Vector3.Dot(offset, tangent), 0f, 1f);

                float u = 0.5f + 0.5f * x;
                float v = 1f - y;

                texturedVertices[i + 1] = new VertexPositionColorTexture(point, color, new Vector2(u, v));
            }

            var indices = new short[segments * 3];
            for (int i = 0; i < segments; i++)
            {
                int baseIndex = i * 3;
                indices[baseIndex] = 0;
                indices[baseIndex + 1] = (short)(i + 1);
                indices[baseIndex + 2] = (short)(i + 2);
            }

            return new PrimitiveMesh(texturedVertices, indices, PrimitiveType.TriangleList);
        }

        var colorVertices = new VertexPositionColor[segments + 2];
        colorVertices[0] = new VertexPositionColor(center, color);

        float step = MathF.PI / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = step * i;
            Vector3 offset = right * MathF.Cos(angle) + tangent * MathF.Sin(angle);
            colorVertices[i + 1] = new VertexPositionColor(center + offset * radius, color);
        }

        var colorIndices = new short[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 3;
            colorIndices[baseIndex] = 0;
            colorIndices[baseIndex + 1] = (short)(i + 1);
            colorIndices[baseIndex + 2] = (short)(i + 2);
        }

        return new PrimitiveMesh(colorVertices, colorIndices, PrimitiveType.TriangleList);
    }

    /// <summary>
    /// Given a normal and an up hint, builds an orthonormal basis where the normal is the "forward" direction,
    /// the right vector is perpendicular to both, and the up vector is perpendicular to the normal.
    /// The implementation normalizes the inputs, handles near-parallel cases, and then re-orthogonalizes the up vector
    /// from the computed right vector (so it is not guaranteed to be the closest possible to <paramref name="upHint"/>).
    /// </summary>
    /// <param name="normal">The forward/normal direction (fallbacks to <see cref="Vector3.Backward"/> if near-zero).</param>
    /// <param name="upHint">A preferred up direction (fallbacks to <see cref="Vector3.Up"/> if near-zero).</param>
    /// <param name="right">The computed right vector.</param>
    /// <param name="up">The computed up vector.</param>
    private static void BuildFrame(Vector3 normal, Vector3 upHint, out Vector3 right, out Vector3 up)
    {
        var n = normal.LengthSquared() < 1e-6f ? Vector3.Backward : Vector3.Normalize(normal);
        up = upHint.LengthSquared() < 1e-6f ? Vector3.Up : Vector3.Normalize(upHint);

        if (MathF.Abs(Vector3.Dot(n, up)) > 0.999f)
            up = Vector3.Normalize(Vector3.Cross(n, Vector3.Right));

        right = Vector3.Cross(up, n);
        if (right.LengthSquared() < 1e-6f)
            right = Vector3.Cross(Vector3.Forward, n);

        right.Normalize();
        up = Vector3.Normalize(Vector3.Cross(n, right));
    }

    private static Vector3 PolarToCartesian(int index, int total, Vector3 right, Vector3 up)
    {
        float angle = MathHelper.TwoPi * index / total;
        return right * MathF.Cos(angle) + up * MathF.Sin(angle);
    }
    private static Vector3 EllipseDirection(int index, int total, Vector3 right, Vector3 up, Vector2 radii)
    {
        float angle = MathHelper.TwoPi * index / total;
        return right * (MathF.Cos(angle) * radii.X) + up * (MathF.Sin(angle) * radii.Y);
    }

    private static Vector3 SphericalDirection(float sinPhi, float cosPhi, float theta)
    {
        float cosTheta = MathF.Cos(theta);
        float sinTheta = MathF.Sin(theta);
        return new Vector3(sinPhi * cosTheta, cosPhi, sinPhi * sinTheta);
    }
}
