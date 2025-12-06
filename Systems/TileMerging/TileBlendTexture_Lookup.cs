using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using static CalamityMod.Systems.BlendSideFlags;

namespace CalamityMod.Systems
{
    public abstract partial class TileBlendTexture : ModTexturedType
    {
        // Welcome to Hardcoded hell
        // Do NOT change anything in here unless you understand absolutely everything

        #region Sheet Rects
        // I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME
        // I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME
        // I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME I HATE THIS GAME

        //
        // I shapes (16 variants)
        //

        private static readonly Rectangle[] _I_Up = Create3RectsDirX(1, 2);
        private static readonly Rectangle[] _I_Up_End = Create3RectsDirX(8, 6);
        private static readonly Rectangle[] _I_Up_LeftEnd = Create3RectsDirY(6, 6);
        private static readonly Rectangle[] _I_Up_RightEnd = Create3RectsDirY(7, 6);

        private static readonly Rectangle[] _I_Down = Create3RectsDirX(1, 0);
        private static readonly Rectangle[] _I_Down_End = Create3RectsDirX(8, 3);
        private static readonly Rectangle[] _I_Down_LeftEnd = Create3RectsDirY(6, 3);
        private static readonly Rectangle[] _I_Down_RightEnd = Create3RectsDirY(7, 3);

        private static readonly Rectangle[] _I_Left = Create3RectsDirY(4, 0);
        private static readonly Rectangle[] _I_Left_End = Create3RectsDirY(8, 0);
        private static readonly Rectangle[] _I_Left_UpEnd = Create3RectsDirX(3, 9);
        private static readonly Rectangle[] _I_Left_DownEnd = Create3RectsDirX(3, 10);

        private static readonly Rectangle[] _I_Right = Create3RectsDirY(0, 0);
        private static readonly Rectangle[] _I_Right_End = Create3RectsDirY(5, 0);
        private static readonly Rectangle[] _I_Right_UpEnd = Create3RectsDirX(0, 9);
        private static readonly Rectangle[] _I_Right_DownEnd = Create3RectsDirX(0, 10);

        //
        // L shapes (16 variants)
        //

        private static readonly Rectangle[] _L_UpLeft = Create3RectsDirX(0, 5, increment: 2);
        private static readonly Rectangle[] _L_UpLeft_End = Create3RectsDirX(0, 7, increment: 2);
        private static readonly Rectangle[] _L_UpLeft_RightEnd = Create3RectsDirY(11, 3, increment: 2);
        private static readonly Rectangle[] _L_UpLeft_DownEnd = Create3RectsDirY(13, 3, increment: 2);


        private static readonly Rectangle[] _L_UpRight = Create3RectsDirX(1, 5, increment: 2);
        private static readonly Rectangle[] _L_UpRight_End = Create3RectsDirX(1, 7, increment: 2);
        private static readonly Rectangle[] _L_UpRight_LeftEnd = Create3RectsDirY(12, 3, increment: 2);
        private static readonly Rectangle[] _L_UpRight_DownEnd = Create3RectsDirY(14, 3, increment: 2);


        private static readonly Rectangle[] _L_DownLeft = Create3RectsDirX(0, 6, increment: 2);
        private static readonly Rectangle[] _L_DownLeft_End = Create3RectsDirX(0, 8, increment: 2);
        private static readonly Rectangle[] _L_DownLeft_RightEnd = Create3RectsDirY(11, 4, increment: 2);
        private static readonly Rectangle[] _L_DownLeft_UpEnd = Create3RectsDirY(13, 4, increment: 2);


        private static readonly Rectangle[] _L_DownRight = Create3RectsDirX(1, 6, increment: 2);
        private static readonly Rectangle[] _L_DownRight_End = Create3RectsDirX(1, 8, increment: 2);
        private static readonly Rectangle[] _L_DownRight_LeftEnd = Create3RectsDirY(12, 4, increment: 2);
        private static readonly Rectangle[] _L_DownRight_UpEnd = Create3RectsDirY(14, 4, increment: 2);

        //
        // U shapes (16 variants)
        //

