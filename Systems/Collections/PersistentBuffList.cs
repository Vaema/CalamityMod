using System.Collections.Generic;
using CalamityMod.Buffs.Potions;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class PersistentBuffList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffType<WeaponImbueBrimstone>(),
                BuffType<WeaponImbueCrumbling>(),
                BuffType<WeaponImbueHolyFlames>(),
                BuffID.WeaponImbueVenom,
                BuffID.WeaponImbueCursedFlames,
                BuffID.WeaponImbueFire,
                BuffID.WeaponImbueGold,
                BuffID.WeaponImbueIchor,
                BuffID.WeaponImbueNanites,
                BuffID.WeaponImbueConfetti,
                BuffID.WeaponImbuePoison
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if this buffType is a PersistentBuff.
        /// </summary>
        public static bool IsPersistentBuff(int buffType) => List.Contains(buffType);
    }
}
