using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> which contains a <see cref="IList{T}"/> of all NPC's IDs of NPCs who are buffed dungeon enemies.
    /// </summary>
    public sealed class BuffedDungeonEnemiesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.SkeletonSniper,
                NPCID.TacticalSkeleton,
                NPCID.SkeletonCommando,
                NPCID.Paladin,
                NPCID.GiantCursedSkull,
                NPCID.BoneLee,
                NPCID.DiabolistWhite,
                NPCID.DiabolistRed,
                NPCID.NecromancerArmored,
                NPCID.Necromancer,
                NPCID.RaggedCasterOpenCoat,
                NPCID.RaggedCaster,
                NPCID.HellArmoredBonesSword,
                NPCID.HellArmoredBonesMace,
                NPCID.HellArmoredBonesSpikeShield,
                NPCID.HellArmoredBones,
                NPCID.BlueArmoredBonesSword,
                NPCID.BlueArmoredBonesNoPants,
                NPCID.BlueArmoredBonesMace,
                NPCID.BlueArmoredBones,
                NPCID.RustyArmoredBonesSwordNoArmor,
                NPCID.RustyArmoredBonesSword,
                NPCID.RustyArmoredBonesFlail,
                NPCID.RustyArmoredBonesAxe
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if an NPC is a buffed Dungeon enemy.
        /// </summary>
        public static bool IsNPCBuffedDungeonEnemy(NPC npc) => List.Contains(npc.type);
    }
}