        private static readonly Rectangle[] _U_UpEmpty = Create3RectsDirX(8, 5);
        private static readonly Rectangle[] _U_UpEmpty_End = Create3RectsDirX(8, 8);
        private static readonly Rectangle[] _U_UpEmpty_LeftEnd = Create3RectsDirX(8, 10);
        private static readonly Rectangle[] _U_UpEmpty_RightEnd = Create3RectsDirX(11, 10);

        private static readonly Rectangle[] _U_DownEmpty = Create3RectsDirX(8, 4);
        private static readonly Rectangle[] _U_DownEmpty_End = Create3RectsDirX(8, 7);
        private static readonly Rectangle[] _U_DownEmpty_LeftEnd = Create3RectsDirX(8, 9);
        private static readonly Rectangle[] _U_DownEmpty_RightEnd = Create3RectsDirX(11, 9);

        private static readonly Rectangle[] _U_LeftEmpty = Create3RectsDirY(7, 0);
        private static readonly Rectangle[] _U_LeftEmpty_End = Create3RectsDirY(10, 0);
        private static readonly Rectangle[] _U_LeftEmpty_UpEnd = Create3RectsDirY(12, 0);
        private static readonly Rectangle[] _U_LeftEmpty_DownEnd = Create3RectsDirY(14, 0);

        private static readonly Rectangle[] _U_RightEmpty = Create3RectsDirY(6, 0);
        private static readonly Rectangle[] _U_RightEmpty_End = Create3RectsDirY(9, 0);
        private static readonly Rectangle[] _U_RightEmpty_UpEnd = Create3RectsDirY(11, 0);
        private static readonly Rectangle[] _U_RightEmpty_DownEnd = Create3RectsDirY(13, 0);

        //
        // Special shapes (5 variants)
        //

        private static readonly Rectangle[] _AllClosed = Create3RectsDirX(1, 1);
        private static readonly Rectangle[] _Corner_UpLeft = Create3RectsDirX(1, 4, increment: 2);
        private static readonly Rectangle[] _Corner_UpRight = Create3RectsDirX(0, 4, increment: 2);
        private static readonly Rectangle[] _Corner_DownLeft = Create3RectsDirX(1, 3, increment: 2);
        private static readonly Rectangle[] _Corner_DownRight = Create3RectsDirX(0, 3, increment: 2);

        // TOTAL: 53
        #endregion

