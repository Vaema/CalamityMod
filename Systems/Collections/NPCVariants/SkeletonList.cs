using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    public sealed class SkeletonList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.Skeleton,
                NPCID.HeadacheSkeleton,
                NPCID.MisassembledSkeleton,
                NPCID.PantlessSkeleton,
                NPCID.BoneThrowingSkeleton,
                NPCID.BoneThrowingSkeleton2,
                NPCID.BoneThrowingSkeleton3,
                NPCID.BoneThrowingSkeleton4,
                NPCID.BigPantlessSkeleton,
                NPCID.SmallPantlessSkeleton,
                NPCID.BigMisassembledSkeleton,
                NPCID.SmallMisassembledSkeleton,
                NPCID.BigHeadacheSkeleton,
                NPCID.SmallHeadacheSkeleton,
                NPCID.BigSkeleton,
                NPCID.SmallSkeleton,

                //Note: These skeletons don't count for Skeleton Banner for some god forsaken reason
                NPCID.SkeletonTopHat,
                NPCID.SkeletonAstonaut,
                NPCID.SkeletonAlien,

                //Other skeleton types
                NPCID.ArmoredSkeleton,
                NPCID.HeavySkeleton,
                NPCID.SkeletonArcher,
                NPCID.GreekSkeleton,
                NPCID.SporeSkeleton
            ];
        }

        public override void Unload() => List = null;

        public static bool IsSkeleton(NPC npc) => List.Contains(npc.type);
    }
}
