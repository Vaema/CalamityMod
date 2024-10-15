using System.Collections.Generic;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains all of the NPC's IDs of the NPCs who are immune to the Confusion debuff.
    /// </summary>
    public sealed class ConfusionDebuffImmunitiesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<AeroSlime>(),
                NPCType<Rimehound>(),
                NPCType<AstralachneaGround>(),
                NPCType<AstralachneaWall>(),
                NPCType<BloomSlime>(),
                NPCType<Bohldohr>(),
                NPCType<CalamityEye>(),
                NPCType<CrimulanBlightSlime>(),
                NPCType<Cryon>(),
                NPCType<CryoSlime>(),
                NPCType<RenegadeWarlock>(),
                NPCType<DespairStone>(),
                NPCType<EbonianBlightSlime>(),
                NPCType<FearlessGoldfishWarrior>(),
                NPCType<HeatSpirit>(),
                NPCType<MantisShrimp>(),
                NPCType<OverloadedSoldier>(),
                NPCType<PerennialSlime>(),
                NPCType<Rotdog>(),
                NPCType<Scryllar>(),
                NPCType<ScryllarRage>(),
                NPCType<SeaUrchin>(),
                NPCType<StellarCulex>(),
                NPCType<Stormlion>(),
                NPCType<SuperDummyNPC>(),
                NPCType<WulfrumGyrator>(),
                NPCType<WulfrumRover>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if an NPC is supposed to be immune to the Confusion debuff.
        /// </summary>
        public static bool IsNPCImmune(NPC npc) => List.Contains(npc.type);
    }
}
