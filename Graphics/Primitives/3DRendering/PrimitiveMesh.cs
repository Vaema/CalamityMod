#nullable enable
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public readonly struct PrimitiveMesh
{
    private readonly VertexPositionColorTexture[]? _texturedVertices;
    private readonly VertexPositionColor[]? _colorVertices;

    public PrimitiveMesh(VertexPositionColorTexture[] vertices, short[] indices, PrimitiveType primitiveType)
    {
        _texturedVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        _colorVertices = null;
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        PrimitiveType = primitiveType;

        if (_texturedVertices.Length == 0 || Indices.Length == 0)
            throw new ArgumentException("Mesh must contain vertices and indices.");
        if (_texturedVertices.Length > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertices), "Vertex count exceeds index buffer range.");

        UsesTexture = true;
    }

    public PrimitiveMesh(VertexPositionColor[] vertices, short[] indices, PrimitiveType primitiveType)
    {
        _texturedVertices = null;
        _colorVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        PrimitiveType = primitiveType;

        if (_colorVertices.Length == 0 || Indices.Length == 0)
            throw new ArgumentException("Mesh must contain vertices and indices.");
        if (_colorVertices.Length > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertices), "Vertex count exceeds index buffer range.");

        UsesTexture = false;
    }

    public bool UsesTexture { get; }
    public int VertexCount => UsesTexture ? _texturedVertices!.Length : _colorVertices!.Length;
    public VertexPositionColorTexture[] TexturedVertices => _texturedVertices ?? throw new InvalidOperationException("Mesh does not contain textured vertices.");
    public VertexPositionColor[] ColorVertices => _colorVertices ?? throw new InvalidOperationException("Mesh does not contain color-only vertices.");
    public short[] Indices { get; }
    public PrimitiveType PrimitiveType { get; }
    public bool IsValid => VertexCount > 0 && Indices.Length > 0;

    public PrimitiveMeshView AsView()
    {
        return UsesTexture
            ? new PrimitiveMeshView(_texturedVertices!, Indices, PrimitiveType, 0, _texturedVertices!.Length, 0, Indices.Length)
            : new PrimitiveMeshView(_colorVertices!, Indices, PrimitiveType, 0, _colorVertices!.Length, 0, Indices.Length);
    }

    public static PrimitiveMesh FromSequential(VertexPositionColorTexture[] vertices, PrimitiveType primitiveType)
    {
        if (vertices == null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Length == 0)
            throw new ArgumentException("Vertices collection is empty.", nameof(vertices));
        if (vertices.Length > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertices), "Vertex count exceeds index buffer range.");

        var indices = new short[vertices.Length];
        PrimitiveSimd.FillSequentialIndices(indices.AsSpan());

        return new PrimitiveMesh(vertices, indices, primitiveType);
    }

    public static PrimitiveMesh FromSequential(VertexPositionColor[] vertices, PrimitiveType primitiveType)
    {
        if (vertices == null)
            throw new ArgumentNullException(nameof(vertices));
        if (vertices.Length == 0)
            throw new ArgumentException("Vertices collection is empty.", nameof(vertices));
        if (vertices.Length > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertices), "Vertex count exceeds index buffer range.");

        var indices = new short[vertices.Length];
        PrimitiveSimd.FillSequentialIndices(indices.AsSpan());

        return new PrimitiveMesh(vertices, indices, primitiveType);
    }
}
