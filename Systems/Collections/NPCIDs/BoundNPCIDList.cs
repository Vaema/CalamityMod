using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class BoundNPCIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.BoundGoblin,
                NPCID.BoundWizard,
                NPCID.BoundMechanic,
                NPCID.SleepingAngler,
                NPCID.BartenderUnconscious,
                NPCID.WebbedStylist,
                NPCID.GolferRescue
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(NPC npc) => List.Contains(npc.type);
        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
