using System;
using CalamityMod.Events;
using CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class BloodlettingServant : ModNPC
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.WanderingEye}";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.GetNPCDamage();
            NPC.width = 30;
            NPC.height = 32;
            if (Main.tenthAnniversaryWorld)
                NPC.scale *= 0.5f;
            if (Main.getGoodWorld)
                NPC.scale *= 1.1f;

            NPC.defense = 5;

            NPC.lifeMax = 96;
            double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
            NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);

            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToSickness = true;
        }

        public override void AI()
        {
            NPC.TargetClosest();
            Player target = Main.player[NPC.target];

            Vector2 lookAt = target.Center - NPC.Center;
            if (lookAt.X > 0f)
            {
                NPC.spriteDirection = 1;
                NPC.rotation = (float)Math.Atan2(lookAt.Y, lookAt.X);
            }
            if (lookAt.X < 0f)
            {
                NPC.spriteDirection = -1;
                NPC.rotation = (float)Math.Atan2(lookAt.Y, lookAt.X) + MathHelper.Pi;
            }

            Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

            bool phase2 = NPC.life < NPC.lifeMax * 0.5;
            float enrageScale = 1f + NPC.ai[2];
            float enrageScaleMaxSpeedBonus = 2f * enrageScale;
            float maxSpeedX = (phase2 ? (CalamityWorld.death ? 10f : 8f) : (CalamityWorld.death ? 8f : 6f)) + enrageScaleMaxSpeedBonus;
            float maxSpeedY = (phase2 ? (CalamityWorld.death ? 8f : 6f) : (CalamityWorld.death ? 4f : 3f)) + enrageScaleMaxSpeedBonus;
            float xAccel = maxSpeedX * 0.01f;
            float xAccelBoost1 = maxSpeedX * 0.01f;
            float xAccelBoost2 = maxSpeedX * 0.0075f;
            float yAccel = maxSpeedY * 0.01f;
            float yAccelBoost1 = maxSpeedY * 0.01f;
            float yAccelBoost2 = maxSpeedY * 0.0075f;

            if (target.dead)
            {
                NPC.velocity.Y -= 0.04f;

                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;
            }
            else
                DemonEyeAI.DemonEyeBatMovement(NPC, maxSpeedX, maxSpeedY, xAccel, xAccelBoost1, xAccelBoost2, yAccel, yAccelBoost1, yAccelBoost2);

            if (phase2)
                NPC.damage = (int)Math.Round(NPC.defDamage * 1.3);
            else
                NPC.damage = NPC.defDamage;

            if (!target.dead)
                NPC.ai[3] += enrageScale;

            float projectileShootGateValue = CalamityWorld.death ? 180f : 270f;
            if (Main.getGoodWorld)
                projectileShootGateValue *= 0.5f;

            bool shootProjectile = Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1) && Vector2.Distance(NPC.Center, target.Center) > 240f;
            if (NPC.ai[3] >= projectileShootGateValue && shootProjectile)
            {
                NPC.ai[3] = 0f;
                float projectileSpeed = CalamityWorld.death ? 16f : 12f;
                Vector2 projectileVelocity = NPC.SafeDirectionTo(target.Center) * projectileSpeed;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projType = ProjectileID.BloodShot;
                    int projDamage = NPC.GetProjectileDamage(projType);
                    int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projectileVelocity.SafeNormalize(Vector2.UnitY) * 10f, projectileVelocity, projType, projDamage, 0f, Main.myPlayer);
                    Main.projectile[proj].timeLeft = 600;
                }

                NPC.netUpdate = true;
            }

            if (Main.rand.NextBool(20))
            {
                Vector2 dustSpawnTopLeft = new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 0.25f);
                Dust blood = Dust.NewDustDirect(dustSpawnTopLeft, NPC.width, NPC.height / 2, DustID.Blood, NPC.velocity.X, 2f, 0, new Color(128, 0, 0, 255 - NPC.alpha), 1f);
                blood.velocity.X *= 0.5f;
                blood.velocity.Y *= 0.1f;
            }

            float pushVelocity = 0.2f * enrageScale;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    if (i != NPC.whoAmI && Main.npc[i].type == NPC.type)
                    {
                        if (Vector2.Distance(NPC.Center, Main.npc[i].Center) < 48f * NPC.scale)
                        {
                            if (NPC.position.X < Main.npc[i].position.X)
                                NPC.velocity.X -= pushVelocity;
                            else
                                NPC.velocity.X += pushVelocity;

                            if (NPC.position.Y < Main.npc[i].position.Y)
                                NPC.velocity.Y -= pushVelocity;
                            else
                                NPC.velocity.Y += pushVelocity;
                        }
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1D;
            if (NPC.frameCounter >= 8D)
                NPC.frame.Y = frameHeight;
            else
                NPC.frame.Y = 0;

            if (NPC.frameCounter >= 16D)
            {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0D;
            }
            if (NPC.life < NPC.lifeMax * 0.5)
                NPC.frame.Y += frameHeight * 2;
        }

        public override Color? GetAlpha(Color drawColor) => new Color(128, 0, 0, 255 - NPC.alpha);

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; i < hit.Damage / (double)NPC.lifeMax * 100; i++)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);

                if (NPC.life < NPC.lifeMax * 0.5f && NPC.localAI[0] == 0f)
                {
                    NPC.localAI[0] = 1f;
                    for (int i = 0; i < 50; i++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2f * hit.HitDirection, -2f);
                        Main.dust[dust].scale = Main.rand.NextFloat(1.6f, 2.4f);
                        Main.dust[dust].velocity *= Main.rand.NextFloat(1.6f, 2.4f);
                        Main.dust[dust].color = new Color(128, 0, 0, 255 - NPC.alpha);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 100; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2f * hit.HitDirection, -2f);
                    Main.dust[dust].scale = Main.rand.NextFloat(1.6f, 2.4f);
                    Main.dust[dust].velocity *= Main.rand.NextFloat(1.6f, 2.4f);
                    Main.dust[dust].color = new Color(128, 0, 0, 255 - NPC.alpha);
                }
            }
        }
    }
}
