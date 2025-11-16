using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class MimicAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            bool isLostHoppingPresent = NPC.type == NPCID.PresentMimic && !Main.snowMoon;

            if (NPC.ai[3] == 0f)
            {
                NPC.position.X += 8f;
                if (NPC.position.Y / 16f > Main.UnderworldLayer)
                {
                    NPC.ai[3] = 3f;
                }
                else if (NPC.position.Y / 16f > Main.worldSurface)
                {
                    NPC.TargetClosest(true);
                    NPC.ai[3] = 2f;
                }
                else
                {
                    NPC.ai[3] = 1f;
                }
            }

            // Never wait. Go straight for the player.
            if (NPC.type == NPCID.PresentMimic || NPC.type == NPCID.IceMimic)
            {
                NPC.ai[3] = 1f;
            }

            NPC.dontTakeDamage = NPC.ai[0] == 0f;

            // Sitting around, waiting for a potential player
            if (NPC.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                if (!isLostHoppingPresent)
                {
                    NPC.TargetClosest(true);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (NPC.velocity.X != 0f || NPC.velocity.Y < 0f || NPC.velocity.Y > 0.3f)
                    {
                        NPC.ai[0] = 1f;
                        NPC.netUpdate = true;
                        return false;
                    }

                    Rectangle detectionZone = new Rectangle((int)NPC.position.X - 80, (int)NPC.position.Y - 80, NPC.width + 160, NPC.height + 160);
                    // If a player is nearby, become active.
                    if (detectionZone.Intersects(Main.player[NPC.target].Hitbox) || NPC.life < NPC.lifeMax)
                    {
                        NPC.ai[0] = 1f;
                        NPC.netUpdate = true;
                    }
                }
            }
            else if (NPC.velocity.Y == 0f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.ai[2] += 1f;

                int timeSpentStopping = 20;
                if (NPC.ai[1] == 0f)
                {
                    timeSpentStopping = 12;
                }
                if (NPC.ai[2] < timeSpentStopping)
                {
                    NPC.velocity.X *= 0.9f;
                    return false;
                }

                NPC.ai[2] = 0f;

                if (!isLostHoppingPresent)
                {
                    NPC.TargetClosest(true);
                }
                if (NPC.direction == 0)
                {
                    NPC.direction = -1;
                }

                NPC.spriteDirection = NPC.direction;

                NPC.ai[1] += 1f;
                // Hop
                if (NPC.ai[1] == 2f)
                {
                    NPC.velocity.X = NPC.direction * 4f;
                    NPC.velocity.Y = -8f;
                    NPC.ai[1] = 0f;
                }
                else
                {
                    NPC.velocity.X = NPC.direction * 5.5f;
                    NPC.velocity.Y = -4f;
                }

                NPC.netUpdate = true;
            }
            else
            {
                // Set damage
                NPC.damage = NPC.defDamage;

                if (NPC.direction == 1 && NPC.velocity.X < 1f)
                {
                    NPC.velocity.X += 0.1f;
                    return false;
                }

                if (NPC.direction == -1 && NPC.velocity.X > -1f)
                {
                    NPC.velocity.X -= 0.1f;
                }
            }
            return false;
        }
    }
}