        #region Sheet Lookup
        private static readonly Dictionary<BlendSideFlags, Rectangle[]> _BasicShapeLookup = new()
        {
            // Special Shapes: 5
            [Shape_AllSide] = _AllClosed,
            [ShapeCorner_UpLeft] = _Corner_UpLeft,
            [ShapeCorner_UpRight] = _Corner_UpRight,
            [ShapeCorner_DownLeft] = _Corner_DownLeft,
            [ShapeCorner_DownRight] = _Corner_DownRight,

            // I Shape: 16
            [ShapeI_Up] = _I_Up,
            [ShapeI_Up_End] = _I_Up_End,
            [ShapeI_Up_RightEnd] = _I_Up_RightEnd,
            [ShapeI_Up_LeftEnd] = _I_Up_LeftEnd,

            [ShapeI_Down] = _I_Down,
            [ShapeI_Down_End] = _I_Down_End,
            [ShapeI_Down_RightEnd] = _I_Down_RightEnd,
            [ShapeI_Down_LeftEnd] = _I_Down_LeftEnd,

            [ShapeI_Left] = _I_Left,
            [ShapeI_Left_End] = _I_Left_End,
            [ShapeI_Left_DownEnd] = _I_Left_DownEnd,
            [ShapeI_Left_UpEnd] = _I_Left_UpEnd,

            [ShapeI_Right] = _I_Right,
            [ShapeI_Right_End] = _I_Right_End,
            [ShapeI_Right_DownEnd] = _I_Right_DownEnd,
            [ShapeI_Right_UpEnd] = _I_Right_UpEnd,

            // L Shape: 16
            [ShapeL_UpLeft] = _L_UpLeft,
            [ShapeL_UpLeft_End] = _L_UpLeft_End,
            [ShapeL_UpLeft_RightEnd] = _L_UpLeft_RightEnd,
            [ShapeL_UpLeft_DownEnd] = _L_UpLeft_DownEnd,

            [ShapeL_UpRight] = _L_UpRight,
            [ShapeL_UpRight_End] = _L_UpRight_End,
            [ShapeL_UpRight_LeftEnd] = _L_UpRight_LeftEnd,
            [ShapeL_UpRight_DownEnd] = _L_UpRight_DownEnd,

            [ShapeL_DownLeft] = _L_DownLeft,
            [ShapeL_DownLeft_End] = _L_DownLeft_End,
            [ShapeL_DownLeft_RightEnd] = _L_DownLeft_RightEnd,
            [ShapeL_DownLeft_UpEnd] = _L_DownLeft_UpEnd,

            [ShapeL_DownRight] = _L_DownRight,
            [ShapeL_DownRight_End] = _L_DownRight_End,
            [ShapeL_DownRight_LeftEnd] = _L_DownRight_LeftEnd,
            [ShapeL_DownRight_UpEnd] = _L_DownRight_UpEnd,

            // U Shape: 16
            [ShapeU_UpEmpty] = _U_UpEmpty,
            [ShapeU_UpEmpty_End] = _U_UpEmpty_End,
            [ShapeU_UpEmpty_LeftEnd] = _U_UpEmpty_LeftEnd,
            [ShapeU_UpEmpty_RightEnd] = _U_UpEmpty_RightEnd,

            [ShapeU_DownEmpty] = _U_DownEmpty,
            [ShapeU_DownEmpty_End] = _U_DownEmpty_End,
            [ShapeU_DownEmpty_LeftEnd] = _U_DownEmpty_LeftEnd,
            [ShapeU_DownEmpty_RightEnd] = _U_DownEmpty_RightEnd,

            [ShapeU_LeftEmpty] = _U_LeftEmpty,
            [ShapeU_LeftEmpty_End] = _U_LeftEmpty_End,
            [ShapeU_LeftEmpty_UpEnd] = _U_LeftEmpty_UpEnd,
            [ShapeU_LeftEmpty_DownEnd] = _U_LeftEmpty_DownEnd,

            [ShapeU_RightEmpty] = _U_RightEmpty,
            [ShapeU_RightEmpty_End] = _U_RightEmpty_End,
            [ShapeU_RightEmpty_UpEnd] = _U_RightEmpty_UpEnd,
            [ShapeU_RightEmpty_DownEnd] = _U_RightEmpty_DownEnd,
        };

        private static SheetPosition[] _SheetPositionLookup;

        private static void CalculateSheetPositionLookup()
        {
            if (_SheetPositionLookup != null)
                return;

            _SheetPositionLookup = new SheetPosition[256 * VariantCount];

            int bakeSheetIndex = 0;
            for (int i = 0; i < 256; i++)
            {
                bool hasAddedToSheet = false;

                for (byte randomFrame = 0; randomFrame < VariantCount; randomFrame++)
                {
                    var mergeSides = (BlendSideFlags)i;
                    var key = new SheetPositionKey(mergeSides, randomFrame);

                    if (_BasicShapeLookup.TryGetValue(mergeSides, out var rects))
                    {
                        var rect = rects[randomFrame];
                        _SheetPositionLookup[key] = new SheetPosition(rect.X, rect.Y, bakedSheetIndex: -1);
                    }
                    else
                    {
                        var y = Math.DivRem(bakeSheetIndex, BlendTextureXCount, out var x);
                        _SheetPositionLookup[key] = new SheetPosition(x * BlendTextureFrameWidth, y * BlendTextureFrameHeight, bakedSheetIndex: (sbyte)randomFrame);
                        hasAddedToSheet = true;
                    }
                }

                if (hasAddedToSheet)
                    bakeSheetIndex++;
            }
        }

        #endregion

        #region Shape Lookup

        private static readonly IReadOnlyCollection<BlendSideFlags> _Corner_Shapes = [
            ShapeCorner_UpLeft,
            ShapeCorner_UpRight,
            ShapeCorner_DownLeft,
            ShapeCorner_DownRight
        ];

