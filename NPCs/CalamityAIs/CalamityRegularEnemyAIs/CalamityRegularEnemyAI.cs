using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.SulphurousSea;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Sounds;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs
{
    public static class CalamityRegularEnemyAI
    {
        #region Gem Crawler AI
        public static void GemCrawlerAI(NPC npc, Mod mod, float speedDetect, float speedAdditive)
        {
            int turnAroundDelay = 30;
            bool unusedFlag = false;
            bool isRunning = false;
            bool shouldTurnAround = false;
            if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction > 0) || (npc.velocity.X < 0f && npc.direction < 0)))
            {
                isRunning = true;
                npc.ai[3] += 1f;
            }
            if ((npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)turnAroundDelay) | isRunning)
            {
                npc.ai[3] += 1f;
                shouldTurnAround = true;
            }
            else if (npc.ai[3] > 0f)
            {
                npc.ai[3] -= 1f;
            }
            if (npc.ai[3] > (float)(turnAroundDelay * 10))
            {
                npc.ai[3] = 0f;
            }
            if (npc.justHit)
            {
                npc.ai[3] = 0f;
            }
            if (npc.ai[3] == (float)turnAroundDelay)
            {
                npc.netUpdate = true;
            }
            Vector2 npcPos = new Vector2(npc.Center.X, npc.Center.Y);
            float xDist = Main.player[npc.target].Center.X - npcPos.X;
            float yDist = Main.player[npc.target].Center.Y - npcPos.Y;
            float targetDist = (float)Math.Sqrt((double)(xDist * xDist + yDist * yDist));
            if (targetDist < 200f && !shouldTurnAround)
            {
                npc.ai[3] = 0f;
            }
            if (npc.ai[3] < (float)turnAroundDelay)
            {
                npc.TargetClosest(true);
            }
            else
            {
                if (npc.velocity.X == 0f)
                {
                    if (npc.velocity.Y == 0f)
                    {
                        npc.ai[0] += 1f;
                        if (npc.ai[0] >= 2f)
                        {
                            npc.direction *= -1;
                            npc.spriteDirection = -npc.direction;
                            npc.ai[0] = 0f;
                        }
                    }
                }
                else
                {
                    npc.ai[0] = 0f;
                }
                npc.directionY = -1;
                if (npc.direction == 0)
                {
                    npc.direction = 1;
                }
            }
            float maxVelocity = speedDetect; //5
            float acceleration = speedAdditive; //0.05
            if (!unusedFlag && (npc.velocity.Y == 0f || npc.wet || (npc.velocity.X <= 0f && npc.direction > 0) || (npc.velocity.X >= 0f && npc.direction < 0)))
            {
                if (npc.velocity.X < -maxVelocity || npc.velocity.X > maxVelocity)
                {
                    if (npc.velocity.Y == 0f)
                    {
                        npc.velocity *= 0.8f;
                    }
                }
                else if (npc.velocity.X < maxVelocity && npc.direction == -1)
                {
                    npc.velocity.X = npc.velocity.X + acceleration;
                    if (npc.velocity.X > maxVelocity)
                    {
                        npc.velocity.X = maxVelocity;
                    }
                }
                else if (npc.velocity.X > -maxVelocity && npc.direction == 1)
                {
                    npc.velocity.X = npc.velocity.X - acceleration;
                    if (npc.velocity.X < -maxVelocity)
                    {
                        npc.velocity.X = -maxVelocity;
                    }
                }
            }
            if (npc.velocity.Y >= 0f)
            {
                int faceDirection = 0;
                if (npc.velocity.X < 0f)
                {
                    faceDirection = -1;
                }
                if (npc.velocity.X > 0f)
                {
                    faceDirection = 1;
                }
                Vector2 position = npc.position;
                position.X += npc.velocity.X;
                int x = (int)((position.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * faceDirection)) / 16f);
                int y = (int)((position.Y + (float)npc.height - 1f) / 16f);
                if ((float)(x * 16) < position.X + (float)npc.width && (float)(x * 16 + 16) > position.X && ((Main.tile[x, y].HasUnactuatedTile && !Main.tile[x, y].TopSlope && !Main.tile[x, y - 1].TopSlope && Main.tileSolid[(int)Main.tile[x, y].TileType] && !Main.tileSolidTop[(int)Main.tile[x, y].TileType]) || (Main.tile[x, y - 1].IsHalfBlock && Main.tile[x, y - 1].HasUnactuatedTile)) && (!Main.tile[x, y - 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 1].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 1].TileType] || (Main.tile[x, y - 1].IsHalfBlock && (!Main.tile[x, y - 4].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 4].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 4].TileType]))) && (!Main.tile[x, y - 2].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 2].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 2].TileType]) && (!Main.tile[x, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 3].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 3].TileType]) && (!Main.tile[x - faceDirection, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x - faceDirection, y - 3].TileType]))
                {
                    float npcBottom = (float)(y * 16);
                    if (Main.tile[x, y].IsHalfBlock)
                    {
                        npcBottom += 8f;
                    }
                    if (Main.tile[x, y - 1].IsHalfBlock)
                    {
                        npcBottom -= 8f;
                    }
                    if (npcBottom < position.Y + (float)npc.height)
                    {
                        float percentageTileRisen = position.Y + (float)npc.height - npcBottom;
                        if (percentageTileRisen <= 16.1f)
                        {
                            npc.gfxOffY += npc.position.Y + (float)npc.height - npcBottom;
                            npc.position.Y = npcBottom - (float)npc.height;
                            if (percentageTileRisen < 9f)
                            {
                                npc.stepSpeed = 1f;
                            }
                            else
                            {
                                npc.stepSpeed = 2f;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Dungeon Spirit AI
        public static void DungeonSpiritAI(NPC npc, Mod mod, float speed, float rotation, bool lantern = false)
        {
            npc.TargetClosest(true);
            Vector2 npcPos = new Vector2(npc.Center.X, npc.Center.Y);
            float xDist = Main.player[npc.target].Center.X - npcPos.X;
            float yDist = Main.player[npc.target].Center.Y - npcPos.Y;
            float targetDist = (float)Math.Sqrt((double)(xDist * xDist + yDist * yDist));
            float homingSpeed = speed;

            if (lantern)
            {
                if (npc.localAI[0] < 85f)
                {
                    homingSpeed = 0.1f;
                    targetDist = homingSpeed / targetDist;
                    xDist *= targetDist;
                    yDist *= targetDist;
                    npc.velocity = (npc.velocity * 100f + new Vector2(xDist, yDist)) / 101f;
                    npc.localAI[0] += 1f;
                    return;
                }

                npc.dontTakeDamage = false;
            }

            targetDist = homingSpeed / targetDist;
            xDist *= targetDist;
            yDist *= targetDist;
            npc.velocity.X = (npc.velocity.X * 100f + xDist) / 101f;
            npc.velocity.Y = (npc.velocity.Y * 100f + yDist) / 101f;

            if (lantern)
            {
                npc.rotation = npc.velocity.X * 0.08f;
                npc.spriteDirection = (npc.direction > 0) ? 1 : -1;
            }
            else
                npc.rotation = (float)Math.Atan2((double)yDist, (double)xDist) + rotation;
        }
        #endregion

        #region Unicorn AI
        public static void UnicornAI(NPC npc, Mod mod, bool spin, float bounciness, float speedDetect, float speedAdditive, float bouncy1 = -8.5f, float bouncy2 = -7.5f, float bouncy3 = -7f, float bouncy4 = -6f, float bouncy5 = -8f)
        {
            bool DogPhase1 = npc.type == ModContent.NPCType<Rimehound>() && npc.life > npc.lifeMax * (CalamityWorld.death ? 0.9 : CalamityWorld.revenge ? 0.7 : 0.5);
            bool DogPhase2 = npc.type == ModContent.NPCType<Rimehound>() && npc.life <= npc.lifeMax * (CalamityWorld.death ? 0.9 : CalamityWorld.revenge ? 0.7 : 0.5);
            int turnAroundDelay = 30;
            bool isRunning = false;
            bool shouldTurnAround = false;
            if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
            {
                isRunning = true;
                npc.ai[3] += 1f;
            }
            int turnAroundDelayMult = DogPhase1 ? 10 : 4;
            if (!DogPhase1)
            {
                bool noYVelocity = npc.velocity.Y == 0f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (i != npc.whoAmI && Main.npc[i].active && Main.npc[i].type == npc.type && Math.Abs(npc.position.X - Main.npc[i].position.X) + Math.Abs(npc.position.Y - Main.npc[i].position.Y) < (float)npc.width)
                    {
                        if (npc.position.X < Main.npc[i].position.X)
                        {
                            npc.velocity.X -= 0.05f;
                        }
                        else
                        {
                            npc.velocity.X += 0.05f;
                        }
                        if (npc.position.Y < Main.npc[i].position.Y)
                        {
                            npc.velocity.Y -= 0.05f;
                        }
                        else
                        {
                            npc.velocity.Y += 0.05f;
                        }
                    }
                }
                if (noYVelocity)
                {
                    npc.velocity.Y = 0f;
                }
            }
            if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)turnAroundDelay || isRunning)
            {
                npc.ai[3] += 1f;
                shouldTurnAround = true;
            }
            else if (npc.ai[3] > 0f)
            {
                npc.ai[3] -= 1f;
            }
            if (npc.ai[3] > (float)(turnAroundDelay * turnAroundDelayMult))
            {
                npc.ai[3] = 0f;
            }
            if (npc.justHit)
            {
                npc.ai[3] = 0f;
            }
            if (npc.ai[3] == (float)turnAroundDelay)
            {
                npc.netUpdate = true;
            }
            Vector2 npcPos = new Vector2(npc.Center.X, npc.Center.Y);
            float xDist = Main.player[npc.target].Center.X - npcPos.X;
            float yDist = Main.player[npc.target].Center.Y - npcPos.Y;
            float targetDist = (float)Math.Sqrt((double)(xDist * xDist + yDist * yDist));
            if (targetDist < 200f && !shouldTurnAround)
            {
                npc.ai[3] = 0f;
            }
            if (!DogPhase1)
            {
                if (npc.velocity.Y == 0f && Math.Abs(npc.velocity.X) > 3f && ((npc.Center.X < Main.player[npc.target].Center.X && npc.velocity.X > 0f) || (npc.Center.X > Main.player[npc.target].Center.X && npc.velocity.X < 0f)))
                {
                    npc.velocity.Y -= bounciness;
                    if (npc.type == ModContent.NPCType<DespairStone>())
                    {
                        SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                        for (int k = 0; k < 10; k++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, (int)CalamityDusts.Brimstone, 0f, -1f, 0, default, 1f);
                        }
                        if (Main.zenithWorld)
                        {
                            float screenShakePower = 2 * Utils.GetLerpValue(1300f, 0f, npc.Distance(Main.LocalPlayer.Center), true);
                            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakePower)
                                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;
                        }
                    }
                    if (npc.type == ModContent.NPCType<Bohldohr>())
                    {
                        SoundEngine.PlaySound(SoundID.NPCHit7, npc.Center);
                    }
                    if (DogPhase2)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Water, 0f, -1f, 0, default, 1f);
                        }
                    }
                    if (npc.type == ModContent.NPCType<AquaticUrchin>() || npc.type == ModContent.NPCType<SeaUrchin>())
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Water, 0f, -1f, 0, default, 1f);
                        }
                    }
                }
            }
            if (npc.ai[3] < (float)turnAroundDelay)
            {
                npc.TargetClosest(true);
            }
            else
            {
                if (npc.velocity.X == 0f)
                {
                    if (npc.velocity.Y == 0f)
                    {
                        npc.ai[0] += 1f;
                        if (npc.ai[0] >= 2f)
                        {
                            npc.direction *= -1;
                            npc.spriteDirection = npc.direction;
                            npc.ai[0] = 0f;
                        }
                    }
                }
                else
                {
                    npc.ai[0] = 0f;
                }
                npc.directionY = -1;
                if (npc.direction == 0)
                {
                    npc.direction = 1;
                }
            }

            if (npc.velocity.Y == 0f || npc.wet || (npc.velocity.X <= 0f && npc.direction < 0) || (npc.velocity.X >= 0f && npc.direction > 0))
            {
                if (Math.Sign(npc.velocity.X) != npc.direction && !DogPhase1)
                {
                    npc.velocity.X *= 0.92f;
                }
                float sandstormPush = MathHelper.Lerp(0.6f, 1f, Math.Abs(Main.windSpeedCurrent)) * (float)Math.Sign(Main.windSpeedCurrent);
                if (!Main.player[npc.target].ZoneSandstorm)
                {
                    sandstormPush = 0f;
                }
                float maxVelocity = speedDetect;
                float acceleration = speedAdditive;
                if (npc.velocity.X < -maxVelocity || npc.velocity.X > maxVelocity)
                {
                    if (npc.velocity.Y == 0f)
                    {
                        npc.velocity *= 0.8f;
                    }
                }
                else if (npc.velocity.X < maxVelocity && npc.direction == 1)
                {
                    npc.velocity.X += acceleration;
                    if (npc.velocity.X > maxVelocity)
                    {
                        npc.velocity.X = maxVelocity;
                    }
                }
                else if (npc.velocity.X > -maxVelocity && npc.direction == -1)
                {
                    npc.velocity.X -= acceleration;
                    if (npc.velocity.X < -maxVelocity)
                    {
                        npc.velocity.X = -maxVelocity;
                    }
                }
            }
            if (npc.velocity.Y >= 0f)
            {
                int faceDirection = 0;
                if (npc.velocity.X < 0f)
                {
                    faceDirection = -1;
                }
                if (npc.velocity.X > 0f)
                {
                    faceDirection = 1;
                }
                Vector2 position = npc.position;
                position.X += npc.velocity.X;
                int x = (int)((position.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * faceDirection)) / 16f);
                int y = (int)((position.Y + (float)npc.height - 1f) / 16f);

                // 21JAN2023: Ozzatron -- this is probably the single most unmaintainable if statement I've ever seen
                // I have broken all the individual statements out even though I do not fully understand the code.
                Tile t_xy = CalamityUtils.ParanoidTileRetrieval(x, y);
                Tile t_xy1 = CalamityUtils.ParanoidTileRetrieval(x, y - 1);
                Tile t_xy2 = CalamityUtils.ParanoidTileRetrieval(x, y - 2);
                Tile t_xy3 = CalamityUtils.ParanoidTileRetrieval(x, y - 3);
                Tile t_xOffY3 = CalamityUtils.ParanoidTileRetrieval(x - faceDirection, y - 3); // 3 down, offset 1 in the direction of the NPC's movement
                Tile t_xy4 = CalamityUtils.ParanoidTileRetrieval(x, y - 4);
                bool positionCheck = (float)(x * 16) < position.X + (float)npc.width && (float)(x * 16 + 16) > position.X;
                bool tileSolidityCheck1 = t_xy.HasUnactuatedTile && !t_xy.TopSlope && !t_xy1.TopSlope && Main.tileSolid[t_xy.TileType] && !Main.tileSolidTop[t_xy.TileType];
                bool oneBelowIsSolidHalf = t_xy1.IsHalfBlock && t_xy1.HasUnactuatedTile;
                bool canFallThrough = !t_xy1.HasUnactuatedTile || !Main.tileSolid[t_xy1.TileType] || Main.tileSolidTop[t_xy1.TileType] || (t_xy1.IsHalfBlock && (!t_xy4.HasUnactuatedTile || !Main.tileSolid[t_xy4.TileType] || Main.tileSolidTop[t_xy4.TileType]));
                bool twoDownIsNonSolid = !t_xy2.HasUnactuatedTile || !Main.tileSolid[t_xy2.TileType] || Main.tileSolidTop[t_xy2.TileType];
                bool threeDownIsNonSolid = !t_xy3.HasUnactuatedTile || !Main.tileSolid[t_xy3.TileType] || Main.tileSolidTop[t_xy3.TileType];
                // Notice it doesn't check for platforms in ther offset position. This is why walking AIs twirl on 1-wide platforms in hellevators. They have to turn around before this check succeeds.
                bool threeDownOffsetIsNonSolid = !t_xOffY3.HasUnactuatedTile || !Main.tileSolid[t_xOffY3.TileType];
                if (positionCheck && (tileSolidityCheck1 || oneBelowIsSolidHalf) && canFallThrough && twoDownIsNonSolid && threeDownIsNonSolid && threeDownOffsetIsNonSolid)
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
                    if (tilePixelPosition < position.Y + (float)npc.height)
                    {
                        float percentageTileRisen = position.Y + (float)npc.height - tilePixelPosition;
                        if ((double)percentageTileRisen <= 16.1)
                        {
                            npc.gfxOffY += npc.position.Y + (float)npc.height - tilePixelPosition;
                            npc.position.Y = tilePixelPosition - (float)npc.height;
                            if (percentageTileRisen < 9f)
                            {
                                npc.stepSpeed = 1f;
                            }
                            else
                            {
                                npc.stepSpeed = 2f;
                            }
                        }
                    }
                }
            }
            if (npc.velocity.Y == 0f)
            {
                int npcTileX = (int)((npc.position.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 2) * npc.direction) + npc.velocity.X * 5f) / 16f);
                int npcTileY = (int)((npc.position.Y + (float)npc.height - 15f) / 16f);
                int spriteDirection = npc.spriteDirection;
                spriteDirection *= -1;
                if ((npc.velocity.X < 0f && spriteDirection == -1) || (npc.velocity.X > 0f && spriteDirection == 1))
                {
                    if (Main.tile[npcTileX, npcTileY - 2].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 2].TileType])
                    {
                        if (Main.tile[npcTileX, npcTileY - 3].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 3].TileType])
                        {
                            npc.velocity.Y = bouncy1;
                            npc.netUpdate = true;
                        }
                        else
                        {
                            npc.velocity.Y = bouncy2;
                            npc.netUpdate = true;
                        }
                    }
                    else if (Main.tile[npcTileX, npcTileY - 1].HasUnactuatedTile && !Main.tile[npcTileX, npcTileY - 1].TopSlope && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY - 1].TileType])
                    {
                        npc.velocity.Y = bouncy3;
                        npc.netUpdate = true;
                    }
                    else if (npc.position.Y + (float)npc.height - (float)(npcTileY * 16) > 20f && Main.tile[npcTileX, npcTileY].HasUnactuatedTile && !Main.tile[npcTileX, npcTileY].TopSlope && Main.tileSolid[(int)Main.tile[npcTileX, npcTileY].TileType])
                    {
                        npc.velocity.Y = bouncy4;
                        npc.netUpdate = true;
                    }
                    else if ((npc.directionY < 0 || Math.Abs(npc.velocity.X) > 3f) && (!Main.tile[npcTileX, npcTileY + 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX, npcTileY + 1].TileType]) && (!Main.tile[npcTileX, npcTileY + 2].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX, npcTileY + 2].TileType]) && (!Main.tile[npcTileX + npc.direction, npcTileY + 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[npcTileX + npc.direction, npcTileY + 3].TileType]))
                    {
                        npc.velocity.Y = bouncy5;
                        npc.netUpdate = true;
                    }
                }
            }
            if (spin)
            {
                npc.rotation += npc.velocity.X * 0.05f;
                npc.spriteDirection = -npc.direction;
            }
        }
        #endregion

        #region Swimming AI
        // Passiveness:
        // 0 = Catfish, Flounder, Frogfish, Viperfish, Moray Eel, Laserfish
        // 1 = Blinded Angler
        // 2 = Prism-Back, Toxic Minnow
        // 3 = Sea Minnow
        public static void PassiveSwimmingAI(NPC npc, Mod mod, int passiveness, float detectRange, float xSpeed, float ySpeed, float speedLimitX, float speedLimitY, float rotation, bool spriteFacesLeft = true)
        {
            if (spriteFacesLeft)
                npc.spriteDirection = (npc.direction > 0) ? 1 : -1;
            else
                npc.spriteDirection = (npc.direction > 0) ? -1 : 1;

            npc.noGravity = true;
            if (npc.direction == 0)
            {
                npc.TargetClosest(true);
            }
            Player target = Main.player[npc.target];
            if (npc.justHit && passiveness != 3)
            {
                npc.chaseable = true;
            }
            if (npc.wet)
            {
                bool hasWetTarget = npc.chaseable;
                npc.TargetClosest(false);
                target = Main.player[npc.target];
                // Player detection behavior
                if (passiveness != 2)
                {
                    if (npc.type == ModContent.NPCType<Frogfish>())
                    {
                        if (target.wet && !target.dead)
                        {
                            hasWetTarget = true;
                            npc.chaseable = true; //once the enemy has detected the player, let minions fuck it up
                        }
                    }
                    if (npc.type == ModContent.NPCType<Sulflounder>())
                    {
                        if (!target.dead)
                        {
                            hasWetTarget = true;
                            npc.chaseable = true; //once the enemy has detected the player, let minions fuck it up
                        }
                    }
                    else if (target.wet && !target.dead && (target.Center - npc.Center).Length() < detectRange &&
                        Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height))
                    {
                        hasWetTarget = true;
                        npc.chaseable = true; //once the enemy has detected the player, let minions fuck it up
                    }
                    else
                    {
                        if (passiveness == 1)
                        {
                            hasWetTarget = false;
                        }
                    }
                }
                if ((target.dead || !Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height)) && hasWetTarget)
                {
                    hasWetTarget = false;
                }

                // Swim back and forth
                if (!hasWetTarget || passiveness == 2)
                {
                    if (passiveness == 0)
                    {
                        npc.TargetClosest(true);
                        target = Main.player[npc.target];
                    }
                    if (npc.collideX)
                    {
                        npc.velocity.X = npc.velocity.X * -1f;
                        npc.direction *= -1;
                        npc.netUpdate = true;
                    }
                    if (npc.collideY)
                    {
                        npc.netUpdate = true;
                        if (npc.velocity.Y > 0f)
                        {
                            npc.velocity.Y = Math.Abs(npc.velocity.Y) * -1f;
                            npc.directionY = -1;
                            npc.ai[0] = -1f;
                        }
                        else if (npc.velocity.Y < 0f)
                        {
                            npc.velocity.Y = Math.Abs(npc.velocity.Y);
                            npc.directionY = 1;
                            npc.ai[0] = 1f;
                        }
                    }
                }

                if (hasWetTarget && passiveness != 2)
                {
                    npc.TargetClosest(true);
                    target = Main.player[npc.target];
                    // Swim away from the player
                    if (passiveness == 3)
                    {
                        npc.velocity.X = npc.velocity.X - (float)npc.direction * xSpeed;
                        npc.velocity.Y = npc.velocity.Y - (float)npc.directionY * ySpeed;
                    }
                    // Swim toward the player
                    else
                    {
                        npc.velocity.X = npc.velocity.X + (float)npc.direction * (CalamityWorld.death ? 2f * xSpeed : CalamityWorld.revenge ? 1.5f * xSpeed : xSpeed);
                        npc.velocity.Y = npc.velocity.Y + (float)npc.directionY * (CalamityWorld.death ? 2f * ySpeed : CalamityWorld.revenge ? 1.5f * ySpeed : ySpeed);
                    }
                    float velocityCapX = CalamityWorld.death && passiveness != 3 ? 2f * speedLimitX : CalamityWorld.revenge ? 1.5f * speedLimitX : speedLimitX;
                    float velocityCapY = CalamityWorld.death && passiveness != 3 ? 2f * speedLimitY : CalamityWorld.revenge ? 1.5f * speedLimitY : speedLimitY;
                    npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -velocityCapX, velocityCapX);
                    npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -velocityCapY, velocityCapY);

                    if (npc.justHit)
                        npc.localAI[0] = 0f;

                    // Laserfish shoot the player
                    if (npc.type == ModContent.NPCType<Laserfish>())
                    {
                        npc.localAI[0] += (CalamityWorld.death ? 2f : CalamityWorld.revenge ? 1.5f : 1f);
                        if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] >= 120f)
                        {
                            npc.localAI[0] = 0f;
                            if (Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height))
                            {
                                float speed = 5f;
                                Vector2 vector = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)(npc.height / 2));
                                float velX = target.Center.X - vector.X + Main.rand.NextFloat(-20f, 20f);
                                float velY = target.Center.Y - vector.Y + Main.rand.NextFloat(-20f, 20f);
                                float dist = (float)Math.Sqrt((double)(velX * velX + velY * velY));
                                dist = speed / dist;
                                velX *= dist;
                                velY *= dist;
                                int damage = Main.masterMode ? 34 : Main.expertMode ? 40 : 50;
                                int beam = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X + (npc.spriteDirection == 1 ? 25f : -25f), npc.Center.Y + (target.position.Y > npc.Center.Y ? 5f : -5f), velX, velY, ProjectileID.EyeBeam, damage, 0f, Main.myPlayer);
                                Main.projectile[beam].tileCollide = true;
                            }
                        }
                    }

                    // Flounder shoot Sulphuric Mist at the player
                    if (npc.type == ModContent.NPCType<Sulflounder>())
                    {
                        if ((target.Center - npc.Center).Length() < 350f)
                        {
                            npc.localAI[0] += (CalamityWorld.death ? 3f : CalamityWorld.revenge ? 2f : 1f);
                            if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] >= 180f)
                            {
                                npc.localAI[0] = 0f;
                                if (Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height))
                                {
                                    float speed = 4f;
                                    Vector2 vector = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)(npc.height / 2));
                                    float velX = target.Center.X - vector.X + Main.rand.NextFloat(-20f, 20f);
                                    float velY = target.Center.Y - vector.Y + Main.rand.NextFloat(-20f, 20f);
                                    float dist = (float)Math.Sqrt((double)(velX * velX + velY * velY));
                                    dist = speed / dist;
                                    velX *= dist;
                                    velY *= dist;
                                    int damage = Main.masterMode ? 21 : Main.expertMode ? 25 : 35;
                                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X + (npc.spriteDirection == 1 ? 10f : -10f), npc.Center.Y, velX, velY, ModContent.ProjectileType<SulphuricAcidMist>(), damage, 0f, Main.myPlayer);
                                }
                            }
                        }
                    }

                    // Sea Minnows face away from the player
                    if (npc.type == ModContent.NPCType<SeaMinnow>())
                    {
                        npc.direction *= -1;
                    }
                }
                else
                {
                    // No target behavior
                    npc.velocity.X += (float)npc.direction * 0.1f;
                    if (npc.velocity.X < -2.5f || npc.velocity.X > 2.5f)
                    {
                        npc.velocity.X *= 0.95f;
                    }
                    if (npc.ai[0] == -1f)
                    {
                        npc.velocity.Y -= 0.01f;
                        if (npc.velocity.Y < -0.3f)
                        {
                            npc.ai[0] = 1f;
                        }
                    }
                    else
                    {
                        npc.velocity.Y += 0.01f;
                        if (npc.velocity.Y > 0.3f)
                        {
                            npc.ai[0] = -1f;
                        }
                    }
                }
                int npcTileX = (int)(npc.position.X + (float)(npc.width / 2)) / 16;
                int npcTileY = (int)(npc.position.Y + (float)(npc.height / 2)) / 16;
                if (Main.tile[npcTileX, npcTileY - 1].LiquidAmount > 128)
                {
                    if (Main.tile[npcTileX, npcTileY + 1].HasTile)
                    {
                        npc.ai[0] = -1f;
                    }
                    else if (Main.tile[npcTileX, npcTileY + 2].HasTile)
                    {
                        npc.ai[0] = -1f;
                    }
                }
                if (npc.velocity.Y > 0.4f || npc.velocity.Y < -0.4f)
                {
                    npc.velocity.Y = npc.velocity.Y * 0.95f;
                }
            }
            else
            {
                // Out of water behavior
                if (npc.velocity.Y == 0f)
                {
                    npc.velocity.X = npc.velocity.X * 0.94f;
                    if (npc.velocity.X > -0.2f && npc.velocity.X < 0.2f)
                    {
                        npc.velocity.X = 0f;
                    }
                }
                npc.velocity.Y = npc.velocity.Y + 0.4f;
                if (npc.velocity.Y > 12f)
                {
                    npc.velocity.Y = 12f;
                }
                npc.ai[0] = 1f;
            }
            npc.rotation = npc.velocity.Y * (float)npc.direction * rotation;
            float rotationLimit = 2f * rotation;
            npc.rotation = MathHelper.Clamp(npc.rotation, -rotationLimit, rotationLimit);
        }
        #endregion
    }
}
