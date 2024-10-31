using System.Collections.Generic;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.TreasureBags;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of Item IDs of those which are forced to be inside the world map.
    /// </summary>
    public sealed class ItemsForcedInsideWorldList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ItemType<SubmarineShocker>(),
                ItemType<Barinautical>(),
                ItemType<Downpour>(),
                ItemType<DeepseaStaff>(),
                ItemType<ScourgeoftheSeas>(),
                ItemType<InsidiousImpaler>(),
                ItemType<SepticSkewer>(),
                ItemType<FetidEmesis>(),
                ItemType<VitriolicViper>(),
                ItemType<CadaverousCarrion>(),
                ItemType<ToxicantTwister>(),
                ItemType<OldDukeScales>(),
                ItemType<Greentide>(),
                ItemType<Leviatitan>(),
                ItemType<Atlantis>(),
                ItemType<AnahitasArpeggio>(),
                ItemType<Whitewater>(),
                ItemType<LeviathanTeeth>(),
                ItemType<GastricBelcherStaff>(),
                ItemType<PearlofEnthrallment>(),
                ItemType<AquaticScourgeBag>(),
                ItemType<OldDukeBag>(),
                ItemType<LeviathanBag>(),
                ItemType<OldDukeMask>(),
                ItemType<LeviathanMask>(),
                ItemType<AquaticScourgeMask>(),
                ItemType<OldDukeTrophy>(),
                ItemType<LeviathanTrophy>(),
                ItemType<AquaticScourgeTrophy>(),
                ItemType<LoreAquaticScourge>(),
                ItemType<LoreLeviathanAnahita>(),
                ItemType<LoreSulphurSea>(),
                ItemType<LoreAbyss>(),
                ItemType<LoreOldDuke>(),
                ItemType<PearlShard>(),
                ItemType<AeroStone>(),
                ItemType<TheCommunity>(),
                ItemType<DukesDecapitator>(),
                ItemType<SulphurousSand>(),
                ItemID.HotlineFishingHook,
                ItemID.BottomlessBucket,
                ItemID.SuperAbsorbantSponge,
                ItemID.FishingPotion,
                ItemID.SonarPotion,
                ItemID.CratePotion,
                ItemID.AnglerTackleBag,
                ItemID.HighTestFishingLine,
                ItemID.TackleBox,
                ItemID.AnglerEarring,
                ItemID.FishermansGuide,
                ItemID.WeatherRadio,
                ItemID.Sextant,
                ItemID.AnglerHat,
                ItemID.AnglerVest,
                ItemID.AnglerPants,
                ItemID.GoldenBugNet,
                ItemID.FishronWings,
                ItemID.Flairon,
                ItemID.Tsunami,
                ItemID.BubbleGun,
                ItemID.RazorbladeTyphoon,
                ItemID.TempestStaff,
                ItemID.FishronBossBag,
                ItemID.Coral,
                ItemID.Seashell,
                ItemID.Starfish,
                ItemID.SoulofSight,
                ItemID.GreaterHealingPotion,
                ItemID.SuperHealingPotion
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check whether or not an item is an item that should be forced inside the world map.
        /// </summary>
        public static bool Includes(int itemType) => List.Contains(itemType);
    }
}
