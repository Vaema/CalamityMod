using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics;
using CalamityMod.Tiles.Abyss;
using CalamityMod.Tiles.Astral;
using CalamityMod.Tiles.AstralDesert;
using CalamityMod.Tiles.AstralSnow;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static bool[,] _TileBlendable; // dimension: [TileTypeCount, AllBlendTextureCount]
        private static byte[] _TileTypeToBlendTextureSlot; // dimension: [TileTypeCount]

        #region Load/Unload
        public override void OnModLoad()
        {
            var tileCount = TileLoader.TileCount;
            var blendTextureCount = TileBlendTextureLoader.Count;
            _TileBlendable = new bool[tileCount, blendTextureCount];
            _TileTypeToBlendTextureSlot = new byte[tileCount];

            Array.Fill(_TileTypeToBlendTextureSlot, (byte)TileBlendTextureLoader.EmptySlot);

            for (int i = 0; i<blendTextureCount; i++)
            {
                var blendTexture = TileBlendTextureLoader.Registry[i];
                _TileTypeToBlendTextureSlot[blendTexture.TileType] = (byte)i;
            }

            // Draw Code
            On_TileDrawing.DrawSingleTile += OnDrawSingleTile;
        }

        private static void SetupMergeData()
        {
            foreach (var blendTexture in TileBlendTextureLoader.AllTextures)
            {
                blendTexture.BakeBlendTexture(blendTexture.TextureAsset.Value);
            }
        }

        public override void Unload()
        {
            _TileBlendable = null;
        }
        #endregion

        #region Public API
        public static void RegisterMerge(int myType, int blendTileType)
        {

            if (myType == blendTileType)
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(myType))
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(blendTileType))
                return;

            var blendTextureSlot = _TileTypeToBlendTextureSlot[blendTileType];
            if (blendTextureSlot == TileBlendTextureLoader.EmptySlot)
                return;

            _TileBlendable[myType, blendTextureSlot] = true;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void RegisterMerge(int myType, TileBlendTexture blendTexture)
        {
            if (blendTexture == null)
                return;

            if (blendTexture.Slot < 0)
                return;

            if (!_TileTypeToBlendTextureSlot.IndexInRange(myType))
                return;

            var blendTileType = blendTexture.TileType;
            if (myType == blendTileType) // Self blending should never be case, That will be extremely heavy to render!
                return;

            _TileBlendable[myType, blendTexture.Slot] = true;
            CalamityUtils.SetMerge(myType, blendTileType, true);
        }

        public static void RegisterMerge<T>(int myType) where T : TileBlendTexture
        {
            var blendTexture = ModContent.GetInstance<T>();
            RegisterMerge(myType, blendTexture);
        }
        #endregion
    }
}
