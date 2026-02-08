using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class GraniteElementalAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.noGravity = true;
            NPC.noTileCollide = false;

            // Set damage
            NPC.damage = NPC.defDamage;

            NPC.defense = NPC.defDefense;

            if (NPC.justHit && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(10))
            {
                NPC.netUpdate = true;
                NPC.ai[0] = -1f;
                NPC.ai[1] = 0f;
            }
            if (NPC.ai[0] == -1f)
            {
                // Avoid cheap bullshit
                NPC.damage = 0;

                NPC.defense = NPC.defDefense + 10;

                NPC.noGravity = false;
                NPC.velocity.X *= 0.98f;
                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 120f)
                {
                    NPC.ai[0] = (NPC.ai[1] = (NPC.ai[2] = (NPC.ai[3] = 0f)));
                }
            }
            else if (NPC.ai[0] == 0f)
            {
                NPC.TargetClosest(true);
                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 1f;
                    return false;
                }
                Vector2 targetDirection = Main.player[NPC.target].Center - NPC.Center;
                targetDirection.Y -= (float)(Main.player[NPC.target].height / 4);
                float attackTimeMax1 = targetDirection.Length();
                if (attackTimeMax1 > (CalamityWorld.death ? 400f : 800f))
                {
                    NPC.ai[0] = 2f;
                    return false;
                }
                Vector2 elementalCenter = NPC.Center;
                elementalCenter.X = Main.player[NPC.target].Center.X;
                Vector2 targetDistance = elementalCenter - NPC.Center;
                if (targetDistance.Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, elementalCenter, 1, 1))
                {
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = elementalCenter.X;
                    NPC.ai[2] = elementalCenter.Y;
                    Vector2 elementalCenter2 = NPC.Center;
                    elementalCenter2.Y = Main.player[NPC.target].Center.Y;
                    if (targetDistance.Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, elementalCenter2, 1, 1) && Collision.CanHit(elementalCenter2, 1, 1, Main.player[NPC.target].position, 1, 1))
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = elementalCenter2.X;
                        NPC.ai[2] = elementalCenter2.Y;
                    }
                }
                else
                {
                    elementalCenter = NPC.Center;
                    elementalCenter.Y = Main.player[NPC.target].Center.Y;
                    if ((elementalCenter - NPC.Center).Length() > 8f && Collision.CanHit(NPC.Center, 1, 1, elementalCenter, 1, 1))
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = elementalCenter.X;
                        NPC.ai[2] = elementalCenter.Y;
                    }
                }
                if (NPC.ai[0] == 0f)
                {
                    NPC.localAI[0] = 0f;
                    targetDirection.Normalize();
                    targetDirection *= 0.5f;
                    NPC.velocity += targetDirection;
                    NPC.ai[0] = 4f;
                    NPC.ai[1] = 0f;
                }
            }
            else if (NPC.ai[0] == 1f)
            {
                Vector2 targetDirectionAgain = Main.player[NPC.target].Center - NPC.Center;
                float attackTimeMax2 = targetDirectionAgain.Length();
                float attackTimeMax3 = 2f;
                attackTimeMax3 += attackTimeMax2 / (CalamityWorld.death ? 160f : 180f);
                int attackTimeMax4 = 50;
                targetDirectionAgain.Normalize();
                targetDirectionAgain *= attackTimeMax3;
                NPC.velocity = (NPC.velocity * (float)(attackTimeMax4 - 1) + targetDirectionAgain) / (float)attackTimeMax4;
                if (!Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                NPC.noTileCollide = true;
                Vector2 targetDirection3 = Main.player[NPC.target].Center - NPC.Center;
                float attackTimeMax5 = targetDirection3.Length();
                float scaleFactor23 = CalamityWorld.death ? 3f : 2.5f;
                int attackTimeMax6 = 4;
                targetDirection3.Normalize();
                targetDirection3 *= scaleFactor23;
                NPC.velocity = (NPC.velocity * (float)(attackTimeMax6 - 1) + targetDirection3) / (float)attackTimeMax6;
                if (attackTimeMax5 < 600f && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.ai[0] = 0f;
                }
            }
            else if (NPC.ai[0] == 3f)
            {
                Vector2 elementalCenter3 = new Vector2(NPC.ai[1], NPC.ai[2]);
                Vector2 elementalDirection = elementalCenter3 - NPC.Center;
                float attackTimeMax7 = elementalDirection.Length();
                float attackTimeMax8 = 2f;
                float attackTimeMax9 = 3f;
                elementalDirection.Normalize();
                elementalDirection *= attackTimeMax8;
                NPC.velocity = (NPC.velocity * (attackTimeMax9 - 1f) + elementalDirection) / attackTimeMax9;
                if (NPC.collideX || NPC.collideY)
                {
                    NPC.ai[0] = 4f;
                    NPC.ai[1] = 0f;
                }
                if (attackTimeMax7 < attackTimeMax8 || attackTimeMax7 > 800f || Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 0f;
                }
            }
            else if (NPC.ai[0] == 4f)
            {
                if (NPC.collideX)
                {
                    NPC.velocity.X = NPC.velocity.X * -0.8f;
                }
                if (NPC.collideY)
                {
                    NPC.velocity.Y = NPC.velocity.Y * -0.8f;
                }
                Vector2 stationaryTargetDist;
                if (NPC.velocity.X == 0f && NPC.velocity.Y == 0f)
                {
                    stationaryTargetDist = Main.player[NPC.target].Center - NPC.Center;
                    stationaryTargetDist.Y -= (float)(Main.player[NPC.target].height / 4);
                    stationaryTargetDist.Normalize();
                    NPC.velocity = stationaryTargetDist * 0.1f;
                }
                float scaleFactor24 = CalamityWorld.death ? 2.5f : 2f;
                stationaryTargetDist = NPC.velocity;
                stationaryTargetDist.Normalize();
                stationaryTargetDist *= scaleFactor24;
                NPC.velocity = (NPC.velocity * 19f + stationaryTargetDist) / 20f;
                NPC.ai[1] += 1f;
                if (NPC.ai[1] > 180f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                }
                if (Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.ai[0] = 0f;
                }
                NPC.localAI[0] += 1f;
                if (NPC.localAI[0] >= 5f && !Collision.SolidCollision(NPC.position - new Vector2(10f, 10f), NPC.width + 20, NPC.height + 20))
                {
                    NPC.localAI[0] = 0f;
                    Vector2 elementalCenter4 = NPC.Center;
                    elementalCenter4.X = Main.player[NPC.target].Center.X;
                    if (Collision.CanHit(NPC.Center, 1, 1, elementalCenter4, 1, 1) && Collision.CanHit(NPC.Center, 1, 1, elementalCenter4, 1, 1) && Collision.CanHit(Main.player[NPC.target].Center, 1, 1, elementalCenter4, 1, 1))
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = elementalCenter4.X;
                        NPC.ai[2] = elementalCenter4.Y;
                        return false;
                    }
                    elementalCenter4 = NPC.Center;
                    elementalCenter4.Y = Main.player[NPC.target].Center.Y;
                    if (Collision.CanHit(NPC.Center, 1, 1, elementalCenter4, 1, 1) && Collision.CanHit(Main.player[NPC.target].Center, 1, 1, elementalCenter4, 1, 1))
                    {
                        NPC.ai[0] = 3f;
                        NPC.ai[1] = elementalCenter4.X;
                        NPC.ai[2] = elementalCenter4.Y;
                    }
                }
            }
            return false;
        }
    }
}
