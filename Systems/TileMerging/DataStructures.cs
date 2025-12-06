using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace CalamityMod.Systems
{
    // bit to relative tile pos for reference
    // MSB 0->7 LSB
    // 4 0 5
    // 2 X 3
    // 6 1 7

    //  NE  N   NW
    //
    //  E   □   W
    //
    //  SE  S   SW

    public enum BlendSideFlags : byte
    {
        None = 0,

        Up = 0b1000_0000,
        Down = 0b0100_0000,
        Right = 0b0010_0000,
        Left = 0b0001_0000,
        UpLeft = 0b0000_1000,
        UpRight = 0b0000_0100,
        DownLeft = 0b0000_0010,
        DownRight = 0b0000_0001,

        AllSide = 0b1111_1111,

        // Special Shapes
        Shape_AllSide = AllSide,
        ShapeCorner_UpLeft = UpLeft,
        ShapeCorner_UpRight = UpRight,
        ShapeCorner_DownLeft = DownLeft,
        ShapeCorner_DownRight = DownRight,

        // I Shapes

        ShapeI_Up = Up | UpLeft | UpRight,
        ShapeI_Up_End = Up,
        ShapeI_Up_LeftEnd = Up | UpRight,
        ShapeI_Up_RightEnd = Up | UpLeft,

        ShapeI_Down = Down | DownLeft | DownRight,
        ShapeI_Down_End = Down,
        ShapeI_Down_LeftEnd = Down | DownRight,
        ShapeI_Down_RightEnd = Down | DownLeft,

        ShapeI_Left = Left | UpLeft | DownLeft,
        ShapeI_Left_End = Left,
        ShapeI_Left_UpEnd = Left | DownLeft,
        ShapeI_Left_DownEnd = Left | UpLeft,

        ShapeI_Right = Right | UpRight | DownRight,
        ShapeI_Right_End = Right,
        ShapeI_Right_UpEnd = Right | DownRight,
        ShapeI_Right_DownEnd = Right | UpRight,

        // L Shapes

        ShapeL_UpLeft = ShapeI_Up | ShapeI_Left,
        ShapeL_UpLeft_End = ShapeI_Up_End | ShapeI_Left_End | UpLeft,
        ShapeL_UpLeft_RightEnd = ShapeI_Up_RightEnd | ShapeI_Left,
        ShapeL_UpLeft_DownEnd = ShapeI_Up | ShapeI_Left_DownEnd,

        ShapeL_UpRight = ShapeI_Up | ShapeI_Right,
        ShapeL_UpRight_End = ShapeI_Up_End | ShapeI_Right_End | UpRight,
        ShapeL_UpRight_LeftEnd = ShapeI_Up_End | ShapeI_Right,
        ShapeL_UpRight_DownEnd = ShapeI_Up | ShapeI_Right_End,

        ShapeL_DownLeft = ShapeI_Down | ShapeI_Left,
        ShapeL_DownLeft_End = ShapeI_Down_End | ShapeI_Left_End | DownLeft,
        ShapeL_DownLeft_RightEnd = ShapeI_Down_End | ShapeI_Left,
        ShapeL_DownLeft_UpEnd = ShapeI_Down | ShapeI_Left_End,

        ShapeL_DownRight = ShapeI_Down | ShapeI_Right,
        ShapeL_DownRight_End = ShapeI_Down_End | ShapeI_Right_End | DownRight,
        ShapeL_DownRight_LeftEnd = ShapeI_Down_End | ShapeI_Right,
        ShapeL_DownRight_UpEnd = ShapeI_Down | ShapeI_Right_End,

        // U Shapes

        ShapeU_UpEmpty = ShapeI_Left | ShapeI_Right | ShapeI_Down,
        ShapeU_UpEmpty_End = ShapeI_Left_End | ShapeI_Right_End | ShapeI_Down,
        ShapeU_UpEmpty_LeftEnd = ShapeI_Left_End | ShapeI_Right | ShapeI_Down,
        ShapeU_UpEmpty_RightEnd = ShapeI_Left | ShapeI_Right_End | ShapeI_Down,

        ShapeU_DownEmpty = ShapeI_Left | ShapeI_Right | ShapeI_Up,
        ShapeU_DownEmpty_End = ShapeI_Left_End | ShapeI_Right_End | ShapeI_Up,
        ShapeU_DownEmpty_LeftEnd = ShapeI_Left_End | ShapeI_Right | ShapeI_Up,
        ShapeU_DownEmpty_RightEnd = ShapeI_Left | ShapeI_Right_End | ShapeI_Up,

        ShapeU_LeftEmpty = ShapeI_Up | ShapeI_Down | ShapeI_Right,
        ShapeU_LeftEmpty_End = ShapeI_Up_End | ShapeI_Down_End | ShapeI_Right,
        ShapeU_LeftEmpty_UpEnd = ShapeI_Up_End | ShapeI_Down | ShapeI_Right,
        ShapeU_LeftEmpty_DownEnd = ShapeI_Up | ShapeI_Down_End | ShapeI_Right,

        ShapeU_RightEmpty = ShapeI_Up | ShapeI_Down | ShapeI_Left,
        ShapeU_RightEmpty_End = ShapeI_Up_End | ShapeI_Down_End | ShapeI_Left,
        ShapeU_RightEmpty_UpEnd = ShapeI_Up_End | ShapeI_Down | ShapeI_Left,
        ShapeU_RightEmpty_DownEnd = ShapeI_Up | ShapeI_Down_End | ShapeI_Left,
    }

    public struct SheetPositionKey(BlendSideFlags blendSides, byte randomFrameIndex)
    {
        public BlendSideFlags BlendSides = blendSides;
        public byte RandomFrameIndex = randomFrameIndex;
    }

    public sealed class SheetPositionKeyEqualityComparator : IEqualityComparer<SheetPositionKey>
    {
        public bool Equals(SheetPositionKey x, SheetPositionKey y)
        {
            return x.BlendSides == y.BlendSides && x.RandomFrameIndex == y.RandomFrameIndex;
        }

        public int GetHashCode([DisallowNull] SheetPositionKey obj)
        {
            return ((int)obj.BlendSides << 8) | obj.RandomFrameIndex;
        }
    }

    public struct SheetPosition
    {
        public byte X;
        public byte Y;
        public sbyte BakedSheetIndex;

        public SheetPosition(int x, int y, sbyte bakedSheetIndex = -1)
        {
            BakedSheetIndex = bakedSheetIndex;
            if (IsUsingBaseTexture)
            {
                X = (byte)(x / 18);
                Y = (byte)(y / 18);
            }
            else
            {
                X = (byte)(x / TileBlendTexture.BlendTextureFrameWidth);
                Y = (byte)(y / TileBlendTexture.BlendTextureFrameHeight);
            }
        }

        public readonly Vector2 GetDrawPosition()
        {
            if (IsUsingBaseTexture) return new Vector2(X * 18.0f, Y * 18.0f);
            else return new Vector2(X * TileBlendTexture.BlendTextureFrameWidth, Y * TileBlendTexture.BlendTextureFrameHeight);
        }

        public readonly Rectangle GetDrawRect()
        {
            if (IsUsingBaseTexture) return new Rectangle(X * 18, Y * 18, 16, 16);
            else return new Rectangle(X * TileBlendTexture.BlendTextureFrameWidth, Y * TileBlendTexture.BlendTextureFrameHeight, 16, 16);
        }

        public readonly bool IsUsingBaseTexture => BakedSheetIndex < 0;
    }

    public struct TileBlendingRef(ushort sheetIdx, byte blendData)
    {
        public ushort SheetIndex = sheetIdx;
        public byte BlendData = blendData;
    }
}
