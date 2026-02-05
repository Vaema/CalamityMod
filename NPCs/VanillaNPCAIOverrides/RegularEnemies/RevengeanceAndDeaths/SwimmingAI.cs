using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class SwimmingAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (NPC.wet)
            {
                bool noWetTargets = false;
                NPC.TargetClosest(false);
                if ((Main.player[NPC.target].wet || (CalamityWorld.death && NPC.Distance(Main.player[NPC.target].Center) < 400f)) && !Main.player[NPC.target].dead)
                {
                    noWetTargets = true;
                }
                if (!noWetTargets)
                {
                    if (NPC.collideX)
                    {
                        NPC.velocity.X *= -1f;
                        NPC.direction *= -1;
                        NPC.netUpdate = true;
                    }
                    if (NPC.collideY)
                    {
                        NPC.netUpdate = true;
                        if (NPC.velocity.Y > 0f)
                        {
                            NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
                            NPC.directionY = -1;
                            NPC.ai[0] = -1f;
                        }
                        else if (NPC.velocity.Y < 0f)
                        {
                            NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
                            NPC.directionY = 1;
                            NPC.ai[0] = 1f;
                        }
                    }
                }
                if (NPC.type == NPCID.AnglerFish)
                {
                    Lighting.AddLight((int)(NPC.position.X + (float)(NPC.width / 2) + (float)(NPC.direction * (NPC.width + 8))) / 16, (int)(NPC.position.Y + 2f) / 16, 0.07f, 0.04f, 0.025f);
                }
                if (noWetTargets)
                {
                    NPC.TargetClosest(true);
                    if (NPC.type == NPCID.Arapaima)
                    {
                        // Check if the direction value signs match
                        if ((NPC.velocity.X > 0).ToDirectionInt() != (NPC.velocity.X > 0).ToDirectionInt())
                        {
                            NPC.velocity.X *= 0.95f;
                        }
                        NPC.velocity.X += NPC.direction * 0.5f;
                        NPC.velocity.Y += NPC.directionY * 0.4f;

                        // I don't really understand why a boundary break penalty of 2 is used here, but just to be safe, I'll leave it alone.
                        if (NPC.velocity.X > 16f)
                        {
                            NPC.velocity.X = 14f;
                        }
                        if (NPC.velocity.X < -16f)
                        {
                            NPC.velocity.X = -14f;
                        }
                        if (NPC.velocity.Y > 10f)
                        {
                            NPC.velocity.Y = 8f;
                        }
                        if (NPC.velocity.Y < -10f)
                        {
                            NPC.velocity.Y = -8f;
                        }
                    }
                    else if (NPC.type == NPCID.Shark || NPC.type == NPCID.AnglerFish)
                    {
                        NPC.velocity.X += NPC.direction * 0.3f;
                        NPC.velocity.Y += NPC.directionY * 0.3f;
                        if (NPC.velocity.X > 10f)
                        {
                            NPC.velocity.X = 10f;
                        }
                        if (NPC.velocity.X < -10f)
                        {
                            NPC.velocity.X = -10f;
                        }
                        if (NPC.velocity.Y > 6f)
                        {
                            NPC.velocity.Y = 6f;
                        }
                        if (NPC.velocity.Y < -6f)
                        {
                            NPC.velocity.Y = -6f;
                        }
                        NPC.velocity = Vector2.Clamp(NPC.velocity, new Vector2(-10f, -6f), new Vector2(10f, 6f));
                    }
                    else
                    {
                        NPC.velocity.X += NPC.direction * 0.2f;
                        NPC.velocity.Y += NPC.directionY * 0.2f;
                        NPC.velocity = Vector2.Clamp(NPC.velocity, new Vector2(-6f, -4f), new Vector2(6f, 4f));
                    }
                }
                else
                {
                    if (NPC.type == NPCID.Arapaima)
                    {
                        NPC.directionY = (Main.player[NPC.target].position.Y > NPC.position.Y).ToDirectionInt();
                        NPC.velocity.X += NPC.direction * 0.2f;
                        if (NPC.velocity.X < -2f || NPC.velocity.X > 2f)
                        {
                            NPC.velocity.X *= 0.95f;
                        }
                        // Bob up and down in the water
                        if (NPC.ai[0] == -1f)
                        {
                            float yVelocityMin = -0.6f;
                            if (NPC.directionY < 0)
                            {
                                yVelocityMin = -1f;
                            }
                            if (NPC.directionY > 0)
                            {
                                yVelocityMin = -0.2f;
                            }
                            NPC.velocity.Y -= 0.02f;
                            if (NPC.velocity.Y < yVelocityMin)
                            {
                                NPC.ai[0] = 1f;
                            }
                        }
                        else
                        {
                            float yVelocityMin = 0.6f;
                            if (NPC.directionY < 0)
                            {
                                yVelocityMin = 0.2f;
                            }
                            if (NPC.directionY > 0)
                            {
                                yVelocityMin = 1f;
                            }
                            NPC.velocity.Y += 0.02f;
                            if (NPC.velocity.Y > yVelocityMin)
                            {
                                NPC.ai[0] = -1f;
                            }
                        }
                    }
                    else
                    {
                        NPC.velocity.X += NPC.direction * 0.1f;
                        if (NPC.velocity.X < -1f || NPC.velocity.X > 1f)
                        {
                            NPC.velocity.X *= 0.95f;
                        }
                        if (NPC.ai[0] == -1f)
                        {
                            NPC.velocity.Y -= 0.01f;
                            if (NPC.velocity.Y < -0.3f)
                            {
                                NPC.ai[0] = 1f;
                            }
                        }
                        else
                        {
                            NPC.velocity.Y += 0.01f;
                            if (NPC.velocity.Y > 0.3)
                            {
                                NPC.ai[0] = -1f;
                            }
                        }
                    }
                    int x = (int)NPC.Center.X / 16;
                    int y = (int)NPC.Center.Y / 16;
                    if (Main.tile[x, y - 1].LiquidAmount > 128)
                    {
                        if (Main.tile[x, y + 1].HasTile)
                        {
                            NPC.ai[0] = -1f;
                        }
                        else if (Main.tile[x, y + 2].HasTile)
                        {
                            NPC.ai[0] = -1f;
                        }
                    }
                    if (NPC.type != NPCID.Arapaima && Math.Abs(NPC.velocity.Y) < 0.4f)
                    {
                        NPC.velocity.Y *= 0.95f;
                    }
                }
            }
            else
            {
                if (NPC.velocity.Y == 0f)
                {
                    // Sit helplessly on land and do absolutely nothing.
                    if (NPC.type == NPCID.Shark)
                    {
                        NPC.velocity.X *= 0.94f;
                        if (Math.Abs(NPC.velocity.X) < 0.2)
                        {
                            NPC.velocity.X = 0f;
                        }
                    }
                    // Flop around
                    else if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.velocity.Y = Main.rand.NextFloat(-5f, -2f);
                        NPC.velocity.X = Main.rand.NextFloat(-2f, -2f);
                        NPC.netUpdate = true;
                    }
                }
                NPC.velocity.Y += 0.3f;
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                NPC.ai[0] = 1f;
            }
            NPC.rotation = NPC.velocity.Y * NPC.direction * 0.1f;
            NPC.rotation = MathHelper.Clamp(NPC.rotation, -0.2f, 0.2f);
            return false;
        }
    }
}
