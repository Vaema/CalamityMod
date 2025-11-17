using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class StarCellAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.noTileCollide = false;
            if (NPC.ai[0] == 0f)
            {
                // Avoid cheap bullshit
                if (NPC.type == NPCID.DeadlySphere || NPC.type == NPCID.NebulaHeadcrab)
                    NPC.damage = 0;

                NPC.TargetClosest();
                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 1f;
                }
                else
                {
                    Vector2 cellTargetDirection = Main.player[NPC.target].Center - NPC.Center;
                    cellTargetDirection.Y -= Main.player[NPC.target].height / 4;
                    float cellTargetDist = cellTargetDirection.Length();
                    if (cellTargetDist > 800f)
                    {
                        NPC.ai[0] = 2f;
                    }
                    else
                    {
                        Vector2 cellCenter = NPC.Center;
                        cellCenter.X = Main.player[NPC.target].Center.X;
                        Vector2 cellFaceDirection = cellCenter - NPC.Center;
                        if (cellFaceDirection.Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, cellCenter, 1, 1))
                        {
                            NPC.ai[0] = 3f;
                            NPC.ai[1] = cellCenter.X;
                            NPC.ai[2] = cellCenter.Y;
                            Vector2 cellCenter2 = NPC.Center;
                            cellCenter2.Y = Main.player[NPC.target].Center.Y;
                            if (cellFaceDirection.Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, cellCenter2, 1, 1) && Collision.CanHit(cellCenter2, 1, 1, Main.player[NPC.target].position, 1, 1))
                            {
                                NPC.ai[0] = 3f;
                                NPC.ai[1] = cellCenter2.X;
                                NPC.ai[2] = cellCenter2.Y;
                            }
                        }
                        else
                        {
                            cellCenter = NPC.Center;
                            cellCenter.Y = Main.player[NPC.target].Center.Y;
                            if ((cellCenter - NPC.Center).Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, cellCenter, 1, 1))
                            {
                                NPC.ai[0] = 3f;
                                NPC.ai[1] = cellCenter.X;
                                NPC.ai[2] = cellCenter.Y;
                            }
                        }

                        if (NPC.ai[0] == 0f)
                        {
                            NPC.localAI[0] = 0f;
                            cellTargetDirection.Normalize();
                            cellTargetDirection *= 0.5f;
                            NPC.velocity += cellTargetDirection;
                            NPC.ai[0] = 4f;
                            NPC.ai[1] = 0f;
                        }
                    }
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                // Set damage or avoid cheap bullshit
                if (NPC.type == NPCID.DeadlySphere)
                    NPC.damage = NPC.defDamage;
                else if (NPC.type == NPCID.NebulaHeadcrab)
                    NPC.damage = 0;

                NPC.rotation += NPC.direction * 0.3f;
                Vector2 attacktargetDirection = Main.player[NPC.target].Center - NPC.Center;
                if (NPC.type == NPCID.NebulaHeadcrab)
                    attacktargetDirection = Main.player[NPC.target].Top - NPC.Center;

                float attackTargetDist = attacktargetDirection.Length();
                float attackVelocity = CalamityWorld.death ? 9f : 7.5f;
                attackVelocity += attackTargetDist / 100f;
                int attackVelocityMult = CalamityWorld.death ? 40 : 45;
                attacktargetDirection.Normalize();
                attacktargetDirection *= attackVelocity;
                NPC.velocity = (NPC.velocity * (attackVelocityMult - 1) + attacktargetDirection) / attackVelocityMult;
                if (!Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                }

                if (NPC.type == NPCID.NebulaHeadcrab && attackTargetDist < 40f && Main.player[NPC.target].active && !Main.player[NPC.target].dead)
                {
                    bool headcrabAttach = true;
                    for (int p = 0; p < Main.maxNPCs; p++)
                    {
                        NPC nPC7 = Main.npc[p];
                        if (nPC7.active && nPC7.type == NPC.type && nPC7.ai[0] == 5f && nPC7.target == NPC.target)
                        {
                            headcrabAttach = false;
                            break;
                        }
                    }

                    if (headcrabAttach)
                    {
                        NPC.Center = Main.player[NPC.target].Top;
                        NPC.velocity = Vector2.Zero;
                        NPC.ai[0] = 5f;
                        NPC.ai[1] = 0f;
                        NPC.netUpdate = true;
                    }
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                // Avoid cheap bullshit
                if (NPC.type == NPCID.DeadlySphere || NPC.type == NPCID.NebulaHeadcrab)
                    NPC.damage = 0;

                NPC.rotation = NPC.velocity.X * 0.1f;
                NPC.noTileCollide = true;
                Vector2 idleTargetDirection = Main.player[NPC.target].Center - NPC.Center;
                float idleTargetDist = idleTargetDirection.Length();
                float idleVelocity = CalamityWorld.death ? 6f : 4.5f;
                int idleVelocityMult = 2;
                idleTargetDirection.Normalize();
                idleTargetDirection *= idleVelocity;
                NPC.velocity = (NPC.velocity * (idleVelocityMult - 1) + idleTargetDirection) / idleVelocityMult;
                if (idleTargetDist < 600f && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.ai[0] = 0f;
            }
            else if (NPC.ai[0] == 3f)
            {
                // Avoid cheap bullshit
                if (NPC.type == NPCID.DeadlySphere || NPC.type == NPCID.NebulaHeadcrab)
                    NPC.damage = 0;

                NPC.rotation = NPC.velocity.X * 0.1f;
                Vector2 blockedCellCenter = new Vector2(NPC.ai[1], NPC.ai[2]);
                Vector2 blockedCellDirection = blockedCellCenter - NPC.Center;
                float blockedTargetDist = blockedCellDirection.Length();
                float blockedVelocity = CalamityWorld.death ? 4f : 3f;
                float blockedVelocityMult = 2f;
                blockedCellDirection.Normalize();
                blockedCellDirection *= blockedVelocity;
                NPC.velocity = (NPC.velocity * (blockedVelocityMult - 1f) + blockedCellDirection) / blockedVelocityMult;
                if (NPC.collideX || NPC.collideY)
                {
                    NPC.ai[0] = 4f;
                    NPC.ai[1] = 0f;
                }

                if (blockedTargetDist < blockedVelocity || blockedTargetDist > 800f || Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                    NPC.ai[0] = 0f;
            }
            else if (NPC.ai[0] == 4f)
            {
                // Avoid cheap bullshit
                if (NPC.type == NPCID.DeadlySphere || NPC.type == NPCID.NebulaHeadcrab)
                    NPC.damage = 0;

                NPC.rotation = NPC.velocity.X * 0.1f;
                if (NPC.collideX)
                    NPC.velocity.X *= -0.8f;

                if (NPC.collideY)
                    NPC.velocity.Y *= -0.8f;

                Vector2 smolCellDirection;
                if (NPC.velocity.X == 0f && NPC.velocity.Y == 0f)
                {
                    smolCellDirection = Main.player[NPC.target].Center - NPC.Center;
                    smolCellDirection.Y -= Main.player[NPC.target].height / 4;
                    smolCellDirection.Normalize();
                    NPC.velocity = smolCellDirection * 0.1f;
                }

                float smolCellVelocity = CalamityWorld.death ? 4f : 3f;
                float smolCellVelocityMult = CalamityWorld.death ? 16f : 18f;
                smolCellDirection = NPC.velocity;
                smolCellDirection.Normalize();
                smolCellDirection *= smolCellVelocity;
                NPC.velocity = (NPC.velocity * (smolCellVelocityMult - 1f) + smolCellDirection) / smolCellVelocityMult;
                NPC.ai[1] += 1f;
                if (NPC.ai[1] > 180f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                }

                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                    NPC.ai[0] = 0f;

                NPC.localAI[0] += 1f;
                if (NPC.localAI[0] >= 5f && !Collision.SolidCollision(NPC.position - new Vector2(10f, 10f), NPC.width + 20, NPC.height + 20))
                {
                    NPC.localAI[0] = 0f;
                    Vector2 cellCentered = NPC.Center;
                    cellCentered.X = Main.player[NPC.target].Center.X;
                    if (Collision.CanHit(NPC.Center, 1, 1, cellCentered, 1, 1) && Collision.CanHit(NPC.Center, 1, 1, cellCentered, 1, 1) && Collision.CanHit(Main.player[NPC.target].Center, 1, 1, cellCentered, 1, 1))
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = cellCentered.X;
                        NPC.ai[2] = cellCentered.Y;
                    }
                    else
                    {
                        cellCentered = NPC.Center;
                        cellCentered.Y = Main.player[NPC.target].Center.Y;
                        if (Collision.CanHit(NPC.Center, 1, 1, cellCentered, 1, 1) && Collision.CanHit(Main.player[NPC.target].Center, 1, 1, cellCentered, 1, 1))
                        {
                            NPC.ai[0] = 3f;
                            NPC.ai[1] = cellCentered.X;
                            NPC.ai[2] = cellCentered.Y;
                        }
                    }
                }
            }
            else if (NPC.ai[0] == 5f)
            {
                Player player8 = Main.player[NPC.target];
                if (!player8.active || player8.dead)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
                else
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    NPC.Center = ((player8.gravDir == 1f) ? player8.Top : player8.Bottom) + new Vector2(player8.direction * 4, 0f);
                    NPC.gfxOffY = player8.gfxOffY;
                    NPC.velocity = Vector2.Zero;
                    if (!player8.creativeGodMode)
                        player8.AddBuff(BuffID.Obstructed, 59);
                }
            }

            if (NPC.type == NPCID.StardustCellBig)
            {
                NPC.rotation = 0f;
                for (int r = 0; r < Main.maxNPCs; r++)
                {
                    if (r != NPC.whoAmI && Main.npc[r].active && Main.npc[r].type == NPC.type && Math.Abs(NPC.position.X - Main.npc[r].position.X) + Math.Abs(NPC.position.Y - Main.npc[r].position.Y) < NPC.width)
                    {
                        if (NPC.position.X < Main.npc[r].position.X)
                            NPC.velocity.X -= 0.05f;
                        else
                            NPC.velocity.X += 0.05f;

                        if (NPC.position.Y < Main.npc[r].position.Y)
                            NPC.velocity.Y -= 0.05f;
                        else
                            NPC.velocity.Y += 0.05f;
                    }
                }
            }
            else
            {
                if (NPC.type != NPCID.NebulaHeadcrab)
                    return false;

                NPC.hide = NPC.ai[0] == 5f;
                NPC.rotation = NPC.velocity.X * 0.1f;
                for (int s = 0; s < Main.maxNPCs; s++)
                {
                    if (s != NPC.whoAmI && Main.npc[s].active && Main.npc[s].type == NPC.type && Math.Abs(NPC.position.X - Main.npc[s].position.X) + Math.Abs(NPC.position.Y - Main.npc[s].position.Y) < NPC.width)
                    {
                        if (NPC.position.X < Main.npc[s].position.X)
                            NPC.velocity.X -= 0.05f;
                        else
                            NPC.velocity.X += 0.05f;

                        if (NPC.position.Y < Main.npc[s].position.Y)
                            NPC.velocity.Y -= 0.05f;
                        else
                            NPC.velocity.Y += 0.05f;
                    }
                }
            }

            return false;
        }
    }
}