        private static readonly IReadOnlyCollection<BlendSideFlags> _I_Shapes = [
            ShapeI_Up,
            ShapeI_Up_End,
            ShapeI_Up_LeftEnd,
            ShapeI_Up_RightEnd,

            ShapeI_Down,
            ShapeI_Down_End,
            ShapeI_Down_LeftEnd,
            ShapeI_Down_RightEnd,

            ShapeI_Left,
            ShapeI_Left_End,
            ShapeI_Left_UpEnd,
            ShapeI_Left_DownEnd,

            ShapeI_Right,
            ShapeI_Right_End,
            ShapeI_Right_UpEnd,
            ShapeI_Right_DownEnd,
        ];

        private static readonly IReadOnlyCollection<BlendSideFlags> _L_Shapes = [
            ShapeL_UpLeft,
            ShapeL_UpLeft_End,
            ShapeL_UpLeft_RightEnd,
            ShapeL_UpLeft_DownEnd,

            ShapeL_UpRight,
            ShapeL_UpRight_End,
            ShapeL_UpRight_LeftEnd,
            ShapeL_UpRight_DownEnd,

            ShapeL_DownLeft,
            ShapeL_DownLeft_End,
            ShapeL_DownLeft_RightEnd,
            ShapeL_DownLeft_UpEnd,

            ShapeL_DownRight,
            ShapeL_DownRight_End,
            ShapeL_DownRight_LeftEnd,
            ShapeL_DownRight_UpEnd
        ];

        private static readonly IReadOnlyCollection<BlendSideFlags> _U_Shapes = [
            ShapeU_UpEmpty,
            ShapeU_UpEmpty_End,
            ShapeU_UpEmpty_LeftEnd,
            ShapeU_UpEmpty_RightEnd,

            ShapeU_DownEmpty,
            ShapeU_DownEmpty_End,
            ShapeU_DownEmpty_LeftEnd,
            ShapeU_DownEmpty_RightEnd,

            ShapeU_LeftEmpty,
            ShapeU_LeftEmpty_End,
            ShapeU_LeftEmpty_UpEnd,
            ShapeU_LeftEmpty_DownEnd,

            ShapeU_RightEmpty,
            ShapeU_RightEmpty_End,
            ShapeU_RightEmpty_UpEnd,
            ShapeU_RightEmpty_DownEnd
        ];

        /// <summary>
        /// An Descend Ordered Consume Lookup Table
        /// [U -> L -> I -> Corner] Order
        /// [Most flags -> Least flags] Order
        /// </summary>
        private static readonly IReadOnlyCollection<IReadOnlyCollection<BlendSideFlags>> _ShapeConsumeMap = [
            _U_Shapes.OrderByDescending(HotFlagCount).ToImmutableArray(),
            _L_Shapes.OrderByDescending(HotFlagCount).ToImmutableArray(),
            _I_Shapes.OrderByDescending(HotFlagCount).ToImmutableArray(),
            // I shape includes up, down, left, right. So we don't need to consume them manually
            _Corner_Shapes,
        ];
        #endregion

        #region Utils
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rectangle CreateRect(int sheetX, int sheetY)
        {
            return new Rectangle(sheetX * 18, sheetY * 18, 16, 16);
        }

        private static Rectangle[] Create3RectsDirX(int leftSheetX, int leftSheetY, int increment = 1)
        {
            return [
                CreateRect(leftSheetX, leftSheetY),
                CreateRect(leftSheetX + (1 * increment), leftSheetY),
                CreateRect(leftSheetX + (2 * increment), leftSheetY)
            ];
        }

        private static Rectangle[] Create3RectsDirY(int topSheetX, int topSheetY, int increment = 1)
        {
            return [
                CreateRect(topSheetX, topSheetY),
                CreateRect(topSheetX, topSheetY + (1 * increment)),
                CreateRect(topSheetX, topSheetY + (2 * increment))
            ];
        }

        private static int HotFlagCount(BlendSideFlags flags)
        {
            var count = 0;
            for (int i = 0; i < 8; i++)
            {
                var flag = (BlendSideFlags)(1 << i);
                if (flag == (flags & flag))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
    }
}
