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
    public class HoveringAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            bool hoverDownDistCheck = false;
            bool runAway = NPC.type == NPCID.Poltergeist && !Main.pumpkinMoon;

            if (NPC.type == NPCID.Reaper && !Main.eclipse)
            {
                runAway = true;
            }
            if (NPC.type == NPCID.Drippler && Main.dayTime)
            {
                runAway = true;
            }

            if (!runAway)
            {
                if (NPC.ai[2] >= 0f)
                {
                    int hoverDistance = 16;
                    bool changeDirectionX = false;
                    bool changeDirectionY = false;
                    if (NPC.position.X > NPC.ai[0] - (float)hoverDistance && NPC.position.X < NPC.ai[0] + (float)hoverDistance)
                    {
                        changeDirectionX = true;
                    }
                    else if ((NPC.velocity.X < 0f && NPC.direction > 0) || (NPC.velocity.X > 0f && NPC.direction < 0))
                    {
                        changeDirectionX = true;
                    }
                    hoverDistance += 24;
                    if (NPC.position.Y > NPC.ai[1] - (float)hoverDistance && NPC.position.Y < NPC.ai[1] + (float)hoverDistance)
                    {
                        changeDirectionY = true;
                    }
                    if (changeDirectionX & changeDirectionY)
                    {
                        NPC.ai[2] += 1f;
                        if (NPC.ai[2] >= 40f)
                        {
                            NPC.ai[2] = -200f;
                            NPC.direction *= -1;
                            NPC.velocity.X = NPC.velocity.X * -1f;
                            NPC.collideX = false;
                        }
                    }
                    else
                    {
                        NPC.ai[0] = NPC.position.X;
                        NPC.ai[1] = NPC.position.Y;
                        NPC.ai[2] = 0f;
                    }
                    NPC.TargetClosest(true);
                }
                else if (NPC.type == NPCID.Reaper)
                {
                    NPC.TargetClosest(true);
                    NPC.ai[2] += 30f;
                }
                else
                {
                    if (NPC.type == NPCID.Poltergeist)
                    {
                        NPC.ai[2] += 5f;
                    }
                    else
                    {
                        NPC.ai[2] += 15f;
                    }
                    if (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) > NPC.position.X + (float)(NPC.width / 2))
                    {
                        NPC.direction = -1;
                    }
                    else
                    {
                        NPC.direction = 1;
                    }
                }
            }

            int npcTileX = (int)((NPC.position.X + (float)(NPC.width / 2)) / 16f) + NPC.direction * 2;
            int npcTileY = (int)((NPC.position.Y + (float)NPC.height) / 16f);
            bool hoverDownwards = true;
            bool canOpenDoor = false;
            int tileCheckLoopAmt = 6;

            if (NPC.type == NPCID.Gastropod)
            {
                float gastropodProjSpeed = 6f;
                Vector2 gastropodPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float gastropodTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - gastropodPosition.X;
                float gastropodTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - gastropodPosition.Y;
                float gastropodTargetDist = (float)Math.Sqrt((double)(gastropodTargetX * gastropodTargetX + gastropodTargetY * gastropodTargetY));

                gastropodTargetDist = gastropodProjSpeed / gastropodTargetDist;
                gastropodTargetX *= gastropodTargetDist;
                gastropodTargetY *= gastropodTargetDist;

                if (NPC.justHit)
                {
                    NPC.localAI[1] = 0f;
                    NPC.ai[3] = 0f;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[3] == 32f && !Main.player[NPC.target].npcTypeNoAggro[NPC.type])
                {
                    int damage = 25;
                    int projType = ProjectileID.PinkLaser;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), gastropodPosition.X, gastropodPosition.Y, gastropodTargetX, gastropodTargetY, projType, damage, 0f, Main.myPlayer, 0f, 0f);
                }

                tileCheckLoopAmt = 12;

                if (NPC.ai[3] > 0f)
                {
                    NPC.ai[3] += 1f;
                    if (NPC.ai[3] >= 64f)
                    {
                        NPC.ai[3] = 0f;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[3] == 0f)
                {
                    NPC.localAI[1] += 1f;
                    if (NPC.localAI[1] > (CalamityWorld.death ? 60f : 120f) && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) && !Main.player[NPC.target].npcTypeNoAggro[NPC.type])
                    {
                        NPC.localAI[1] = 0f;
                        NPC.ai[3] = 1f;
                        NPC.netUpdate = true;
                    }
                }
            }
            else if (NPC.type == NPCID.Pixie)
            {
                tileCheckLoopAmt = 8;

                if (Main.rand.NextBool(6))
                {
                    int pixieDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Pixie, 0f, 0f, 200, NPC.color, 1f);
                    Dust dust = Main.dust[pixieDust];
                    dust.velocity *= 0.3f;
                }

                if (Main.rand.NextBool(40))
                {
                    SoundEngine.PlaySound(SoundID.Pixie, NPC.Center);
                }
            }
            else if (NPC.type == NPCID.IceElemental)
            {
                Lighting.AddLight((int)((NPC.position.X + (float)(NPC.width / 2)) / 16f), (int)((NPC.position.Y + (float)(NPC.height / 2)) / 16f), 0f, 0.6f, 0.75f);

                NPC.alpha = 30;

                if (Main.rand.NextBool(3))
                {
                    int iceEleDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Frost, 0f, 0f, 200, default, 1f);
                    Dust dust = Main.dust[iceEleDust];
                    dust.velocity *= 0.3f;
                    Main.dust[iceEleDust].noGravity = true;
                }

                float iceElementalProjSpeed = 6f;
                Vector2 iceElementalPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float iceElementalTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - iceElementalPosition.X;
                float iceElementalTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - iceElementalPosition.Y;
                float iceElementalTargetDist = (float)Math.Sqrt((double)(iceElementalTargetX * iceElementalTargetX + iceElementalTargetY * iceElementalTargetY));

                iceElementalTargetDist = iceElementalProjSpeed / iceElementalTargetDist;
                iceElementalTargetX *= iceElementalTargetDist;
                iceElementalTargetY *= iceElementalTargetDist;

                if (iceElementalTargetX > 0f)
                    NPC.direction = 1;
                else
                    NPC.direction = -1;

                NPC.spriteDirection = NPC.direction;

                if (NPC.direction < 0)
                    NPC.rotation = (float)Math.Atan2((double)(-(double)iceElementalTargetY), (double)(-(double)iceElementalTargetX));
                else
                    NPC.rotation = (float)Math.Atan2((double)iceElementalTargetY, (double)iceElementalTargetX);

                if (NPC.justHit || !Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    NPC.localAI[1] = 0f;

                tileCheckLoopAmt = 15;

                // Emit frost dust when about to fire
                if (NPC.localAI[1] > (CalamityWorld.death ? IceElementalFrostBlastGateValue_Death : IceElementalFrostBlastGateValue) - IceElementalFrostBlastTelegraphTime)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Frost, 0f, 0f, 200, default, 2f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.localAI[1] += 1f;
                    if (NPC.localAI[1] > (CalamityWorld.death ? IceElementalFrostBlastGateValue_Death : IceElementalFrostBlastGateValue) && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    {
                        NPC.localAI[1] = 0f;

                        int dmg = 45;
                        int projType = ProjectileID.FrostBlastHostile;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), iceElementalPosition.X, iceElementalPosition.Y, iceElementalTargetX, iceElementalTargetY, projType, dmg, 0f, Main.myPlayer);

                        NPC.netUpdate = true;
                    }
                }
            }
            else if (NPC.type == NPCID.IchorSticker)
            {
                NPC.rotation = NPC.velocity.X * 0.1f;

                if (Main.player[NPC.target].Center.Y < NPC.Center.Y)
                    tileCheckLoopAmt = 18;
                else
                    tileCheckLoopAmt = 9;

                if (NPC.justHit)
                    NPC.ai[3] = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.confused)
                {
                    NPC.ai[3] += 1f;
                    if (NPC.ai[3] >= (CalamityWorld.death ? IchorStickerShootGateValue_Death : CalamityWorld.revenge ? IchorStickerShootGateValue_Rev : IchorStickerShootGateValue))
                    {
                        NPC.ai[3] = 0f;
                        Vector2 ichorStickerPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f - 4f, NPC.position.Y + (float)NPC.height * 0.7f);
                        if (Collision.CanHit(ichorStickerPosition, 1, 1, Main.player[NPC.target].Center, 1, 1))
                        {
                            float ichorStickerProjSpeed = CalamityWorld.death ? 6f : CalamityWorld.revenge ? 5f : 4f;
                            float ichorStickerTargetX = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) - ichorStickerPosition.X;
                            float ichorStickerAbsTargetX = Math.Abs(ichorStickerTargetX) * 0.1f;
                            float ichorStickerTargetY = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2) - ichorStickerPosition.Y - ichorStickerAbsTargetX;
                            float ichorStickerTargetDist = (float)Math.Sqrt((double)(ichorStickerTargetX * ichorStickerTargetX + ichorStickerTargetY * ichorStickerTargetY));
                            ichorStickerTargetDist = ichorStickerProjSpeed / ichorStickerTargetDist;
                            ichorStickerTargetX *= ichorStickerTargetDist;
                            ichorStickerTargetY *= ichorStickerTargetDist;
                            int dmg = 40;
                            int projType = ProjectileID.GoldenShowerHostile;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), ichorStickerPosition.X, ichorStickerPosition.Y, ichorStickerTargetX, ichorStickerTargetY, projType, dmg, 0f, Main.myPlayer);
                        }
                    }
                }
            }

            if (NPC.type == NPCID.Drippler)
            {
                tileCheckLoopAmt = 8;

                if (NPC.target >= 0)
                {
                    float dripperTargetDist = (Main.player[NPC.target].Center - NPC.Center).Length();
                    dripperTargetDist /= 70f;
                    if (dripperTargetDist > 8f)
                    {
                        dripperTargetDist = 8f;
                    }
                    tileCheckLoopAmt += (int)dripperTargetDist;
                }
            }

            for (int y = npcTileY; y < npcTileY + tileCheckLoopAmt; y++)
            {
                if ((Main.tile[npcTileX, y].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, y].TileType]) || Main.tile[npcTileX, y].LiquidAmount > 0)
                {
                    if (y <= npcTileY + 1)
                    {
                        canOpenDoor = true;
                    }
                    hoverDownwards = false;
                    break;
                }
            }

            if (Main.player[NPC.target].npcTypeNoAggro[NPC.type])
            {
                bool canOpenTallGate = false;
                for (int yInc = npcTileY; yInc < npcTileY + tileCheckLoopAmt - 2; yInc++)
                {
                    if ((Main.tile[npcTileX, yInc].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, yInc].TileType]) || Main.tile[npcTileX, yInc].LiquidAmount > 0)
                    {
                        canOpenTallGate = true;
                        break;
                    }
                }
                NPC.directionY = (!canOpenTallGate).ToDirectionInt();
            }

            if (NPC.type == NPCID.IceElemental || NPC.type == NPCID.IchorSticker)
            {
                for (int iceIchorY = npcTileY - 3; iceIchorY < npcTileY; iceIchorY++)
                {
                    if ((Main.tile[npcTileX, iceIchorY].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, iceIchorY].TileType]) || Main.tile[npcTileX, iceIchorY].LiquidAmount > 0)
                    {
                        canOpenDoor = false;
                        hoverDownDistCheck = true;
                        break;
                    }
                }
            }

            if (hoverDownDistCheck)
            {
                hoverDownwards = true;
                if (NPC.type == NPCID.IchorSticker)
                    NPC.velocity.Y += (CalamityWorld.revenge ? 3f : 2f);
            }

            if (hoverDownwards)
            {
                if (NPC.type == NPCID.Pixie || NPC.type == NPCID.IceElemental)
                {
                    NPC.velocity.Y += CalamityWorld.revenge ? 0.3f : 0.2f;
                    if (NPC.velocity.Y > (CalamityWorld.revenge ? 3f : 2f))
                        NPC.velocity.Y = CalamityWorld.revenge ? 3f : 2f;
                }
                else if (NPC.type == NPCID.Drippler)
                {
                    NPC.velocity.Y += 0.05f;
                    if (NPC.velocity.Y > 1f)
                        NPC.velocity.Y = 1f;
                }
                else
                {
                    NPC.velocity.Y += 0.15f;
                    if (NPC.velocity.Y > 4f)
                        NPC.velocity.Y = 4f;
                }
            }
            else
            {
                if (NPC.type == NPCID.Pixie || NPC.type == NPCID.IceElemental)
                {
                    if ((NPC.directionY < 0 && NPC.velocity.Y > 0f) | canOpenDoor)
                        NPC.velocity.Y -= CalamityWorld.revenge ? 0.3f : 0.2f;
                }
                else if (NPC.type == NPCID.Drippler)
                {
                    if ((NPC.directionY < 0 && NPC.velocity.Y > 0f) | canOpenDoor)
                        NPC.velocity.Y -= 0.1f;
                    if (NPC.velocity.Y < -1f)
                        NPC.velocity.Y = -1f;
                }
                else if (NPC.directionY < 0 && NPC.velocity.Y > 0f)
                    NPC.velocity.Y -= 0.15f;

                if (NPC.velocity.Y < -(CalamityWorld.revenge ? 5.5f : 4f))
                    NPC.velocity.Y = -(CalamityWorld.revenge ? 5.5f : 4f);
            }

            if (NPC.type == NPCID.Pixie && NPC.wet)
            {
                NPC.velocity.Y = NPC.velocity.Y - 0.3f;
                if (NPC.velocity.Y < -3f)
                {
                    NPC.velocity.Y = -3f;
                }
            }

            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.oldVelocity.X * -0.4f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 1f)
                {
                    NPC.velocity.X = 1f;
                }
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -1f)
                {
                    NPC.velocity.X = -1f;
                }
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.25f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
                {
                    NPC.velocity.Y = 1f;
                }
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
                {
                    NPC.velocity.Y = -1f;
                }
            }

            float maxHoverVel = 2f;
            if (NPC.type == NPCID.Pixie)
            {
                maxHoverVel = 3f;
            }
            if (NPC.type == NPCID.Reaper)
            {
                maxHoverVel = 4f;
            }
            if (NPC.type == NPCID.Drippler)
            {
                maxHoverVel = 1.5f;
            }
            if (CalamityWorld.death)
            {
                maxHoverVel *= 1.25f;
            }

            if (NPC.type == NPCID.Poltergeist)
            {
                NPC.alpha = 0;
                maxHoverVel = 6f;
                if (!runAway)
                {
                    NPC.TargetClosest();
                }
                else if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                }
                if (NPC.direction < 0 && NPC.velocity.X > 0f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.8f;
                }
                if (NPC.direction > 0 && NPC.velocity.X < 0f)
                {
                    NPC.velocity.X = NPC.velocity.X * 0.8f;
                }
            }

            if (NPC.direction == -1 && NPC.velocity.X > -maxHoverVel)
            {
                NPC.velocity.X -= CalamityWorld.revenge ? 0.15f : 0.1f;
                if (NPC.velocity.X > maxHoverVel)
                    NPC.velocity.X -= CalamityWorld.revenge ? 0.15f : 0.1f;
                else if (NPC.velocity.X > 0f)
                    NPC.velocity.X += CalamityWorld.revenge ? 0.1f : 0.05f;

                if (NPC.velocity.X < -maxHoverVel)
                    NPC.velocity.X = -maxHoverVel;
            }
            else if (NPC.direction == 1 && NPC.velocity.X < maxHoverVel)
            {
                NPC.velocity.X += CalamityWorld.revenge ? 0.15f : 0.1f;
                if (NPC.velocity.X < -maxHoverVel)
                    NPC.velocity.X += CalamityWorld.revenge ? 0.15f : 0.1f;
                else if (NPC.velocity.X < 0f)
                    NPC.velocity.X -= CalamityWorld.revenge ? 0.1f : 0.05f;

                if (NPC.velocity.X > maxHoverVel)
                    NPC.velocity.X = maxHoverVel;
            }

            if (NPC.type == NPCID.Drippler)
                maxHoverVel = 1.5f;
            else
                maxHoverVel = CalamityWorld.revenge ? 2.5f : 1.5f;

            if (CalamityWorld.death)
                maxHoverVel *= 1.25f;

            if (NPC.directionY == -1 && NPC.velocity.Y > -maxHoverVel)
            {
                NPC.velocity.Y -= CalamityWorld.revenge ? 0.06f : 0.04f;
                if (NPC.velocity.Y > maxHoverVel)
                    NPC.velocity.Y -= CalamityWorld.revenge ? 0.1f : 0.05f;
                else if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y += CalamityWorld.revenge ? 0.05f : 0.03f;

                if (NPC.velocity.Y < -maxHoverVel)
                    NPC.velocity.Y = -maxHoverVel;
            }
            else if (NPC.directionY == 1 && NPC.velocity.Y < maxHoverVel)
            {
                NPC.velocity.Y += CalamityWorld.revenge ? 0.06f : 0.04f;
                if (NPC.velocity.Y < -maxHoverVel)
                    NPC.velocity.Y += CalamityWorld.revenge ? 0.1f : 0.05f;
                else if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y -= CalamityWorld.revenge ? 0.05f : 0.03f;

                if (NPC.velocity.Y > maxHoverVel)
                    NPC.velocity.Y = maxHoverVel;
            }

            if (NPC.type == NPCID.Gastropod)
                Lighting.AddLight((int)NPC.position.X / 16, (int)NPC.position.Y / 16, 0.4f, 0f, 0.25f);

            return false;
        }
    }
}
