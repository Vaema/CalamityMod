using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class AncientVisionAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.alpha > 0)
            {
                NPC.alpha -= 30;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;
            }

            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.whoAmI == NPC.whoAmI || n.type != NPC.type)
                    continue;

                Vector2 targetDirection = n.Center - NPC.Center;
                if (!(targetDirection.Length() < 50f))
                    continue;

                targetDirection.Normalize();
                if (targetDirection.X == 0f && targetDirection.Y == 0f)
                {
                    if (n.whoAmI > NPC.whoAmI)
                        targetDirection.X = 1f;
                    else
                        targetDirection.X = -1f;
                }

                targetDirection *= 0.4f;
                NPC.velocity -= targetDirection;
                n.velocity += targetDirection;
            }

            if (NPC.type == NPCID.ShadowFlameApparition)
            {
                if (NPC.localAI[0] < 120f)
                {
                    if (NPC.localAI[0] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.TargetClosest();
                        if (NPC.direction > 0)
                            NPC.velocity.X += 2f;
                        else
                            NPC.velocity.X -= 2f;

                        NPC.position += NPC.netOffset;
                        for (int j = 0; j < 20; j++)
                        {
                            Vector2 apparitionCenter = NPC.Center;
                            apparitionCenter.Y -= 18f;
                            Vector2 apparitionRandVelocity = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                            apparitionRandVelocity.Normalize();
                            apparitionRandVelocity *= Main.rand.Next(0, 100) * 0.1f;
                            apparitionCenter += apparitionRandVelocity;
                            apparitionRandVelocity.Normalize();
                            apparitionRandVelocity *= Main.rand.Next(50, 90) * 0.2f;
                            int shadowflameDust = Dust.NewDust(apparitionCenter, 1, 1, DustID.Shadowflame);
                            Main.dust[shadowflameDust].velocity = -apparitionRandVelocity * 0.3f;
                            Main.dust[shadowflameDust].alpha = 100;
                            if (Main.rand.NextBool())
                            {
                                Main.dust[shadowflameDust].noGravity = true;
                                Dust dust = Main.dust[shadowflameDust];
                                dust.scale += 0.3f;
                            }
                        }

                        NPC.position -= NPC.netOffset;
                    }

                    NPC.localAI[0] += 1f;
                    float localAIDustControl = 1f - NPC.localAI[0] / 120f;
                    float dustAmt = localAIDustControl * 20f;
                    for (int k = 0; k < dustAmt; k++)
                    {
                        if (Main.rand.NextBool(5))
                        {
                            NPC.position += NPC.netOffset;
                            int idleShadowflameDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame);
                            Main.dust[idleShadowflameDust].alpha = 100;
                            Dust dust = Main.dust[idleShadowflameDust];
                            dust.velocity *= 0.3f;
                            dust = Main.dust[idleShadowflameDust];
                            dust.velocity += NPC.velocity * 0.75f;
                            Main.dust[idleShadowflameDust].noGravity = true;
                            NPC.position -= NPC.netOffset;
                        }
                    }
                }
            }

            if (NPC.type == NPCID.AncientCultistSquidhead)
            {
                if (NPC.localAI[0] < 120f)
                {
                    if (NPC.localAI[0] == 0f)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.TargetClosest();
                        if (NPC.direction > 0)
                            NPC.velocity.X += 2f;
                        else
                            NPC.velocity.X -= 2f;
                    }

                    NPC.localAI[0] += 1f;
                    int dustPosition = 10;
                    for (int l = 0; l < 2; l++)
                    {
                        NPC.position += NPC.netOffset;
                        int visionDust = Dust.NewDust(NPC.position - new Vector2(dustPosition), NPC.width + dustPosition * 2, NPC.height + dustPosition * 2, DustID.GoldFlame, 0f, 0f, 100, default(Color), 2f);
                        Main.dust[visionDust].noGravity = true;
                        Main.dust[visionDust].noLight = true;
                        NPC.position -= NPC.netOffset;
                    }
                }
            }

            if (NPC.ai[0] == 0f)
            {
                NPC.TargetClosest();
                NPC.ai[0] = 1f;
                NPC.ai[1] = NPC.direction;
            }
            else if (NPC.ai[0] == 1f)
            {
                NPC.TargetClosest();
                float xVelocityMult1 = 0.5f;
                float maxXVelocity1 = 10f;
                float maxYVelocity1 = 4f;
                float turnAroundDist1 = 550f;
                float yVelocityMult1 = 3f;
                if (NPC.type == NPCID.AncientCultistSquidhead)
                {
                    xVelocityMult1 = 0.8f;
                    maxXVelocity1 = 16f;
                    turnAroundDist1 = 440f;
                    maxYVelocity1 = 6f;
                }
                if (CalamityWorld.death)
                {
                    xVelocityMult1 *= 1.25f;
                    maxXVelocity1 *= 1.25f;
                    turnAroundDist1 *= 0.9f;
                    yVelocityMult1 -= 1f;
                }

                NPC.velocity.X += NPC.ai[1] * xVelocityMult1;
                if (NPC.velocity.X > maxXVelocity1)
                    NPC.velocity.X = maxXVelocity1;

                if (NPC.velocity.X < 0f - maxXVelocity1)
                    NPC.velocity.X = 0f - maxXVelocity1;

                float targetYDist1 = Main.player[NPC.target].Center.Y - NPC.Center.Y;
                if (Math.Abs(targetYDist1) > maxYVelocity1)
                    yVelocityMult1 = CalamityWorld.death ? 10f : 12f;

                if (targetYDist1 > maxYVelocity1)
                    targetYDist1 = maxYVelocity1;
                else if (targetYDist1 < 0f - maxYVelocity1)
                    targetYDist1 = 0f - maxYVelocity1;

                NPC.velocity.Y = (NPC.velocity.Y * (yVelocityMult1 - 1f) + targetYDist1) / yVelocityMult1;
                if ((NPC.ai[1] > 0f && Main.player[NPC.target].Center.X - NPC.Center.X < 0f - turnAroundDist1) || (NPC.ai[1] < 0f && Main.player[NPC.target].Center.X - NPC.Center.X > turnAroundDist1))
                {
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    if (NPC.Center.Y + 20f > Main.player[NPC.target].Center.Y)
                        NPC.ai[1] = -1f;
                    else
                        NPC.ai[1] = 1f;
                }
            }
            else if (NPC.ai[0] == 2f)
            {
                float decelYVelocityMult = 0.6f;
                float deceleration = 0.93f;
                float decelerationDist = 7f;
                if (NPC.type == NPCID.AncientCultistSquidhead)
                {
                    decelYVelocityMult = 0.45f;
                    decelerationDist = 10f;
                    deceleration = 0.87f;
                }
                if (CalamityWorld.death)
                {
                    decelYVelocityMult *= 1.25f;
                    deceleration *= 0.9f;
                    decelerationDist *= 1.25f;
                }

                NPC.velocity.Y += NPC.ai[1] * decelYVelocityMult;
                if (NPC.velocity.Length() > decelerationDist)
                    NPC.velocity *= deceleration;

                if (NPC.velocity.X > -1f && NPC.velocity.X < 1f)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 3f;
                    NPC.ai[1] = NPC.direction;
                }
            }
            else if (NPC.ai[0] == 3f)
            {
                float xVelocityMult3 = 0.6f;
                float yAlignSpeed = 0.3f;
                float decelerationDist3 = 7f;
                float deceleration3 = 0.93f;
                if (NPC.type == NPCID.AncientCultistSquidhead)
                {
                    xVelocityMult3 = 0.8f;
                    yAlignSpeed = 0.45f;
                    decelerationDist3 = 9f;
                    deceleration3 = 0.87f;
                }
                if (CalamityWorld.death)
                {
                    xVelocityMult3 *= 1.25f;
                    yAlignSpeed *= 1.25f;
                    decelerationDist3 *= 1.25f;
                    deceleration3 *= 0.9f;
                }

                NPC.velocity.X += NPC.ai[1] * xVelocityMult3;
                if (NPC.Center.Y > Main.player[NPC.target].Center.Y)
                    NPC.velocity.Y -= yAlignSpeed;
                else
                    NPC.velocity.Y += yAlignSpeed;

                if (NPC.velocity.Length() > decelerationDist3)
                    NPC.velocity *= deceleration3;

                if (NPC.velocity.Y > -1f && NPC.velocity.Y < 1f)
                {
                    NPC.TargetClosest();
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = NPC.direction;
                }
            }

            if (NPC.type == NPCID.AncientCultistSquidhead)
            {
                int squidDustVelocity = 10;
                NPC.position += NPC.netOffset;

                int squidDust = Dust.NewDust(NPC.position - new Vector2(squidDustVelocity), NPC.width + squidDustVelocity * 2, NPC.height + squidDustVelocity * 2, DustID.GoldFlame, 0f, 0f, 100, default(Color), 2f);
                Main.dust[squidDust].noGravity = true;
                Main.dust[squidDust].noLight = true;

                NPC.position -= NPC.netOffset;
            }

            return false;
        }
    }
}
