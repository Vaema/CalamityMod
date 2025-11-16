using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class SpiderAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            //Find a target
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }

            float speed = 2.5f;
            float mvtAdjust = 0.1f;
            if (NPC.type == NPCID.DesertScorpionWall)
            {
                speed = 5f;
                mvtAdjust = 0.2f;
            }
            if (CalamityWorld.death)
            {
                speed *= 1.25f;
                mvtAdjust *= 1.25f;
            }

            //Calculate how to reach the target
            Vector2 npcPos = NPC.Center;
            Vector2 targetPos = Main.player[NPC.target].Center;
            targetPos.X = (float)((int)(targetPos.X / 8f) * 8);
            targetPos.Y = (float)((int)(targetPos.Y / 8f) * 8);
            npcPos.X = (float)((int)(npcPos.X / 8f) * 8);
            npcPos.Y = (float)((int)(npcPos.Y / 8f) * 8);
            targetPos.X -= npcPos.X;
            targetPos.Y -= npcPos.Y;
            float targetDist = targetPos.Length();
            if (targetDist == 0f)
            {
                targetPos.X = NPC.velocity.X;
                targetPos.Y = NPC.velocity.Y;
            }
            else
            {
                targetDist = speed / targetDist;
                targetPos.X *= targetDist;
                targetPos.Y *= targetDist;
            }

            //If the target is dead, nobody cares
            if (Main.player[NPC.target].dead)
            {
                targetPos.X = (float)NPC.direction * speed / 2f;
                targetPos.Y = -speed / 2f;
            }

            //Sprite direction
            NPC.spriteDirection = -1;

            //If you can't see the target, wander around
            if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
            {
                NPC.ai[0] += 1f;
                if (NPC.ai[0] > 0f)
                {
                    NPC.velocity.Y += 0.023f;
                }
                else
                {
                    NPC.velocity.Y -= 0.023f;
                }
                if (NPC.ai[0] < -100f || NPC.ai[0] > 100f)
                {
                    NPC.velocity.X += 0.023f;
                }
                else
                {
                    NPC.velocity.X -= 0.023f;
                }
                if (NPC.ai[0] > 200f)
                {
                    NPC.ai[0] = -200f;
                }
                NPC.velocity.X += targetPos.X * 0.009f;
                NPC.velocity.Y += targetPos.Y * 0.009f;
                NPC.rotation = NPC.velocity.ToRotation();
                if (NPC.velocity.X > 2.5f)
                {
                    NPC.velocity.X *= 0.9f;
                }
                if (NPC.velocity.X < -2.5f)
                {
                    NPC.velocity.X *= 0.9f;
                }
                if (NPC.velocity.Y > 2.5f)
                {
                    NPC.velocity.Y *= 0.9f;
                }
                if (NPC.velocity.Y < -2.5f)
                {
                    NPC.velocity.Y *= 0.9f;
                }
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -4f, 4f);
                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4f, 4f);
            }
            //If target is in sight, move toward target
            else
            {
                if (NPC.velocity.X < targetPos.X)
                {
                    NPC.velocity.X += mvtAdjust;
                    if (NPC.velocity.X < 0f && targetPos.X > 0f)
                    {
                        NPC.velocity.X += mvtAdjust;
                    }
                }
                else if (NPC.velocity.X > targetPos.X)
                {
                    NPC.velocity.X -= mvtAdjust;
                    if (NPC.velocity.X > 0f && targetPos.X < 0f)
                    {
                        NPC.velocity.X -= mvtAdjust;
                    }
                }
                if (NPC.velocity.Y < targetPos.Y)
                {
                    NPC.velocity.Y += mvtAdjust;
                    if (NPC.velocity.Y < 0f && targetPos.Y > 0f)
                    {
                        NPC.velocity.Y += mvtAdjust;
                    }
                }
                else if (NPC.velocity.Y > targetPos.Y)
                {
                    NPC.velocity.Y -= mvtAdjust;
                    if (NPC.velocity.Y > 0f && targetPos.Y < 0f)
                    {
                        NPC.velocity.Y -= mvtAdjust;
                    }
                }
                NPC.rotation = targetPos.ToRotation();
            }

            //Desert Scorpion has a different sprite orientation
            if (NPC.type == NPCID.DesertScorpionWall)
            {
                NPC.rotation += MathHelper.PiOver2;
            }

            //Wall collision behavior?
            float half = 0.5f;
            if (NPC.collideX)
            {
                NPC.netUpdate = true;
                NPC.velocity.X = NPC.oldVelocity.X * -half;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                {
                    NPC.velocity.X = 2f;
                }
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                {
                    NPC.velocity.X = -2f;
                }
            }
            if (NPC.collideY)
            {
                NPC.netUpdate = true;
                NPC.velocity.Y = NPC.oldVelocity.Y * -half;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1.5f)
                {
                    NPC.velocity.Y = 2f;
                }
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1.5f)
                {
                    NPC.velocity.Y = -2f;
                }
            }

            // Net update for changing directions
            if (((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f) || (NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f) || (NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f) || (NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f)) && !NPC.justHit)
            {
                NPC.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                bool prehardmodeSpiders = (NPC.type == NPCID.WallCreeper || NPC.type == NPCID.WallCreeperWall || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.BloodCrawlerWall) && CalamityWorld.revenge;
                if (NPC.target >= 0 && Main.expertMode && (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.BlackRecluseWall || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.JungleCreeperWall || prehardmodeSpiders) &&
                    Collision.CanHit(NPC.Center, 1, 1, Main.player[NPC.target].Center, 1, 1))
                {
                    NPC.localAI[0] += 1f;
                    if (NPC.justHit)
                        NPC.localAI[0] = 0f;

                    float webSpitGateValue = CalamityWorld.death ? SpiderWebSpitGateValue_Death : CalamityWorld.revenge ? SpiderWebSpitGateValue_Rev : SpiderWebSpitGateValue;

                    // Emit web dust from mouth when about to fire
                    if (NPC.localAI[0] > webSpitGateValue - SpiderWebSpitTelegraphTime)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.Center, 1, 1, DustID.Web, 0f, 0f, 100, default, 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0f;
                    }

                    if (NPC.localAI[0] >= webSpitGateValue)
                    {
                        NPC.localAI[0] = 0f;
                        Vector2 velocity = Main.player[NPC.target].Center - NPC.Center;
                        velocity.Normalize();
                        velocity *= prehardmodeSpiders ? 5f : 8f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ProjectileID.WebSpit, 18, 0f, Main.myPlayer);
                    }
                }
                else
                    NPC.localAI[0] = 0f;

                // Check for walls
                int npcX = (int)NPC.Center.X / 16;
                int npcY = (int)NPC.Center.Y / 16;
                bool climbingWall = false;
                for (int i = npcX - 1; i <= npcX + 1; i++)
                {
                    for (int j = npcY - 1; j <= npcY + 1; j++)
                    {
                        if (Main.tile[i, j].WallType > 0)
                        {
                            climbingWall = true;
                        }
                    }
                }
                //If not on a wall, transform to fighter form
                if (!climbingWall)
                {
                    if (NPC.type == NPCID.JungleCreeperWall)
                    {
                        NPC.Transform(NPCID.JungleCreeper);
                        return false;
                    }
                    if (NPC.type == NPCID.BlackRecluseWall)
                    {
                        NPC.Transform(NPCID.BlackRecluse);
                        return false;
                    }
                    if (NPC.type == NPCID.BloodCrawlerWall)
                    {
                        NPC.Transform(NPCID.BloodCrawler);
                        return false;
                    }
                    if (NPC.type == NPCID.DesertScorpionWall)
                    {
                        NPC.Transform(NPCID.DesertScorpionWalk);
                        return false;
                    }
                    NPC.Transform(NPCID.WallCreeper);
                }
            }
            return false;
        }
    }
}
