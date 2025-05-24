using System.Collections.Generic;
using CalamityMod.Buffs.Summon.Whips;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of all summon tag buff's IDs.
    /// </summary>
    public sealed class SummonTagBuffList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffID.CoolWhipPlayerBuff,
                BuffID.ScytheWhipPlayerBuff,
                BuffID.SwordWhipPlayerBuff,
                BuffID.ThornWhipPlayerBuff,
                BuffType<ProfanedCrystalWhipBuff>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not a buff ID is a summon tag buff.
        /// </summary>
        public static bool Includes(int buffID) => List.Contains(buffID);
    }
}
