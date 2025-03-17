using System.Collections.Generic;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class BossTypeDict : ModSystem
    {
        public static IDictionary<int, int> Dict { get; private set; }

        public override void OnModLoad()
        {
            Dict = new SortedDictionary<int, int>()
            {
                { NPCID.KingSlime, 1 },
                { NPCType<DesertScourgeHead>(), 2 },
                { NPCID.EyeofCthulhu, 3 },
                { NPCType<Crabulon>(), 4 },
                { NPCID.EaterofWorldsHead, 5 },
                { NPCID.EaterofWorldsBody, 5 },
                { NPCID.EaterofWorldsTail, 5 },
                { NPCID.BrainofCthulhu, 6 },
                { NPCType<HiveMind>(), 7 },
                { NPCType<PerforatorHive>(), 8 },
                { NPCID.QueenBee, 9 },
                { NPCID.SkeletronHead, 10 },
                { NPCType<SlimeGodCore>(), 11 },
                { NPCType<SplitEbonianPaladin>(), 11 },
                { NPCType<SplitCrimulanPaladin>(), 11 },
                { NPCID.WallofFlesh, 12 },
                { NPCType<Cryogen>(), 13 },
                { NPCID.Retinazer, 14 },
                { NPCID.Spazmatism, 14 },
                { NPCType<AquaticScourgeHead>(), 15 },
                { NPCID.TheDestroyer, 16 },
                { NPCType<BrimstoneElemental>(), 17 },
                { NPCID.SkeletronPrime, 18 },
                { NPCType<CalamitasClone>(), 19 },
                { NPCID.Plantera, 20 },
                { NPCType<Leviathan>(), 21 },
                { NPCType<Anahita>(), 21 },
                { NPCType<AstrumAureus>(), 22 },
                { NPCID.Golem, 23 },
                { NPCType<PlaguebringerGoliath>(), 24 },
                { NPCID.DukeFishron, 25 },
                { NPCType<RavagerBody>(), 26 },
                { NPCID.CultistBoss, 27 },
                { NPCType<AstrumDeusHead>(), 28 },
                { NPCID.MoonLordCore, 29 },
                { NPCType<ProfanedGuardianCommander>(), 30 },
                { NPCType<Bumblefuck>(), 31 },
                { NPCType<Providence>(), 32 },
                { NPCType<CeaselessVoid>(), 33 },
                { NPCType<StormWeaverHead>(), 34 },
                { NPCType<Signus>(), 35 },
                { NPCType<Polterghast>(), 36 },
                { NPCType<OldDuke>(), 37 },
                { NPCType<DevourerofGodsHead>(), 38 },
                { NPCType<Yharon>(), 39 },
                { NPCType<SupremeCalamitas>(), 40 },
                { NPCType<AresBody>(), 41 },
                { NPCType<ThanatosHead>(), 41 },
                { NPCType<Artemis>(), 41 },
                { NPCType<Apollo>(), 41 },
                { NPCID.QueenSlimeBoss, 42 },
                { NPCID.HallowBoss, 43 },
                { NPCID.Deerclops, 44 }
            };
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        public static bool TryGet(int npcType, out int type)
        {
            return Dict.TryGetValue(npcType, out type);
        }
    }
}
