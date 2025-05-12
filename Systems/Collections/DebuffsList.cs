using System.Collections.Generic;
using CalamityMod.Buffs;
using CalamityMod.Buffs.Cooldowns;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> that has all Debuff IDs.
    /// </summary>
    public sealed class DebuffsList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                BuffID.Poisoned,
                BuffID.Darkness,
                BuffID.Cursed,
                BuffID.OnFire,
                BuffID.Bleeding,
                BuffID.Confused,
                BuffID.Slow,
                BuffID.Weak,
                BuffID.Silenced,
                BuffID.BrokenArmor,
                BuffID.CursedInferno,
                BuffID.Frostburn,
                BuffID.Chilled,
                BuffID.Frozen,
                BuffID.Burning,
                BuffID.Suffocation,
                BuffID.Ichor,
                BuffID.Venom,
                BuffID.Blackout,
                BuffID.Electrified,
                BuffID.Rabies,
                BuffID.Webbed,
                BuffID.Stoned,
                BuffID.Dazed,
                BuffID.VortexDebuff,
                BuffID.WitheredArmor,
                BuffID.WitheredWeapon,
                BuffID.OgreSpit,
                BuffID.BetsysCurse,
                BuffType<SulphuricPoisoning>(),
                BuffType<Shadowflame>(),
                BuffType<BrimstoneFlames>(),
                BuffType<BurningBlood>(),
                BuffType<BrainRot>(),
                BuffType<ElementalMix>(),
                BuffType<GlacialState>(),
                BuffType<GodSlayerInferno>(),
                BuffType<AstralInfectionDebuff>(),
                BuffType<HolyFlames>(),
                BuffType<Irradiated>(),
                BuffType<Plague>(),
                BuffType<CrushDepth>(),
                BuffType<RiptideDebuff>(),
                BuffType<MarkedforDeath>(),
                BuffType<AbsorberAffliction>(),
                BuffType<ArmorCrunch>(),
                BuffType<Crumbling>(),
                BuffType<Vaporfied>(),
                BuffType<Eutrophication>(),
                BuffType<Dragonfire>(),
                BuffType<VermillionFlux>(),
                BuffType<AuricRebuke>(),
                BuffType<StaticDischarge>(),
                BuffType<Nightwither>(),
                BuffType<VulnerabilityHex>(),
                BuffType<MiracleBlight>(),
                BuffType<WhisperingDeath>(),
                BuffType<FrozenLungs>(),
                BuffType<FishAlert>(),
                BuffType<HolyInferno>(),
                BuffType<IcarusFolly>(),
                BuffType<DoGExtremeGravity>(),
                // BuffType<NOU>(),
                BuffType<PopoNoselessBuff>(),
                BuffType<SearingLava>(),
                BuffType<WeakBrimstoneFlames>(),
                BuffType<Withered>(),
                BuffType<ManaBurn>(),
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if a buff ID is a debuff.
        /// </summary>
        public static bool Includes(int buffID) => List.Contains(buffID);
    }
}
