using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.MiniBosses
{
    public class PumpkingAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.localAI[0] += 1f;
            if (NPC.localAI[0] > 6f)
            {
                NPC.localAI[0] = 0f;
                NPC.localAI[1] += 1f;

                if (NPC.localAI[1] > 4f)
                    NPC.localAI[1] = 0f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.localAI[2] += 1f;
                if (NPC.localAI[2] > 300f)
                {
                    NPC.ai[3] = (float)Main.rand.Next(3);
                    NPC.localAI[2] = 0f;
                }
                else if (NPC.ai[3] == 0f && NPC.localAI[2] % 30f == 0f && NPC.localAI[2] > 30f)
                {
                    float greekFireSpeed = 10f;
                    Vector2 greekFireSpawnPos = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f + 30f);
                    if (!WorldGen.SolidTile((int)greekFireSpawnPos.X / 16, (int)greekFireSpawnPos.Y / 16))
                    {
                        float greekFireTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - greekFireSpawnPos.X;
                        float greekFireTargetY = Main.player[NPC.target].position.Y - greekFireSpawnPos.Y;
                        greekFireTargetX += (float)Main.rand.Next(-50, 51);
                        greekFireTargetY += (float)Main.rand.Next(50, 201);
                        greekFireTargetY *= 0.2f;
                        float greekFireTargetDist = (float)Math.Sqrt((double)(greekFireTargetX * greekFireTargetX + greekFireTargetY * greekFireTargetY));
                        greekFireTargetDist = greekFireSpeed / greekFireTargetDist;
                        greekFireTargetX *= greekFireTargetDist;
                        greekFireTargetY *= greekFireTargetDist;
                        greekFireTargetX *= 1f + (float)Main.rand.Next(-30, 31) * 0.01f;
                        greekFireTargetY *= 1f + (float)Main.rand.Next(-30, 31) * 0.01f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), greekFireSpawnPos.X, greekFireSpawnPos.Y, greekFireTargetX, greekFireTargetY, ProjectileID.GreekFire1 + Main.rand.Next(3), 60, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
            }

            if (NPC.ai[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.TargetClosest(true);
                NPC.ai[0] = 1f;

                int pumpkingBlades = NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.position.X + (float)(NPC.width / 2)), (int)NPC.position.Y + NPC.height / 2, NPCID.PumpkingBlade, NPC.whoAmI, 0f, 0f, 0f, 0f, 255);
                Main.npc[pumpkingBlades].ai[0] = -1f;
                Main.npc[pumpkingBlades].ai[1] = (float)NPC.whoAmI;
                Main.npc[pumpkingBlades].target = NPC.target;
                Main.npc[pumpkingBlades].netUpdate = true;

                pumpkingBlades = NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.position.X + (float)(NPC.width / 2)), (int)NPC.position.Y + NPC.height / 2, NPCID.PumpkingBlade, NPC.whoAmI, 0f, 0f, 0f, 0f, 255);
                Main.npc[pumpkingBlades].ai[0] = 1f;
                Main.npc[pumpkingBlades].ai[1] = (float)NPC.whoAmI;
                Main.npc[pumpkingBlades].ai[3] = 150f;
                Main.npc[pumpkingBlades].target = NPC.target;
                Main.npc[pumpkingBlades].netUpdate = true;
            }

            if (Main.player[NPC.target].dead || Math.Abs(NPC.position.X - Main.player[NPC.target].position.X) > 2000f || Math.Abs(NPC.position.Y - Main.player[NPC.target].position.Y) > 2000f)
            {
                NPC.TargetClosest(true);

                if (Main.player[NPC.target].dead || Math.Abs(NPC.position.X - Main.player[NPC.target].position.X) > 2000f || Math.Abs(NPC.position.Y - Main.player[NPC.target].position.Y) > 2000f)
                    NPC.ai[1] = 2f;
            }

            if (Main.dayTime)
            {
                NPC.velocity.Y += 0.3f;
                NPC.velocity.X *= 0.9f;
            }
            else if (NPC.ai[1] == 0f)
            {
                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= 300f)
                {
                    if (NPC.ai[3] != 1f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                    }
                    else
                    {
                        NPC.ai[2] = 0f;
                        NPC.ai[1] = 1f;
                        NPC.TargetClosest(true);
                        NPC.netUpdate = true;
                    }
                }

                Vector2 aggressivePosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float aggressiveTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - aggressivePosition.X;
                float aggressiveTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - 200f - aggressivePosition.Y;
                float aggressiveTargetDist = (float)Math.Sqrt((double)(aggressiveTargetX * aggressiveTargetX + aggressiveTargetY * aggressiveTargetY));
                float aggressiveSpeed = 8f;

                if (NPC.ai[3] == 1f)
                {
                    if (aggressiveTargetDist > 900f)
                        aggressiveSpeed = 14f;
                    else if (aggressiveTargetDist > 600f)
                        aggressiveSpeed = 12f;
                    else if (aggressiveTargetDist > 300f)
                        aggressiveSpeed = 10f;
                }

                if (aggressiveTargetDist > 50f)
                {
                    aggressiveTargetDist = aggressiveSpeed / aggressiveTargetDist;
                    NPC.velocity.X = (NPC.velocity.X * 14f + aggressiveTargetX * aggressiveTargetDist) / 15f;
                    NPC.velocity.Y = (NPC.velocity.Y * 14f + aggressiveTargetY * aggressiveTargetDist) / 15f;
                }
            }
            else if (NPC.ai[1] == 1f)
            {
                NPC.ai[2] += 1f;
                if (NPC.ai[2] >= 600f || NPC.ai[3] != 1f)
                {
                    NPC.ai[2] = 0f;
                    NPC.ai[1] = 0f;
                }

                Vector2 scytheAttackPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float scytheAttackTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - scytheAttackPosition.X;
                float scytheAttackTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - scytheAttackPosition.Y;
                float scytheAttackTargetDist = (float)Math.Sqrt((double)(scytheAttackTargetX * scytheAttackTargetX + scytheAttackTargetY * scytheAttackTargetY));
                scytheAttackTargetDist = 20f / scytheAttackTargetDist;

                NPC.velocity.X = (NPC.velocity.X * 49f + scytheAttackTargetX * scytheAttackTargetDist) / 50f;
                NPC.velocity.Y = (NPC.velocity.Y * 49f + scytheAttackTargetY * scytheAttackTargetDist) / 50f;
            }
            else if (NPC.ai[1] == 2f)
            {
                NPC.ai[1] = 3f;
                NPC.velocity.Y += 0.1f;

                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.95f;

                NPC.velocity.X *= 0.95f;

                if (NPC.timeLeft > 500)
                    NPC.timeLeft = 500;
            }
            NPC.rotation = NPC.velocity.X * -0.02f;

            return false;
        }

        public class BladeAI : VanillaAIOverride
        {
            public override bool AI(Mod mod)
            {
                NPC.spriteDirection = -(int)NPC.ai[0];

                if (!Main.npc[(int)NPC.ai[1]].active || Main.npc[(int)NPC.ai[1]].aiStyle != NPCAIStyleID.Pumpking)
                {
                    NPC.ai[2] += 10f;
                    if (NPC.ai[2] > 50f || !Main.dedServ)
                    {
                        NPC.life = -1;
                        NPC.HitEffect(0, 10.0);
                        NPC.active = false;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && Main.npc[(int)NPC.ai[1]].ai[3] == 2f)
                {
                    NPC.localAI[1] += 1f;
                    if (NPC.localAI[1] > 30f)
                    {
                        NPC.localAI[1] = 0f;

                        float scytheProjSpeed = 0.01f;
                        Vector2 scytheProjSpawn = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f + 30f);
                        float scytheProjTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - scytheProjSpawn.X;
                        float scytheProjTargetY = Main.player[NPC.target].position.Y - scytheProjSpawn.Y;
                        float scytheProjTargetDist = (float)Math.Sqrt((double)(scytheProjTargetX * scytheProjTargetX + scytheProjTargetY * scytheProjTargetY));

                        scytheProjTargetDist = scytheProjSpeed / scytheProjTargetDist;
                        scytheProjTargetX *= scytheProjTargetDist;
                        scytheProjTargetY *= scytheProjTargetDist;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, scytheProjTargetX, scytheProjTargetY, ProjectileID.FlamingScythe, 70, 0f, Main.myPlayer, NPC.rotation, (float)NPC.spriteDirection);
                    }
                }

                if (Main.dayTime)
                {
                    NPC.velocity.Y += 0.3f;
                    NPC.velocity.X *= 0.9f;
                    return false;
                }

                if (NPC.ai[2] == 0f || NPC.ai[2] == 3f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    if (Main.npc[(int)NPC.ai[1]].ai[1] == 3f && NPC.timeLeft > 10)
                        NPC.timeLeft = 10;

                    NPC.ai[3] += 1f;
                    if (NPC.ai[3] >= 180f)
                    {
                        NPC.ai[2] += 1f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }

                    Vector2 scytheSwipePosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float scytheSwipeTargetX = (Main.player[NPC.target].Center.X + Main.npc[(int)NPC.ai[1]].Center.X) / 2f;
                    float scytheSwipeTargetY = (Main.player[NPC.target].Center.Y + Main.npc[(int)NPC.ai[1]].Center.Y) / 2f;
                    scytheSwipeTargetX += -170f * NPC.ai[0] - scytheSwipePosition.X;
                    scytheSwipeTargetY += 90f - scytheSwipePosition.Y;

                    float scytheSwipeReelbackDist = Math.Abs(Main.player[NPC.target].Center.X - Main.npc[(int)NPC.ai[1]].Center.X) + Math.Abs(Main.player[NPC.target].Center.Y - Main.npc[(int)NPC.ai[1]].Center.Y);
                    if (scytheSwipeReelbackDist > 700f)
                    {
                        scytheSwipeTargetX = Main.npc[(int)NPC.ai[1]].Center.X - 170f * NPC.ai[0] - scytheSwipePosition.X;
                        scytheSwipeTargetY = Main.npc[(int)NPC.ai[1]].Center.Y + 90f - scytheSwipePosition.Y;
                    }

                    float scytheSwipeTargetDist = (float)Math.Sqrt((double)(scytheSwipeTargetX * scytheSwipeTargetX + scytheSwipeTargetY * scytheSwipeTargetY));
                    float scytheSwipeSpeed = 8f;
                    if (scytheSwipeTargetDist > 1000f)
                        scytheSwipeSpeed = 23f;
                    else if (scytheSwipeTargetDist > 800f)
                        scytheSwipeSpeed = 20f;
                    else if (scytheSwipeTargetDist > 600f)
                        scytheSwipeSpeed = 17f;
                    else if (scytheSwipeTargetDist > 400f)
                        scytheSwipeSpeed = 14f;
                    else if (scytheSwipeTargetDist > 200f)
                        scytheSwipeSpeed = 11f;

                    if (NPC.ai[0] < 0f && NPC.Center.X > Main.npc[(int)NPC.ai[1]].Center.X)
                        scytheSwipeTargetX -= 4f;
                    if (NPC.ai[0] > 0f && NPC.Center.X < Main.npc[(int)NPC.ai[1]].Center.X)
                        scytheSwipeTargetX += 4f;

                    scytheSwipeTargetDist = scytheSwipeSpeed / scytheSwipeTargetDist;
                    NPC.velocity.X = (NPC.velocity.X * 14f + scytheSwipeTargetX * scytheSwipeTargetDist) / 15f;
                    NPC.velocity.Y = (NPC.velocity.Y * 14f + scytheSwipeTargetY * scytheSwipeTargetDist) / 15f;
                    scytheSwipeTargetDist = (float)Math.Sqrt((double)(scytheSwipeTargetX * scytheSwipeTargetX + scytheSwipeTargetY * scytheSwipeTargetY));

                    if (scytheSwipeTargetDist > 20f)
                        NPC.rotation = (float)Math.Atan2((double)scytheSwipeTargetY, (double)scytheSwipeTargetX) + MathHelper.PiOver2;
                }
                else if (NPC.ai[2] == 1f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    Vector2 scytheReturnPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float scytheReturnTargetX = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - 200f * NPC.ai[0] - scytheReturnPosition.X;
                    float scytheReturnTargetY = Main.npc[(int)NPC.ai[1]].position.Y + 230f - scytheReturnPosition.Y;
                    float scytheReturnTargetDist = (float)Math.Sqrt((double)(scytheReturnTargetX * scytheReturnTargetX + scytheReturnTargetY * scytheReturnTargetY));

                    NPC.rotation = (float)Math.Atan2((double)scytheReturnTargetY, (double)scytheReturnTargetX) + MathHelper.PiOver2;
                    NPC.velocity.X *= 0.95f;
                    NPC.velocity.Y -= 0.3f;

                    if (NPC.velocity.Y < -18f)
                        NPC.velocity.Y = -18f;

                    if (NPC.position.Y < Main.npc[(int)NPC.ai[1]].position.Y - 200f)
                    {
                        // Set damage
                        NPC.damage = NPC.defDamage;

                        NPC.TargetClosest(true);
                        NPC.ai[2] = 2f;

                        scytheReturnPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                        scytheReturnTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - scytheReturnPosition.X;
                        scytheReturnTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - scytheReturnPosition.Y;
                        scytheReturnTargetDist = (float)Math.Sqrt((double)(scytheReturnTargetX * scytheReturnTargetX + scytheReturnTargetY * scytheReturnTargetY));
                        scytheReturnTargetDist = 24f / scytheReturnTargetDist;

                        NPC.velocity.X = scytheReturnTargetX * scytheReturnTargetDist;
                        NPC.velocity.Y = scytheReturnTargetY * scytheReturnTargetDist;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.ai[2] == 2f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = NPC.defDamage;

                    float scytheReturnDestination = Math.Abs(NPC.Center.X - Main.npc[(int)NPC.ai[1]].Center.X) + Math.Abs(NPC.Center.Y - Main.npc[(int)NPC.ai[1]].Center.Y);

                    if (NPC.position.Y > Main.player[NPC.target].position.Y || NPC.velocity.Y < 0f || scytheReturnDestination > 800f)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        NPC.ai[2] = 3f;
                    }
                }
                else if (NPC.ai[2] == 4f)
                {
                    // Avoid cheap bullshit
                    NPC.damage = 0;

                    Vector2 scytheLesserSwipePos = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float scytheLesserSwipeTargetX = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - 200f * NPC.ai[0] - scytheLesserSwipePos.X;
                    float scytheLesserSwipeTargetY = Main.npc[(int)NPC.ai[1]].position.Y + 230f - scytheLesserSwipePos.Y;
                    float scytheLesserSwipeTargetDist = (float)Math.Sqrt((double)(scytheLesserSwipeTargetX * scytheLesserSwipeTargetX + scytheLesserSwipeTargetY * scytheLesserSwipeTargetY));

                    NPC.rotation = (float)Math.Atan2((double)scytheLesserSwipeTargetY, (double)scytheLesserSwipeTargetX) + MathHelper.PiOver2;
                    NPC.velocity.Y *= 0.95f;
                    NPC.velocity.X += 0.3f * -NPC.ai[0];

                    if (NPC.velocity.X < -18f)
                        NPC.velocity.X = -18f;
                    if (NPC.velocity.X > 18f)
                        NPC.velocity.X = 18f;

                    if (NPC.position.X + (float)(NPC.width / 2) < Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) - 500f || NPC.position.X + (float)(NPC.width / 2) > Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2) + 500f)
                    {
                        // Set damage
                        NPC.damage = NPC.defDamage;

                        NPC.TargetClosest(true);
                        NPC.ai[2] = 5f;

                        scytheLesserSwipePos = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                        scytheLesserSwipeTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - scytheLesserSwipePos.X;
                        scytheLesserSwipeTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - scytheLesserSwipePos.Y;
                        scytheLesserSwipeTargetDist = (float)Math.Sqrt((double)(scytheLesserSwipeTargetX * scytheLesserSwipeTargetX + scytheLesserSwipeTargetY * scytheLesserSwipeTargetY));
                        scytheLesserSwipeTargetDist = 17f / scytheLesserSwipeTargetDist;

                        NPC.velocity.X = scytheLesserSwipeTargetX * scytheLesserSwipeTargetDist;
                        NPC.velocity.Y = scytheLesserSwipeTargetY * scytheLesserSwipeTargetDist;
                        NPC.netUpdate = true;
                    }
                }
                else if (NPC.ai[2] == 5f)
                {
                    // Set damage
                    NPC.damage = NPC.defDamage;

                    float scytheLesserSwipeReturnDest = Math.Abs(NPC.Center.X - Main.npc[(int)NPC.ai[1]].Center.X) + Math.Abs(NPC.Center.Y - Main.npc[(int)NPC.ai[1]].Center.Y);

                    if ((NPC.velocity.X > 0f && NPC.position.X + (float)(NPC.width / 2) > Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)) || (NPC.velocity.X < 0f && NPC.position.X + (float)(NPC.width / 2) < Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)) || scytheLesserSwipeReturnDest > 800f)
                    {
                        // Avoid cheap bullshit
                        NPC.damage = 0;

                        NPC.ai[2] = 0f;
                    }
                }

                return false;
            }
        }
    }
}
