using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> that has all the Sickness Debuffs' IDs.
    /// </summary>
    public sealed class SicknessDebuffsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffID.Poisoned,
                BuffID.Venom,
                BuffType<SulphuricPoisoning>(),
                BuffType<AstralInfectionDebuff>(),
                BuffType<Plague>(),
                BuffType<AbsorberAffliction>(),
                BuffType<WhisperingDeath>(),
                BuffType<Irradiated>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if a buff ID is a Sickness Debuff.
        /// </summary>
        public static bool IsSickenessDebuff(int buffID) => List.Contains(buffID);
    }
}
