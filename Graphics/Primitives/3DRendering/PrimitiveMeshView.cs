#nullable enable
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public readonly struct PrimitiveMeshView
{
    private readonly VertexPositionColorTexture[]? _texturedVertices;
    private readonly VertexPositionColor[]? _colorVertices;

    public PrimitiveMeshView(
        VertexPositionColorTexture[] vertices,
        short[] indices,
        PrimitiveType primitiveType,
        int vertexOffset,
        int vertexCount,
        int indexOffset,
        int indexCount)
    {
        _texturedVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        _colorVertices = null;
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        PrimitiveType = primitiveType;

        ValidateRanges(vertices.Length, indices.Length, vertexOffset, vertexCount, indexOffset, indexCount);

        VertexOffset = vertexOffset;
        VertexCount = vertexCount;
        IndexOffset = indexOffset;
        IndexCount = indexCount;
        UsesTexture = true;
    }

    public PrimitiveMeshView(
        VertexPositionColor[] vertices,
        short[] indices,
        PrimitiveType primitiveType,
        int vertexOffset,
        int vertexCount,
        int indexOffset,
        int indexCount)
    {
        _texturedVertices = null;
        _colorVertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        PrimitiveType = primitiveType;

        ValidateRanges(vertices.Length, indices.Length, vertexOffset, vertexCount, indexOffset, indexCount);

        VertexOffset = vertexOffset;
        VertexCount = vertexCount;
        IndexOffset = indexOffset;
        IndexCount = indexCount;
        UsesTexture = false;
    }

    public bool UsesTexture { get; }
    public int VertexOffset { get; }
    public int VertexCount { get; }
    public int IndexOffset { get; }
    public int IndexCount { get; }
    public PrimitiveType PrimitiveType { get; }
    public bool IsValid => VertexCount > 0 && IndexCount > 0;

    public VertexPositionColorTexture[] TexturedVertices => _texturedVertices ?? throw new InvalidOperationException("Mesh view does not contain textured vertices.");
    public VertexPositionColor[] ColorVertices => _colorVertices ?? throw new InvalidOperationException("Mesh view does not contain color-only vertices.");
    public short[] Indices { get; }

    private static void ValidateRanges(int vertexArrayLength, int indexArrayLength, int vertexOffset, int vertexCount, int indexOffset, int indexCount)
    {
        if (vertexOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(vertexOffset));
        if (vertexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (indexOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(indexOffset));
        if (indexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(indexCount));
        if (vertexOffset + vertexCount > vertexArrayLength)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Vertex range exceeds the vertex buffer length.");
        if (indexOffset + indexCount > indexArrayLength)
            throw new ArgumentOutOfRangeException(nameof(indexCount), "Index range exceeds the index buffer length.");
        if (vertexCount > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Vertex count exceeds index buffer range.");
    }
}
