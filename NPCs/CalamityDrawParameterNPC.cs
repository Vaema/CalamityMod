using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.NPCs.ExoMechs.Ares;
using Steamworks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs
{
    // Why this class is exist?
    // It because we want to avoid Threading Issues while We using NPC's properties on Draw Thread
    public sealed class CalamityDrawParameterNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;

        #region Draw Parameters
        public static bool[] DrawingMiracleBlight { get; private set; }
        public static bool[] DrawingPolarity { get; private set; }
        #endregion

        #region Filtering Fields
        public static List<int> MiracleBlightExcludedNPCs => new()
        {
            // List the reason why the NPC(s) are excluded :)

            // The particle sets break with the visuals and this is the easiest way to fix this that isn't stupidly complex.
            ModContent.NPCType<AresBody>(),
            ModContent.NPCType<AresGaussNuke>(),
            ModContent.NPCType<AresLaserCannon>(),
            ModContent.NPCType<AresPlasmaFlamethrower>(),
            ModContent.NPCType<AresTeslaCannon>(),

            // Breaks with being behind tiles, and causes a funny interaction where his head goes behind his neck.
            NPCID.MoonLordCore,
            NPCID.MoonLordHand,
            NPCID.MoonLordHead
        };
        #endregion

        #region Load / Unload
        public override void Load()
        {
            DrawingMiracleBlight = new bool[Main.maxNPCs + 1];
            DrawingPolarity = new bool[Main.maxNPCs + 1];
        }

        public override void Unload()
        {
            DrawingMiracleBlight = null;
            DrawingPolarity = null;
        }
        #endregion

        public override void SetDefaults(NPC entity) => ResetParameters(entity);
        public override bool PreAI(NPC npc)
        {
            if (npc is null)
                return true;

            var whoAmI = npc.whoAmI;
            DrawingMiracleBlight[whoAmI] = ShouldDrawMiracleBlight(npc);
            DrawingPolarity[whoAmI] = ShouldDrawPolarity(npc);

            return true;
        }

        public static void ResetParameters(NPC npc)
        {
            if (npc is null)
                return;

            var whoAmI = npc.whoAmI;
            DrawingMiracleBlight[whoAmI] = false;
            DrawingPolarity[whoAmI] = false;
        }

        public static bool ShouldDrawMiracleBlight(NPC npc)
        {
            if (npc is null || !npc.active)
                return false;

            // Do not draw weird MP types less than or equal to 0.
            if (npc.type <= NPCID.None)
                return false;

            // Do not draw other mod's bosses.
            if (npc.ModNPC != null && npc.ModNPC.Mod != CalamityMod.Instance && npc.boss)
                return false;

            // Don't draw excluded NPCs, or if the npc is a bestiary dummy.
            if (MiracleBlightExcludedNPCs.Contains(npc.type) || npc.IsABestiaryIconDummy)
                return false;

            // Safety check for weird MP bug when getting global npcs.
            if (!npc.TryGetGlobalNPCSafer<CalamityGlobalNPC>(out var calNPC) || !npc.TryGetGlobalNPCSafer<CalamityPolarityNPC>(out var polNPC))
                return false;

            // Do not draw if the npc does not have miracle blight, or has the polarity effect.
            if (!calNPC.miracleBlight || polNPC.CurPolarity > 0f)
                return false;

            // Do not draw if the current player has the trippy effect.
            if (Main.LocalPlayer.Calamity().trippy)
                return false;

            return true;
        }

        public static bool ShouldDrawPolarity(NPC npc)
        {
            if (npc is null || !npc.active)
                return false;

            // Safety check for weird MP bug when getting global npcs.
            if (!npc.TryGetGlobalNPCSafer<CalamityGlobalNPC>(out var calNPC) || !npc.TryGetGlobalNPCSafer<CalamityPolarityNPC>(out var polNPC))
                return false;

            // I don't know who would be using this while also inflicting miracle blight, but in that rare case, do not draw these.
            if (calNPC.miracleBlight)
                return false;

            // Do not draw if the npc doesn't have the polarity effect.
            if (polNPC.CurPolarity <= 0f)
                return false;

            return true;
        }
    }
}
