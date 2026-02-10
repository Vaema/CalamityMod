#nullable enable
using System;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public readonly struct PooledPrimitiveMesh : IDisposable
{
    private readonly PrimitiveMeshCache? _cache;
    private readonly VertexPositionColorTexture[]? _textured;
    private readonly VertexPositionColor[]? _colored;
    private readonly short[] _indices;

    internal PooledPrimitiveMesh(
        PrimitiveMeshCache cache,
        VertexPositionColorTexture[]? textured,
        VertexPositionColor[]? colored,
        short[] indices,
        int vertexCount,
        int indexCount,
        PrimitiveType primitiveType,
        bool usesTexture)
    {
        _cache = cache;
        _textured = textured;
        _colored = colored;
        _indices = indices ?? throw new ArgumentNullException(nameof(indices));
        VertexCount = vertexCount;
        IndexCount = indexCount;
        PrimitiveType = primitiveType;
        UsesTexture = usesTexture;
    }

    public bool UsesTexture { get; }
    public int VertexCount { get; }
    public int IndexCount { get; }
    public PrimitiveType PrimitiveType { get; }

    public VertexPositionColorTexture[] TexturedVertices => _textured ?? throw new InvalidOperationException("Mesh lease does not contain textured vertices.");
    public VertexPositionColor[] ColorVertices => _colored ?? throw new InvalidOperationException("Mesh lease does not contain color-only vertices.");
    public short[] Indices => _indices;

    public PrimitiveMeshView View => UsesTexture
        ? new PrimitiveMeshView(_textured!, _indices, PrimitiveType, 0, VertexCount, 0, IndexCount)
        : new PrimitiveMeshView(_colored!, _indices, PrimitiveType, 0, VertexCount, 0, IndexCount);

    public void Dispose()
    {
        _cache?.Return(_textured, _colored, _indices, UsesTexture);
    }
}
