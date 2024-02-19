using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using CalamityMod.Particles;

namespace CalamityMod.NPCs.SunkenSea
{
    public class LostShoal : ModNPC
    {
        public static Texture2D RedTexture;
        public static Texture2D BlueTexture;

        public ref float Variant => ref NPC.ai[1];
        public ref float Leader => ref NPC.ai[2];
        public ref float Role => ref NPC.ai[3];

        public enum ShoalColor
        {
            Red = 0,
            Blue = 1,
            Green = 2
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            if (!Main.dedServ)
            {
                RedTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/LostShoalRed", AssetRequestMode.ImmediateLoad).Value;
                BlueTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/LostShoalBlue", AssetRequestMode.ImmediateLoad).Value;
            }
        }
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
            NPC.alpha = 120;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<LostShoalBanner>();
            NPC.chaseable = false;
            NPC.catchItem = ItemID.AshBlock;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.LostShoal")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Randomize the color of the fish
            Variant = Main.rand.Next(0, 3);
        }

        public override void AI()
        {
            // swim normally if the fish is the leader of the shoal
            if (Role != 1)
            {
                LeaderMovement();
                if (NPC.direction == 0)
                {
                    NPC.direction = Main.rand.NextBool(2) ? 1 : -1;
                }
                if (Main.rand.NextBool(1200))
                {
                    NPC.direction *= -1;
                }
            }
            else
            {
                NPC owner = Main.npc[(int)Leader];
                // if the owner of the shoal isn't a lost shoal or is dead, find a new shoal to attach to
                if (!owner.active || owner.type != ModContent.NPCType<LostShoal>())
                {
                    bool anyShoals = false;
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC n = Main.npc[k];
                        if (!n.active)
                            continue;
                        if (owner.type == ModContent.NPCType<LostShoal>())
                        {
                            // if a nearby shoal leader is found, go follow it
                            if (Role != 1 && n.Distance(NPC.position) < 1200)
                            {
                                anyShoals = true;
                                Leader = n.whoAmI;
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
                            if (owner.type == ModContent.NPCType<LostShoal>())
                            {
                                if (n.Distance(NPC.position) < 1200)
                                {
                                    // the found fish becomes the new leader, and this fish becomes a member of its school
                                    n.ai[3] = 2;
                                    Leader = n.whoAmI;
                                }
                            }
                        }
                    }
                }
                NPC.velocity = owner.velocity;
                NPC.direction = owner.direction;
                NPC.spriteDirection = owner.spriteDirection;
                // gather behind the leader
                // basically a pet
                // if we want to make the fish scared of players or other predators, then set enemyClose to true. For now this is commented out.
                //NPC.TargetClosest(false);
                //bool enemyClose = Main.player[NPC.target] != null && Main.player[NPC.target].active && Main.player[NPC.target].Distance(NPC.position) < 128;
                bool enemyClose = false;
                float SAImovement = enemyClose ? 0.02f : 0.1f;
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC otherFish = Main.npc[k];
                    // Short circuits to make the loop as fast as possible
                    if (!otherFish.active || k == NPC.whoAmI || owner.type != ModContent.NPCType<LostShoal>())
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
            }
            NPC.noGravity = true;
            // leaders will naturally spawn a school of followers upon spawning
            if (Role == 0)
            {                
                // the amount of fish to spawn
                int fishCount = 5;
                for (int i = 0; i < fishCount; i++)
                {
                    int n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<LostShoal>());
                    Main.npc[n].ai[3] = 1; // the 1 means that the spawned fish will not be a leader, and will not spawn even more fish
                    Main.npc[n].ai[2] = NPC.whoAmI; // marks this fish as the owner of the spawned fish
                }
                Role = 2; // don't spawn any more fish
            }
            // leaders are a tiny bit more brighter
            float intensity = Role == 1 ? 0.002f : 0.004f;
            Color glowColor = new(0.983f, 1f, 0.78f);
            switch (Variant)
            {
                case (int)ShoalColor.Blue:
                    glowColor = new(0.78f, 0.77f, 0.988f);
                    break;
                case (int)ShoalColor.Red:
                    glowColor = new(1f, 0.83f, 0.819f);
                    break;
            }
            Lighting.AddLight(NPC.Center, glowColor.R * intensity, glowColor.G * intensity, glowColor.B * intensity);
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
                    //int golddust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SilverCoin, 0f, 0f, 254, new Color(255, 255, 0), 0.5f);
                    //Main.dust[golddust].velocity *= 0f;
                }
            }
            if (Main.rand.NextBool(120))
            {
                Particle nanoDust = new SquareParticle(NPC.Center, new Vector2(Main.rand.NextFloat(-1, 2), 4), false, 300, Main.rand.NextFloat(0.65f, 0.9f), Color.White);
                GeneralParticleHandler.SpawnParticle(nanoDust);
            }
            NPC.position -= NPC.netOffset;
        }

        public void LeaderMovement()
        {
            NPC.spriteDirection = (NPC.direction > 0) ? -1 : 1;

            NPC.velocity.X = NPC.velocity.X - (float)NPC.direction * 0.25f;
            NPC.noGravity = true;
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
                return 0.125f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                int goreType = Main.rand.Next(11, 14);
                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, new Vector2(Main.rand.Next(-10, 11) * 0.2f, Main.rand.Next(-10, 11) * 0.2f), goreType, 0.2f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            switch (Variant)
            {
                case (int)ShoalColor.Blue:
                    texture = BlueTexture;
                    break;
                case (int)ShoalColor.Red:
                    texture = RedTexture;
                    break;
            }
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[NPC.type] / 2));
            Color white = Color.White;
            float colorLerpAmt = 0.5f;
            int afterimageAmt = 7;

            if (CalamityConfig.Instance.Afterimages)
            {
                for (int i = 1; i < afterimageAmt; i += 2)
                {
                    Color afterimageColor = drawColor;
                    afterimageColor = Color.Lerp(afterimageColor, white, colorLerpAmt);
                    afterimageColor = NPC.GetAlpha(afterimageColor);
                    afterimageColor *= (float)(afterimageAmt - i) / 15f;
                    Vector2 offset = NPC.oldPos[i] + new Vector2((float)NPC.width, (float)NPC.height) / 2f - screenPos;
                    offset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[NPC.type])) * NPC.scale / 2f;
                    offset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                    spriteBatch.Draw(texture, offset, NPC.frame, afterimageColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                }
            }

            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[NPC.type])) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, npcOffset, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}
