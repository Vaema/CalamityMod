using System.Collections.Generic;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.GreatSandShark;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class MinibossList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                NPCType<EidolonWyrmHead>(),
                NPCType<Mauler>(),
                NPCType<ReaperShark>(),
                NPCType<ColossalSquid>(),
                NPCType<GreatSandShark>(),
                NPCType<GiantClam>(),
                NPCType<ArmoredDiggerHead>(),
                NPCType<ArmoredDiggerBody>(),
                NPCType<ArmoredDiggerTail>(),
                NPCType<ThiccWaifu>(),
                NPCType<Horse>(),
                NPCType<PlaguebringerMiniboss>(),
                NPCID.Pumpking,
                NPCID.MourningWood,
                NPCID.IceQueen,
                NPCID.SantaNK1,
                NPCID.Everscream,
                NPCID.DD2Betsy,
                NPCID.Mothron,
                NPCID.MartianSaucer,
                NPCID.MartianSaucerCannon,
                NPCID.MartianSaucerCore,
                NPCID.MartianSaucerTurret
            ];
        }

        public override void Unload() => List = null;

        public static bool IsMiniboss(NPC npc) => List.Contains(npc.type);
    }
}
