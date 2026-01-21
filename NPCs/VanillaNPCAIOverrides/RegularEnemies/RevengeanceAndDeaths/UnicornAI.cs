using System;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class UnicornAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            int turnAroundDelay = 30;
            int turnAroundDelayMult = 8;

            bool flag = false;
            bool isRunning = false;
            bool shouldTurnAround = false;

            if (NPC.velocity.Y == 0f && ((NPC.velocity.X > 0f && NPC.direction < 0) || (NPC.velocity.X < 0f && NPC.direction > 0)))
            {
                isRunning = true;
                NPC.ai[3] += 1f;
            }

            if (NPC.type == NPCID.Tumbleweed)
            {
                turnAroundDelayMult = 3;
                bool noYVelocity = NPC.velocity.Y == 0f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.whoAmI != NPC.whoAmI && n.type == NPC.type && Math.Abs(NPC.position.X - n.position.X) + Math.Abs(NPC.position.Y - n.position.Y) < (float)NPC.width)
                    {
                        if (NPC.position.X < n.position.X)
                        {
                            NPC.velocity.X -= 0.05f;
                        }
                        else
                        {
                            NPC.velocity.X += 0.05f;
                        }
                        if (NPC.position.Y < n.position.Y)
                        {
                            NPC.velocity.Y -= 0.05f;
                        }
                        else
                        {
                            NPC.velocity.Y += 0.05f;
                        }
                    }
                }
                if (noYVelocity)
                {
                    NPC.velocity.Y = 0f;
                }
            }

            if ((NPC.position.X == NPC.oldPosition.X || NPC.ai[3] >= (float)turnAroundDelay) | isRunning)
            {
                NPC.ai[3] += 1f;
                shouldTurnAround = true;
            }
            else if (NPC.ai[3] > 0f)
            {
                NPC.ai[3] -= 1f;
            }

            if (NPC.ai[3] > (float)(turnAroundDelay * turnAroundDelayMult))
            {
                NPC.ai[3] = 0f;
            }

            if (NPC.justHit)
            {
                NPC.ai[3] = 0f;
            }

            if (NPC.ai[3] == (float)turnAroundDelay)
            {
                NPC.netUpdate = true;
            }

            Vector2 npcPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
            float targetXDist = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - npcPosition.X;
            float targetYDist = Main.player[NPC.target].position.Y - npcPosition.Y;
            float targetDistance = (float)Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));

            if (targetDistance < 200f && !shouldTurnAround)
            {
                NPC.ai[3] = 0f;
            }

            if (NPC.type == NPCID.StardustSpiderSmall)
            {
                NPC.ai[1] += 1f;
                bool spawnTwinkle = NPC.ai[1] >= (CalamityWorld.death ? 60f : 120f);
                if (!spawnTwinkle && NPC.velocity.Y == 0f)
                {
                    foreach (Player plr in Main.ActivePlayers)
                    {
                        if (!plr.dead && plr.Distance(NPC.Center) < 800f && plr.Center.Y < NPC.Center.Y && Math.Abs(plr.Center.X - NPC.Center.X) < 20f)
                        {
                            spawnTwinkle = true;
                            break;
                        }
                    }
                }

                if (spawnTwinkle && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y, (Main.rand.NextFloat() - 0.5f) * 2f, -4f - 10f * Main.rand.NextFloat(), ProjectileID.Twinkle, 50, 0f, Main.myPlayer, 0f, 0f);
                    }
                    NPC.HitEffect(9999, 10.0);
                    NPC.active = false;
                    return false;
                }
            }
            else if (NPC.type == NPCID.NebulaBeast)
            {
                if (NPC.ai[2] == 1f)
                {
                    NPC.ai[1] += 1f;
                    NPC.velocity.X = NPC.velocity.X * 0.7f;
                    if (NPC.ai[1] < 30f)
                    {
                        Vector2 nebulaBeastDustRotation = NPC.Center + Vector2.UnitX * (float)NPC.spriteDirection * -20f;
                        Dust nebulaBeastDust = Main.dust[Dust.NewDust(nebulaBeastDustRotation, 0, 0, DustID.PinkTorch, 0f, 0f, 0, default(Color), 1f)];
                        Vector2 nebulaBeastDustVelocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                        nebulaBeastDust.position = nebulaBeastDustRotation + nebulaBeastDustVelocity * 20f;
                        nebulaBeastDust.velocity = -nebulaBeastDustVelocity * 2f;
                        nebulaBeastDust.scale = 0.5f + nebulaBeastDustVelocity.X * (float)(-(float)NPC.spriteDirection);
                        nebulaBeastDust.fadeIn = 1f;
                        nebulaBeastDust.noGravity = true;
                    }
                    else if (NPC.ai[1] == 30f)
                    {
                        for (int l = 0; l < 20; l++)
                        {
                            Vector2 nebulaBeastDustRotation2 = NPC.Center + Vector2.UnitX * (float)NPC.spriteDirection * -20f;
                            Dust nebulaBeastDust2 = Main.dust[Dust.NewDust(nebulaBeastDustRotation2, 0, 0, DustID.PinkTorch, 0f, 0f, 0, default(Color), 1f)];
                            Vector2 nebulaBeastDustVelocity2 = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                            nebulaBeastDust2.position = nebulaBeastDustRotation2 + nebulaBeastDustVelocity2 * 4f;
                            nebulaBeastDust2.velocity = nebulaBeastDustVelocity2 * 4f + Vector2.UnitX * Main.rand.NextFloat() * (float)NPC.spriteDirection * -5f;
                            nebulaBeastDust2.scale = 0.5f + nebulaBeastDustVelocity2.X * (float)(-(float)NPC.spriteDirection);
                            nebulaBeastDust2.fadeIn = 1f;
                            nebulaBeastDust2.noGravity = true;
                        }
                    }

                    if (NPC.velocity.X > -0.5f && NPC.velocity.X < 0.5f)
                    {
                        NPC.velocity.X = 0f;
                    }

                    if (NPC.ai[1] == 30f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int nebulaBeastProjDamage = Main.expertMode ? 35 : 50;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X + (float)(NPC.spriteDirection * -20), NPC.Center.Y, (float)(NPC.spriteDirection * -7), 0f, ProjectileID.NebulaSphere, nebulaBeastProjDamage, 0f, Main.myPlayer, (float)NPC.target, 0f);
                    }

                    if (NPC.ai[1] >= 60f)
                    {
                        NPC.ai[1] = (float)(-(float)Main.rand.Next(320, CalamityWorld.death ? 361 : 601));
                        NPC.ai[2] = 0f;
                    }
                }
                else
                {
                    NPC.ai[1] += 1f;
                    if (NPC.ai[1] >= 180f && targetDistance < 500f && NPC.velocity.Y == 0f)
                    {
                        flag = true;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 1f;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.velocity.Y == 0f && targetDistance < 100f && Math.Abs(NPC.velocity.X) > 3f && ((NPC.Center.X < Main.player[NPC.target].Center.X && NPC.velocity.X > 0f) || (NPC.Center.X > Main.player[NPC.target].Center.X && NPC.velocity.X < 0f)))
                    {
                        NPC.velocity.Y = NPC.velocity.Y - 6f;
                    }
                }
            }
            else if (NPC.type == NPCID.Wolf || NPC.type == NPCID.Hellhound || NPC.type == ModContent.NPCType<Rotdog>())
            {
                if (NPC.velocity.Y == 0f && targetDistance < 100f && Math.Abs(NPC.velocity.X) > 3f && ((NPC.position.X + (float)(NPC.width / 2) < Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) && NPC.velocity.X > 0f) || (NPC.position.X + (float)(NPC.width / 2) > Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) && NPC.velocity.X < 0f)))
                {
                    NPC.velocity.Y -= 6f;
                }
            }

            else if (NPC.type == NPCID.Tumbleweed && NPC.velocity.Y == 0f && Math.Abs(NPC.velocity.X) > 3f && ((NPC.Center.X < Main.player[NPC.target].Center.X && NPC.velocity.X > 0f) || (NPC.Center.X > Main.player[NPC.target].Center.X && NPC.velocity.X < 0f)))
            {
                NPC.velocity.Y -= 6f;
                SoundEngine.PlaySound(SoundID.NPCHit11, NPC.Center);
            }

            if (NPC.ai[3] < (float)turnAroundDelay)
            {
                if ((NPC.type == NPCID.Hellhound || NPC.type == NPCID.HeadlessHorseman) && !Main.pumpkinMoon)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                }
                else
                {
                    NPC.TargetClosest(true);
                }
            }
            else
            {
                if (NPC.velocity.X == 0f)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        NPC.ai[0] += 1f;
                        if (NPC.ai[0] >= 2f)
                        {
                            NPC.direction *= -1;
                            NPC.spriteDirection = NPC.direction;
                            NPC.ai[0] = 0f;
                        }
                    }
                }
                else
                {
                    NPC.ai[0] = 0f;
                }
                NPC.directionY = -1;
                if (NPC.direction == 0)
                {
                    NPC.direction = 1;
                }
            }

            float maxVelocity = 9f;
            float acceleration = 0.1f;
            if (CalamityWorld.death)
            {
                maxVelocity *= 1.25f;
                acceleration *= 1.25f;
            }

            if (!flag && (NPC.velocity.Y == 0f || NPC.wet || (NPC.velocity.X <= 0f && NPC.direction < 0) || (NPC.velocity.X >= 0f && NPC.direction > 0)))
            {
                if (NPC.type == ModContent.NPCType<Rotdog>())
                {
                    NPC.velocity.X *= 0.99f;
                }
                if (NPC.type == NPCID.Wolf)
                {
                    if (NPC.velocity.X > 0f && NPC.direction < 0)
                    {
                        NPC.velocity.X *= 0.9f;
                    }
                    if (NPC.velocity.X < 0f && NPC.direction > 0)
                    {
                        NPC.velocity.X *= 0.9f;
                    }
                }
                else if (NPC.type == NPCID.Hellhound)
                {
                    if (NPC.velocity.X > 0f && NPC.direction < 0)
                    {
                        NPC.velocity.X *= 0.2f;
                    }
                    if (NPC.velocity.X < 0f && NPC.direction > 0)
                    {
                        NPC.velocity.X *= 0.2f;
                    }
                    if (NPC.direction > 0 && NPC.velocity.X < 3f)
                    {
                        NPC.velocity.X += 0.15f;
                    }
                    if (NPC.direction < 0 && NPC.velocity.X > -3f)
                    {
                        NPC.velocity.X -= 0.15f;
                    }
                }
                else if (NPC.type == NPCID.HeadlessHorseman)
                {
                    if (NPC.velocity.X > 0f && NPC.direction < 0)
                    {
                        NPC.velocity.X *= 0.9f;
                    }
                    if (NPC.velocity.X < 0f && NPC.direction > 0)
                    {
                        NPC.velocity.X *= 0.9f;
                    }
                    if (NPC.velocity.X < -maxVelocity || NPC.velocity.X > maxVelocity)
                    {
                        if (NPC.velocity.Y == 0f)
                        {
                            NPC.velocity *= 0.8f;
                        }
                    }
                    else if (NPC.velocity.X < maxVelocity && NPC.direction == 1)
                    {
                        NPC.velocity.X += 0.1f;
                        if (NPC.velocity.X > maxVelocity)
                        {
                            NPC.velocity.X = maxVelocity;
                        }
                    }
                    else if (NPC.velocity.X > -maxVelocity && NPC.direction == -1)
                    {
                        NPC.velocity.X -= 0.1f;
                        if (NPC.velocity.X < -maxVelocity)
                        {
                            NPC.velocity.X = -maxVelocity;
                        }
                    }
                }
                else if (NPC.type == NPCID.StardustSpiderSmall)
                {
                    if (Math.Sign(NPC.velocity.X) != NPC.direction)
                    {
                        NPC.velocity.X *= 0.8f;
                    }
                    acceleration = 0.2f;
                }
                else if (NPC.type == NPCID.NebulaBeast)
                {
                    if (Math.Sign(NPC.velocity.X) != NPC.direction)
                    {
                        NPC.velocity.X *= 0.8f;
                    }
                    maxVelocity = 12f;
                    acceleration = 0.2f;
                }
                else if (NPC.type == NPCID.Tumbleweed)
                {
                    if (Math.Sign(NPC.velocity.X) != NPC.direction)
                    {
                        NPC.velocity.X *= 0.9f;
                    }
                    float sandstormPush = MathHelper.Lerp(0.6f, 1f, Math.Abs(Main.windSpeedCurrent)) * (float)Math.Sign(Main.windSpeedCurrent);
                    if (!Main.player[NPC.target].ZoneSandstorm)
                    {
                        sandstormPush = 0f;
                    }
                    maxVelocity = 6f + sandstormPush * (float)NPC.direction * 4f;
                    acceleration = 0.2f;
                }
                if (CalamityWorld.death)
                {
                    maxVelocity *= 1.25f;
                    acceleration *= 1.25f;
                }
                if (NPC.velocity.X < -maxVelocity || NPC.velocity.X > maxVelocity)
                {
                    if (NPC.velocity.Y == 0f)
                    {
                        NPC.velocity *= 0.8f;
                    }
                }
                else if (NPC.velocity.X < maxVelocity && NPC.direction == 1)
                {
                    NPC.velocity.X = NPC.velocity.X + acceleration;
                    if (NPC.velocity.X > maxVelocity)
                    {
                        NPC.velocity.X = maxVelocity;
                    }
                }
                else if (NPC.velocity.X > -maxVelocity && NPC.direction == -1)
                {
                    NPC.velocity.X = NPC.velocity.X - acceleration;
                    if (NPC.velocity.X < -maxVelocity)
                    {
                        NPC.velocity.X = -maxVelocity;
                    }
                }
            }

            if (NPC.velocity.Y >= 0f)
            {
                int faceDirection = 0;
                if (NPC.velocity.X < 0f)
                {
                    faceDirection = -1;
                }
                if (NPC.velocity.X > 0f)
                {
                    faceDirection = 1;
                }

                Vector2 position = NPC.position;
                position.X += NPC.velocity.X;
                int x = (int)((position.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * faceDirection)) / 16f);
                int y = (int)((position.Y + (float)NPC.height - 1f) / 16f);

                if ((float)(x * 16) < position.X + (float)NPC.width && (float)(x * 16 + 16) > position.X && ((Main.tile[x, y].HasUnactuatedTile && !Main.tile[x, y].TopSlope && !Main.tile[x, y - 1].TopSlope && Main.tileSolid[(int)Main.tile[x, y].TileType] && !Main.tileSolidTop[(int)Main.tile[x, y].TileType]) || (Main.tile[x, y - 1].IsHalfBlock && Main.tile[x, y - 1].HasUnactuatedTile)) && (!Main.tile[x, y - 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 1].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 1].TileType] || (Main.tile[x, y - 1].IsHalfBlock && (!Main.tile[x, y - 4].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 4].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 4].TileType]))) && (!Main.tile[x, y - 2].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 2].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 2].TileType]) && (!Main.tile[x, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 3].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 3].TileType]) && (!Main.tile[x - faceDirection, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x - faceDirection, y - 3].TileType]))
                {
                    float tilePixelPosition = (float)(y * 16);
                    if (Main.tile[x, y].IsHalfBlock)
                    {
                        tilePixelPosition += 8f;
                    }
                    if (Main.tile[x, y - 1].IsHalfBlock)
                    {
                        tilePixelPosition -= 8f;
                    }
                    if (tilePixelPosition < position.Y + (float)NPC.height)
                    {
                        float percentageTileRisen = position.Y + (float)NPC.height - tilePixelPosition;
                        if ((double)percentageTileRisen <= 16.1)
                        {
                            NPC.gfxOffY += NPC.position.Y + (float)NPC.height - tilePixelPosition;
                            NPC.position.Y = tilePixelPosition - (float)NPC.height;
                            if (percentageTileRisen < 9f)
                            {
                                NPC.stepSpeed = 1f;
                            }
                            else
                            {
                                NPC.stepSpeed = 2f;
                            }
                        }
                    }
                }
            }

            if (NPC.velocity.Y == 0f)
            {
                int npcTileX = (int)((NPC.position.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 2) * NPC.direction) + NPC.velocity.X * 5f) / 16f);
                int npcTileY = (int)((NPC.position.Y + (float)NPC.height - 15f) / 16f);

                int spriteDirection = NPC.spriteDirection;
                if (NPC.type == NPCID.NebulaBeast || NPC.type == NPCID.StardustSpiderSmall || NPC.type == NPCID.Tumbleweed)
                {
                    spriteDirection *= -1;
                }

                if ((NPC.velocity.X < 0f && spriteDirection == -1) || (NPC.velocity.X > 0f && spriteDirection == 1))
                {
                    bool pillarEnemy = NPC.type == NPCID.StardustSpiderSmall || NPC.type == NPCID.NebulaBeast;

                    if (Main.tile[npcTileX, npcTileY - 2].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 2].TileType])
                    {
                        if (Main.tile[npcTileX, npcTileY - 3].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 3].TileType])
                        {
                            NPC.velocity.Y = -10.5f;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            NPC.velocity.Y = -9.5f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (Main.tile[npcTileX, npcTileY - 1].HasUnactuatedTile && !Main.tile[npcTileX, npcTileY - 1].TopSlope && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 1].TileType])
                    {
                        NPC.velocity.Y = -9f;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.position.Y + (float)NPC.height - (float)(npcTileY * 16) > 20f && Main.tile[npcTileX, npcTileY].HasUnactuatedTile && !Main.tile[npcTileX, npcTileY].TopSlope && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY].TileType])
                    {
                        NPC.velocity.Y = -7f;
                        NPC.netUpdate = true;
                    }
                    else if ((NPC.directionY < 0 || Math.Abs(NPC.velocity.X) > 3f) && (!pillarEnemy || !Main.tile[npcTileX, npcTileY + 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX, npcTileY + 1].TileType]) && (!Main.tile[npcTileX, npcTileY + 2].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX, npcTileY + 2].TileType]) && (!Main.tile[npcTileX + NPC.direction, npcTileY + 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX + NPC.direction, npcTileY + 3].TileType]))
                    {
                        NPC.velocity.Y = -10f;
                        NPC.netUpdate = true;
                    }
                }
            }

            if (NPC.type == NPCID.NebulaBeast && Math.Abs(NPC.velocity.X) >= maxVelocity * 0.95f)
            {
                Rectangle hitbox = NPC.Hitbox;
                for (int m = 0; m < 2; m++)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Dust nebulaBeastIdleDust = Main.dust[Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.PinkTorch, 0f, 0f, 0, default(Color), 1f)];
                        nebulaBeastIdleDust.velocity = Vector2.Zero;
                        nebulaBeastIdleDust.noGravity = true;
                        nebulaBeastIdleDust.fadeIn = 1f;
                        nebulaBeastIdleDust.scale = 0.5f + Main.rand.NextFloat();
                    }
                }
            }

            if (NPC.type == NPCID.Tumbleweed)
            {
                NPC.rotation += NPC.velocity.X * 0.05f;
                NPC.spriteDirection = -NPC.direction;
            }

            return false;
        }
    }
}
