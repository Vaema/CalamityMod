using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of all NPC Types that receive Post-DoG Frost Moon Buff
    /// </summary>
    public sealed class FrostMoonBuffList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCID.ZombieElf,
                NPCID.ZombieElfBeard,
                NPCID.ZombieElfGirl,
                NPCID.PresentMimic,
                NPCID.GingerbreadMan,
                NPCID.Yeti,
                NPCID.Everscream,
                NPCID.IceQueen,
                NPCID.SantaNK1,
                NPCID.ElfCopter,
                NPCID.Nutcracker,
                NPCID.NutcrackerSpinning,
                NPCID.ElfArcher,
                NPCID.Krampus,
                NPCID.Flocko
            ];
        }

        public override void Unload() => List = null;

        public static bool IsFrostMoonBuffNPC(NPC npc) => List.Contains(npc.type);
    }
}
