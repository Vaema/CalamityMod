using System.Collections.Generic;
using CalamityMod.Items.Weapons.Rogue;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// To ban projectiles from locket, mainly spikeballs altho Toasty asked me to add mod calls for adding stuff like Dreamtastic
    /// </summary>
    public sealed class VeneratedLocketBanList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ItemType<SkyStabber>(),
                ItemType<Nychthemeron>(),
                ItemType<GodsParanoia>(),
                ItemType<SlickCane>(),
                ItemType<Mycoroot>(),
                ItemType<CosmicKunai>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int itemType) => List.Contains(itemType);
    }
}
