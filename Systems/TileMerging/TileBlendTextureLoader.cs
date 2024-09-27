using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class TileBlendTextureLoader : ModSystem
    {
        internal const int EmptySlot = 255;

        internal static TileBlendTexture[] Registry { get; private set; }
        internal static IEnumerable<TileBlendTexture> AllTextures => Registry.Take(Count);
        internal static int Count => _UniqueSlot;

        private static int _UniqueSlot = 0;

        public override void Load()
        {
            Registry = new TileBlendTexture[256];
        }

        public override void Unload()
        {
            Registry = null;
            _UniqueSlot = 0;
        }

        internal static int Register(TileBlendTexture sheet)
        {
            var slot = _UniqueSlot++;
            Registry[slot] = sheet;
            return slot;
        }
    }
}
