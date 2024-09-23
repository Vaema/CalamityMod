using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;

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

    public enum MergeSideFlags : byte
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
        Shape_AllSide         = AllSide,
        ShapeCorner_UpLeft    = UpLeft,
        ShapeCorner_UpRight   = UpRight,
        ShapeCorner_DownLeft  = DownLeft,
        ShapeCorner_DownRight = DownRight,

        // I Shapes

        ShapeI_Up          = Up | UpLeft | UpRight,
        ShapeI_Up_End      = Up,
        ShapeI_Up_LeftEnd  = Up | UpRight,
        ShapeI_Up_RightEnd = Up | UpLeft,

        ShapeI_Down          = Down | DownLeft | DownRight,
        ShapeI_Down_End      = Down,
        ShapeI_Down_LeftEnd  = Down | DownRight,
        ShapeI_Down_RightEnd = Down | DownLeft,

        ShapeI_Left         = Left | UpLeft | DownLeft,
        ShapeI_Left_End     = Left,
        ShapeI_Left_UpEnd   = Left | DownLeft,
        ShapeI_Left_DownEnd = Left | UpLeft,

        ShapeI_Right         = Right | UpRight | DownRight,
        ShapeI_Right_End     = Right,
        ShapeI_Right_UpEnd   = Right | DownRight,
        ShapeI_Right_DownEnd = Right | UpRight,

        // L Shapes

        ShapeL_UpLeft          = ShapeI_Up | ShapeI_Left,
        ShapeL_UpLeft_End      = ShapeI_Up_End | ShapeI_Left_End | UpLeft,
        ShapeL_UpLeft_RightEnd = ShapeI_Up_RightEnd | ShapeI_Left,
        ShapeL_UpLeft_DownEnd  = ShapeI_Up | ShapeI_Left_DownEnd,

        ShapeL_UpRight         = ShapeI_Up | ShapeI_Right,
        ShapeL_UpRight_End     = ShapeI_Up_End | ShapeI_Right_End | UpRight,
        ShapeL_UpRight_LeftEnd = ShapeI_Up_End | ShapeI_Right,
        ShapeL_UpRight_DownEnd = ShapeI_Up | ShapeI_Right_End,

        ShapeL_DownLeft          = ShapeI_Down | ShapeI_Left,
        ShapeL_DownLeft_End      = ShapeI_Down_End | ShapeI_Left_End | DownLeft,
        ShapeL_DownLeft_RightEnd = ShapeI_Down_End | ShapeI_Left,
        ShapeL_DownLeft_UpEnd    = ShapeI_Down | ShapeI_Left_End,

        ShapeL_DownRight         = ShapeI_Down | ShapeI_Right,
        ShapeL_DownRight_End     = ShapeI_Down_End | ShapeI_Right_End | DownRight,
        ShapeL_DownRight_LeftEnd = ShapeI_Down_End | ShapeI_Right,
        ShapeL_DownRight_UpEnd   = ShapeI_Down | ShapeI_Right_End,

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
        ShapeU_RightEmpty_End = ShapeI_Up | ShapeI_Down | ShapeI_Left,
        ShapeU_RightEmpty_UpEnd = ShapeI_Up_End | ShapeI_Down | ShapeI_Left,
        ShapeU_RightEmpty_DownEnd = ShapeI_Up | ShapeI_Down_End | ShapeI_Left,
    }

    public enum MergeTextureID : byte
    {
        Everything = 0,

        AbyssGravel,
        Ash,
        AstralDirt,
        AstralSand,
        AstralSandstone,
        AstralSnow,
        BrimstoneSlag,
        Cloud,
        Dirt,
        EutrophicSand,
        HardenedSand,
        HardenedSulphurousSandstone,
        Luminite,
        Mud,
        Navystone,
        PyreMantle,
        RainCloud,
        Sand,
        Sandstone,
        SnowCloud,
        Snow,
        Stone,
        SulphurousSand,
        SulphurousSandstone,
        SulphurousShale,
        Voidstone
    }

    public struct TileBlendingData : ITileData
    {
        private byte SheetIndex0;
        private byte SheetIndex1;
        private byte SheetIndex2;
        private byte SheetIndex3;
        private byte SheetIndex4;
        private byte SheetIndex5;
        private byte SheetIndex6;
        private byte SheetIndex7;

        private byte Data0;
        private byte Data1;
        private byte Data2;
        private byte Data3;
        private byte Data4;
        private byte Data5;
        private byte Data6;
        private byte Data7;

        public void Clear()
        {
            for (int i = 0; i<8; i++)
            {
                SetData(i, 0);
                SetSheetIndex(i, TileBlendMergeSystem.EmptySheetIndex);
            }
        }

        public void SetData(int idx, byte data)
        {
            if (idx == 0) Data0 = data;
            else if (idx == 1) Data1 = data;
            else if (idx == 2) Data2 = data;
            else if (idx == 3) Data3 = data;
            else if (idx == 4) Data4 = data;
            else if (idx == 5) Data5 = data;
            else if (idx == 6) Data6 = data;
            else if (idx == 7) Data7 = data;
            else throw new IndexOutOfRangeException();
        }

        public readonly byte GetData(int idx)
        {
            return idx switch
            {
                0 => Data0,
                1 => Data1,
                2 => Data2,
                3 => Data3,
                4 => Data4,
                5 => Data5,
                6 => Data6,
                7 => Data7,
                _ => throw new IndexOutOfRangeException()
            };
        }

        public void SetSheetIndex(int idx, byte sheetIdx)
        {
            if (idx == 0) SheetIndex0 = sheetIdx;
            else if (idx == 1) SheetIndex1 = sheetIdx;
            else if (idx == 2) SheetIndex2 = sheetIdx;
            else if (idx == 3) SheetIndex3 = sheetIdx;
            else if (idx == 4) SheetIndex4 = sheetIdx;
            else if (idx == 5) SheetIndex5 = sheetIdx;
            else if (idx == 6) SheetIndex6 = sheetIdx;
            else if (idx == 7) SheetIndex7 = sheetIdx;
            else throw new IndexOutOfRangeException();
        }

        public readonly byte GetSheetIndex(int idx)
        {
            return idx switch
            {
                0 => SheetIndex0,
                1 => SheetIndex1,
                2 => SheetIndex2,
                3 => SheetIndex3,
                4 => SheetIndex4,
                5 => SheetIndex5,
                6 => SheetIndex6,
                7 => SheetIndex7,
                _ => throw new IndexOutOfRangeException()
            };
        }
    }
}
