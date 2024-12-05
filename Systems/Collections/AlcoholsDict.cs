using System.Collections.Generic;
using CalamityMod.Buffs.Alcohol;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IDictionary{T}"/> of all alcohol buff's IDs and their corresponding poison level.
    /// </summary>
    public sealed class AlcoholsDict : ModSystem
    {
        public static IDictionary<int, int> Dict { get; private set; }

        public override void OnModLoad()
        {
            Dict = new Dictionary<int, int>
            {
                { BuffID.Tipsy, 1 },
                { BuffType<BloodyMaryBuff>(), 1 },
                { BuffType<CaribbeanRumBuff>(), 1 },
                { BuffType<CinnamonRollBuff>(), 1 },
                { BuffType<EverclearBuff>(), 2 },
                { BuffType<EvergreenGinBuff>(), 1 },
                { BuffType<FireballBuff>(), 1 },
                { BuffType<GrapeBeerBuff>(), 1 },
                { BuffType<MargaritaBuff>(), 1 },
                { BuffType<MoonshineBuff>(), 1 },
                { BuffType<MoscowMuleBuff>(), 1 },
                { BuffType<OldFashionedBuff>(), 1 },
                { BuffType<RedWineBuff>(), 1 },
                { BuffType<RumBuff>(), 1 },
                { BuffType<ScrewdriverBuff>(), 1 },
                { BuffType<StarBeamRyeBuff>(), 1 },
                { BuffType<TequilaBuff>(), 1 },
                { BuffType<TequilaSunriseBuff>(), 1 },
                { BuffType<VodkaBuff>(), 1 },
                { BuffType<WhiskeyBuff>(), 1 },
                { BuffType<WhiteWineBuff>(), 1 }
            };
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        /// <summary>
        /// A shorthand method to obtain an alcohol buff's poison level from its buff ID.
        /// </summary>
        public static bool TryGet(int buffType, out int poisonLevel)
        {
            return Dict.TryGetValue(buffType, out poisonLevel);
        }
    }
}
