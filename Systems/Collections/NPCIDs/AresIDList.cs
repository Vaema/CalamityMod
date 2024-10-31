using System.Collections.Generic;
using CalamityMod.NPCs.ExoMechs.Ares;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class AresIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<AresBody>(),
                NPCType<AresGaussNuke>(),
                NPCType<AresLaserCannon>(),
                NPCType<AresPlasmaFlamethrower>(),
                NPCType<AresTeslaCannon>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(NPC npc) => List.Contains(npc.type);
        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
