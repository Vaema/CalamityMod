using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class DemonEyeList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.DemonEye,
                NPCID.CataractEye,
                NPCID.SleepyEye,
                NPCID.DialatedEye, // it is spelled "dilated"
                NPCID.GreenEye,
                NPCID.PurpleEye,
                NPCID.DemonEyeOwl,
                NPCID.DemonEyeSpaceship,
                NPCID.DemonEye2,
                NPCID.PurpleEye2,
                NPCID.GreenEye2,
                NPCID.DialatedEye2,
                NPCID.SleepyEye2,
                NPCID.CataractEye2
            ];
        }

        public override void Unload() => List = null;

        public static bool IsDemonEye(NPC npc) => List.Contains(npc.type);
    }
}
