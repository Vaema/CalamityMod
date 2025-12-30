using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using BlendSidesReg = System.Collections.Generic.Dictionary<int, CalamityMod.Systems.BlendSideFlags>;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        [ThreadStatic]
        private static BlendSidesReg TempBlendSidesReg;

        private static void TileFrame(int i, int j, int type)
        {
            if (!WorldGen.InWorld(i, j))
                return;

            var tile = Main.tile[i, j];
            ref var drawData = ref tile.Get<TileSpecialDrawData>();
            drawData.HasBlendMergeData = false;
            RemoveBlendingRefData(i, j);

            if (!tile.HasTile) // Is this even possible? But I'm doing this for sanity check anyways
                return;

            var blendDataUniqueIndex = 0;
            var blendSidesReg = PopulateBlendSidesReg(i, j, tile.TileType);

            var regCount = blendSidesReg.Count;
            if (regCount > 0)
            {
                CalculateSides(i, j, in blendSidesReg);

                var tileBlendingRefs = new TileBlendingRef[regCount];

                foreach (var pair in blendSidesReg)
                {
                    var blendTextureSlot = pair.Key;
                    var sideFlags = pair.Value;

                    if (sideFlags != BlendSideFlags.None)
                    {
                        tileBlendingRefs[blendDataUniqueIndex] = new TileBlendingRef((ushort)blendTextureSlot, (byte)sideFlags);
                        blendDataUniqueIndex++;
                    }
                }

                SetBlendingRefData(i, j, tileBlendingRefs);
                drawData.HasBlendMergeData = true;
            }
        }

        private static void CalculateSides(int i, int j, in BlendSidesReg blendSidesReg)
        {
            _ = TryGetTile(i, j, out var centerTile, out _);

            bool leftMerged = false;
            bool rightMerged = false;
            bool upMerged = false;
            bool downMerged = false;

            #region Basic 4 Direction Merges

            bool hasLeft = TryGetTile(i - 1, j, out var leftTile, out var leftType);
            if (hasLeft && HasLeftMerge(centerTile, leftTile))
            {
                leftMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[leftType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.Left;
                }
            }

            bool hasRight = TryGetTile(i + 1, j, out var rightTile, out var rightType);
            if (hasRight && HasRightMerge(centerTile, rightTile))
            {
                rightMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[rightType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.Right;
                }
            }

            bool hasUp = TryGetTile(i, j - 1, out var upTile, out var upType);
            if (hasUp && HasUpMerge(centerTile, upTile))
            {
                upMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[upType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.Up;
                }
            }

            bool hasDown = TryGetTile(i, j + 1, out var downTile, out var downType);
            if (hasDown && HasDownMerge(centerTile, downTile))
            {
                downMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[downType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.Down;
                }
            }

            #endregion

            #region Diagonal Direction Merges

            bool upLeftMerged = false;
            bool upRightMerged = false;
            bool downLeftMerged = false;
            bool downRightMerged = false;

            bool hasUpLeft = TryGetTile(i - 1, j - 1, out var upLeftTile, out var upLeftType);
            if (hasUpLeft && upMerged && leftMerged && HasRightMerge(upLeftTile, upTile) && HasDownMerge(upLeftTile, leftTile))
            {
                upLeftMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[upLeftType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.UpLeft;
                }
            }

            bool hasUpRight = TryGetTile(i + 1, j - 1, out var upRightTile, out var upRightType);
            if (hasUpRight && upMerged && rightMerged && HasLeftMerge(upRightTile, upTile) && HasDownMerge(upRightTile, leftTile))
            {
                upRightMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[upRightType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.UpRight;
                }
            }

            bool hasDownLeft = TryGetTile(i - 1, j + 1, out var downLeftTile, out var downLeftType);
            if (hasDownLeft && downMerged && leftMerged && HasRightMerge(downLeftTile, downTile) && HasUpMerge(downLeftTile, leftTile))
            {
                downLeftMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[downLeftType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.DownLeft;
                }
            }

            bool hasDownRight = TryGetTile(i + 1, j + 1, out var downRightTile, out var downRightType);
            if (hasDownRight && downMerged && rightMerged && HasLeftMerge(downRightTile, downTile) && HasUpMerge(downLeftTile, leftTile))
            {
                downRightMerged = true;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[downRightType];
                if (blendSidesReg.ContainsKey(blendTextureSlot))
                {
                    blendSidesReg[blendTextureSlot] |= BlendSideFlags.DownRight;
                }
            }

            #endregion

            #region Special Corner Cases
            foreach (var kv in blendSidesReg)
            {
                var slot = kv.Key;
                var sides = kv.Value;

                // Up
                if (sides.HasFlag(BlendSideFlags.Up))
                {
                    if (leftMerged && upLeftMerged && IsBlendableOrSame(leftType, slot) && IsBlendableOrSame(upLeftType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.UpLeft;
                    }

                    if (rightMerged && upRightMerged && IsBlendableOrSame(rightType, slot) && IsBlendableOrSame(upRightType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.UpRight;
                    }
                }

                // Down
                if (sides.HasFlag(BlendSideFlags.Down))
                {
                    if (leftMerged && downLeftMerged && IsBlendableOrSame(leftType, slot) && IsBlendableOrSame(downLeftType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.DownLeft;
                    }

                    if (rightMerged && downRightMerged && IsBlendableOrSame(rightType, slot) && IsBlendableOrSame(downRightType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.DownRight;
                    }
                }

                // Left
                if (sides.HasFlag(BlendSideFlags.Left))
                {
                    if (upLeftMerged && upMerged && IsBlendableOrSame(upLeftType, slot) && IsBlendableOrSame(upType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.UpLeft;
                    }

                    if (downLeftMerged && downMerged && IsBlendableOrSame(downLeftType, slot) && IsBlendableOrSame(downType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.DownLeft;
                    }
                }

                // Right
                if (sides.HasFlag(BlendSideFlags.Right))
                {
                    if (upRightMerged && upMerged && IsBlendableOrSame(upRightType, slot) && IsBlendableOrSame(upType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.UpRight;
                    }

                    if (downRightMerged && downMerged && IsBlendableOrSame(downRightType, slot) && IsBlendableOrSame(downType, slot))
                    {
                        blendSidesReg[slot] |= BlendSideFlags.DownRight;
                    }
                }
            }
            #endregion
        }

        #region Merge Utilities
        private static bool IsBlendableOrSame(int tileType, int blendTextureSlot)
        {
            if (_TileBlendable[tileType, blendTextureSlot]) return true;
            if (!_TileBlendLooselyFillDiagonal[tileType, blendTextureSlot] && _TileTypeToBlendTextureSlot[tileType] == blendTextureSlot) return true;
            return false;
        }

        private static bool HasLeftMerge(Tile tileOnCenter, Tile tileOnLeft)
        {
            if (!IsMergable(tileOnCenter.TileType, tileOnLeft.TileType)) return false;
            if (!HasLeftSolid(tileOnCenter)) return false;
            if (!HasRightSolid(tileOnLeft)) return false;

            return true;
        }

        private static bool HasRightMerge(Tile tileOnCenter, Tile tileOnRight)
        {
            if (!IsMergable(tileOnCenter.TileType, tileOnRight.TileType)) return false;
            if (!HasRightSolid(tileOnCenter)) return false;
            if (!HasLeftSolid(tileOnRight)) return false;

            return true;
        }

        private static bool HasUpMerge(Tile tileOnCenter, Tile tileOnUp)
        {
            if (!IsMergable(tileOnCenter.TileType, tileOnUp.TileType)) return false;
            if (!HasUpSolid(tileOnCenter)) return false;
            if (!HasDownSolid(tileOnUp)) return false;

            return true;
        }

        private static bool HasDownMerge(Tile tileOnCenter, Tile tileOnDown)
        {
            if (!IsMergable(tileOnCenter.TileType, tileOnDown.TileType)) return false;
            if (!HasDownSolid(tileOnCenter)) return false;
            if (!HasUpSolid(tileOnDown)) return false;

            return true;
        }

        // BlockType.Solid, BlockType.SlopeUpLeft, BlockType.SlopeDownLeft
        // 024
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasLeftSolid(Tile tile) => ((int)tile.BlockType & 1) == 0;

        // BlockType.Solid, BlockType.DownRight, BlockType.SlopeUpRight
        // 035 (there is no way to optimize this)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasRightSolid(Tile tile)
        {
            int b = (int)tile.BlockType;
            return b == 0 || b == 3 || b == 5;
        }

        // BlockType.Solid, BlockType.HalfBlock, BlockType.SlopeUpLeft, BlockType.SlopeUpRight
        // 0145
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasUpSolid(Tile tile)
        {
            int b = (int)tile.BlockType;
            return b < 2 || b > 3;
        }

        // BlockType.Solid, BlockType.SlopeDownLeft, BlockType.SlopeDownRight
        // 023
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasDownSolid(Tile tile)
        {
            int b = (int)tile.BlockType;
            return (b & 4) == 0 && b != 1;
        }

        private static bool IsMergable(int type, int otherType)
        {
            if (type < 0 || otherType < 0)
                return false;

            if (type == otherType)
                return true;

            var canBeMerged = Main.tileMerge[type][otherType];
            if (Main.tileBlendAll[type])
            {
                return canBeMerged && !TileID.Sets.BlockMergesWithMergeAllBlock[otherType];
            }
            else
            {
                return canBeMerged;
            }
        }

        private static BlendSidesReg PopulateBlendSidesReg(int i, int j, int centerType)
        {
            // Prepare Registry
            TempBlendSidesReg ??= new BlendSidesReg(capacity: 8);
            TempBlendSidesReg.Clear();

            var reg = TempBlendSidesReg;

            int left = GetTileType(i - 1, j);
            int right = GetTileType(i + 1, j);

            int up = GetTileType(i, j - 1);
            int upLeft = GetTileType(i - 1, j - 1);
            int upRight = GetTileType(i + 1, j - 1);

            int down = GetTileType(i, j + 1);
            int downLeft = GetTileType(i - 1, j + 1);
            int downRight = GetTileType(i + 1, j + 1);

            void Populate(int sideType)
            {
                if (sideType < 0)
                    return;

                var blendTextureSlot = _TileTypeToBlendTextureSlot[sideType];
                if (blendTextureSlot == TileBlendTextureLoader.EmptySlot)
                    return;

                if (_TileBlendable[centerType, blendTextureSlot])
                {
                    reg[blendTextureSlot] = BlendSideFlags.None;
                }
            }

            Populate(left);
            Populate(right);

            Populate(up);
            Populate(upLeft);
            Populate(upRight);

            Populate(down);
            Populate(downLeft);
            Populate(downRight);

            return reg;
        }

        private static int GetTileType(int i, int j)
        {
            if (!WorldGen.InWorld(i, j))
                return -1;

            var tile = Main.tile[i, j];
            if (!tile.HasTile)
                return -1;

            return tile.TileType;
        }

        private static bool TryGetTile(int i, int j, out Tile tile, out int type)
        {
            if (!WorldGen.InWorld(i, j))
            {
                tile = default;
                type = -1;
                return false;
            }

            tile = Main.tile[i, j];
            if (!tile.HasTile)
            {
                tile = default;
                type = -1;
                return false;
            }

            type = tile.TileType;
            return true;
        }
        #endregion
    }
}
