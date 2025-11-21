using System;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class FlyingFishAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.noGravity = true;
            if (NPC.collideX)
            {
                if (NPC.oldVelocity.X > 0f)
                {
                    NPC.direction = -1;
                }
                else
                {
                    NPC.direction = 1;
                }
                NPC.velocity.X = (float)NPC.direction;
            }
            if (NPC.collideY)
            {
                if (NPC.oldVelocity.Y > 0f)
                {
                    NPC.directionY = -1;
                }
                else
                {
                    NPC.directionY = 1;
                }
                NPC.velocity.Y = (float)NPC.directionY;
            }
            if (NPC.type == NPCID.EyeballFlyingFish)
            {
                NPC.position += NPC.netOffset;
                if (NPC.alpha == 255)
                {
                    NPC.velocity.Y = -6f;
                    NPC.netUpdate = true;
                    for (int i = 0; i < 15; i++)
                    {
                        Dust eyeFishDust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood);
                        Dust dust = eyeFishDust;
                        dust.velocity *= 0.5f;
                        eyeFishDust.scale = 1f + Main.rand.NextFloat() * 0.5f;
                        eyeFishDust.fadeIn = 1.5f + Main.rand.NextFloat() * 0.5f;
                        dust = eyeFishDust;
                        dust.velocity += NPC.velocity * 0.5f;
                    }
                }

                NPC.alpha -= 15;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;

                if (NPC.alpha != 0)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Dust eyeFishDust2 = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood);
                        Dust dust = eyeFishDust2;
                        dust.velocity *= 1f;
                        eyeFishDust2.scale = 1f + Main.rand.NextFloat() * 0.5f;
                        eyeFishDust2.fadeIn = 1.5f + Main.rand.NextFloat() * 0.5f;
                        dust = eyeFishDust2;
                        dust.velocity += NPC.velocity * 0.3f;
                    }
                }

                if (Main.rand.NextBool(3))
                {
                    Dust eyeFishIdleDust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood);
                    Dust dust = eyeFishIdleDust;
                    dust.velocity *= 0f;
                    eyeFishIdleDust.alpha = 120;
                    eyeFishIdleDust.scale = 0.7f + Main.rand.NextFloat() * 0.5f;
                    dust = eyeFishIdleDust;
                    dust.velocity += NPC.velocity * 0.3f;
                }

                NPC.position -= NPC.netOffset;
            }
            int fishTarget = NPC.target;
            int fishDirection = NPC.direction;
            if (NPC.target == Main.maxPlayers || (Main.player[NPC.target].wet && NPC.type != NPCID.EyeballFlyingFish) || Main.player[NPC.target].dead || Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
            {
                NPC.ai[0] = 90f;
                NPC.TargetClosest(true);
            }
            else if (NPC.ai[0] > 0f)
            {
                NPC.ai[0] -= 1f;
                NPC.TargetClosest(true);
            }
            if (NPC.netUpdate && fishTarget == NPC.target && fishDirection == NPC.direction)
            {
                NPC.netUpdate = false;
            }
            float acceleration = 0.05f;
            float verticalAcceleration = 0.01f;
            float maxVelocity = 6f;
            float maxYSpeed = 3f;
            float turnAroundXDist = 30f;
            float turnAroundYDist = 100f;
            float targetXDist = Math.Abs(NPC.position.X + (float)(NPC.width / 2) - (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)));
            float targetYDist = Main.player[NPC.target].position.Y - (float)(NPC.height / 2);
            if (NPC.type == NPCID.FlyingAntlion || NPC.type == NPCID.GiantFlyingAntlion)
            {
                acceleration = 0.09f;
                verticalAcceleration = 0.03f;
                maxVelocity = 9f;
                maxYSpeed = 6f;
                turnAroundXDist = 40f;
                turnAroundYDist = 150f;
                targetYDist = Main.player[NPC.target].Center.Y - (float)(NPC.height / 2);
                NPC.rotation = NPC.velocity.X * 0.1f;
                int playerInc;
                for (int p = 0; p < Main.maxNPCs; p = playerInc + 1)
                {
                    if (p != NPC.whoAmI && Main.npc[p].active && Main.npc[p].type == NPC.type && Math.Abs(NPC.position.X - Main.npc[p].position.X) + Math.Abs(NPC.position.Y - Main.npc[p].position.Y) < (float)NPC.width)
                    {
                        if (NPC.position.X < Main.npc[p].position.X)
                        {
                            NPC.velocity.X = NPC.velocity.X - 0.05f;
                        }
                        else
                        {
                            NPC.velocity.X = NPC.velocity.X + 0.05f;
                        }
                        if (NPC.position.Y < Main.npc[p].position.Y)
                        {
                            NPC.velocity.Y = NPC.velocity.Y - 0.05f;
                        }
                        else
                        {
                            NPC.velocity.Y = NPC.velocity.Y + 0.05f;
                        }
                    }
                    playerInc = p;
                }
            }
            else if (NPC.type == NPCID.EyeballFlyingFish)
            {
                acceleration = 0.16f;
                verticalAcceleration = 0.12f;
                maxVelocity = 9f;
                maxYSpeed = 5f;
                turnAroundXDist = 0f;
                turnAroundYDist = 250f;
                targetYDist = Main.player[NPC.target].position.Y;
                if (Main.dayTime)
                {
                    targetYDist = 0f;
                    NPC.direction *= -1;
                }
            }
            if (CalamityWorld.death)
            {
                maxVelocity *= 1.25f;
                acceleration *= 1.25f;
            }
            if (NPC.ai[0] <= 0f)
            {
                maxVelocity *= 0.8f;
                acceleration *= 0.7f;
                targetYDist = NPC.Center.Y + (float)(NPC.directionY * 1000);
                if (NPC.velocity.X < 0f)
                {
                    NPC.direction = -1;
                }
                else if (NPC.velocity.X > 0f || NPC.direction == 0)
                {
                    NPC.direction = 1;
                }
            }
            if (targetXDist > turnAroundXDist)
            {
                if (NPC.direction == -1 && NPC.velocity.X > -maxVelocity)
                {
                    NPC.velocity.X = NPC.velocity.X - acceleration;
                    if (NPC.velocity.X > maxVelocity)
                    {
                        NPC.velocity.X = NPC.velocity.X - acceleration;
                    }
                    else if (NPC.velocity.X > 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X - acceleration / 2f;
                    }
                    if (NPC.velocity.X < -maxVelocity)
                    {
                        NPC.velocity.X = -maxVelocity;
                    }
                }
                else if (NPC.direction == 1 && NPC.velocity.X < maxVelocity)
                {
                    NPC.velocity.X = NPC.velocity.X + acceleration;
                    if (NPC.velocity.X < -maxVelocity)
                    {
                        NPC.velocity.X = NPC.velocity.X + acceleration;
                    }
                    else if (NPC.velocity.X < 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X + acceleration / 2f;
                    }
                    if (NPC.velocity.X > maxVelocity)
                    {
                        NPC.velocity.X = maxVelocity;
                    }
                }
            }
            if (targetXDist > turnAroundYDist)
            {
                targetYDist -= turnAroundYDist / 2f;
            }
            if (NPC.position.Y < targetYDist)
            {
                NPC.velocity.Y = NPC.velocity.Y + verticalAcceleration;
                if (NPC.velocity.Y < 0f)
                {
                    NPC.velocity.Y = NPC.velocity.Y + verticalAcceleration;
                }
            }
            else
            {
                NPC.velocity.Y = NPC.velocity.Y - verticalAcceleration;
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y = NPC.velocity.Y - verticalAcceleration;
                }
            }
            if (NPC.velocity.Y < -maxYSpeed)
            {
                NPC.velocity.Y = -maxYSpeed;
            }
            if (NPC.velocity.Y > maxYSpeed)
            {
                NPC.velocity.Y = maxYSpeed;
            }
            if (NPC.wet && NPC.type != NPCID.EyeballFlyingFish)
            {
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.95f;
                }
                NPC.velocity.Y = NPC.velocity.Y - 0.7f;
                if (NPC.velocity.Y < -6f)
                {
                    NPC.velocity.Y = -6f;
                }
            }
            return false;
        }
    }
}
