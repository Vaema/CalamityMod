using System.Collections.Generic;
using CalamityMod.Buffs.Alcohol;
using CalamityMod.Buffs.Potions;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> with all buff IDs of those who are boosted by The Amalgam accessory.
    /// </summary>
    public sealed class AmalgamBuffList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffType<AnechoicCoatingBuff>(),
                BuffType<AstralInjectionBuff>(),
                BuffType<BaguetteBuff>(),
                BuffType<BloodfinBoost>(),
                BuffType<BoundingBuff>(),
                BuffType<CalciumBuff>(),
                BuffType<CeaselessHunger>(),
                BuffType<GravityNormalizerBuff>(),
                BuffType<Omniscience>(),
                BuffType<PhotosynthesisBuff>(),
                BuffType<ShadowBuff>(),
                BuffType<Soaring>(),
                BuffType<SulphurskinBuff>(),
                BuffType<WeaponImbueBrimstone>(),
                BuffType<WeaponImbueCrumbling>(),
                BuffType<WeaponImbueHolyFlames>(),
                BuffType<Zen>(),
                BuffType<Zerg>(),
                BuffType<BloodyMaryBuff>(),
                BuffType<CaribbeanRumBuff>(),
                BuffType<CinnamonRollBuff>(),
                BuffType<EverclearBuff>(),
                BuffType<EvergreenGinBuff>(),
                BuffType<CirrusVodkaBuff>(),
                BuffType<FireballBuff>(),
                BuffType<GrapeBeerBuff>(),
                BuffType<MargaritaBuff>(),
                BuffType<MoonshineBuff>(),
                BuffType<MoscowMuleBuff>(),
                BuffType<RedWineBuff>(),
                BuffType<RumBuff>(),
                BuffType<ScrewdriverBuff>(),
                BuffType<StarBeamRyeBuff>(),
                BuffType<TequilaBuff>(),
                BuffType<TequilaSunriseBuff>(),
                BuffType<Trippy>(),
                BuffType<VodkaBuff>(),
                BuffType<WhiskeyBuff>(),
                BuffType<WhiteWineBuff>(),
                BuffID.ObsidianSkin,
                BuffID.Regeneration,
                BuffID.Swiftness,
                BuffID.Gills,
                BuffID.Ironskin,
                BuffID.ManaRegeneration,
                BuffID.MagicPower,
                BuffID.Featherfall,
                BuffID.Spelunker,
                BuffID.Invisibility,
                BuffID.Shine,
                BuffID.NightOwl,
                BuffID.Battle,
                BuffID.Thorns,
                BuffID.WaterWalking,
                BuffID.Archery,
                BuffID.Hunter,
                BuffID.Gravitation,
                BuffID.Tipsy,
                BuffID.WellFed,
                BuffID.WellFed2,
                BuffID.WellFed3,
                BuffID.Honey,
                BuffID.WeaponImbueVenom,
                BuffID.WeaponImbueCursedFlames,
                BuffID.WeaponImbueFire,
                BuffID.WeaponImbueGold,
                BuffID.WeaponImbueIchor,
                BuffID.WeaponImbueNanites,
                BuffID.WeaponImbueConfetti,
                BuffID.WeaponImbuePoison,
                BuffID.Lucky,
                BuffID.Mining,
                BuffID.Heartreach,
                BuffID.Calm,
                BuffID.Builder,
                BuffID.Titan,
                BuffID.Flipper,
                BuffID.Summoning,
                BuffID.Dangersense,
                BuffID.AmmoReservation,
                BuffID.Lifeforce,
                BuffID.Endurance,
                BuffID.Rage,
                BuffID.Inferno,
                BuffID.Wrath,
                BuffID.Lovestruck,
                BuffID.Stinky,
                BuffID.Fishing,
                BuffID.Sonar,
                BuffID.Crate,
                BuffID.Warmth,
                BuffID.SugarRush
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not a buff is boosted by The Amalgam or not.
        /// </summary>
        public static bool Includes(int buffID) => List.Contains(buffID);
    }
}
