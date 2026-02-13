#nullable enable
using System;
using System.Buffers;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Graphics.Primitives;

public sealed class PrimitiveMeshCache
{
    public static PrimitiveMeshCache Shared { get; } = new PrimitiveMeshCache();

    private readonly ArrayPool<VertexPositionColorTexture> _texturedPool;
    private readonly ArrayPool<VertexPositionColor> _colorPool;
    private readonly ArrayPool<short> _indexPool;
    private readonly bool _clearOnReturn;

    public PrimitiveMeshCache(
        ArrayPool<VertexPositionColorTexture>? texturedPool = null,
        ArrayPool<VertexPositionColor>? colorPool = null,
        ArrayPool<short>? indexPool = null,
        bool clearOnReturn = false)
    {
        _texturedPool = texturedPool ?? ArrayPool<VertexPositionColorTexture>.Shared;
        _colorPool = colorPool ?? ArrayPool<VertexPositionColor>.Shared;
        _indexPool = indexPool ?? ArrayPool<short>.Shared;
        _clearOnReturn = clearOnReturn;
    }

    public PooledPrimitiveMesh RentTextured(int vertexCount, int indexCount, PrimitiveType primitiveType)
    {
        if (vertexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "You're trying to rent.. nothing?");
        if (indexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(indexCount), "You're trying to rent.. nothing?");
        if (vertexCount > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Vertex count exceeds index buffer range.");

        var vertices = _texturedPool.Rent(vertexCount);
        var indices = _indexPool.Rent(indexCount);
        return new PooledPrimitiveMesh(this, vertices, null, indices, vertexCount, indexCount, primitiveType, true);
    }

    public PooledPrimitiveMesh RentColored(int vertexCount, int indexCount, PrimitiveType primitiveType)
    {
        if (vertexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Cannot meaningfully rent zero vertices.");
        if (indexCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(indexCount), "Cannot meaningfully rent an index buffer of size zero.");
        if (vertexCount > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(vertexCount), "Vertex count exceeds index buffer range.");

        var vertices = _colorPool.Rent(vertexCount);
        var indices = _indexPool.Rent(indexCount);
        return new PooledPrimitiveMesh(this, null, vertices, indices, vertexCount, indexCount, primitiveType, false);
    }

    public PooledPrimitiveMesh RentFromMesh(in PrimitiveMesh mesh)
    {
        if (!mesh.IsValid)
            throw new ArgumentException("Mesh is invalid.", nameof(mesh));

        if (mesh.UsesTexture)
        {
            var lease = RentTextured(mesh.VertexCount, mesh.Indices.Length, mesh.PrimitiveType);
            Array.Copy(mesh.TexturedVertices, lease.TexturedVertices, mesh.VertexCount);
            Array.Copy(mesh.Indices, lease.Indices, mesh.Indices.Length);
            return lease;
        }
        else
        {
            var lease = RentColored(mesh.VertexCount, mesh.Indices.Length, mesh.PrimitiveType);
            Array.Copy(mesh.ColorVertices, lease.ColorVertices, mesh.VertexCount);
            Array.Copy(mesh.Indices, lease.Indices, mesh.Indices.Length);
            return lease;
        }
    }

    internal void Return(VertexPositionColorTexture[]? textured, VertexPositionColor[]? colored, short[] indices, bool usesTexture)
    {
        if (usesTexture)
        {
            if (textured != null)
                _texturedPool.Return(textured, _clearOnReturn);
        }
        else
        {
            if (colored != null)
                _colorPool.Return(colored, _clearOnReturn);
        }

        _indexPool.Return(indices, _clearOnReturn);
    }
}
