using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> that has all the IDs of all Fire-type debuffs.
    /// </summary>
    public sealed class FireDebuffsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffID.OnFire,
                BuffID.OnFire3, // Hellfire
                BuffID.Burning, // Touching meteorite ore or hellstone without obsidian skull
                BuffID.CursedInferno,
                BuffID.ShadowFlame, // Vanilla Shadowflame, can normally never be applied to players
                BuffType<Shadowflame>(), // Calamity Shadowflame copy for players
                BuffType<SearingLava>(), // Crags lava
                BuffType<BrimstoneFlames>(),
                BuffType<HolyFlames>(),
                BuffType<GodSlayerInferno>(),
                BuffType<Dragonfire>(),
                BuffType<WeakBrimstoneFlames>(), // Aflame enchant self damage
                BuffType<BanishingFire>(),
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if a buff ID is a fire debuff.
        /// </summary>
        public static bool Includes(int buffID) => List.Contains(buffID);
    }
}
