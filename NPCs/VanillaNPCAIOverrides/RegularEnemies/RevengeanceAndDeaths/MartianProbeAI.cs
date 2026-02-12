using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class MartianProbeAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            // Float around
            if (NPC.ai[0] == 0f)
            {
                if (NPC.direction == 0)
                {
                    NPC.TargetClosest(true);
                    NPC.netUpdate = true;
                }

                if (NPC.collideX)
                {
                    NPC.direction = -NPC.direction;
                    NPC.netUpdate = true;
                }

                NPC.velocity.X = 6f * (float)NPC.direction;
                Point centerTileCoords = NPC.Center.ToTileCoordinates();
                int distanceFromGround = 30;

                if (WorldGen.InWorld(centerTileCoords.X, centerTileCoords.Y, 30))
                {
                    for (int y = 0; y < 30; y++)
                    {
                        if (WorldGen.SolidTile(centerTileCoords.X, centerTileCoords.Y + y))
                        {
                            distanceFromGround = y;
                            break;
                        }
                    }
                }

                if (distanceFromGround < 15)
                {
                    NPC.velocity.Y = Math.Max(NPC.velocity.Y - 0.05f, -3.5f);
                }
                else if (distanceFromGround < 20)
                {
                    NPC.velocity.Y *= 0.95f;
                }
                else
                {
                    NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.05f, 1.5f);
                }

                int playerIndex = NPC.FindClosestPlayer(out float distanceFromPlayer);
                if (playerIndex == -1 || Main.player[playerIndex].dead)
                {
                    return false;
                }

                // If a player is below and nearby the probe, become active
                if (distanceFromPlayer < 440f && Main.player[playerIndex].Center.Y > NPC.Center.Y)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
            }
            // Wait
            else if (NPC.ai[0] == 1f)
            {
                NPC.ai[1] += 1f;

                NPC.velocity *= 0.93f;

                if (NPC.ai[1] >= (CalamityWorld.death ? 5f : 45f))
                {
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 2f;

                    int closetPlayer = NPC.FindClosestPlayer();
                    // Update the X acceleration
                    NPC.ai[3] = closetPlayer != -1 ? (Main.player[closetPlayer].Center.X < NPC.Center.X).ToDirectionInt() : 1f;

                    NPC.netUpdate = true;
                }
            }
            // And fly away
            else if (NPC.ai[0] == 2f)
            {
                NPC.noTileCollide = true;

                NPC.ai[1] += 1f;

                NPC.velocity.Y = Math.Max(NPC.velocity.Y - 0.2f, -12f);
                NPC.velocity.X = Math.Min(NPC.velocity.X + NPC.ai[3] * 0.1f, 6f);

                // If above the world or enough time has passed, summon the naked grey gods.
                if ((NPC.position.Y < -NPC.height || NPC.ai[1] >= 135f) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Main.StartInvasion(InvasionID.MartianMadness);
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
            }

            Vector3 lightColor = Color.SkyBlue.ToVector3();
            if (NPC.ai[0] == 2f)
            {
                lightColor = Color.Red.ToVector3();
            }
            lightColor *= 0.65f;

            Lighting.AddLight(NPC.Center, lightColor);

            return false;
        }
    }
}
