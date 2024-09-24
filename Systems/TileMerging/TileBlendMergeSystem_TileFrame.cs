using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Potions.Alcohol;
using Terraria;
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

                CalculateSide(tileType, left, blendTextureID, BlendSideFlags.Left, ref sideFlags, out var leftMerged);
                CalculateSide(tileType, right, blendTextureID, BlendSideFlags.Right, ref sideFlags, out var rightMerged);
                CalculateSide(tileType, up, blendTextureID, BlendSideFlags.Up, ref sideFlags, out var upMerged);
                CalculateSide(tileType, down, blendTextureID, BlendSideFlags.Down, ref sideFlags, out var downMerged);

                if (upMerged && leftMerged) CalculateSide(tileType, upLeft, blendTextureID, BlendSideFlags.UpLeft, ref sideFlags, out _);
                if (upMerged && rightMerged) CalculateSide(tileType, upRight, blendTextureID, BlendSideFlags.UpRight, ref sideFlags, out _);
                if (downMerged && leftMerged) CalculateSide(tileType, downLeft, blendTextureID, BlendSideFlags.DownLeft, ref sideFlags, out _);
                if (downMerged && rightMerged) CalculateSide(tileType, downRight, blendTextureID, BlendSideFlags.DownRight, ref sideFlags, out _);

                if (sideFlags != BlendSideFlags.None)
                {
                    tile.Get<TileBlendingData>().Set(blendDataUniqueIndex, (byte)blendTextureID, (byte)sideFlags);
                    blendDataUniqueIndex++;
                }
            }
        }

        private static void CalculateSide(int type, int sideType, BlendTextureID blendingWith, BlendSideFlags flagToAdd, ref BlendSideFlags flags, out bool sideMerged)
        {
            sideMerged = false;

            if (sideType < 0)
                return;

            var canBeMerged = Main.tileMerge[type][sideType];
            if (Main.tileBlendAll[type])
            {
                sideMerged = canBeMerged && !TileID.Sets.BlockMergesWithMergeAllBlock[sideType];
            }
            else
            {
                sideMerged = canBeMerged;
            }

            var blendTextureID = _TileTypeBlendTexture[sideType];
            if (sideMerged && (blendTextureID == blendingWith))
            {
                flags |= flagToAdd;
            }
        }

        private static HashSet<BlendTextureID> PopulateBlendTileTypes(params int[] types)
        {
            var hashSet = new HashSet<BlendTextureID>();
            foreach (var type in types)
            {
                if (type < 0)
                    continue;

                var blendTextureID = _TileTypeBlendTexture[type];
                if (blendTextureID != BlendTextureID.None)
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
    }
}
