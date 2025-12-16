using System;
using System.Collections.Generic;
using CalamityMod.Events;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public class CultistAI : VanillaAIOverride
    {
        // Vanilla values
        public static int CloneFireballDamage = 18; // 72
        public static int FireballDamage = 20; // 80
        public static int IceMistDamage = 25; // 100
        public static int LightningDamage = 30; // 120
        public static int DoomDamage = 45; // 180 (Also fixes Master Mode scaling)

        public override bool AI(Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = NPC.Calamity();

            // Chant sound
            if (NPC.ai[0] != -1f && Main.rand.NextBool(1000))
            {
                SoundStyle chantSound = Utils.SelectRandom(Main.rand, new SoundStyle[]
                {
                SoundID.Zombie88,
                SoundID.Zombie89,
                SoundID.Zombie90,
                SoundID.Zombie91
                });

                SoundEngine.PlaySound(chantSound, NPC.Center);
            }

            // Percent life remaining
            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Phases
            bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
            bool phase2 = lifeRatio < 0.85f || death;
            bool phase3 = lifeRatio < 0.7f || death;
            bool phase4 = lifeRatio < (death ? 0.8f : 0.55f);
            bool phase5 = lifeRatio < (death ? 0.6f : 0.4f);
            bool phase6 = lifeRatio < (death ? 0.4f : 0.25f);
            bool phase7 = death && lifeRatio < 0.2f;
            bool phase8 = death && lifeRatio < 0.1f;

            // Variables
            bool isCultist = NPC.type == NPCID.CultistBoss;
            bool dontTakeDamage = false;

            float predictionDistance = 480f;
            float distanceAboveTarget = -240f;
            float moveSpeed = death ? 300f : 75f;

            int iceMistDamage = isCultist ? IceMistDamage : 0;
            int fireballDamage = isCultist ? FireballDamage : CloneFireballDamage;
            int lightningDamage = isCultist ? LightningDamage : 0;

            int iceMistFireRate = phase2 ? 50 : 60;
            float iceMistSpeed = (phase6 ? 12f : 10f) + (death ? (1f - lifeRatio) : 0f);
            int iceMistAmt = phase3 ? 2 : 1;
            int fireballFireRate = (phase5 ? 10 : 12) - (death ? 6 : 0);
            float fireballSpeed = ((phase7 ? 8f : phase6 ? 7f : 6f) + (death ? (1f - lifeRatio) : 0f)) * (isCultist ? 1f : 0.5f);
            int fireballAmt = death ? 20 : 4;
            int lightningOrbPhaseTime = phase2 ? 90 : 120;
            int ancientLightSpawnRate = phase7 ? 20 : phase4 ? 25 : 30;
            int ancientLightAmt = phase7 ? 4 : phase4 ? 3 : 2;
            int ancientDoomLimit = 10;
            int idleTime = phase8 ? 20 : phase7 ? 30 : phase3 ? 35 : 40;
            float timeToFinishRitual = phase8 ? 180f : phase7 ? 240f : phase5 ? 300f : 360f;

            if (Main.getGoodWorld)
            {
                iceMistFireRate = 40;
                iceMistSpeed = 15f;
                fireballFireRate = 8;
                fireballSpeed *= 1.25f;
                lightningOrbPhaseTime = 60;
                ancientLightSpawnRate = 10;
                ancientLightAmt = 5;
                idleTime = 20;
            }

            // Center and target
            Player player = Main.player[NPC.target];
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || player.dead || !player.active || Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles)
            {
                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.faceTarget = false;
                CalamityUtils.CalamityTargeting(NPC, options);

                player = Main.player[NPC.target];
                NPC.netUpdate = true;
            }

            // Enrage
            if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
            {
                calamityGlobalNPC.newAI[0] += 1f;
                if (calamityGlobalNPC.newAI[0] >= 120f)
                {
                    calamityGlobalNPC.newAI[0] = 120f;
                    iceMistSpeed = 16f;
                    iceMistFireRate = 15;
                    lightningOrbPhaseTime = 30;
                    ancientLightSpawnRate = 5;
                    idleTime = 10;
                    timeToFinishRitual = 120f;
                }
            }
            else
            {
                if (calamityGlobalNPC.newAI[0] > 0f)
                    calamityGlobalNPC.newAI[0] -= 1f;
            }

            // Cultist clone AI
            if (!isCultist)
            {
                if (NPC.ai[3] < 0f || !Main.npc[(int)NPC.ai[3]].active || Main.npc[(int)NPC.ai[3]].type != NPCID.CultistBoss)
                {
                    NPC.life = 0;
                    NPC.HitEffect(0, 10.0);
                    NPC.active = false;
                    return false;
                }

                NPC.ai[0] = Main.npc[(int)NPC.ai[3]].ai[0];
                NPC.ai[1] = Main.npc[(int)NPC.ai[3]].ai[1];
                dontTakeDamage = true;
            }

            // Stop spawning ritual if hit
            else if (NPC.ai[0] == 5f && NPC.ai[1] >= 120f && NPC.ai[1] < timeToFinishRitual && NPC.justHit)
            {
                NPC.ai[0] = 0f;
                NPC.ai[1] = 0f;
                NPC.ai[3] += 1f;
                NPC.velocity = Vector2.Zero;

                CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                options.faceTarget = false;
                CalamityUtils.CalamityTargeting(NPC, options);

                NPC.netUpdate = true;
                Main.projectile[(int)NPC.ai[2]].ai[1] = -1f;
                Main.projectile[(int)NPC.ai[2]].netUpdate = true;
            }

            // Despawn
            if (player.dead || !player.active || Vector2.Distance(player.Center, NPC.Center) > CalamityGlobalNPC.CatchUpDistance350Tiles)
            {
                NPC.life = 0;
                NPC.HitEffect(0, 10.0);
                NPC.active = false;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, NPC.whoAmI, -1f);

                for (int j = 0; j < Main.maxNPCs; j++)
                {
                    if (Main.npc[j].active && Main.npc[j].type == NPCID.CultistBossClone && Main.npc[j].ai[3] == NPC.whoAmI)
                    {
                        Main.npc[j].life = 0;
                        Main.npc[j].HitEffect(0, 10.0);
                        Main.npc[j].active = false;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, NPC.whoAmI, -1f);
                    }
                }
            }

            // Clones set to Cultist phase
            float clonePhase = NPC.ai[3];

            // Spawn and play sound
            if (NPC.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(SoundID.Zombie89, NPC.Center);
                NPC.localAI[0] = 1f;
                NPC.alpha = 255;
                NPC.rotation = 0f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[0] = -1f;
                    NPC.netUpdate = true;
                }
            }

            // Appear and do weird ritual shit with tablet
            if (NPC.ai[0] == -1f)
            {
                NPC.alpha -= 5;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 420f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.netUpdate = true;
                }
                else if (NPC.ai[1] > 360f)
                {
                    NPC.velocity *= 0.95f;

                    if (NPC.localAI[2] != 13f)
                        SoundEngine.PlaySound(SoundID.Zombie105, NPC.Center);

                    NPC.localAI[2] = 13f;
                }
                else if (NPC.ai[1] > 300f)
                {
                    NPC.velocity = -Vector2.UnitY;
                    NPC.localAI[2] = 10f;
                }
                else if (NPC.ai[1] > 120f)
                    NPC.localAI[2] = 1f;
                else
                    NPC.localAI[2] = 0f;

                dontTakeDamage = true;
            }

            // Phase switch
            if (NPC.ai[0] == 0f)
            {
                if (NPC.ai[1] == 0f)
                {
                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.faceTarget = false;
                    CalamityUtils.CalamityTargeting(NPC, options);
                }

                NPC.localAI[2] = 10f;

                int facePlayerDirection = Math.Sign(player.Center.X - NPC.Center.X);
                if (facePlayerDirection != 0)
                    NPC.direction = NPC.spriteDirection = facePlayerDirection;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= idleTime & isCultist)
                {
                    // Phase switch
                    int phase = 0;
                    switch ((int)NPC.ai[3])
                    {
                        // Move to target
                        case 0:
                        case 2:
                        case 4:
                        case 6:
                        case 8:
                        case 10:
                        case 12:
                        case 14:
                        case 16:
                        case 18:
                        case 20:
                            phase = 0;
                            break;

                        // Fireball
                        case 1:
                        case 15:
                            phase = 1;
                            break;

                        // Ancient Light
                        case 3:
                        case 11:
                            phase = 5;
                            break;

                        // Lightning Orb
                        case 5:
                        case 13:
                            phase = 3;
                            break;

                        // Ice Mist
                        case 7:
                        case 17:
                            phase = 2;
                            break;

                        // Ancient Doom
                        case 9:
                        case 19:
                            // Pick a different random phase if too many Ancient Dooms are active
                            int[] attackPhases = new int[4] { 1, 2, 3, 5 };
                            phase = NPC.CountNPCS(NPCID.AncientDoom) < ancientDoomLimit ? 6 : attackPhases[Main.rand.Next(attackPhases.Length)];
                            break;

                        // Dragon Summon
                        case 21:
                            phase = 4;
                            NPC.ai[3] = -1f;
                            break;

                        default:
                            NPC.ai[3] = -1f;
                            break;
                    }

                    // Set AI phase
                    switch (phase)
                    {
                        // Movement
                        case 0:
                            // Set a location to move to
                            Vector2 predictionVector = new Vector2(0f + player.velocity.SafeNormalize(Vector2.Zero).X * predictionDistance, distanceAboveTarget);
                            float moveDistance = (float)Math.Ceiling((player.Center + predictionVector - NPC.Center).Length() / moveSpeed);
                            if (moveDistance == 0f)
                                moveDistance = 1f;

                            // Add self and clones to list
                            List<int> list2 = new List<int>();
                            int cloneAmt = 0;
                            list2.Add(NPC.whoAmI);
                            for (int k = 0; k < Main.maxNPCs; k++)
                            {
                                if (Main.npc[k].active && Main.npc[k].type == NPCID.CultistBossClone && Main.npc[k].ai[3] == NPC.whoAmI)
                                    list2.Add(k);
                            }

                            // Move self and clones to location
                            bool cloneAmtIsEven = list2.Count % 2 == 0;
                            foreach (int current2 in list2)
                            {
                                NPC nPC2 = Main.npc[current2];
                                Vector2 center2 = nPC2.Center;
                                float cloneOffset = (cloneAmt + cloneAmtIsEven.ToInt() + 1) / 2 * MathHelper.TwoPi * 0.4f / list2.Count;
                                if (cloneAmt % 2 == 1)
                                    cloneOffset *= -1f;
                                if (list2.Count == 1)
                                    cloneOffset = 0f;

                                Vector2 cloneRotation = new Vector2(0f, -1f).RotatedBy(cloneOffset) * new Vector2(150f, 200f);
                                Vector2 finalClonePos = player.Center + (Vector2.UnitX * predictionVector.X) + cloneRotation - center2;
                                nPC2.ai[0] = 1f;
                                nPC2.ai[1] = moveDistance;
                                nPC2.velocity = finalClonePos / moveDistance * 2f;
                                if (NPC.whoAmI >= nPC2.whoAmI)
                                    nPC2.position -= nPC2.velocity;

                                nPC2.netUpdate = true;
                                cloneAmt++;
                            }
                            break;

                        // Fireball
                        case 1:
                            NPC.ai[0] = 3f;
                            NPC.ai[1] = 0f;
                            break;

                        // Ice Mist
                        case 2:
                            NPC.ai[0] = 2f;
                            NPC.ai[1] = 0f;
                            break;

                        // Lightning Orb
                        case 3:
                            NPC.ai[0] = 4f;
                            NPC.ai[1] = 0f;
                            break;

                        // Dragon Summon
                        case 4:
                            NPC.ai[0] = 5f;
                            NPC.ai[1] = 0f;
                            break;

                        // Ancient Light
                        case 5:
                            NPC.ai[0] = 7f;
                            NPC.ai[1] = 0f;
                            break;

                        // Ancient Doom
                        case 6:
                            NPC.ai[0] = 8f;
                            NPC.ai[1] = 0f;
                            break;

                        default:
                            break;
                    }

                    NPC.netUpdate = true;
                }
            }

            // Movement, then switch to a different attack
            else if (NPC.ai[0] == 1f)
            {
                NPC.localAI[2] = 10f;

                if (NPC.ai[1] % 2f != 0f && NPC.ai[1] != 1f)
                    NPC.position -= NPC.velocity;

                NPC.ai[1] -= 1f;
                if (NPC.ai[1] <= 0f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Ice Mist
            else if (NPC.ai[0] == 2f)
            {
                NPC.localAI[2] = 11f;

                Vector2 vec = Vector2.Normalize(player.Center - NPC.Center);
                if (vec.HasNaNs())
                    vec = new Vector2(NPC.direction, 0f);

                if ((NPC.ai[1] >= 4f & isCultist) && (int)(NPC.ai[1] - 4f) % iceMistFireRate == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        List<int> list3 = new List<int>();
                        for (int l = 0; l < Main.maxNPCs; l++)
                        {
                            if (Main.npc[l].active && Main.npc[l].type == NPCID.CultistBossClone && Main.npc[l].ai[3] == NPC.whoAmI)
                                list3.Add(l);
                        }

                        foreach (int current3 in list3)
                        {
                            NPC nPC3 = Main.npc[current3];
                            Vector2 center3 = nPC3.Center;

                            int cloneFacePlayerDirection = Math.Sign(player.Center.X - center3.X);
                            if (cloneFacePlayerDirection != 0)
                                nPC3.direction = nPC3.spriteDirection = cloneFacePlayerDirection;

                            vec = Vector2.Normalize(player.Center - center3);
                            if (vec.HasNaNs())
                                vec = new Vector2(NPC.direction, 0f);

                            Vector2 shadowFireballDirection = center3 + new Vector2(NPC.direction * 30, 12f);
                            Vector2 shadowFireballVelocity = vec * (fireballSpeed + (float)Main.rand.NextDouble());
                            shadowFireballVelocity = shadowFireballVelocity.RotatedByRandom(Math.PI / 6D);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), shadowFireballDirection, shadowFireballVelocity, ProjectileID.CultistBossFireBallClone, fireballDamage, 0f, Main.myPlayer);
                        }
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        vec = Vector2.Normalize(player.Center - NPC.Center);
                        if (vec.HasNaNs())
                            vec = new Vector2(NPC.direction, 0f);

                        Vector2 iceMistDirection = NPC.Center + new Vector2(NPC.direction * 30, 12f);
                        Vector2 iceMistVelocity = vec * iceMistSpeed;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), iceMistDirection, iceMistVelocity, ProjectileID.CultistBossIceMist, iceMistDamage, 0f, Main.myPlayer, 0f, 1f);
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= (4 + iceMistFireRate * iceMistAmt))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Fireballs
            else if (NPC.ai[0] == 3f)
            {
                NPC.localAI[2] = 11f;

                Vector2 playerDirection = Vector2.Normalize(player.Center - NPC.Center);
                if (playerDirection.HasNaNs())
                    playerDirection = new Vector2(NPC.direction, 0f);

                if ((NPC.ai[1] >= 4f & isCultist) && (int)(NPC.ai[1] - 4f) % fireballFireRate == 0)
                {
                    if ((int)(NPC.ai[1] - 4f) / fireballFireRate == 2)
                    {
                        List<int> list4 = new List<int>();
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone && Main.npc[i].ai[3] == NPC.whoAmI)
                                list4.Add(i);
                        }

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            foreach (int current4 in list4)
                            {
                                NPC nPC4 = Main.npc[current4];
                                Vector2 center4 = nPC4.Center;

                                int cloneFireballFaceDirection = Math.Sign(player.Center.X - center4.X);
                                if (cloneFireballFaceDirection != 0)
                                    nPC4.direction = nPC4.spriteDirection = cloneFireballFaceDirection;

                                playerDirection = Vector2.Normalize(player.Center - center4);
                                if (playerDirection.HasNaNs())
                                    playerDirection = new Vector2(NPC.direction, 0f);

                                Vector2 shadowFireballDirection = center4 + new Vector2(NPC.direction * 30, 12f);
                                Vector2 shadowFireballVelocity = playerDirection * (fireballSpeed + (float)Main.rand.NextDouble());
                                shadowFireballVelocity = shadowFireballVelocity.RotatedByRandom(Math.PI / 6D);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), shadowFireballDirection, shadowFireballVelocity, ProjectileID.CultistBossFireBallClone, CloneFireballDamage, 0f, Main.myPlayer);
                            }
                        }
                    }

                    int cultistFireballFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
                    if (cultistFireballFaceDirection != 0)
                        NPC.direction = NPC.spriteDirection = cultistFireballFaceDirection;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        playerDirection = Vector2.Normalize(player.Center - NPC.Center);
                        if (playerDirection.HasNaNs())
                            playerDirection = new Vector2(NPC.direction, 0f);

                        Vector2 fireballDirection = NPC.Center + new Vector2(NPC.direction * 30, 12f);
                        Vector2 fireballVelocity = playerDirection * (fireballSpeed + (float)Main.rand.NextDouble() * 2f);
                        fireballVelocity = fireballVelocity.RotatedByRandom(Math.PI / 6D);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), fireballDirection, fireballVelocity, ProjectileID.CultistBossFireBall, fireballDamage, 0f, Main.myPlayer);
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= (4 + fireballFireRate * fireballAmt))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Lightning Orb
            else if (NPC.ai[0] == 4f)
            {
                if (isCultist)
                    NPC.localAI[2] = 12f;
                else
                    NPC.localAI[2] = 11f;

                if ((NPC.ai[1] == 20f & isCultist) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    List<int> list5 = new List<int>();
                    for (int j = 0; j < Main.maxNPCs; j++)
                    {
                        if (Main.npc[j].active && Main.npc[j].type == NPCID.CultistBossClone && Main.npc[j].ai[3] == NPC.whoAmI)
                            list5.Add(j);
                    }

                    foreach (int current5 in list5)
                    {
                        NPC nPC5 = Main.npc[current5];
                        Vector2 center5 = nPC5.Center;

                        int clonePlayerFaceDirection = Math.Sign(player.Center.X - center5.X);
                        if (clonePlayerFaceDirection != 0)
                            nPC5.direction = nPC5.spriteDirection = clonePlayerFaceDirection;

                        Vector2 playerDirection = Vector2.Normalize(player.Center - center5);
                        if (playerDirection.HasNaNs())
                            playerDirection = new Vector2(NPC.direction, 0f);

                        Vector2 shadowFireballDirection = center5 + new Vector2(NPC.direction * 30, 12f);
                        Vector2 shadowFireballVelocity = playerDirection * (fireballSpeed + (float)Main.rand.NextDouble());
                        shadowFireballVelocity = shadowFireballVelocity.RotatedByRandom(Math.PI / 6D);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), shadowFireballDirection, shadowFireballVelocity, ProjectileID.CultistBossFireBallClone, fireballDamage, 0f, Main.myPlayer);
                    }

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X, NPC.Center.Y + (death ? 210f : -100f), 0f, 0f, ProjectileID.CultistBossLightningOrb, lightningDamage, 0f, Main.myPlayer);
                    if (death)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X + 210f, NPC.Center.Y - 210f, 0f, 0f, ProjectileID.CultistBossLightningOrb, lightningDamage, 0f, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center.X - 210f, NPC.Center.Y - 210f, 0f, 0f, ProjectileID.CultistBossLightningOrb, lightningDamage, 0f, Main.myPlayer);
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= (20 + lightningOrbPhaseTime))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Spawn Clones, and Dragon or Cthulhu head
            else if (NPC.ai[0] == 5f)
            {
                NPC.localAI[2] = 10f;

                if (Vector2.Normalize(player.Center - NPC.Center).HasNaNs())
                    new Vector2(NPC.direction, 0f);

                if (NPC.ai[1] >= 0f && NPC.ai[1] < 30f)
                {
                    dontTakeDamage = true;
                    float cultistAlphaControl = (NPC.ai[1] - 0f) / 30f;
                    NPC.alpha = (int)(cultistAlphaControl * 255f);
                }
                else if (NPC.ai[1] >= 30f && NPC.ai[1] < 90f)
                {
                    if ((NPC.ai[1] == 30f && Main.netMode != NetmodeID.MultiplayerClient) & isCultist)
                    {
                        NPC.localAI[1] += 1f;

                        Vector2 spinningpoint = new Vector2(180f, 0f);

                        List<int> list6 = new List<int>();
                        for (int k = 0; k < Main.maxNPCs; k++)
                        {
                            if (Main.npc[k].active && Main.npc[k].type == NPCID.CultistBossClone && Main.npc[k].ai[3] == NPC.whoAmI)
                                list6.Add(k);
                        }

                        int potentialExtraClones = 6 - list6.Count;
                        if (potentialExtraClones > 2)
                            potentialExtraClones = 2;

                        int newCloneAmt = list6.Count + potentialExtraClones + 1;
                        float[] array = new float[newCloneAmt];
                        for (int cloneInc = 0; cloneInc < array.Length; cloneInc++)
                            array[cloneInc] = Vector2.Distance(NPC.Center + spinningpoint.RotatedBy(cloneInc * MathHelper.TwoPi / newCloneAmt - MathHelper.PiOver2), player.Center);

                        int rotateDistance = 0;
                        for (int j = 1; j < array.Length; j++)
                        {
                            if (array[rotateDistance] > array[j])
                                rotateDistance = j;
                        }

                        if (rotateDistance < newCloneAmt / 2)
                            rotateDistance += newCloneAmt / 2;
                        else
                            rotateDistance -= newCloneAmt / 2;

                        int clonesToSpawn = potentialExtraClones;
                        for (int k = 0; k < array.Length; k++)
                        {
                            if (rotateDistance != k)
                            {
                                Vector2 cloneRotation = NPC.Center + spinningpoint.RotatedBy(k * MathHelper.TwoPi / newCloneAmt - MathHelper.PiOver2);
                                if (clonesToSpawn-- > 0)
                                {
                                    int cloneSpawn = NPC.NewNPC(NPC.GetSource_FromAI(), (int)cloneRotation.X, (int)cloneRotation.Y + NPC.height / 2, NPCID.CultistBossClone, NPC.whoAmI);
                                    Main.npc[cloneSpawn].ai[3] = NPC.whoAmI;
                                    Main.npc[cloneSpawn].netUpdate = true;
                                    Main.npc[cloneSpawn].localAI[1] = NPC.localAI[1];
                                }
                                else
                                {
                                    int currentClone = list6[-clonesToSpawn - 1];
                                    Main.npc[currentClone].Center = cloneRotation;
                                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, currentClone);
                                }
                            }
                        }

                        NPC.ai[2] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileID.CultistRitual, 0, 0f, Main.myPlayer, 0f, NPC.whoAmI);
                        NPC.Center += spinningpoint.RotatedBy(rotateDistance * MathHelper.TwoPi / newCloneAmt - MathHelper.PiOver2);
                        NPC.netUpdate = true;
                        list6.Clear();
                    }

                    dontTakeDamage = true;
                    NPC.alpha = 255;

                    if (isCultist)
                    {
                        Vector2 ritualCenterDirection = Main.projectile[(int)NPC.ai[2]].Center;
                        ritualCenterDirection -= NPC.Center;
                        if (ritualCenterDirection == Vector2.Zero)
                            ritualCenterDirection = -Vector2.UnitY;

                        ritualCenterDirection.Normalize();

                        if (Math.Abs(ritualCenterDirection.Y) < 0.77f)
                            NPC.localAI[2] = 11f;
                        else if (ritualCenterDirection.Y < 0f)
                            NPC.localAI[2] = 12f;
                        else
                            NPC.localAI[2] = 10f;

                        int ritualFaceDirection = Math.Sign(ritualCenterDirection.X);
                        if (ritualFaceDirection != 0)
                            NPC.direction = NPC.spriteDirection = ritualFaceDirection;
                    }
                    else
                    {
                        Vector2 ritualCenterFailDirection = Main.projectile[(int)Main.npc[(int)NPC.ai[3]].ai[2]].Center;
                        ritualCenterFailDirection -= NPC.Center;
                        if (ritualCenterFailDirection == Vector2.Zero)
                            ritualCenterFailDirection = -Vector2.UnitY;

                        ritualCenterFailDirection.Normalize();

                        if (Math.Abs(ritualCenterFailDirection.Y) < 0.77f)
                            NPC.localAI[2] = 11f;
                        else if (ritualCenterFailDirection.Y < 0f)
                            NPC.localAI[2] = 12f;
                        else
                            NPC.localAI[2] = 10f;

                        int ritualFailFaceDirection = Math.Sign(ritualCenterFailDirection.X);
                        if (ritualFailFaceDirection != 0)
                            NPC.direction = NPC.spriteDirection = ritualFailFaceDirection;
                    }
                }
                else if (NPC.ai[1] >= 90f && NPC.ai[1] < 120f)
                {
                    dontTakeDamage = true;
                    float ritualAlphaControl = (NPC.ai[1] - 90f) / 30f;
                    NPC.alpha = 255 - (int)(ritualAlphaControl * 255f);
                }
                else if (NPC.ai[1] >= 120f && NPC.ai[1] < timeToFinishRitual)
                {
                    NPC.alpha = 0;

                    if (isCultist)
                    {
                        Vector2 ritualTimeAlmostUpCenterDirection = Main.projectile[(int)NPC.ai[2]].Center;
                        ritualTimeAlmostUpCenterDirection -= NPC.Center;
                        if (ritualTimeAlmostUpCenterDirection == Vector2.Zero)
                            ritualTimeAlmostUpCenterDirection = -Vector2.UnitY;

                        ritualTimeAlmostUpCenterDirection.Normalize();

                        if (Math.Abs(ritualTimeAlmostUpCenterDirection.Y) < 0.77f)
                            NPC.localAI[2] = 11f;
                        else if (ritualTimeAlmostUpCenterDirection.Y < 0f)
                            NPC.localAI[2] = 12f;
                        else
                            NPC.localAI[2] = 10f;

                        int ritualTimeAlmostUpFaceDirection = Math.Sign(ritualTimeAlmostUpCenterDirection.X);
                        if (ritualTimeAlmostUpFaceDirection != 0)
                            NPC.direction = NPC.spriteDirection = ritualTimeAlmostUpFaceDirection;
                    }
                    else
                    {
                        Vector2 ritualTimeUpCenterDirection = Main.projectile[(int)Main.npc[(int)NPC.ai[3]].ai[2]].Center;
                        ritualTimeUpCenterDirection -= NPC.Center;
                        if (ritualTimeUpCenterDirection == Vector2.Zero)
                            ritualTimeUpCenterDirection = -Vector2.UnitY;

                        ritualTimeUpCenterDirection.Normalize();

                        if (Math.Abs(ritualTimeUpCenterDirection.Y) < 0.77f)
                            NPC.localAI[2] = 11f;
                        else if (ritualTimeUpCenterDirection.Y < 0f)
                            NPC.localAI[2] = 12f;
                        else
                            NPC.localAI[2] = 10f;

                        int ritualTimeUpFaceDirection = Math.Sign(ritualTimeUpCenterDirection.X);
                        if (ritualTimeUpFaceDirection != 0)
                            NPC.direction = NPC.spriteDirection = ritualTimeUpFaceDirection;
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= timeToFinishRitual)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;

                    CalamityTargetingParameters options = CalamityTargetingParameters.BossDefaults;
                    options.faceTarget = false;
                    CalamityUtils.CalamityTargeting(NPC, options);

                    NPC.netUpdate = true;
                }
            }

            // Pause
            else if (NPC.ai[0] == 6f)
            {
                NPC.localAI[2] = 13f;

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= (idleTime * 3))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Ancient Light
            else if (NPC.ai[0] == 7f)
            {
                NPC.localAI[2] = 11f;

                Vector2 playerDirection = Vector2.Normalize(player.Center - NPC.Center);
                if (playerDirection.HasNaNs())
                    playerDirection = new Vector2(NPC.direction, 0f);

                if ((NPC.ai[1] >= 4f & isCultist) && (int)(NPC.ai[1] - 4f) % ancientLightSpawnRate == 0)
                {
                    if ((int)(NPC.ai[1] - 4f) / ancientLightSpawnRate == 2)
                    {
                        List<int> list7 = new List<int>();
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone && Main.npc[i].ai[3] == NPC.whoAmI)
                                list7.Add(i);
                        }

                        foreach (int current6 in list7)
                        {
                            NPC nPC6 = Main.npc[current6];
                            Vector2 center6 = nPC6.Center;

                            int cloneFaceDirection = Math.Sign(player.Center.X - center6.X);
                            if (cloneFaceDirection != 0)
                                nPC6.direction = nPC6.spriteDirection = cloneFaceDirection;
                        }
                    }

                    int cultistFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
                    if (cultistFaceDirection != 0)
                        NPC.direction = NPC.spriteDirection = cultistFaceDirection;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        playerDirection = Vector2.Normalize(player.Center - NPC.Center);
                        if (playerDirection.HasNaNs())
                            playerDirection = new Vector2(NPC.direction, 0f);

                        Vector2 ancientLightShootDirection = NPC.Center + new Vector2(NPC.direction * 30, 12f);
                        float scaleFactor = death ? 6f : 4f;

                        float ancientLightSpread = MathHelper.ToRadians(15f);
                        int ancientLightInc = 0;
                        float totalAncientLights = 5f;
                        while (ancientLightInc < totalAncientLights)
                        {
                            Vector2 ancientLightSpeed = playerDirection * scaleFactor;
                            ancientLightSpeed = ancientLightSpeed.RotatedBy(ancientLightSpread * ancientLightInc - (MathHelper.Pi / totalAncientLights * 2f - ancientLightSpread) / 2f);
                            float ai = (Main.rand.NextFloat() - 0.5f) * 0.3f * MathHelper.TwoPi / 60f;
                            int ancientLightProj = NPC.NewNPC(NPC.GetSource_FromAI(), (int)ancientLightShootDirection.X, (int)ancientLightShootDirection.Y + 7, NPCID.AncientLight, 0, 0f, ai, ancientLightSpeed.X, ancientLightSpeed.Y);
                            Main.npc[ancientLightProj].velocity = ancientLightSpeed;
                            ancientLightInc++;
                        }
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= (4 + ancientLightSpawnRate * ancientLightAmt))
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Ancient Doom
            else if (NPC.ai[0] == 8f)
            {
                NPC.localAI[2] = 13f;

                if ((NPC.ai[1] >= 4f & isCultist) && (int)(NPC.ai[1] - 4f) % 20f == 0f)
                {
                    List<int> list8 = new List<int>();
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        if (Main.npc[k].active && Main.npc[k].type == NPCID.CultistBossClone && Main.npc[k].ai[3] == NPC.whoAmI)
                            list8.Add(k);
                    }

                    int ancientDoomAmt = list8.Count + 1;
                    if (ancientDoomAmt > 2)
                        ancientDoomAmt = 2;

                    int ancientDoomFaceDirection = Math.Sign(player.Center.X - NPC.Center.X);
                    if (ancientDoomFaceDirection != 0)
                        NPC.direction = NPC.spriteDirection = ancientDoomFaceDirection;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (phase4)
                        {
                            // Spawn circle of Ancient Dooms around target
                            for (int i = 0; i < ancientDoomLimit; i++)
                            {
                                float ai2 = i * 120;
                                NPC.NewNPC(NPC.GetSource_FromAI(), (int)(player.Center.X + (float)(Math.Sin(i * 120) * 550)), (int)(player.Center.Y + (float)(Math.Cos(i * 120) * 550)),
                                    NPCID.AncientDoom, 0, NPC.whoAmI, 0f, ai2, 0f, Main.maxPlayers);
                            }
                        }
                        else
                        {
                            // Spawn Ancient Dooms randomly around the target
                            for (int i = 0; i < ancientDoomAmt; i++)
                            {
                                Point cultistCenterTileCoords = NPC.Center.ToTileCoordinates();
                                Point targetCenterTileCoords = Main.player[NPC.target].Center.ToTileCoordinates();
                                Vector2 targetDistanceDoom = Main.player[NPC.target].Center - NPC.Center;

                                int randSpawnOffset = 20;
                                int cultistCenterSpawnOffset = 3;
                                int targetCenterSpawnOffset = 7;
                                int tileCollisionRange = 2;
                                int spawnAttempts = 0;
                                bool doomSufficientlyFar = targetDistanceDoom.Length() > 2800f;
                                while (!doomSufficientlyFar && spawnAttempts < 100)
                                {
                                    spawnAttempts++;
                                    int ancientDoomSpawnX = Main.rand.Next(targetCenterTileCoords.X - randSpawnOffset, targetCenterTileCoords.X + randSpawnOffset + 1);
                                    int ancientDoomSpawnY = Main.rand.Next(targetCenterTileCoords.Y - randSpawnOffset, targetCenterTileCoords.Y + randSpawnOffset + 1);
                                    if ((ancientDoomSpawnY < targetCenterTileCoords.Y - targetCenterSpawnOffset || ancientDoomSpawnY > targetCenterTileCoords.Y + targetCenterSpawnOffset || ancientDoomSpawnX < targetCenterTileCoords.X - targetCenterSpawnOffset || ancientDoomSpawnX > targetCenterTileCoords.X + targetCenterSpawnOffset) && (ancientDoomSpawnY < cultistCenterTileCoords.Y - cultistCenterSpawnOffset || ancientDoomSpawnY > cultistCenterTileCoords.Y + cultistCenterSpawnOffset || ancientDoomSpawnX < cultistCenterTileCoords.X - cultistCenterSpawnOffset || ancientDoomSpawnX > cultistCenterTileCoords.X + cultistCenterSpawnOffset) && !Main.tile[ancientDoomSpawnX, ancientDoomSpawnY].HasUnactuatedTile)
                                    {
                                        bool notInsideTiles = true;
                                        if (notInsideTiles && Collision.SolidTiles(ancientDoomSpawnX - tileCollisionRange, ancientDoomSpawnX + tileCollisionRange, ancientDoomSpawnY - tileCollisionRange, ancientDoomSpawnY + tileCollisionRange))
                                            notInsideTiles = false;

                                        if (notInsideTiles)
                                        {
                                            NPC.NewNPC(NPC.GetSource_FromAI(), ancientDoomSpawnX * 16 + 8, ancientDoomSpawnY * 16 + 8, NPCID.AncientDoom, 0, NPC.whoAmI);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                NPC.ai[1] += 1f;
                if (NPC.ai[1] >= 64f)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[3] += 1f;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }

            // Set Clones to Cultist phase
            if (!isCultist)
                NPC.ai[3] = clonePhase;

            // Take damage or not
            NPC.dontTakeDamage = dontTakeDamage;
            NPC.chaseable = NPC.ai[0] != -1f && NPC.ai[0] != 5f;

            return false;
        }

        public class AncientLightAI : VanillaAIOverride
        {
            public override bool AI(Mod mod)
            {
                NPC.dontTakeDamage = true;

                // Slow and kill code
                if (NPC.ai[0] == -1f)
                {
                    // Slow down over time
                    if (NPC.velocity.Length() >= 0.2f)
                    {
                        NPC.velocity *= 0.96f;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                        NPC.position = NPC.oldPosition;

                        // Kill after 4 seconds
                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] >= 240f)
                        {
                            NPC.HitEffect(0, 9999.0);
                            NPC.active = false;
                        }
                    }

                    return false;
                }

                NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;

                // Set velocity and emit dust when spawned
                if (NPC.localAI[0] == 0f)
                {
                    NPC.localAI[0] = 1f;
                    NPC.velocity.X = NPC.ai[2];
                    NPC.velocity.Y = NPC.ai[3];
                    for (int i = 0; i < 13; i++)
                    {
                        int ancientLight = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AncientLight, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 90, default, 2.5f);
                        Main.dust[ancientLight].noGravity = true;
                        Main.dust[ancientLight].fadeIn = 1f;
                        Dust dust = Main.dust[ancientLight];
                        dust.velocity *= 4f;
                        Main.dust[ancientLight].noLight = true;
                    }
                }

                // Spawn dust
                for (int j = 0; j < 2; j++)
                {
                    if (Main.rand.Next(10 - (int)Math.Min(7f, NPC.velocity.Length())) < 1)
                    {
                        int ancientLight2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.AncientLight, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 90, default, 2.5f);
                        Main.dust[ancientLight2].noGravity = true;
                        Dust dust = Main.dust[ancientLight2];
                        dust.velocity *= 0.2f;
                        Main.dust[ancientLight2].fadeIn = 0.4f;
                        if (Main.rand.NextBool(6))
                        {
                            dust = Main.dust[ancientLight2];
                            dust.velocity *= 5f;
                            Main.dust[ancientLight2].noLight = true;
                        }
                        else
                            Main.dust[ancientLight2].velocity = NPC.DirectionFrom(Main.dust[ancientLight2].position) * Main.dust[ancientLight2].velocity.Length();
                    }
                }

                if (NPC.ai[0] >= 0f)
                {
                    // Triple damage if the Primordial Wyrm is alive
                    if (NPC.ai[0] == 0f)
                    {
                        if (CalamityGlobalNPC.adultEidolonWyrmHead != -1)
                        {
                            if (Main.npc[CalamityGlobalNPC.adultEidolonWyrmHead].active)
                                NPC.damage = (int)Math.Round(NPC.defDamage * PrimordialWyrmHead.LightDamageMult);
                        }
                    }

                    NPC.ai[0] += 1f;

                    float duration = 120f;

                    // Increase velocity for 1 second
                    if (NPC.ai[0] < duration - 60f)
                    {
                        if (NPC.velocity.Length() < 20f)
                            NPC.velocity *= 1.03f;
                    }

                    // Intersect velocity paths with other Ancient Lights for 1 second
                    if (NPC.ai[0] >= duration - 60f)
                        NPC.velocity = NPC.velocity.RotatedBy(NPC.ai[1]);

                    // Engage slow code
                    if (NPC.ai[0] >= duration)
                        NPC.ai[0] = -1f;
                }

                return false;
            }
        }

        public class AncientDoomAI : VanillaAIOverride
        {
            public override bool AI(Mod mod)
            {
                bool death = CalamityWorld.death || BossRushEvent.BossRushActive;
                NPC.damage = NPC.defDamage = 0;
                float duration = 420f;
                float spawnAnimTime = 120f;
                int rateOfChange = death ? 2 : 1;
                float splitProjVelocity = death ? 4.5f : 3f;

                // Percent life remaining for Cultist or Eidolon Wyrm
                float lifeRatio = Main.npc[(int)NPC.ai[0]].life / (float)Main.npc[(int)NPC.ai[0]].lifeMax;

                bool phase2 = lifeRatio < 0.7f;
                bool phase3 = lifeRatio < (Main.npc[(int)NPC.ai[0]].type == ModContent.NPCType<PrimordialWyrmHead>() ? 0.6f : 0.55f);
                bool phase4 = lifeRatio < 0.4f;

                bool kill = NPC.ai[1] < 0f || !Main.npc[(int)NPC.ai[0]].active;
                int target = Main.maxPlayers;
                if (Main.npc[(int)NPC.ai[0]].type == NPCID.CultistBoss || Main.npc[(int)NPC.ai[0]].type == ModContent.NPCType<PrimordialWyrmHead>())
                {
                    if (target == Main.maxPlayers)
                        target = Main.npc[(int)NPC.ai[0]].target;

                    if (phase2 || death)
                        rateOfChange++;

                    if (phase4 || death)
                        rateOfChange++;

                    if (Main.npc[(int)NPC.ai[0]].type == ModContent.NPCType<PrimordialWyrmHead>())
                        NPC.dontTakeDamage = true;
                }
                else
                    kill = true;

                NPC.ai[1] += rateOfChange;
                float growthRate = NPC.ai[1] / spawnAnimTime;
                growthRate = MathHelper.Clamp(growthRate, 0f, 1f);
                NPC.position = NPC.Center;
                NPC.scale = MathHelper.Lerp(0f, 1f, growthRate);
                NPC.Center = NPC.position;
                NPC.alpha = (int)(255f - growthRate * 255f);

                if (phase3)
                {
                    if (NPC.ai[3] == 0f)
                        NPC.ai[3] = NPC.ai[2];

                    double deg = NPC.ai[3];
                    double rad = deg * (Math.PI / 180);
                    double dist = 550;
                    if (Main.npc[(int)NPC.ai[0]].type == ModContent.NPCType<PrimordialWyrmHead>())
                    {
                        float aiGateValue = Main.npc[(int)NPC.ai[0]].Calamity().newAI[2] - 30f;
                        int ancientDoomScale = (int)(aiGateValue / 120f);
                        dist += ancientDoomScale * 45;
                    }
                    NPC.position.X = Main.player[target].Center.X - (int)(Math.Cos(rad) * dist) - NPC.width / 2;
                    NPC.position.Y = Main.player[target].Center.Y - (int)(Math.Sin(rad) * dist) - NPC.height / 2;
                    float spinVelocity = 8f * (1f - (NPC.ai[1] / duration));
                    NPC.ai[3] += spinVelocity;
                }

                if (Main.rand.NextBool(6))
                {
                    Vector2 shadowflameDustRotate = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                    Dust shadowflameDust = Main.dust[Dust.NewDust(NPC.Center - shadowflameDustRotate * 20f, 0, 0, DustID.Shadowflame, 0f, 0f, 0, default, 1f)];
                    shadowflameDust.noGravity = true;
                    shadowflameDust.position = NPC.Center - shadowflameDustRotate * Main.rand.Next(10, 21) * NPC.scale;
                    shadowflameDust.velocity = shadowflameDustRotate.RotatedBy(MathHelper.PiOver2) * 4f;
                    shadowflameDust.scale = 0.5f + Main.rand.NextFloat();
                    shadowflameDust.fadeIn = 0.5f;
                }
                if (Main.rand.NextBool(6))
                {
                    Vector2 darkDustRotate = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                    Dust darkDust = Main.dust[Dust.NewDust(NPC.Center - darkDustRotate * 30f, 0, 0, DustID.Granite, 0f, 0f, 0, default, 1f)];
                    darkDust.noGravity = true;
                    darkDust.position = NPC.Center - darkDustRotate * 20f * NPC.scale;
                    darkDust.velocity = darkDustRotate.RotatedBy(-MathHelper.PiOver2) * 2f;
                    darkDust.scale = 0.5f + Main.rand.NextFloat();
                    darkDust.fadeIn = 0.5f;
                }
                if (Main.rand.NextBool(6))
                {
                    Vector2 darkDustRotate2 = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                    Dust darkDust2 = Main.dust[Dust.NewDust(NPC.Center - darkDustRotate2 * 30f, 0, 0, DustID.Granite, 0f, 0f, 0, default, 1f)];
                    darkDust2.position = NPC.Center - darkDustRotate2 * 20f * NPC.scale;
                    darkDust2.velocity = Vector2.Zero;
                    darkDust2.scale = 0.5f + Main.rand.NextFloat();
                    darkDust2.fadeIn = 0.5f;
                    darkDust2.noLight = true;
                }

                NPC.localAI[0] += (float)(Math.PI / 60D);

                NPC.localAI[1] = 0.25f + Vector2.UnitY.RotatedBy(NPC.ai[1] * MathHelper.TwoPi / 60f).Y * 0.25f;

                if (NPC.ai[1] >= duration)
                {
                    int type = ProjectileID.AncientDoomProjectile;
                    int damage = DoomDamage;

                    // Increase damage if the Primordial Wyrm is alive
                    if (Main.npc[(int)NPC.ai[0]].type == ModContent.NPCType<PrimordialWyrmHead>())
                        damage = (int)Math.Round(damage * PrimordialWyrmHead.DoomDamageMult);

                    kill = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int totalProjectiles = Main.getGoodWorld ? 9 : (Main.npc[(int)NPC.ai[0]].type == NPCID.CultistBoss && !phase3) ? 8 : death ? 5 : 4;
                        float radians = MathHelper.TwoPi / totalProjectiles;
                        Vector2 spinningPoint = new Vector2(0f, -splitProjVelocity);
                        for (int k = 0; k < totalProjectiles; k++)
                        {
                            Vector2 doomProjRotate = spinningPoint.RotatedBy(radians * k);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, doomProjRotate, type, damage, 0f, Main.myPlayer);
                        }
                    }
                }

                if (kill)
                {
                    NPC.HitEffect(0, 9999.0);
                    NPC.active = false;
                }

                return false;
            }
        }
    }
}
