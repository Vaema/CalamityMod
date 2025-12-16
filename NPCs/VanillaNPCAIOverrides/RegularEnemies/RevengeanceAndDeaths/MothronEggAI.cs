using System;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class MothronEggAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            // Define what exactly is going to be shit out of this egg when it's ready.
            if (NPC.ai[1] == 0f)
            {
                NPC.ai[1] = (Main.rand.NextBool(10) && NPC.CountNPCS(NPCID.Mothron) < 2) ? NPCID.Mothron : NPCID.MothronSpawn;

                if ((int)NPC.ai[1] == NPCID.Mothron)
                {
                    NPC.defense = (int)Math.Round(NPC.defDefense * 1.5);
                    NPC.scale *= 2f;
                    NPC.width = NPC.height = (int)(34f * NPC.scale);
                    NPC.netUpdate = true;
                }
            }

            // Fall to the side like a sack of potatoes
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity.X *= 0.9f;
                NPC.rotation += NPC.velocity.X * 0.02f;
            }
            else
            {
                NPC.velocity.X *= 0.99f;
                NPC.rotation += NPC.velocity.X * 0.04f;
            }

            // How much time is needed before the egg hatches
            float hatchTimer = ((int)NPC.ai[1] == NPCID.Mothron ? 480f : 240f);
            if (CalamityWorld.death)
                hatchTimer *= 0.5f;

            NPC.ai[0] += 1f;
            if (NPC.ai[0] >= hatchTimer)
            {
                int hatchType = NPC.CountNPCS(NPCID.Mothron) < 2 ? (int)NPC.ai[1] : NPCID.MothronSpawn;
                NPC.Transform(hatchType);
            }

            // Jump around sometimes
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.velocity.Y == 0f && Math.Abs(NPC.velocity.X) < 0.2f && NPC.ai[0] >= hatchTimer * 0.75f)
            {
                float hatchCompleteness = NPC.ai[0] - hatchTimer * 0.75f;
                hatchCompleteness /= hatchTimer * 0.25f;
                if (Main.rand.Next(-10, 120) < hatchCompleteness * 100f)
                {
                    NPC.velocity.Y -= Main.rand.Next(20, 40) * 0.025f;
                    NPC.velocity.X += Main.rand.Next(-20, 20) * 0.025f;
                    NPC.velocity *= 1f + hatchCompleteness * 2f;
                    NPC.netUpdate = true;
                }
            }

            return false;
        }
    }
}
