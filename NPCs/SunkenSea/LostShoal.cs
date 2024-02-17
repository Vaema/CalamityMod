using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace CalamityMod.NPCs.SunkenSea
{
    public abstract class LostShoal : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 36;
            NPC.height = 22;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = null;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.noTileCollide = true;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<LostShoalBanner>();
            NPC.chaseable = false;
            //NPC.catchItem = (short)ModContent.ItemType<LostShoalItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.LostShoal")
            });
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            //NPC.Calamity().newAI[0] = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            //writer.Write(NPC.Calamity().newAI[0]);
        }
        public override void AI()
        {
            // swim normally if the fish is the leader of the shoal
            if (NPC.ai[3] != 1)
            {
                LeaderMovement();
                if (Main.rand.NextBool(1200))
                {
                    NPC.direction *= -1;
                }
            }
            else
            {
                NPC owner = Main.npc[(int)NPC.ai[2]];
                // if the owner of the shoal isn't a lost shoal or is dead, find a new shoal to attach to
                if (!owner.active || !CheckIfShoal(owner))
                {
                    bool anyShoals = false;
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC n = Main.npc[k];
                        if (!n.active)
                            continue;
                        if (CheckIfShoal(n))
                        {
                            // if a nearby shoal leader is found, go follow it
                            if (n.ai[3] != 1 && n.Distance(NPC.position) < 1200)
                            {
                                anyShoals = true;
                                NPC.ai[2] = n.whoAmI;
                            }
                        }
                    }
                    // if no leaders are found nearby, a new leader is picked
                    if (!anyShoals)
                    {
                        for (int k = 0; k < Main.maxNPCs; k++)
                        {
                            NPC n = Main.npc[k];
                            if (!n.active)
                                continue;
                            if (CheckIfShoal(n))
                            {
                                if (n.Distance(NPC.position) < 1200)
                                {
                                    // the found fish becomes the new leader, and this fish becomes a member of its school
                                    n.ai[3] = 2;
                                    NPC.ai[2] = n.whoAmI;
                                }
                            }
                        }
                    }
                }
                // gather behind the leader
                // basically a pet
                float SAImovement = 0.05f;
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC otherFish = Main.npc[k];
                    // Short circuits to make the loop as fast as possible
                    if (!otherFish.active || k == NPC.whoAmI || !CheckIfShoal(otherFish))
                        continue;

                    float taxicabDist = Math.Abs(NPC.position.X - otherFish.position.X) + Math.Abs(NPC.position.Y - otherFish.position.Y);
                    if (taxicabDist < NPC.width)
                    {
                        if (NPC.position.X < otherFish.position.X)
                            NPC.velocity.X -= SAImovement;
                        else
                            NPC.velocity.X += SAImovement;

                        if (NPC.position.Y < otherFish.position.Y)
                            NPC.velocity.Y -= SAImovement;
                        else
                            NPC.velocity.Y += SAImovement;
                    }
                }

                if (!owner.active)
                    return;

                float passiveMvtFloat = 0.5f;
                float range = 100f;
                Vector2 fischPos = NPC.Center;
                float xDist = owner.Center.X - fischPos.X;
                float yDist = owner.Center.Y - fischPos.Y;
                yDist += Main.rand.NextFloat(-10, 20);
                xDist += Main.rand.NextFloat(-10, 20);
                xDist += 20f * -(float)owner.direction;
                Vector2 leaderVector = new Vector2(xDist, yDist);
                float leaderDist = leaderVector.Length();
                float returnSpeed = 18f;

                //If leader is close enough, resume normal
                if (leaderDist < range && owner.velocity.Y == 0f &&
                    NPC.Bottom.Y <= owner.Bottom.Y &&
                    !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    if (NPC.velocity.Y < -6f)
                    {
                        NPC.velocity.Y = -6f;
                    }
                }

                //Teleport to leader if too far
                if (leaderDist > 2000f)
                {
                    NPC.position.X = owner.Center.X - NPC.width / 2;
                    NPC.position.Y = owner.Center.Y - NPC.height / 2;
                    NPC.netUpdate = true;
                }

                if (leaderDist < 50f)
                {
                    if (Math.Abs(NPC.velocity.X) > 2f || Math.Abs(NPC.velocity.Y) > 2f)
                    {
                        NPC.velocity *= 0.99f;
                    }
                    passiveMvtFloat = 0.01f;
                }
                else
                {
                    if (leaderDist < 100f)
                    {
                        passiveMvtFloat = 0.1f;
                    }
                    if (leaderDist > 300f)
                    {
                        passiveMvtFloat = 1f;
                    }
                    leaderDist = returnSpeed / leaderDist;
                    leaderVector.X *= leaderDist;
                    leaderVector.Y *= leaderDist;
                }
                if (NPC.velocity.X < leaderVector.X)
                {
                    NPC.velocity.X += passiveMvtFloat;
                    if (passiveMvtFloat > 0.05f && NPC.velocity.X < 0f)
                    {
                        NPC.velocity.X += passiveMvtFloat;
                    }
                }
                if (NPC.velocity.X > leaderVector.X)
                {
                    NPC.velocity.X -= passiveMvtFloat;
                    if (passiveMvtFloat > 0.05f && NPC.velocity.X > 0f)
                    {
                        NPC.velocity.X -= passiveMvtFloat;
                    }
                }
                if (NPC.velocity.Y < leaderVector.Y)
                {
                    NPC.velocity.Y += passiveMvtFloat;
                    if (passiveMvtFloat > 0.05f && NPC.velocity.Y < 0f)
                    {
                        NPC.velocity.Y += passiveMvtFloat * 2f;
                    }
                }
                if (NPC.velocity.Y > leaderVector.Y)
                {
                    NPC.velocity.Y -= passiveMvtFloat;
                    if (passiveMvtFloat > 0.05f && NPC.velocity.Y > 0f)
                    {
                        NPC.velocity.Y -= passiveMvtFloat * 2f;
                    }
                }
                if (NPC.velocity.X >= 0.25f)
                {
                    NPC.direction = -1;
                }
                else if (NPC.velocity.X < -0.25f)
                {
                    NPC.direction = 1;
                }
                NPC.spriteDirection = -NPC.direction;
            }
            NPC.noGravity = true;
            // leaders will naturally spawn a school of followers upon spawning
            if (NPC.ai[3] == 0)
            {                
                // the amount of fish to spawn
                int fishCount = 5;
                for (int i = 0; i < fishCount; i++)
                {
                    int fishType = Main.rand.Next(3);
                    switch (fishType)
                    {
                        case 0:
                            fishType = ModContent.NPCType<LostShoalRed>();
                            break;
                        case 1:
                            fishType = ModContent.NPCType<LostShoalGreen>();
                            break;
                        case 2:
                            fishType = ModContent.NPCType<LostShoalBlue>();
                            break;
                    }
                    int n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, fishType);
                    Main.npc[n].ai[3] = 1; // the 1 means that the spawned fish will not be a leader, and will not spawn even more fish
                    Main.npc[n].ai[2] = NPC.whoAmI; // marks this fish as the owner of the spawned fish
                }
                NPC.ai[3] = 2; // don't spawn any more fish
            }
        }

        public void LeaderMovement()
        {
            NPC.spriteDirection = (NPC.direction > 0) ? 1 : -1;

            NPC.noGravity = true;
            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.velocity.X * -1f;
                NPC.direction *= -1;
                NPC.netUpdate = true;
            }
            if (NPC.collideY)
            {
                NPC.netUpdate = true;
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
                    NPC.directionY = -1;
                    NPC.ai[0] = -1f;
                }
                else if (NPC.velocity.Y < 0f)
                {
                    NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
                    NPC.directionY = 1;
                    NPC.ai[0] = 1f;
                }
            }
            // No target behavior
            NPC.velocity.X += (float)NPC.direction * 0.1f;
            if (NPC.velocity.X < -2.5f || NPC.velocity.X > 2.5f)
            {
                NPC.velocity.X *= 0.95f;
            }
            if (NPC.ai[0] == -1f)
            {
                NPC.velocity.Y -= 0.01f;
                if (NPC.velocity.Y < -0.3f)
                {
                    NPC.ai[0] = 1f;
                }
            }
            else
            {
                NPC.velocity.Y += 0.01f;
                if (NPC.velocity.Y > 0.3f)
                {
                    NPC.ai[0] = -1f;
                }
            }
            int NPCTileX = (int)(NPC.position.X + (float)(NPC.width / 2)) / 16;
            int NPCTileY = (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16;
            if (Main.tile[NPCTileX, NPCTileY - 1].LiquidAmount > 128)
            {
                if (Main.tile[NPCTileX, NPCTileY + 1].HasTile)
                {
                    NPC.ai[0] = -1f;
                }
                else if (Main.tile[NPCTileX, NPCTileY + 2].HasTile)
                {
                    NPC.ai[0] = -1f;
                }
            }
            if (NPC.velocity.Y > 0.4f || NPC.velocity.Y < -0.4f)
            {
                NPC.velocity.Y = NPC.velocity.Y * 0.95f;
            }
            NPC.rotation = NPC.velocity.Y * (float)NPC.direction * 0.65f;
            float rotationLimit = 2f * 0.65f;
            NPC.rotation = MathHelper.Clamp(NPC.rotation, -rotationLimit, rotationLimit);
        }

        public static bool CheckIfShoal(NPC n)
        {
            if (n.type == ModContent.NPCType<LostShoalBlue>() || n.type == ModContent.NPCType<LostShoalGreen>() || n.type == ModContent.NPCType<LostShoalRed>())
            {
                return true;
            }
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSeaShores && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ghost, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }

    public class LostShoalRed : LostShoal
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            //Main.npcCatchable[NPC.type] = true;
            //NPCID.Sets.CountsAsCritter[NPC.type] = true;
            this.HideFromBestiary();
        }
    }

    public class LostShoalBlue : LostShoal
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            //Main.npcCatchable[NPC.type] = true;
            //NPCID.Sets.CountsAsCritter[NPC.type] = true;
            this.HideFromBestiary();
        }
    }

    public class LostShoalGreen : LostShoal
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            //Main.npcCatchable[NPC.type] = true;
            //NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }
    }
}
