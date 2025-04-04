using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PrismaticGuppy : SunkenSeaNPC
    {   
        public enum FishColor
        {
            Blue = 0,
            Pink = 1,
            Green = 2,
            Gold = 3,
            Radiant = 4
        }

        // I do not know what to call the variants, so I'm going on intuition
        public enum FishShape
        {
            Normal = 0,
            Cow = 1,
            Angel = 2
        }

        public ref float CurrentColor => ref NPC.ai[1];

        public ref float CurrentShape => ref NPC.ai[2];

        public ref float Role => ref NPC.ai[3];

        public bool Leader => NPC.ai[3] == 1;

        // Each shape has a different frame count
        public int FrameCount => CurrentShape == (int)FishShape.Angel ? 5 : CurrentShape == (int)FishShape.Cow ? 4 : 6;

        public Entity avoidedEntity;

        public Vector2 randomPathPoint;

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 600;
        public static int IdleMaxPathDistance = 1000;

        public static int FleeTileAnticipationDistance = 64;

        #region Textures
        // Welcome to the fish texture wall, have a nice stay, or just collapse this region, either works
        public static Asset<Texture2D> RadiantTexture;
        public static Asset<Texture2D> GreenTexture;
        public static Asset<Texture2D> PinkTexture;
        public static Asset<Texture2D> GoldTexture;

        public static Asset<Texture2D> Texture2;
        public static Asset<Texture2D> RadiantTexture2;
        public static Asset<Texture2D> GreenTexture2;
        public static Asset<Texture2D> PinkTexture2;
        public static Asset<Texture2D> GoldTexture2;

        public static Asset<Texture2D> Texture3;
        public static Asset<Texture2D> RadiantTexture3;
        public static Asset<Texture2D> GreenTexture3;
        public static Asset<Texture2D> PinkTexture3;
        public static Asset<Texture2D> GoldTexture3;
        #endregion

        protected override List<int> PreyIDs => new List<int>()
        {
            //ModContent.NPCType<Slugbun>()
        };

        protected override List<int> PredatorIDs => new List<int>() {
            ModContent.NPCType<SandProwler>(),
            ModContent.NPCType<Polyperil>(),
            ModContent.NPCType<PolyperilTentacle>()
        };
        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.GleamingBurrows | SunkenSeaBiomeFlags.ClamDen;

        public override void Load()
        {
            RadiantTexture = ModContent.Request<Texture2D>(Texture + "Radiant");
            GoldTexture = ModContent.Request<Texture2D>(Texture + "Gold");
            GreenTexture = ModContent.Request<Texture2D>(Texture + "Green");
            PinkTexture = ModContent.Request<Texture2D>(Texture + "Pink");

            Texture2 = ModContent.Request<Texture2D>(Texture + "2");
            RadiantTexture2 = ModContent.Request<Texture2D>(Texture + "Radiant2");
            GoldTexture2 = ModContent.Request<Texture2D>(Texture + "Gold2");
            GreenTexture2 = ModContent.Request<Texture2D>(Texture + "Green2");
            PinkTexture2 = ModContent.Request<Texture2D>(Texture + "Pink2");

            Texture3 = ModContent.Request<Texture2D>(Texture + "3");
            RadiantTexture3 = ModContent.Request<Texture2D>(Texture + "Radiant3");
            GoldTexture3 = ModContent.Request<Texture2D>(Texture + "Gold3");
            GreenTexture3 = ModContent.Request<Texture2D>(Texture + "Green3");
            PinkTexture3 = ModContent.Request<Texture2D>(Texture + "Pink3");

            base.Load();
        }

        public override void SetStaticDefaults()
        {
            // This is technically redundant as variants handle this
            Main.npcFrameCount[Type] = 6;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 0f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 28;
            NPC.height = 28;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PrismaticGuppyBanner>();
            NPC.chaseable = false;
            //NPC.catchItem = (short)ModContent.ItemType<PrismaticGuppyItem>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PrismaticGuppy")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.frameCounter = Main.rand.NextFloat(FrameCount);
            pathfinding = new PathfindingManager(NPC)
            {
                Acceleration = 0.5f,
                MaxSpeed = 4f,
            };
            // Guppies released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            CurrentColor = Main.rand.Next(0, 3);
            CurrentShape = Main.rand.Next(0, 3);
            // 1 in 30 chance for a rare fish variant (rfv)
            if (Main.rand.NextBool(30))
            {
                CurrentColor = Main.rand.Next(3, 5);
                NPC.rarity = 3;
            }
            // Decide item..........................
            switch ((CurrentColor, CurrentShape))
            {
                case ((int)FishColor.Pink, (int)FishShape.Angel):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Pink, (int)FishShape.Cow):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Pink, (int)FishShape.Normal):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;

                case ((int)FishColor.Green, (int)FishShape.Angel):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Green, (int)FishShape.Cow):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Green, (int)FishShape.Normal):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;

                case ((int)FishColor.Blue, (int)FishShape.Angel):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Blue, (int)FishShape.Cow):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Blue, (int)FishShape.Normal):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;

                case ((int)FishColor.Gold, (int)FishShape.Angel):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Gold, (int)FishShape.Cow):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Gold, (int)FishShape.Normal):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;

                case ((int)FishColor.Radiant, (int)FishShape.Angel):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Radiant, (int)FishShape.Cow):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case ((int)FishColor.Radiant, (int)FishShape.Normal):
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
            }
        }
        public override void AI()
        {
            // Spawn more guppies if a leader/naturally spawned guppy
            if (Role == 0)
            {
                Role = 2;
                NPC.TargetClosest(false);
                // Spawn a shoal of guppies
                int fishCount = Main.rand.Next(2, 4);
                // More spawn in the Gleaming Burrows/Clam Den
                if (NPC.HasPlayerTarget)
                {
                    if (Main.player[NPC.target].Calamity().ZoneGleamingBurrows || Main.player[NPC.target].Calamity().ZoneClamDen)
                        fishCount += 5;
                }
                for (int i = 0; i < fishCount; i++)
                {
                    int n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, Type);
                    Main.npc[n].ai[3] = 1; // mark this guppy as a pawn
                    Main.npc[n].localAI[0] = NPC.whoAmI; // makes the spawned guppy recognize this one as the alpha
                    Main.npc[n].netUpdate = true;
                }
            }
            NPC owner = Main.npc[(int)NPC.localAI[0]];
            if (NPC.wet)
            {
                // Behaviour for owners/freed guppies
                if (owner == null || !owner.active || owner.type != ModContent.NPCType<PrismaticGuppy>())
                {
                    Role = 2;
                    pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));
                }
                else
                {
                    float SAImovement = 0.2f;
                    for (int k = 0; k < Main.maxNPCs; k++)
                    {
                        NPC otherFish = Main.npc[k];
                        // Short circuits to make the loop as fast as possible
                        if (!otherFish.active || k == NPC.whoAmI || otherFish.type != ModContent.NPCType<PrismaticGuppy>())
                            continue;

                        float taxicabDist = Math.Abs(NPC.position.X - otherFish.position.X) + Math.Abs(NPC.position.Y - otherFish.position.Y);
                        if (taxicabDist < NPC.width * 2f)
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

                    if (NPC.Distance(owner.Center) > 20)
                        pathfinding.DoPathfinding(new(NPC.Center, owner.Center, SunkenSeaTileValidity));
                }
                int dir = NPC.velocity.X.DirectionalSign();
                NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
                NPC.spriteDirection = NPC.direction = dir;
            }
            /*else
            {
                // Check who is the avoided entity specifically.
                avoidedEntity = avoidedEntity is NPC ? CurrentPredator : CurrentPlayer;

                if (avoidedEntity is not null)
                {
                    // While it doesn't have any obstacles in front of it, run away in a straight line.
                    // Try to manuever if there are any obstacles.
                    if (!Main.tile[(NPC.Center + NPC.DirectionFrom(avoidedEntity.Center) * 96).ToTileCoordinates()].IsTileSolid())
                    {
                        NPC.velocity += NPC.DirectionFrom(avoidedEntity.Center) * pathfinding.Acceleration;
                        pathfinding.ClearResults();

                        // Cap the speed if MaxSpeed has been surpassed.
                        if (NPC.velocity.LengthSquared() > pathfinding.MaxSpeed * pathfinding.MaxSpeed)
                            NPC.velocity = Vector2.Normalize(NPC.velocity) * pathfinding.MaxSpeed;
                    }
                    else
                    {
                        float distanceFromAvoided = Vector2.Distance(NPC.Center, avoidedEntity.Center);
                        randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                        NPC.netUpdate = true;
                        pathfinding.DoPathfinding(new(NPC.Center, randomPathPoint, SunkenSeaTileValidity));
                    }
                }
                else
                {
                    pathfinding.DoPathfinding(new(NPC.Center, Main.rand.NextVector2Unit() * Main.rand.Next(300, 1000), SunkenSeaTileValidity));
                }
            }*/

            if (CurrentColor == (int)FishColor.Gold)
            {
                NPC.rarity = 3;
                NPC.ProduceGoldCritterDust();
            }
            if (CurrentColor == (int)FishColor.Radiant)
            {
                NPC.rarity = 3;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y++;
                NPC.frameCounter = 0;
            }
            if (NPC.frame.Y >= FrameCount - 1)
            {
                NPC.frame.Y = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            // 15 texture variants!
            Asset<Texture2D> textureAsset;
            switch (CurrentColor)
            {
                case (int)FishColor.Radiant:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? RadiantTexture3 : CurrentShape == (int)FishShape.Cow ? RadiantTexture2 : RadiantTexture;
                    break;
                case (int)FishColor.Gold:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? GoldTexture3 : CurrentShape == (int)FishShape.Cow ? GoldTexture2 : GoldTexture;
                    break;
                case (int)FishColor.Green:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? GreenTexture3 : CurrentShape == (int)FishShape.Cow ? GreenTexture2 : GreenTexture;
                    break;
                case (int)FishColor.Pink:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? PinkTexture3 : CurrentShape == (int)FishShape.Cow ? PinkTexture2 : PinkTexture;
                    break;
                default:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? Texture3 : CurrentShape == (int)FishShape.Cow ? Texture2 : TextureAssets.Npc[Type];
                    break;
            }
            Texture2D texture = textureAsset.Value;
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / FrameCount / 2));
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / FrameCount)) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            Rectangle frame = texture.Frame(1, FrameCount, 0, NPC.frame.Y);
            spriteBatch.Draw(texture, npcOffset, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int dustType = DustID.BlueCrystalShard;
            switch (CurrentColor)
            {
                case (int)FishColor.Pink:
                    dustType = DustID.PinkCrystalShard;
                    break;
                case (int)FishColor.Green:
                    dustType = DustID.GemEmerald;
                    break;
                case (int)FishColor.Gold:
                    dustType = DustID.GoldCritter;
                    break;
            }
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(randomPathPoint);
            writer.Write(NPC.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
            NPC.localAI[0] = reader.ReadSingle();
        }
    }
}
