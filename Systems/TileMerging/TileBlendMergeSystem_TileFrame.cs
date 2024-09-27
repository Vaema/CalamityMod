using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Potions.Alcohol;
using Extensions;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static void TileFrame(int i, int j, int type)
        {
            if (!WorldGen.InWorld(i, j))
                return;

            var tile = Main.tile[i, j];
            if (!tile.HasTile) // Is this even possible? But I'm doing this for sanity check anyways
                return;

            tile.Get<TileBlendingData>().Clear();

            int tileType =  tile.TileType;
            int left =      GetTileType(i - 1, j);
            int right =     GetTileType(i + 1, j);

            int up =        GetTileType(i, j - 1);
            int upLeft =    GetTileType(i - 1, j - 1);
            int upRight =   GetTileType(i + 1, j - 1);

            int down =      GetTileType(i, j + 1);
            int downLeft =  GetTileType(i - 1, j + 1);
            int downRight = GetTileType(i + 1, j + 1);

            var blendTexturesSurrounding = PopulateBlendTileTypes(left, right, up, upLeft, upRight, down, downLeft, downRight);
            var blendDataUniqueIndex = 0;
            foreach (var blendTextureID in blendTexturesSurrounding)
            {
                if (!_TileBlendable[tileType, (int)blendTextureID])
                    continue;

                BlendSideFlags sideFlags = BlendSideFlags.None;
                CalculateSides(blendTextureID, i, j, ref sideFlags);

                if (sideFlags != BlendSideFlags.None)
                {
                    tile.Get<TileBlendingData>().Set(blendDataUniqueIndex, (byte)blendTextureID, (byte)sideFlags);
                    blendDataUniqueIndex++;
                }
            }
        }

        private static void CalculateSides(int blendingWith, int i, int j, ref BlendSideFlags flags)
        {
            var hasCenter = TryGetTile(i, j, out var centerTile, out var center);

            bool leftMerged = false;
            bool rightMerged = false;
            bool upMerged = false;
            bool downMerged = false;

            #region Basic 4 Direction Merges

            bool hasLeft = TryGetTile(i - 1, j, out var leftTile, out var leftType);
            if (hasLeft && HasLeftMerge(centerTile, leftTile))
            {
                leftMerged = true;
                if (_TileTypeBlendTexture[leftType] == blendingWith)
                {
                    flags |= BlendSideFlags.Left;
                }
            }

            bool hasRight = TryGetTile(i + 1, j, out var rightTile, out var rightType);
            if (hasRight && HasRightMerge(centerTile, rightTile))
            {
                rightMerged = true;
                if (_TileTypeBlendTexture[rightType] == blendingWith)
                {
                    flags |= BlendSideFlags.Right;
                }
            }

            bool hasUp = TryGetTile(i, j - 1, out var upTile, out var upType);
            if (hasUp && HasUpMerge(centerTile, upTile))
            {
                upMerged = true;
                if (_TileTypeBlendTexture[upType] == blendingWith)
                {
                    flags |= BlendSideFlags.Up;
                }
            }

            bool hasDown = TryGetTile(i, j + 1, out var downTile, out var downType);
            if (hasDown && HasDownMerge(centerTile, downTile))
            {
                downMerged = true;
                if (_TileTypeBlendTexture[downType] == blendingWith)
                {
                    flags |= BlendSideFlags.Down;
                }
            }

            #endregion

            #region Diagonal Direction Merges

            bool hasUpLeft = TryGetTile(i - 1, j - 1, out var upLeftTile, out var upLeftType);
            if (hasUpLeft && upMerged && leftMerged && HasRightMerge(upLeftTile, upTile) && HasDownMerge(upLeftTile, leftTile))
            {
                if (_TileTypeBlendTexture[upLeftType] == blendingWith)
                {
                    flags |= BlendSideFlags.UpLeft;
                }
            }

            bool hasUpRight = TryGetTile(i + 1, j - 1, out var upRightTile, out var upRightType);
            if (hasUpRight && upMerged && rightMerged && HasLeftMerge(upRightTile, upTile) && HasDownMerge(upRightTile, leftTile))
            {
                if (_TileTypeBlendTexture[upRightType] == blendingWith)
                {
                    flags |= BlendSideFlags.UpRight;
                }
            }

            bool hasDownLeft = TryGetTile(i - 1, j + 1, out var downLeftTile, out var downLeftType);
            if (hasDownLeft && downMerged && leftMerged && HasRightMerge(downLeftTile, downTile) && HasUpMerge(downLeftTile, leftTile))
            {
                if (_TileTypeBlendTexture[downLeftType] == blendingWith)
                {
                    flags |= BlendSideFlags.DownLeft;
                }
            }

            bool hasDownRight = TryGetTile(i + 1, j + 1, out var downRightTile, out var downRightType);
            if (hasDownRight && downMerged && rightMerged && HasLeftMerge(downRightTile, downTile) && HasUpMerge(downLeftTile, leftTile))
            {
                if (_TileTypeBlendTexture[downRightType] == blendingWith)
                {
                    flags |= BlendSideFlags.DownRight;
                }
            }

            #endregion
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

        private static bool HasLeftSolid(Tile tile)
        {
            return tile.BlockType switch
            {
                BlockType.Solid => true,
                BlockType.SlopeUpLeft => true,
                BlockType.SlopeDownLeft => true,
                _ => false
            };
        }

        private static bool HasRightSolid(Tile tile)
        {
            return tile.BlockType switch
            {
                BlockType.Solid => true,
                BlockType.SlopeUpRight => true,
                BlockType.SlopeDownRight => true,
                _ => false
            };
        }

        private static bool HasUpSolid(Tile tile)
        {
            return tile.BlockType switch
            {
                BlockType.Solid => true,
                BlockType.HalfBlock => true,
                BlockType.SlopeUpLeft => true,
                BlockType.SlopeUpRight => true,
                _ => false
            };
        }

        private static bool HasDownSolid(Tile tile)
        {
            return tile.BlockType switch
            {
                BlockType.Solid => true,
                BlockType.SlopeDownLeft => true,
                BlockType.SlopeDownRight => true,
                _ => false
            };
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

        private static HashSet<int> PopulateBlendTileTypes(params int[] types)
        {
            var hashSet = new HashSet<int>();
            foreach (var type in types)
            {
                if (type < 0)
                    continue;

                var blendTextureID = _TileTypeBlendTexture[type];
                if (blendTextureID != TileBlendTextureLoader.EmptySlot)
                    hashSet.Add(blendTextureID);
            }
            return hashSet;
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
    }
}
