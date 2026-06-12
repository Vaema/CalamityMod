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
    public class CasterAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.TargetClosest();
            NPC.velocity.X *= 0.93f;
            if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                NPC.velocity.X = 0f;

            if (NPC.ai[0] == 0f)
                NPC.ai[0] = 500f;

            if (NPC.type == NPCID.RuneWizard)
            {
                if (NPC.alpha < 255)
                    NPC.alpha++;
                if (NPC.justHit)
                    NPC.alpha = 0;
            }

            if (NPC.ai[2] != 0f && NPC.ai[3] != 0f)
            {
                if (NPC.type == NPCID.RuneWizard)
                    NPC.alpha = 255;

                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                for (int i = 0; i < 50; i++)
                {
                    if (NPC.type == NPCID.GoblinSorcerer || NPC.type == NPCID.Tim)
                    {
                        int goblinDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default(Color), (float)Main.rand.Next(1, 3));
                        Dust dust = Main.dust[goblinDust];
                        dust.velocity *= 3f;
                        if (Main.dust[goblinDust].scale > 1f)
                            Main.dust[goblinDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.DarkCaster)
                    {
                        int darkCasterDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.DungeonWater, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[darkCasterDust];
                        dust.velocity *= 3f;
                        Main.dust[darkCasterDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.Necromancer || NPC.type == NPCID.NecromancerArmored)
                    {
                        int necromancerDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.ShadowbeamStaff, 0f, 0f, 0, default(Color), 1f);
                        Dust dust = Main.dust[necromancerDust];
                        dust.velocity *= 2f;
                        Main.dust[necromancerDust].scale = 1.4f;
                    }
                    else if (NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite)
                    {
                        int diabolistDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.InfernoFork, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[diabolistDust];
                        dust.velocity *= 3f;
                        Main.dust[diabolistDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
                    {
                        int raggedCasterDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SpectreStaff, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[raggedCasterDust];
                        dust.velocity *= 3f;
                        Main.dust[raggedCasterDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.RuneWizard)
                    {
                        int runeWizardDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RuneWizard, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[runeWizardDust];
                        dust.velocity *= 3f;
                        Main.dust[runeWizardDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.DesertDjinn)
                    {
                        int desertSpiritDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[desertSpiritDust];
                        dust.velocity *= 3f;
                        Main.dust[desertSpiritDust].noGravity = true;
                    }
                    else
                    {
                        int fireImpDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[fireImpDust];
                        dust.velocity *= 3f;
                        Main.dust[fireImpDust].noGravity = true;
                    }
                }

                NPC.position.X = NPC.ai[2] * 16f - (float)(NPC.width / 2) + 8f;
                NPC.position.Y = NPC.ai[3] * 16f - (float)NPC.height;

                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;

                NPC.ai[2] = 0f;
                NPC.ai[3] = 0f;

                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                for (int j = 0; j < 50; j++)
                {
                    if (NPC.type == NPCID.GoblinSorcerer || NPC.type == NPCID.Tim)
                    {
                        int goblinCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default(Color), (float)Main.rand.Next(1, 3));
                        Dust dust = Main.dust[goblinCastDust];
                        dust.velocity *= 3f;
                        if (Main.dust[goblinCastDust].scale > 1f)
                            Main.dust[goblinCastDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.DarkCaster)
                    {
                        int darkCasterCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.DungeonWater, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[darkCasterCastDust];
                        dust.velocity *= 3f;
                        Main.dust[darkCasterCastDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.RuneWizard)
                    {
                        int runeWizardCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RuneWizard, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[runeWizardCastDust];
                        dust.velocity *= 3f;
                        Main.dust[runeWizardCastDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.Necromancer || NPC.type == NPCID.NecromancerArmored)
                    {
                        int necromancerCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.ShadowbeamStaff, 0f, 0f, 0, default(Color), 1f);
                        Dust dust = Main.dust[necromancerCastDust];
                        dust.velocity *= 2f;
                        Main.dust[necromancerCastDust].scale = 1.4f;
                    }
                    else if (NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite)
                    {
                        int diabolistCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.InfernoFork, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[diabolistCastDust];
                        dust.velocity *= 3f;
                        Main.dust[diabolistCastDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
                    {
                        int raggedCasterCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SpectreStaff, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[raggedCasterCastDust];
                        dust.velocity *= 3f;
                        Main.dust[raggedCasterCastDust].noGravity = true;
                    }
                    else if (NPC.type == NPCID.DesertDjinn)
                    {
                        int desertSpiritCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[desertSpiritCastDust];
                        dust.velocity *= 3f;
                        Main.dust[desertSpiritCastDust].noGravity = true;
                    }
                    else
                    {
                        int fireImpCastDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
                        Dust dust = Main.dust[fireImpCastDust];
                        dust.velocity *= 3f;
                        Main.dust[fireImpCastDust].noGravity = true;
                    }
                }
            }

            if (NPC.justHit)
                NPC.ai[0] = (NPC.type == NPCID.RuneWizard && Main.zenithWorld) ? 5f : CalamityWorld.revenge ? 2f : 1f;

            NPC.ai[0] += (NPC.type == NPCID.RuneWizard && Main.zenithWorld) ? 5f : CalamityWorld.revenge ? 2f : 1f;

            if (NPC.type == NPCID.Necromancer || NPC.type == NPCID.NecromancerArmored)
            {
                if (NPC.ai[0] % 50f == 0f && NPC.ai[0] <= 250f)
                {
                    NPC.ai[1] = 55f;
                    NPC.netUpdate = true;
                }

                if (NPC.ai[0] >= 400f)
                    NPC.ai[0] = 700f;
            }
            else if (NPC.type == NPCID.RuneWizard)
            {
                if (NPC.ai[0] == 80f || NPC.ai[0] == 150f || NPC.ai[0] == 230f || NPC.ai[0] == 300f || NPC.ai[0] == 380f || NPC.ai[0] == 450f)
                {
                    NPC.ai[1] = 55f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.type == NPCID.DesertDjinn)
            {
                if (NPC.ai[0] == 180f)
                {
                    NPC.ai[1] = 181f;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
            {
                if (NPC.ai[0] == 20f || NPC.ai[0] == 40f || NPC.ai[0] == 60f || NPC.ai[0] == 120f || NPC.ai[0] == 140f || NPC.ai[0] == 160f || NPC.ai[0] == 220f || NPC.ai[0] == 240f || NPC.ai[0] == 260f)
                {
                    NPC.ai[1] = 55f;
                    NPC.netUpdate = true;
                }

                if (NPC.ai[0] >= 460f)
                    NPC.ai[0] = 700f;
            }
            else
            {
                if (Main.getGoodWorld && NPC.type == NPCID.FireImp)
                {
                    if (NPC.AnyNPCs(NPCID.WallofFlesh))
                    {
                        NPC.ai[0] += 1f;
                        if (NPC.ai[0] % 2f == 1f)
                            NPC.ai[0] -= 1f;
                    }
                }

                if (NPC.ai[0] % 100f == 0f && NPC.ai[0] <= 300f)
                {
                    NPC.ai[1] = 55f;
                    NPC.netUpdate = true;
                }
            }

            if ((NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite) && NPC.ai[0] > 400f)
                NPC.ai[0] = 650f;

            if (NPC.type == NPCID.DesertDjinn && NPC.ai[0] >= 360f)
                NPC.ai[0] = 650f;

            if (NPC.ai[0] >= 650f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[0] = CalamityWorld.revenge ? 2f : 1f;
                int targetTileX = (int)Main.player[NPC.target].position.X / 16;
                int targetTileY = (int)Main.player[NPC.target].position.Y / 16;
                Vector2 chosenTile = Vector2.Zero;
                if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile, targetTileX, targetTileY))
                {
                    NPC.ai[1] = 20f;
                    NPC.ai[2] = chosenTile.X;
                    NPC.ai[3] = chosenTile.Y;
                }

                NPC.netUpdate = true;
            }

            if (NPC.ai[1] > 0f)
            {
                NPC.ai[1] -= 1f;
                if (NPC.type == NPCID.DesertDjinn)
                {
                    if (NPC.ai[1] % 30f == 0f && NPC.ai[1] / 30f < 5f)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Point spiritCenter = NPC.Center.ToTileCoordinates();
                            Point targetCenter = Main.player[NPC.target].Center.ToTileCoordinates();
                            Vector2 targetDirection = Main.player[NPC.target].Center - NPC.Center;
                            int randProjRadius = 6;
                            int spiritSpawnRadius = 6;
                            int targetSpawnRadius = 0;
                            int solidTileCheckRadius = 2;
                            int projSpawnTries = 0;
                            bool targetTooFar = false;
                            if (targetDirection.Length() > 2000f)
                                targetTooFar = true;

                            while (!targetTooFar)
                            {
                                if (projSpawnTries >= 50)
                                    break;

                                projSpawnTries++;
                                int spiritProjSpawnX = Main.rand.Next(targetCenter.X - randProjRadius, targetCenter.X + randProjRadius + 1);
                                int spiritProjSpawnY = Main.rand.Next(targetCenter.Y - randProjRadius, targetCenter.Y + randProjRadius + 1);
                                if ((spiritProjSpawnY < targetCenter.Y - targetSpawnRadius || spiritProjSpawnY > targetCenter.Y + targetSpawnRadius || spiritProjSpawnX < targetCenter.X - targetSpawnRadius || spiritProjSpawnX > targetCenter.X + targetSpawnRadius) && (spiritProjSpawnY < spiritCenter.Y - spiritSpawnRadius || spiritProjSpawnY > spiritCenter.Y + spiritSpawnRadius || spiritProjSpawnX < spiritCenter.X - spiritSpawnRadius || spiritProjSpawnX > spiritCenter.X + spiritSpawnRadius) && !Main.tile[spiritProjSpawnX, spiritProjSpawnY].HasUnactuatedTile)
                                {
                                    bool canSpawnProj = true;
                                    if (canSpawnProj && Main.tile[spiritProjSpawnX, spiritProjSpawnY].LiquidType == LiquidID.Lava)
                                        canSpawnProj = false;
                                    if (canSpawnProj && Collision.SolidTiles(spiritProjSpawnX - solidTileCheckRadius, spiritProjSpawnX + solidTileCheckRadius, spiritProjSpawnY - solidTileCheckRadius, spiritProjSpawnY + solidTileCheckRadius))
                                        canSpawnProj = false;

                                    if (canSpawnProj)
                                    {
                                        int proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), new Vector2((float)(spiritProjSpawnX * 16 + 8), (float)(spiritProjSpawnY * 16 + 8)), Vector2.Zero, ProjectileID.DesertDjinnCurse, 0, 1f, Main.myPlayer, (float)NPC.target, 0f).identity;
                                        if (CalamityWorld.death)
                                        {
                                            Main.projectile[proj].Calamity().extraUpdatesToSync = 1;
                                            if (Main.dedServ)
                                            {
                                                Main.projectile[proj].netSpam = 0;
                                                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (NPC.ai[1] == 25f)
                {
                    if (NPC.type >= NPCID.RaggedCaster && NPC.type <= NPCID.DiabolistWhite)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            float dungeonCasterProjSpeed = CalamityWorld.death ? 8f : 6f;
                            if (NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite)
                                dungeonCasterProjSpeed = CalamityWorld.death ? 10f : 8f;
                            if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
                                dungeonCasterProjSpeed = CalamityWorld.death ? 5f : 4f;

                            Vector2 dungeonCasterPos = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y);
                            float dungeonCasterTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - dungeonCasterPos.X;
                            float dungeonCasterTargetY = Main.player[NPC.target].position.Y + (float)Main.player[NPC.target].height * 0.5f - dungeonCasterPos.Y;
                            float dungeonCasterTargetDist = (float)Math.Sqrt((double)(dungeonCasterTargetX * dungeonCasterTargetX + dungeonCasterTargetY * dungeonCasterTargetY));
                            dungeonCasterTargetDist = dungeonCasterProjSpeed / dungeonCasterTargetDist;
                            dungeonCasterTargetX *= dungeonCasterTargetDist;
                            dungeonCasterTargetY *= dungeonCasterTargetDist;

                            int damage = 16;
                            int projType = ProjectileID.ShadowBeamHostile;
                            if (NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite)
                            {
                                projType = ProjectileID.InfernoHostileBolt;
                                damage = 32;
                            }
                            if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
                            {
                                projType = ProjectileID.LostSoulHostile;
                                damage = 32;
                            }

                            int dungeonCasterProj = Projectile.NewProjectile(NPC.GetSource_FromAI(), dungeonCasterPos.X, dungeonCasterPos.Y, dungeonCasterTargetX, dungeonCasterTargetY, projType, damage, 0f, Main.myPlayer);
                            Main.projectile[dungeonCasterProj].timeLeft = 300;
                            if (projType == ProjectileID.InfernoHostileBolt)
                            {
                                Main.projectile[dungeonCasterProj].ai[0] = Main.player[NPC.target].Center.X;
                                Main.projectile[dungeonCasterProj].ai[1] = Main.player[NPC.target].Center.Y;
                                Main.projectile[dungeonCasterProj].netUpdate = true;
                            }

                            NPC.localAI[0] = 0f;
                        }
                    }
                    else
                    {
                        if (NPC.type != NPCID.RuneWizard)
                            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (NPC.type == NPCID.GoblinSorcerer || NPC.type == NPCID.Tim)
                            {
                                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width / 2, (int)NPC.position.Y - 8, NPCID.ChaosBall);
                            }
                            else if (NPC.type == NPCID.DarkCaster)
                            {
                                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width / 2, (int)NPC.position.Y - 8, NPCID.WaterSphere);
                            }
                            else if (NPC.type == NPCID.RuneWizard)
                            {
                                float runeWizardProjSpeed = CalamityWorld.death ? 12f : 10f;
                                Vector2 vector14 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                                float runeWizardTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - vector14.X;
                                float runeWizardTargetY = Main.player[NPC.target].position.Y + (float)Main.player[NPC.target].height * 0.5f - vector14.Y;
                                float runeWizardTargetDist = (float)Math.Sqrt((double)(runeWizardTargetX * runeWizardTargetX + runeWizardTargetY * runeWizardTargetY));
                                runeWizardTargetDist = runeWizardProjSpeed / runeWizardTargetDist;
                                runeWizardTargetX *= runeWizardTargetDist;
                                runeWizardTargetY *= runeWizardTargetDist;
                                int runeWizardProj = Projectile.NewProjectile(NPC.GetSource_FromAI(), vector14.X, vector14.Y, runeWizardTargetX, runeWizardTargetY, ProjectileID.RuneBlast, 40, 0f, Main.myPlayer);
                                Main.projectile[runeWizardProj].timeLeft = 300;
                                NPC.localAI[0] = 0f;
                            }
                            else
                                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X + NPC.width / 2 + NPC.direction * 8, (int)NPC.position.Y + 20, NPCID.BurningSphere);
                        }
                    }
                }
            }

            if (NPC.type == NPCID.GoblinSorcerer || NPC.type == NPCID.Tim)
            {
                if (Main.rand.NextBool(5))
                {
                    int shadowflameSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.Shadowflame, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 1.5f);
                    Dust dust = Main.dust[shadowflameSpawnDust];
                    dust.noGravity = true;
                    dust.velocity.X *= 0.5f;
                    dust.velocity.Y = -2f;
                }
            }
            else if (NPC.type == NPCID.DarkCaster)
            {
                if (!Main.rand.NextBool(3))
                {
                    int waterSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.DungeonWater, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 0.9f);
                    Dust dust = Main.dust[waterSpawnDust];
                    dust.noGravity = true;
                    dust.velocity.X *= 0.3f;
                    dust.velocity.Y *= 0.2f;
                    dust.velocity.Y -= 1f;
                }
            }
            else
            {
                if (NPC.type == NPCID.RuneWizard)
                {
                    int runeWizardDustAmt = 1;
                    if (NPC.alpha == 255)
                        runeWizardDustAmt = 2;

                    for (int r = 0; r < runeWizardDustAmt; r++)
                    {
                        if (Main.rand.Next(255) > 255 - NPC.alpha)
                        {
                            int runeSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.RuneWizard, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 1.2f);
                            Dust dust = Main.dust[runeSpawnDust];
                            dust.noGravity = true;
                            dust.velocity.X *= (0.1f + (float)Main.rand.Next(30) * 0.01f);
                            dust.velocity.Y *= (0.1f + (float)Main.rand.Next(30) * 0.01f);
                            dust.scale *= 1f + (float)Main.rand.Next(6) * 0.1f;
                        }
                    }

                    return false;
                }

                if (NPC.type == NPCID.Necromancer || NPC.type == NPCID.NecromancerArmored)
                {
                    if (Main.rand.NextBool())
                    {
                        int necroSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.ShadowbeamStaff, 0f, 0f, 0, default(Color), 1f);
                        Dust dust = Main.dust[necroSpawnDust];
                        dust.velocity.X *= 0.5f;
                        dust.velocity.Y *= 0.5f;
                    }
                }
                else if (NPC.type == NPCID.DiabolistRed || NPC.type == NPCID.DiabolistWhite)
                {
                    if (Main.rand.NextBool())
                    {
                        int flameSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.InfernoFork, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 1f);
                        Dust dust = Main.dust[flameSpawnDust];
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                        dust.velocity.Y -= 0.7f;
                        return false;
                    }
                }
                else if (NPC.type == NPCID.RaggedCaster || NPC.type == NPCID.RaggedCasterOpenCoat)
                {
                    if (Main.rand.NextBool())
                    {
                        int ghostSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.SpectreStaff, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 0.1f);
                        Dust dust = Main.dust[ghostSpawnDust];
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                        dust.fadeIn = 1.2f;
                    }
                }
                else
                {
                    if (NPC.type == NPCID.DesertDjinn)
                    {
                        Lighting.AddLight(NPC.Top, 0.6f, 0.6f, 0.3f);
                        return false;
                    }

                    if (Main.rand.NextBool())
                    {
                        int desertSpawnDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + 2f), NPC.width, NPC.height, DustID.Torch, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 2f);
                        Dust dust = Main.dust[desertSpawnDust];
                        dust.noGravity = true;
                        dust.velocity.X *= 1f;
                        dust.velocity.Y *= 1f;
                    }
                }
            }

            return false;
        }
    }
}
