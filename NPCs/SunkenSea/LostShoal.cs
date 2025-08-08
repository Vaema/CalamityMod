using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
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

namespace CalamityMod.NPCs.SunkenSea
{
    public class LostShoal : ModNPC
    {
        public static Asset<Texture2D> RedTexture;
        public static Asset<Texture2D> BlueTexture;
        public static Asset<Texture2D> GoldTexture;

        public float RandomOpacityOffset;
        public ref float Variant => ref NPC.ai[1];
        public ref float Leader => ref NPC.ai[2];
        public ref float Role => ref NPC.ai[3];

        public enum ShoalColor
        {
            Red = 0,
            Blue = 1,
            Green = 2,
            Gold = 3
        }

        public override void Load()
        {
            RedTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/LostShoalRed");
            BlueTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/LostShoalBlue");
            GoldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/LostShoalGold");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.CountsAsCritter[Type] = true;
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
            NPC.catchItem = ItemID.AshBlock; // yeah this is intentional
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
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
            if (Main.rand.NextBool(50))
            {
                Variant = (int)ShoalColor.Gold;
                NPC.catchItem = ItemID.GoldCoin;
            }

            RandomOpacityOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            NPC.frameCounter = Main.rand.NextFloat(Main.npcFrameCount[Type]);
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
                int fishCount = Main.rand.Next(3, 6);
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
                case (int)ShoalColor.Gold:
                    glowColor = new(1f, 1f, 0.678f);
                    break;
            }
            Lighting.AddLight(NPC.Center, glowColor.R * intensity, glowColor.G * intensity, glowColor.B * intensity);
            // Formerly, sprinkle down ash particles
            // Now they trail glowing dust as they move
            // Keeping it as reference in case i want to use it somewhere else (i will)
            if (Main.rand.NextBool(120))
            {
                //Color ashColor = new(40, 48, 41);
                int dustid = DustID.CoralTorch;
                switch (Variant)
                {
                    case (int)ShoalColor.Blue:
                        //ashColor = new(44, 63, 66);
                        dustid = DustID.BoneTorch;
                        break;
                    case (int)ShoalColor.Red:
                        //ashColor = new(79, 41, 42);
                        dustid = DustID.CrimsonTorch;
                        break;
                    case (int)ShoalColor.Gold:
                        //ashColor = Color.Yellow;
                        dustid = DustID.IchorTorch;
                        break;
                }
                //Particle ash = new SquareAshParticle(NPC.Center, new Vector2(0, 4), 150, Main.rand.NextFloat(0.85f, 1f), ashColor);
                //GeneralParticleHandler.SpawnParticle(ash);

                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustid, Scale: 1.5f);
                dust.velocity = Vector2.Zero;
            }
            if (Variant == (int)ShoalColor.Gold)
            {
                NPC.ProduceGoldCritterDust();
            }
        }

        public void LeaderMovement()
        {
            // This is just usual swimmer ai with some modifications
            NPC.spriteDirection = (NPC.direction > 0) ? -1 : 1;

            NPC.velocity.X = NPC.velocity.X - (float)NPC.direction * 0.06f;
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
            if (NPC.Calamity().newAI[0] <= 0 && NPC.Distance(Main.player[NPC.target].Center) > (45 * 16))
            {
                //NPC.velocity.X *= -1;
                NPC.direction *= -1;
                NPC.Calamity().newAI[0] = 300;
            }
            NPC.Calamity().newAI[0] -= 1;
            // No target behavior
            NPC.velocity.X += (float)NPC.direction * 0.02f;
            if (NPC.velocity.X < -1.5f || NPC.velocity.X > 1.5f)
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
            NPC.frameCounter += 0.2f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Player.Calamity().clamity && !NPC.AnyNPCs(Type))
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

        public override void OnCaughtBy(Player player, Item item, bool failed)
        {
            // Gold shoals drop Gold Coins instead of ash when caught
            // The shoal drops 1 coin by default while the extra coins are spawned here in a spread
            if (!failed)
            {
                if (Variant == (int)ShoalColor.Gold)
                {
                    for (int i = 0; i < Main.rand.Next(0, 5); i++)
                    {
                        Item.NewItem(NPC.GetSource_CatchEntity(NPC), NPC.getRect(), ItemID.GoldCoin);
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            // Spooky glowey aura effect
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloom").Value;
            Color glowColor = Color.SeaGreen;
            switch (Variant)
            {
                case (int)ShoalColor.Blue:
                    glowColor = Color.Cyan;
                    break;
                case (int)ShoalColor.Red:
                    glowColor = Color.Pink;
                    break;
                case (int)ShoalColor.Gold:
                    glowColor = Color.Yellow;
                    break;
            }

            spriteBatch.Draw(bloom, NPC.Center - Main.screenPosition, null, glowColor * 0.45f, 0f, bloom.Size() / 2f, 0.3f, SpriteEffects.None, 0);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Regular stuff for drawing the actual fish and its afterimages
            Texture2D texture = TextureAssets.Npc[Type].Value;
            switch (Variant)
            {
                case (int)ShoalColor.Blue:
                    texture = BlueTexture.Value;
                    break;
                case (int)ShoalColor.Red:
                    texture = RedTexture.Value;
                    break;
                case (int)ShoalColor.Gold:
                    texture = GoldTexture.Value;
                    break;
            }
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[Type] / 2));
            Color white = Color.White;
            float colorLerpAmt = 0.5f;
            int afterimageAmt = 7;

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 1; i < afterimageAmt; i += 2)
                {
                    Color afterimageColor = drawColor;
                    afterimageColor = Color.Lerp(afterimageColor, white, colorLerpAmt);
                    afterimageColor = NPC.GetAlpha(afterimageColor);
                    afterimageColor *= (float)(afterimageAmt - i) / 15f;
                    Vector2 offset = NPC.oldPos[i] + new Vector2((float)NPC.width, (float)NPC.height) / 2f - screenPos;
                    offset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
                    offset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                    spriteBatch.Draw(texture, offset, NPC.frame, afterimageColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                }
            }

            float sine = MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + RandomOpacityOffset) / 2f + 0.5f;

            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, npcOffset, NPC.frame, drawColor * MathHelper.Lerp(0.4f, 0.8f, sine), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}
