using System.Collections.Generic;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.StormWeaver;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class NoRageWormSegmentList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<DesertScourgeBody>(),
                NPCType<DesertScourgeTail>(),
                NPCType<AquaticScourgeBody>(),
                NPCType<AquaticScourgeBodyAlt>(),
                NPCType<AquaticScourgeTail>(),
                NPCType<AstrumDeusBody>(),
                NPCType<AstrumDeusTail>(),
                NPCType<StormWeaverBody>(),
                NPCType<StormWeaverTail>(),
                NPCType<DevourerofGodsBody>(),
                NPCType<DevourerofGodsTail>(),
                NPCType<ThanatosBody1>(),
                NPCType<ThanatosBody2>(),
                NPCType<ThanatosTail>(),
                NPCType<AresLaserCannon>(),
                NPCType<AresTeslaCannon>(),
                NPCType<AresPlasmaFlamethrower>(),
                NPCType<AresGaussNuke>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int npcType) => List.Contains(npcType);
    }
}
