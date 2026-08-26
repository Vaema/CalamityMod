#nullable enable

// 10FEB2026: jasper: you probably don't need this file for casual mesh use.
// you should be careful with mesh transformations, they are intended for advanced users.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public static class PrimitiveMeshExtensions
{
    public static PrimitiveMesh Transform(this in PrimitiveMesh mesh, in Matrix transform)
    {
        if (!mesh.IsValid)
            return mesh;

        var indices = (short[])mesh.Indices.Clone();

        if (mesh.UsesTexture)
        {
            var source = mesh.TexturedVertices;
            var transformed = new VertexPositionColorTexture[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var v = source[i];
                transformed[i] = new VertexPositionColorTexture(
                    Vector3.Transform(v.Position, transform),
                    v.Color,
                    v.TextureCoordinate);
            }

            return new PrimitiveMesh(transformed, indices, mesh.PrimitiveType);
        }

        {
            var source = mesh.ColorVertices;
            var transformed = new VertexPositionColor[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                var v = source[i];
                transformed[i] = new VertexPositionColor(
                    Vector3.Transform(v.Position, transform),
                    v.Color);
            }

            return new PrimitiveMesh(transformed, indices, mesh.PrimitiveType);
        }
    }

    public static PrimitiveMesh Translate(this in PrimitiveMesh mesh, in Vector3 offset) =>
        mesh.Transform(Matrix.CreateTranslation(offset));

    public static PrimitiveMesh Scale(this in PrimitiveMesh mesh, in Vector3 scale) =>
        mesh.Transform(Matrix.CreateScale(scale));

    public static PrimitiveMesh Rotate(this in PrimitiveMesh mesh, in Quaternion rotation) =>
        mesh.Transform(Matrix.CreateFromQuaternion(rotation));

    public static PrimitiveMesh Extrude(this in PrimitiveMesh mesh, in Vector3 direction, bool closeCaps = true)
    {
        if (!mesh.IsValid)
            throw new InvalidOperationException("Cannot extrude an invalid mesh.");
        if (mesh.PrimitiveType != PrimitiveType.TriangleList)
            throw new NotSupportedException("Extrusion currently supports triangle list meshes only.");
        if (direction.LengthSquared() <= 1e-6f)
            return mesh;

        return mesh.UsesTexture
            ? ExtrudeTextured(mesh, direction, closeCaps)
            : ExtrudeColored(mesh, direction, closeCaps);
    }

    private static PrimitiveMesh ExtrudeTextured(in PrimitiveMesh mesh, Vector3 direction, bool closeCaps)
    {
        var top = mesh.TexturedVertices;
        var baseIndices = mesh.Indices;
        int originalVertexCount = top.Length;

        var vertices = new List<VertexPositionColorTexture>(originalVertexCount * 2);
        vertices.AddRange(top);

        for (int i = 0; i < originalVertexCount; i++)
        {
            var v = top[i];
            vertices.Add(new VertexPositionColorTexture(
                v.Position + direction,
                v.Color,
                v.TextureCoordinate));
        }

        var indices = new List<short>(closeCaps ? baseIndices.Length * 2 : baseIndices.Length + originalVertexCount * 6);
        indices.AddRange(baseIndices);

        if (closeCaps)
        {
            for (int i = 0; i < baseIndices.Length; i += 3)
            {
                short a = (short)(baseIndices[i] + originalVertexCount);
                short b = (short)(baseIndices[i + 1] + originalVertexCount);
                short c = (short)(baseIndices[i + 2] + originalVertexCount);
                indices.Add(c);
                indices.Add(b);
                indices.Add(a);
            }
        }

        var positions = new Vector3[originalVertexCount];
        for (int i = 0; i < originalVertexCount; i++)
            positions[i] = top[i].Position;

        var loops = BuildBoundaryLoops(positions, baseIndices);
        foreach (var loop in loops)
        {
            float perimeter = 0f;
            for (int i = 0; i < loop.Count; i++)
                perimeter += loop[i].Length;
            if (perimeter <= 1e-6f)
                continue;

            float accumulated = 0f;
            foreach (var edge in loop)
            {
                float u0 = accumulated / perimeter;
                accumulated += edge.Length;
                float u1 = accumulated / perimeter;

                var startTop = top[edge.Start];
                var endTop = top[edge.End];
                var startBottomPos = startTop.Position + direction;
                var endBottomPos = endTop.Position + direction;

                short topStartIndex = AddVertex(vertices, new VertexPositionColorTexture(startTop.Position, startTop.Color, new Vector2(u0, 0f)));
                short bottomStartIndex = AddVertex(vertices, new VertexPositionColorTexture(startBottomPos, startTop.Color, new Vector2(u0, 1f)));
                short topEndIndex = AddVertex(vertices, new VertexPositionColorTexture(endTop.Position, endTop.Color, new Vector2(u1, 0f)));
                short bottomEndIndex = AddVertex(vertices, new VertexPositionColorTexture(endBottomPos, endTop.Color, new Vector2(u1, 1f)));

                indices.Add(topStartIndex);
                indices.Add(topEndIndex);
                indices.Add(bottomEndIndex);

                indices.Add(topStartIndex);
                indices.Add(bottomEndIndex);
                indices.Add(bottomStartIndex);
            }
        }

        return new PrimitiveMesh(vertices.ToArray(), [.. indices], PrimitiveType.TriangleList);
    }

    private static PrimitiveMesh ExtrudeColored(in PrimitiveMesh mesh, Vector3 direction, bool closeCaps)
    {
        var top = mesh.ColorVertices;
        var baseIndices = mesh.Indices;
        int originalVertexCount = top.Length;

        var vertices = new List<VertexPositionColor>(originalVertexCount * 2);
        vertices.AddRange(top);

        for (int i = 0; i < originalVertexCount; i++)
        {
            var v = top[i];
            vertices.Add(new VertexPositionColor(v.Position + direction, v.Color));
        }

        var indices = new List<short>(closeCaps ? baseIndices.Length * 2 : baseIndices.Length + originalVertexCount * 6);
        indices.AddRange(baseIndices);

        if (closeCaps)
        {
            for (int i = 0; i < baseIndices.Length; i += 3)
            {
                short a = (short)(baseIndices[i] + originalVertexCount);
                short b = (short)(baseIndices[i + 1] + originalVertexCount);
                short c = (short)(baseIndices[i + 2] + originalVertexCount);
                indices.Add(c);
                indices.Add(b);
                indices.Add(a);
            }
        }

        var positions = new Vector3[originalVertexCount];
        for (int i = 0; i < originalVertexCount; i++)
            positions[i] = top[i].Position;

        var loops = BuildBoundaryLoops(positions, baseIndices);
        foreach (var loop in loops)
        {
            float perimeter = 0f;
            for (int i = 0; i < loop.Count; i++)
                perimeter += loop[i].Length;
            if (perimeter <= 1e-6f)
                continue;

            float accumulated = 0f;
            foreach (var edge in loop)
            {
                float u0 = accumulated / perimeter;
                accumulated += edge.Length;
                float u1 = accumulated / perimeter;

                var startTop = top[edge.Start];
                var endTop = top[edge.End];
                var startBottomPos = startTop.Position + direction;
                var endBottomPos = endTop.Position + direction;

                short topStartIndex = AddVertex(vertices, new VertexPositionColor(startTop.Position, startTop.Color));
                short bottomStartIndex = AddVertex(vertices, new VertexPositionColor(startBottomPos, startTop.Color));
                short topEndIndex = AddVertex(vertices, new VertexPositionColor(endTop.Position, endTop.Color));
                short bottomEndIndex = AddVertex(vertices, new VertexPositionColor(endBottomPos, endTop.Color));

                indices.Add(topStartIndex);
                indices.Add(topEndIndex);
                indices.Add(bottomEndIndex);

                indices.Add(topStartIndex);
                indices.Add(bottomEndIndex);
                indices.Add(bottomStartIndex);
            }
        }

        return new PrimitiveMesh(vertices.ToArray(), [.. indices], PrimitiveType.TriangleList);
    }

    public static PrimitiveMesh CurveEdges(this in PrimitiveMesh mesh, in Vector3 axis, float magnitude, float exponent = 2f)
    {
        if (!mesh.IsValid)
            return mesh;
        if (axis.LengthSquared() <= 1e-6f)
            throw new ArgumentException("Axis must be non-zero.", nameof(axis));
        if (mesh.PrimitiveType != PrimitiveType.TriangleList && mesh.PrimitiveType != PrimitiveType.TriangleStrip)
            throw new NotSupportedException("CurveEdges supports triangle-based meshes.");

        float clampedExponent = Math.Max(exponent, 1e-3f);
        Vector3 axisDir = Vector3.Normalize(axis);
        var indices = (short[])mesh.Indices.Clone();

        if (mesh.UsesTexture)
        {
            var source = mesh.TexturedVertices;
            var vertices = new VertexPositionColorTexture[source.Length];
            ComputeCentroidAndRadius(source, out var centroid, out float radius);

            for (int i = 0; i < source.Length; i++)
            {
                var v = source[i];
                Vector3 diff = v.Position - centroid;
                float radial = radius <= 1e-6f ? 0f : diff.Length() / radius;
                float factor = MathF.Pow(MathHelper.Clamp(radial, 0f, 1f), clampedExponent);
                Vector3 offset = axisDir * magnitude * factor;

                vertices[i] = new VertexPositionColorTexture(v.Position + offset, v.Color, v.TextureCoordinate);
            }

            return new PrimitiveMesh(vertices, indices, mesh.PrimitiveType);
        }
        else
        {
            var source = mesh.ColorVertices;
            var vertices = new VertexPositionColor[source.Length];
            ComputeCentroidAndRadius(source, out var centroid, out float radius);

            for (int i = 0; i < source.Length; i++)
            {
                var v = source[i];
                Vector3 diff = v.Position - centroid;
                float radial = radius <= 1e-6f ? 0f : diff.Length() / radius;
                float factor = MathF.Pow(MathHelper.Clamp(radial, 0f, 1f), clampedExponent);
                Vector3 offset = axisDir * magnitude * factor;

                vertices[i] = new VertexPositionColor(v.Position + offset, v.Color);
            }

            return new PrimitiveMesh(vertices, indices, mesh.PrimitiveType);
        }
    }

    private static void ComputeCentroidAndRadius(VertexPositionColorTexture[] vertices, out Vector3 centroid, out float radius)
    {
        centroid = Vector3.Zero;
        for (int i = 0; i < vertices.Length; i++)
            centroid += vertices[i].Position;
        centroid /= vertices.Length;

        radius = 1e-6f;
        for (int i = 0; i < vertices.Length; i++)
        {
            float length = Vector3.Distance(centroid, vertices[i].Position);
            if (length > radius)
                radius = length;
        }
    }

    private static void ComputeCentroidAndRadius(VertexPositionColor[] vertices, out Vector3 centroid, out float radius)
    {
        centroid = Vector3.Zero;
        for (int i = 0; i < vertices.Length; i++)
            centroid += vertices[i].Position;
        centroid /= vertices.Length;

        radius = 1e-6f;
        for (int i = 0; i < vertices.Length; i++)
        {
            float length = Vector3.Distance(centroid, vertices[i].Position);
            if (length > radius)
                radius = length;
        }
    }

    /// <summary>
    /// Builds boundary loops from a triangle list. 
    /// Each loop is a sequence of connected boundary edges that form a closed path along the mesh surface.
    /// Loops are returned in no particular order, and may be nested or intersecting.
    /// Edges are oriented consistently within each loop, but not across different loops.
    /// Only zero-length edges are discarded, and loops must close to be included.
    /// </summary>
    /// <param name="positions">Vertex positions for the mesh.</param>
    /// <param name="indices">Triangle-list indices (groups of 3).</param>
    /// <returns>Closed boundary loops built from boundary edges.</returns>
    private static List<List<OrientedEdge>> BuildBoundaryLoops(IReadOnlyList<Vector3> positions, short[] indices)
    {
        var edgeMap = new Dictionary<EdgeKey, EdgeAccumulator>();
        for (int i = 0; i < indices.Length; i += 3)
        {
            RegisterEdge(edgeMap, indices[i], indices[i + 1]);
            RegisterEdge(edgeMap, indices[i + 1], indices[i + 2]);
            RegisterEdge(edgeMap, indices[i + 2], indices[i]);
        }

        var edges = new List<OrientedEdge>();
        foreach (var pair in edgeMap)
        {
            if (pair.Value.Count == 1)
            {
                var oriented = pair.Value.Orientation;
                float length = Vector3.Distance(positions[oriented.Start], positions[oriented.End]);
                if (length > 1e-6f)
                    edges.Add(new OrientedEdge(oriented.Start, oriented.End, length));
            }
        }

        var loops = new List<List<OrientedEdge>>();
        var used = new bool[edges.Count];

        for (int i = 0; i < edges.Count; i++)
        {
            if (used[i])
                continue;

            var loop = new List<OrientedEdge>();
            used[i] = true;
            loop.Add(edges[i]);

            short head = edges[i].End;
            bool closed = head == loop[0].Start;

            while (!closed)
            {
                bool found = false;
                for (int j = 0; j < edges.Count; j++)
                {
                    if (used[j])
                        continue;

                    var candidate = edges[j];
                    if (candidate.Start == head)
                    {
                        used[j] = true;
                        loop.Add(candidate);
                        head = candidate.End;
                        found = true;
                    }
                    else if (candidate.End == head)
                    {
                        candidate = candidate.Reversed();
                        edges[j] = candidate;
                        used[j] = true;
                        loop.Add(candidate);
                        head = candidate.End;
                        found = true;
                    }

                    if (found)
                    {
                        closed = head == loop[0].Start;
                        break;
                    }
                }

                if (!found)
                    break;
            }

            if (loop.Count > 1 && closed)
                loops.Add(loop);
        }

        return loops;
    }

    private static void RegisterEdge(Dictionary<EdgeKey, EdgeAccumulator> edgeMap, short start, short end)
    {
        var key = new EdgeKey(Math.Min(start, end), Math.Max(start, end));
        if (edgeMap.TryGetValue(key, out var accumulator))
        {
            accumulator.Count++;
            edgeMap[key] = accumulator;
        }
        else
        {
            edgeMap[key] = new EdgeAccumulator
            {
                Count = 1,
                Orientation = new OrientedEdge(start, end, 0f)
            };
        }
    }

    /// <summary>
    /// Adds a vertex to the list and returns its index. Most likely, you do not need to use this directly.
    /// </summary> 
    /// <param name="vertices">The vertex list to add to.</param>
    /// <param name="vertex">The vertex to add.</param>
    private static short AddVertex(List<VertexPositionColorTexture> vertices, VertexPositionColorTexture vertex)
    {
        if (vertices.Count >= short.MaxValue)
            throw new InvalidOperationException("Primitive mesh exceeded 16-bit vertex capacity.");
        vertices.Add(vertex);
        return (short)(vertices.Count - 1);
    }

    /// <summary>
    /// Adds a vertex to the list and returns its index. Most likely, you do not need to use this directly.
    /// </summary> 
    /// <param name="vertices">The vertex list to add to.</param>
    /// <param name="vertex">The vertex to add.</param>
    private static short AddVertex(List<VertexPositionColor> vertices, VertexPositionColor vertex)
    {
        if (vertices.Count >= short.MaxValue)
            throw new InvalidOperationException("Primitive mesh exceeded 16-bit vertex capacity.");
        vertices.Add(vertex);
        return (short)(vertices.Count - 1);
    }

    private readonly struct EdgeKey : IEquatable<EdgeKey>
    {
        public readonly short Min;
        public readonly short Max;

        public EdgeKey(short min, short max)
        {
            Min = min;
            Max = max;
        }

        public bool Equals(EdgeKey other) => Min == other.Min && Max == other.Max;

        public override bool Equals(object? obj) => obj is EdgeKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Min * 397) ^ Max;
            }
        }
    }

    private struct EdgeAccumulator
    {
        public int Count;
        public OrientedEdge Orientation;
    }

    private struct OrientedEdge
    {
        public short Start;
        public short End;
        public float Length;

        public OrientedEdge(short start, short end, float length)
        {
            Start = start;
            End = end;
            Length = length;
        }

        public OrientedEdge Reversed() => new(End, Start, Length);
    }
}
