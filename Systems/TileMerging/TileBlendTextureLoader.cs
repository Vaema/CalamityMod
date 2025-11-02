using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class TileBlendTextureLoader : ModSystem
    {
        internal const int EmptySlot = 0;
        internal const int StartingIndex = 1;
        internal const int MaxCount = ushort.MaxValue;

        internal static TileBlendTexture[] Registry { get; private set; }
        internal static IEnumerable<TileBlendTexture> AllTextures => Registry?.Where(tex => tex is not null);
        internal static int Count => _UniqueSlot - StartingIndex;

        private static int _UniqueSlot = StartingIndex;

        public override void Load()
        {
            Registry = new TileBlendTexture[MaxCount];
        }

        public override void Unload()
        {
            Registry = null;
            _UniqueSlot = StartingIndex;
        }

        internal static int Register(TileBlendTexture sheet)
        {
            if (sheet.Slot >= 0)
                throw new ArgumentException("Argument has already registered to System", nameof(sheet));

            if (_UniqueSlot >= MaxCount)
                throw new InvalidOperationException($"Slots are all used up to {MaxCount}, We can't allocate more!");

            var slot = _UniqueSlot++;
            Registry[slot] = sheet;
            return slot;
        }
    }
}
