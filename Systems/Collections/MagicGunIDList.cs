using System.Collections.Generic;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Magic;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class MagicGunIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ItemType<AbyssShocker>(),
                ItemType<AcidGun>(),
                ItemType<AethersWhisper>(),
                ItemType<AetherfluxCannon>(),
                ItemType<Omicron>(),
                ItemType<ApoctosisArray>(),
                ItemType<Cryophobia>(),
                ItemType<Effervescence>(),
                ItemType<EidolicWail>(),
                ItemType<GatlingLaser>(),
                ItemType<Vulcan>(),
                ItemType<Genesis>(),
                ItemType<IonBlaster>(),
                ItemType<NanoPurge>(),
                ItemType<PlasmaCaster>(),
                ItemType<PlasmaRifle>(),
                ItemType<PulsePistol>(),
                ItemType<PurgeGuzzler>(),
                ItemType<RainbowPartyCannon>(),
                ItemType<SHPC>(),
                ItemType<TeslaCannon>(),
                ItemType<TheSwarmer>(),
                ItemType<Volterion>(),
                ItemType<Wingman>(),
                ItemID.BeeGun,
                ItemID.BubbleGun,
                ItemID.ChargedBlasterCannon,
                ItemID.HeatRay,
                ItemID.LaserMachinegun,
                ItemID.LaserRifle,
                ItemID.LeafBlower,
                ItemID.RainbowGun,
                ItemID.SpaceGun,
                ItemID.WaspGun,
                ItemID.ZapinatorGray,
                ItemID.ZapinatorOrange
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if this Item is Magic Gun.
        /// </summary>
        public static bool Includes(int itemType) => List.Contains(itemType);
    }
}
