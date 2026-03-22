using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Fishing.FishingRods;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Placeables.Furniture.BossRelics;
using CalamityMod.Items.Placeables.Furniture.Monoliths;
using CalamityMod.Items.Placeables.Furniture.Paintings;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Tools;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using ReLogic.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    [ReinitializeDuringResizeArrays]
    public static class CalamityItemSets
    {
        public static SetFactory Factory = new SetFactory(ItemLoader.ItemCount, "CalamityMod/ItemID", Search);
        public static IdDictionary Search = IdDictionary.Create<ItemID, int>();

        /// <summary>
        /// If <see langword="true"/> for an item type, prevents an item from removing Calamity's summon damage penalty mechanic despite having tool power.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] WeaponWithToolPowerAffectedBySummonPenalty = Factory.CreateBoolSet(ItemID.ButchersChainsaw, ItemID.LucyTheAxe, ItemID.Rockfish,
                ItemType<AxeofPurity>(), ItemType<HydraulicVoltCrasher>(), ItemType<InfernaCutter>(), ItemType<PhotonRipper>(), ItemType<Respiteblock>());

        /// <summary>
        /// If <see langword="true"/> for an item type, manually disables Calamity's summon damage penalty mechanic while that item type is held.<br/>
        /// Unused by Calamity itself, and is only used for external mods to add to through mod calls.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ItemWhichDisablesSummonerNerf = Factory.CreateBoolSet();

        /// <summary>
        /// If <see langword="true"/> for an item type, holding the item sets <see cref="Player.accFishingLine"/> to true, preventing fishing lines from breaking.<br/>
        /// Should only be set on fishing poles.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] FishingPoleThatNeverBreaks = Factory.CreateBoolSet(ItemID.GoldenFishingRod, ItemType<EarlyBloomRod>(), ItemType<TheDevourerofCods>());

        /// <summary>
        /// If <see langword="true"/> for an item type, forces a dropped item of this type to remain within the bounds of the world if it is spawned outside of it.<br/>
        /// Set this for items which are dropped by enemies or bosses which spawn on the edges of the world.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] ItemForcedInsideWorld = Factory.CreateBoolSet(ItemID.FishronWings, ItemID.Flairon, ItemID.Tsunami, ItemID.BubbleGun, ItemID.RazorbladeTyphoon,
                ItemID.TempestStaff, ItemID.FishronBossBag, ItemID.GreaterHealingPotion, ItemID.SoulofFlight, ItemType<SubmarineShocker>(), ItemType<Barinautical>(), ItemType<Downpour>(),
                ItemType<DeepseaStaff>(), ItemType<ScourgeoftheSeas>(), ItemType<SeasSearing>(), ItemType<InsidiousImpaler>(), ItemType<SepticSkewer>(), ItemType<FetidEmesis>(),
                ItemType<VitriolicViper>(), ItemType<CadaverousCarrion>(), ItemType<MutatedTruffle>(), ItemType<ToxicantTwister>(), ItemType<TheOldReaper>(), ItemType<Greentide>(),
                ItemType<Leviatitan>(), ItemType<Atlantis>(), ItemType<AnahitasArpeggio>(), ItemType<Whitewater>(), ItemType<LeviathanTeeth>(), ItemType<GastricBelcherStaff>(),
                ItemType<PearlofEnthrallment>(), ItemType<AquaticScourgeBag>(), ItemType<OldDukeBag>(), ItemType<LeviathanBag>(), ItemType<OldDukeMask>(), ItemType<LeviathanMask>(),
                ItemType<AnahitaMask>(), ItemType<AquaticScourgeMask>(), ItemType<OldDukeTrophy>(), ItemType<LeviathanTrophy>(), ItemType<AquaticScourgeTrophy>(), ItemType<LoreAquaticScourge>(),
                ItemType<LoreLeviathanAnahita>(), ItemType<LoreSulphurSea>(), ItemType<LoreAbyss>(), ItemType<LoreOldDuke>(), ItemType<OldDukeRelic>(), ItemType<LeviathanAnahitaRelic>(),
                ItemType<AquaticScourgeRelic>(), ItemType<AeroStone>(), ItemType<CorrosiveSpine>(), ItemType<TheCommunity>(), ItemType<DeepSeaAnchor>(), ItemType<BrinyBaron>(),
                ItemType<DukesDecapitator>(), ItemType<SulphurousSand>(), ItemType<SupremeHealingPotion>(), ItemType<EssenceofSunlight>(), ItemType<ThankYouPainting>());

        /// <summary>
        /// If <see langword="true"/> for an item type, prevents this rogue weapon from triggering Venerated Locket's clone projectile effect when used.<br/>
        /// Primarily used for weapons which shoot short-distance projectiles.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] DisablesVeneratedLocketEffect = Factory.CreateBoolSet(ItemType<SlickCane>(), ItemType<Mycoroot>(), ItemType<CosmicKunai>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a magic gun.<br/>
        /// Used for applying Calamity's reworked Meteor armor set bonus.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] MagicGun = Factory.CreateBoolSet(ItemID.BeeGun, ItemID.BubbleGun, ItemID.ChargedBlasterCannon, ItemID.HeatRay, ItemID.LaserMachinegun, ItemID.LaserRifle,
                ItemID.LeafBlower, ItemID.RainbowGun, ItemID.SpaceGun, ItemID.WaspGun, ItemID.ZapinatorGray, ItemID.ZapinatorOrange, ItemType<AbyssShocker>(), ItemType<AcidGun>(),
                ItemType<AethersWhisper>(), ItemType<AetherfluxCannon>(), ItemType<ApoctosisArray>(), ItemType<Cryophobia>(), ItemType<Effervescence>(), ItemType<EidolicWail>(),
                ItemType<Genesis>(), ItemType<IonBlaster>(), ItemType<NanoPurge>(), ItemType<Omicron>(), ItemType<PlasmaCaster>(), ItemType<PlasmaRifle>(), ItemType<PulsePistol>(),
                ItemType<PurgeGuzzler>(), ItemType<RainbowPartyCannon>(), ItemType<SHPC>(), ItemType<TeslaCannon>(), ItemType<TheSwarmer>(), ItemType<Volterion>(), ItemType<Vulcan>(), ItemType<Wingman>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a rogue bomb.<br/>
        /// Currently unused, and exists as an objective classification for the sake of the Wiki.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] RogueBomb = Factory.CreateBoolSet(ItemType<BallisticPoisonBomb>(), ItemType<BlastBarrel>(), ItemType<ContaminatedBile>(), ItemType<ConsecratedWater>(), ItemType<CraniumSmasher>(),
                ItemType<DesecratedWater>(), ItemType<DuststormInABottle>(), ItemType<Exorcism>(), ItemType<LeonidProgenitor>(), ItemType<MeteorFist>(), ItemType<Penumbra>(), ItemType<Plaguenade>(), 
                ItemType<PlasmaGrenade>(), ItemType<PulseGrenade>(), ItemType<Pumpkaboom>(), ItemType<SeafoamBomb>(), ItemType<SealedSingularity>(), ItemType<SkyfinBombers>(), ItemType<SpentFuelContainer>(), 
                ItemType<StarofDestruction>(), ItemType<Supernova>(), ItemType<TotalityBreakers>(), ItemType<WavePounder>(), ItemType<Whitewater>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a rogue boomerang.<br/>
        /// Currently unused, and exists as an objective classification for the sake of the Wiki.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] RogueBoomerang = Factory.CreateBoolSet(ItemType<AerialTracker>(), ItemType<Brimblade>(), ItemType<Celestus>(), ItemType<DefectiveSphere>(), ItemType<DimensionTearingDisk>(), 
                ItemType<DynamicPursuer>(), ItemType<EnchantedAxe>(), ItemType<EpidemicShredder>(), ItemType<Equanimity>(), ItemType<FishboneBoomerang>(), ItemType<FrostcrushValari>(), ItemType<GhoulishGouger>(),
                ItemType<Icebreaker>(), ItemType<InfestedClawmerang>(), ItemType<KelvinCatalyst>(), ItemType<Kylie>(), ItemType<MangroveChakram>(), ItemType<MoltenAmputator>(), ItemType<NanoblackReaper>(), 
                ItemType<ReboundingRainbow>(), ItemType<SamsaraSlicer>(), ItemType<SubductionSlicer>(), ItemType<ToxicantTwister>(), ItemType<Valediction>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a rogue dagger.<br/>
        /// Currently unused, and exists as an objective classification for the sake of the Wiki.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] RogueDagger = Factory.CreateBoolSet(ItemType<AshenStalactite>(), ItemType<Cinquedea>(), ItemType<Crystalline>(), ItemType<FeatherKnife>(), ItemType<GelDart>(),
                ItemType<GildedDagger>(), ItemType<GleamingDagger>(), ItemType<InfernalKris>(), ItemType<Mycoroot>(), ItemType<ShinobiBlade>(), ItemType<SporeKnife>(), ItemType<WulfrumKnife>(), 
                ItemType<CobaltKunai>(), ItemType<CorpusAvertor>(), ItemType<CursedDagger>(), ItemType<LeviathanTeeth>(), ItemType<Malachite>(), ItemType<MythrilKnife>(), ItemType<OrichalcumSpikedGemstone>(),
                ItemType<Prismalline>(), ItemType<RadiantStar>(), ItemType<StellarKnife>(), ItemType<StormfrontRazor>(), ItemType<TerrorTalons>(), ItemType<CosmicKunai>(), ItemType<JawsOfOblivion>(),
                ItemType<LunarKunai>(), ItemType<Sacrifice>(), ItemType<Seraphim>(), ItemType<ShatteredDawn>(), ItemType<TarragonThrowingDart>(), ItemType<TimeBolt>(), ItemType<TwistingThunder>(), 
                ItemType<UtensilPoker>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a rogue javelin.<br/>
        /// Currently unused, and exists as an objective classification for the sake of the Wiki.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] RogueJavelin = Factory.CreateBoolSet(ItemType<AntlionSkewer>(), ItemType<CrystalPiercer>(), ItemType<EclipsesFall>(), ItemType<IchorSpear>(), ItemType<Vega>(),
                ItemType<PalladiumJavelin>(), ItemType<PhantasmalRuin>(), ItemType<ProfanedPartisan>(), ItemType<RealityRupture>(), ItemType<ScarletDevil>(), ItemType<ScourgeoftheDesert>(), 
                ItemType<ScourgeoftheSeas>(), ItemType<ShardofAntumbra>(), ItemType<SpearofDestiny>(), ItemType<SpearofPaleolith>(), ItemType<Turbulance>(), ItemType<TheAtomSplitter>(), ItemType<WaveSkipper>(), 
                ItemType<Wrathwing>());

        /// <summary>
        /// If <see langword="true"/> for an item type, this item is considered to be a rogue spiky ball.<br/>
        /// Currently unused, and exists as an objective classification for the sake of the Wiki.<br/>
        /// Defaults to <see langword="false"/>.
        /// </summary>
        public static bool[] RogueSpikyBall = Factory.CreateBoolSet(ItemType<BurningStrife>(), ItemType<GodsParanoia>(), ItemType<MetalMonstrosity>(), ItemType<NastyCholla>(), ItemType<SystemBane>(),
                ItemType<WebBall>());

        public static bool[] ShowScalingCritDamageTooltip = Factory
            .CreateNamedSet("ShowScalingCritDamageTooltip")
            .Description("Replaces Crit Chance tooltip line with Crit Damage, getting 2% crit dmg per 1% crit chance")
            .RegisterBoolSet();
    }
}
