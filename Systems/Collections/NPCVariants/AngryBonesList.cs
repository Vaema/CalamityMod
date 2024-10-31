using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class AngryBonesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.AngryBones,
                NPCID.AngryBonesBig,
                NPCID.AngryBonesBigMuscle,
                NPCID.AngryBonesBigHelmet,
                NPCID.BigBoned,
                NPCID.ShortBones
            ];
        }

        public override void Unload() => List = null;

        public static bool IsAngryBones(NPC npc) => List.Contains(npc.type);
    }
}
