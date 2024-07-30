using System;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class SeaMinnow : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 36;
            NPC.height = 22;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<SeaMinnowBanner>();
            NPC.chaseable = false;
            NPC.catchItem = (short)ModContent.ItemType<SeaMinnowItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SeaMinnow")
            });
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[2]];
            if (NPC.wet)
            {
                if (owner == null || !owner.active || (owner.type != ModContent.NPCType<AlphaSeaMinnow>() && owner.type != ModContent.NPCType<AlphaSeaMinnowGold>()))
                {
                    CalamityRegularEnemyAI.PassiveSwimmingAI(NPC, Mod, 3, 150f, 0.25f, 0.15f, 6f, 6f, 0.05f);
                    NPC.spriteDirection = (NPC.direction > 0) ? 1 : -1;
                    NPC.noGravity = true;
                    bool shouldSwimAway = false;
                    if (NPC.direction == 0)
                    {
                        NPC.TargetClosest(true);
                    }
                    NPC.TargetClosest(false);
                    if (Main.player[NPC.target].wet && !Main.player[NPC.target].dead &&
                        (Main.player[NPC.target].Center - NPC.Center).Length() < 150f)
                    {
                        shouldSwimAway = true;
                    }
                    if ((!Main.player[NPC.target].wet || Main.player[NPC.target].dead) && shouldSwimAway)
                    {
                        shouldSwimAway = false;
                    }
                    if (!shouldSwimAway)
                    {
                        if (NPC.collideX || NPC.velocity.X == 0f)
                        {
                            NPC.velocity.X = NPC.velocity.X * -3f;
                            NPC.direction *= -1;
                            NPC.netUpdate = true;
                        }
                        if (NPC.collideY)
                        {
                            NPC.netUpdate = true;
                            if (NPC.velocity.Y > 0f)
                            {
                                NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -3f;
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
                    }
                    if (shouldSwimAway)
                    {
                        NPC.TargetClosest(true);
                        NPC.velocity.X = NPC.velocity.X - (float)NPC.direction * 0.25f;
                        NPC.velocity.Y = NPC.velocity.Y - (float)NPC.directionY * 0.15f;
                        if (NPC.velocity.X > 6f)
                        {
                            NPC.velocity.X = 6f;
                        }
                        if (NPC.velocity.X < -6f)
                        {
                            NPC.velocity.X = -6f;
                        }
                        if (NPC.velocity.Y > 6f)
                        {
                            NPC.velocity.Y = 6f;
                        }
                        if (NPC.velocity.Y < -6f)
                        {
                            NPC.velocity.Y = -6f;
                        }
                        NPC.direction *= -1;
                    }
                    else
                    {
                        NPC.velocity.X = NPC.velocity.X + (float)NPC.direction * 0.1f;
                        if (NPC.velocity.X < -2.5f || NPC.velocity.X > 2.5f)
                        {
                            NPC.velocity.X = NPC.velocity.X * 0.95f;
                        }
                        if (NPC.ai[0] == -1f)
                        {
                            NPC.velocity.Y = NPC.velocity.Y - 0.01f;
                            if ((double)NPC.velocity.Y < -0.3)
                            {
                                NPC.ai[0] = 1f;
                            }
                        }
                        else
                        {
                            NPC.velocity.Y = NPC.velocity.Y + 0.01f;
                            if ((double)NPC.velocity.Y > 0.3)
                            {
                                NPC.ai[0] = -1f;
                            }
                        }
                    }
                    int npcTileX = (int)(NPC.position.X + (float)(NPC.width / 2)) / 16;
                    int npcTileY = (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16;
                    if (Main.tile[npcTileX, npcTileY - 1].LiquidAmount > 128)
                    {
                        if (Main.tile[npcTileX, npcTileY + 1].HasTile)
                        {
                            NPC.ai[0] = -1f;
                        }
                        else if (Main.tile[npcTileX, npcTileY + 2].HasTile)
                        {
                            NPC.ai[0] = -1f;
                        }
                    }
                    if ((double)NPC.velocity.Y > 0.4 || (double)NPC.velocity.Y < -0.4)
                    {
                        NPC.velocity.Y = NPC.velocity.Y * 0.95f;
                    }
                }
                else
                {
                    float SAImovement = 0.05f;
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC otherFish = Main.npc[k];
                        // Short circuits to make the loop as fast as possible
                        if (!otherFish.active || k == NPC.whoAmI || (otherFish.type != ModContent.NPCType<SeaMinnow>() && otherFish.type != ModContent.NPCType<AlphaSeaMinnow>() && otherFish.type != ModContent.NPCType<AlphaSeaMinnowGold>()))
                            continue;

                        float taxicabDist = Math.Abs(NPC.position.X - otherFish.position.X) + Math.Abs(NPC.position.Y - otherFish.position.Y);
                        if (taxicabDist < NPC.width * 1.5f)
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
                    xDist += 30f * -(float)owner.direction;
                    Vector2 leaderVector = new Vector2(xDist, yDist);
                    float leaderDist = leaderVector.Length();
                    float returnSpeed = 8f;
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
            }
            else
            {

                {
                    if (NPC.velocity.Y == 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X * 0.94f;
                        if ((double)NPC.velocity.X > -0.2 && (double)NPC.velocity.X < 0.2)
                        {
                            NPC.velocity.X = 0f;
                        }
                    }
                    NPC.velocity.Y = NPC.velocity.Y + 0.3f;
                    if (NPC.velocity.Y > 10f)
                    {
                        NPC.velocity.Y = 10f;
                    }
                    NPC.ai[0] = 1f;
                }
                NPC.rotation = NPC.velocity.X * 0.05f;
                if ((double)NPC.rotation < -0.1)
                {
                    NPC.rotation = -0.1f;
                }
                if ((double)NPC.rotation > 0.1)
                {
                    NPC.rotation = 0.1f;
                }
                if (NPC.type == ModContent.NPCType<SeaMinnowGold>())
                {
                    NPC.position += NPC.netOffset;
                    Color color = Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
                    if (color.R > 20 || color.B > 20 || color.G > 20)
                    {
                        int colorVal = color.R;
                        if (color.G > colorVal)
                        {
                            colorVal = color.G;
                        }
                        if (color.B > colorVal)
                        {
                            colorVal = color.B;
                        }
                        colorVal /= 30;
                        if (Main.rand.Next(300) < colorVal)
                        {
                            int golddust = Dust.NewDust(NPC.position, NPC.width, NPC.height, 43, 0f, 0f, 254, new Color(255, 255, 0), 0.5f);
                            Main.dust[golddust].velocity *= 0f;
                        }
                    }
                    NPC.position -= NPC.netOffset;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 0.075f;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
    public class SeaMinnowGold : SeaMinnow
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            Main.npcCatchable[NPC.type] = true;
            NPCID.Sets.CountsAsCritter[NPC.type] = true;
            this.HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 36;
            NPC.height = 22;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<SeaMinnowBanner>();
            NPC.chaseable = false;
            NPC.catchItem = (short)ModContent.ItemType<SeaMinnowGoldItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
            NPC.rarity = 3;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
