using System.Collections.Generic;
using CalamityMod.Buffs.Alcohol;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of all alcohol buff's IDs.
    /// </summary>
    public sealed class AlcoholsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffID.Tipsy,
                BuffType<BloodyMaryBuff>(),
                BuffType<CaribbeanRumBuff>(),
                BuffType<CinnamonRollBuff>(),
                BuffType<EverclearBuff>(),
                BuffType<EvergreenGinBuff>(),
                BuffType<FireballBuff>(),
                BuffType<GrapeBeerBuff>(),
                BuffType<MargaritaBuff>(),
                BuffType<MoonshineBuff>(),
                BuffType<MoscowMuleBuff>(),
                BuffType<OldFashionedBuff>(),
                BuffType<RedWineBuff>(),
                BuffType<RumBuff>(),
                BuffType<ScrewdriverBuff>(),
                BuffType<StarBeamRyeBuff>(),
                BuffType<TequilaBuff>(),
                BuffType<TequilaSunriseBuff>(),
                BuffType<VodkaBuff>(),
                BuffType<WhiskeyBuff>(),
                BuffType<WhiteWineBuff>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not a buff ID is an alcohol.
        /// </summary>
        public static bool Includes(int buffID) => List.Contains(buffID);
    }
}
