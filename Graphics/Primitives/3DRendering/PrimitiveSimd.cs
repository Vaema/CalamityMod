using System;

namespace CalamityMod.Graphics.Primitives;

internal static class PrimitiveSimd
{
    private static readonly System.Numerics.Vector<int> LaneOffsets = CreateLaneOffsets();

    public static void FillSequentialIndices(Span<short> indices)
    {
        int i = 0;
        if (System.Numerics.Vector.IsHardwareAccelerated)
        {
            int width = System.Numerics.Vector<int>.Count;
            Span<int> temp = width <= 16
                ? stackalloc int[width]
                : new int[width];

            while (i <= indices.Length - width)
            {
                (LaneOffsets + new System.Numerics.Vector<int>(i)).CopyTo(temp);
                for (int lane = 0; lane < width; lane++)
                    indices[i + lane] = (short)temp[lane];
                i += width;
            }
        }

        for (; i < indices.Length; i++)
            indices[i] = (short)i;
    }

    private static System.Numerics.Vector<int> CreateLaneOffsets()
    {
        int width = System.Numerics.Vector<int>.Count;
        Span<int> lanes = stackalloc int[width];
        for (int i = 0; i < width; i++)
            lanes[i] = i;
        return new System.Numerics.Vector<int>(lanes);
    }
}
