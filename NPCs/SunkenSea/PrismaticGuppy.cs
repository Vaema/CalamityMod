using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Tiles.SunkenSea;
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
        public enum PhaseType
        {
            Idle = 0,
            Pawn = 1,
            Fleeing = 2,
            Hiding = 3
        }

        public enum FishColor
        {
            Blue = 0,
            Green = 1,
            Pink = 2,
            Gold = 3,
            Radiant = 4
        }

        // I do not know what to call the variants, so I'm going on intuition
        public enum FishShape
        {
            Normal = 0,
            Cube = 1,
            Angel = 2
        }

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float CurrentColor => ref NPC.ai[1];

        public ref float CurrentShape => ref NPC.ai[2];

        public ref float Role => ref NPC.ai[3];

        public bool Leader => NPC.ai[3] == 1;

        // Each shape has a different frame count
        public int FrameCount => CurrentShape == (int)FishShape.Angel ? 5 : CurrentShape == (int)FishShape.Cube ? 4 : 6;

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
            CurrentShape = Main.rand.Next(0, 3);
            // Guppies released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            // Red/Green in reefs, Green/Blue elsewhere
            if (Main.player[NPC.target].Calamity().ZoneRadiantReefs)
            {
                CurrentColor = Main.rand.Next(1, 3);
            }
            else
            {
                CurrentColor = Main.rand.Next(0, 2);
            }
            NPC.TargetClosest();
            // 1 in 30 chance for a rare fish variant (rfv)
            if (Main.rand.NextBool(30))
            {
                CurrentColor = Main.rand.Next(3, 5);
                NPC.rarity = 3;
            }
            // Decide item..........................
            switch (CurrentColor)
            {
                case (int)FishColor.Pink:
                    NPC.catchItem = ModContent.ItemType<PrismaticGuppyPinkItem>();
                    break;
                case (int)FishColor.Green:
                    NPC.catchItem = ModContent.ItemType<PrismaticGuppyGreenItem>();
                    break;
                case (int)FishColor.Blue:
                    NPC.catchItem = ModContent.ItemType<PrismaticGuppyBlueItem>();
                    break;
                case (int)FishColor.Gold:
                    NPC.catchItem = ModContent.ItemType<PrismaticGuppyGoldItem>();
                    break;
                case (int)FishColor.Radiant:
                    NPC.catchItem = ModContent.ItemType<PrismaticGuppyRadiantItem>();
                    break;
            }
        }
        public override void AI()
        {
            // Spawn more guppies if a leader/naturally spawned guppy
            if (Role == 0 && NPC.releaseOwner == 255)
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
                    Main.npc[n].ai[0] = (int)PhaseType.Pawn;
                    Main.npc[n].ai[3] = 1; // mark this guppy as a pawn
                    Main.npc[n].localAI[0] = NPC.whoAmI; // makes the spawned guppy recognize this one as the alpha
                    Main.npc[n].netUpdate = true;
                }
            }
            NPC owner = Main.npc[(int)NPC.localAI[0]];
            if (NPC.wet)
            {
                switch (CurrentPhase)
                {
                    // Just do generic pathfinding if a leader. Target prey if one exists
                    case (int)PhaseType.Idle:
                        {
                            pathfinding.Acceleration = 0.5f;
                            pathfinding.MaxSpeed = 4;
                            if (CurrentPrey is not null)
                                pathfinding.DoPathfinding(new(NPC.Center, CurrentPrey.Center, SunkenSeaTileValidity));
                            else
                                pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));
                        }
                        break;
                    // The unenlightened masses, They cannot make the judgement call
                    case (int)PhaseType.Pawn:
                        {
                            if (owner == null || !owner.active || owner.type != ModContent.NPCType<PrismaticGuppy>())
                            {
                                CurrentPhase = (int)PhaseType.Idle;
                                Role = 2;
                                break;
                            }
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
                        break;
                    // Run from predators, attempting to find crystals to camo in
                    case (int)PhaseType.Fleeing:
                        {
                            if (CurrentPredator is not null)
                            {
                                pathfinding.Acceleration = 0.6f;
                                pathfinding.MaxSpeed = 6;
                                Vector2? tilePos = new Vector2(NPC.localAI[2], NPC.localAI[3]);

                                // Find a sea prism
                                if (tilePos == null || tilePos == Vector2.Zero)
                                {
                                    tilePos = CalamityUtils.NPCTileDetection(NPC, ModContent.TileType<SeaPrismCrystals>(), 300, true);
                                }

                                // Go to a Prism Shard if one exists nearby
                                if (tilePos != null && tilePos != Vector2.Zero)
                                {
                                    NPC.localAI[2] = tilePos.Value.X;
                                    NPC.localAI[3] = tilePos.Value.Y;
                                    pathfinding.DoPathfinding(new(NPC.Center, tilePos.Value, SunkenSeaTileValiditySizeless));
                                    if (NPC.Distance(tilePos.Value) < 16)
                                    {
                                        CurrentPhase = (int)PhaseType.Hiding;
                                        NPC.netUpdate = true;
                                        break;
                                    }
                                }
                                // While it doesn't have any obstacles in front of it, run away in a straight line.
                                // Try to manuever if there are any obstacles.
                                else if (!Main.tile[(NPC.Center + NPC.DirectionFrom(CurrentPredator.Center) * 96).ToTileCoordinates()].IsTileSolid())
                                {
                                    NPC.velocity += NPC.DirectionFrom(CurrentPredator.Center) * pathfinding.Acceleration;
                                    pathfinding.ClearResults();

                                    // Cap the speed if MaxSpeed has been surpassed.
                                    if (NPC.velocity.LengthSquared() > pathfinding.MaxSpeed * pathfinding.MaxSpeed)
                                        NPC.velocity = Vector2.Normalize(NPC.velocity) * pathfinding.MaxSpeed;
                                }
                                else
                                {
                                    float distanceFromAvoided = Vector2.Distance(NPC.Center, CurrentPredator.Center);
                                    randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                                    NPC.netUpdate = true;
                                    pathfinding.DoPathfinding(new(NPC.Center, randomPathPoint, SunkenSeaTileValidity));
                                }
                            }
                            else
                            {
                                CurrentPhase = (int)PhaseType.Idle;
                            }
                        }
                        break;
                    // Hide
                    case (int)PhaseType.Hiding:
                        {
                            // Stay still
                            NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.05f);
                            NPC.velocity *= 0.95f;
                            Tile t = CalamityUtils.ParanoidTileRetrieval((int)(NPC.localAI[2] / 16), (int)(NPC.localAI[3] / 16));

                            // Assure the prism still exists, if the player breaks it, the disguise is gone
                            if (t.TileType != ModContent.TileType<SeaPrismCrystals>())
                            {
                                CurrentPhase = (int)PhaseType.Idle;
                                NPC.netUpdate = true;
                                break;
                            }

                            // Once the coast is clear, wait 2 seconds then go out
                            if (CurrentPredator is null)
                            {
                                NPC.localAI[1]--;
                                if (NPC.localAI[1] <= 0)
                                {
                                    CurrentPhase = (int)PhaseType.Idle;
                                    NPC.netUpdate = true;
                                }
                            }
                            // Face towards the predator and set the safety timer to 2 seconds
                            else
                            {
                                NPC.direction = NPC.DirectionTo(CurrentPredator.Center).X.DirectionalSign();
                                NPC.localAI[1] = 120;
                            }
                        }
                        break;
                }

                NPC.dontTakeDamage = NPC.alpha > 0;

                int dir = NPC.velocity.X.DirectionalSign();
                // Become transparent while hiding and opaque otherwise
                if (CurrentPhase != (int)PhaseType.Hiding)
                {
                    NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 40;
                        if (NPC.alpha < 0)
                            NPC.alpha = 0;
                    }
                }
                else
                {
                    if (NPC.alpha < 150)
                    {
                        NPC.alpha += 10;
                    }
                }
                NPC.spriteDirection = NPC.direction = dir;
            }
            else
            {
                if (NPC.velocity.Y < 12)
                    NPC.velocity.Y += 1;
            }

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

        protected override void OnPredatorDetection(NPC predator)
        {
            // Don't reset the flee state while hiding
            if (CurrentPhase != (int)PhaseType.Hiding)
                CurrentPhase = (int)PhaseType.Fleeing;
        }

        // Item sprite based on fish
        public override void OnCaughtBy(Player player, Item item, bool failed)
        {
            if (item.ModItem != null)
            {
                if (item.ModItem is PrismaticGuppyItem gup)
                {
                    if (CurrentShape == (int)FishShape.Cube)
                        gup.shapeVariant = 1;

                    if (CurrentShape == (int)FishShape.Angel)
                        gup.shapeVariant = 2;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if ((!NPC.wet && !NPC.IsABestiaryIconDummy) || CurrentPhase == (int)PhaseType.Hiding)
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
                    textureAsset = CurrentShape == (int)FishShape.Angel ? RadiantTexture3 : CurrentShape == (int)FishShape.Cube ? RadiantTexture2 : RadiantTexture;
                    break;
                case (int)FishColor.Gold:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? GoldTexture3 : CurrentShape == (int)FishShape.Cube ? GoldTexture2 : GoldTexture;
                    break;
                case (int)FishColor.Green:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? GreenTexture3 : CurrentShape == (int)FishShape.Cube ? GreenTexture2 : GreenTexture;
                    break;
                case (int)FishColor.Pink:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? PinkTexture3 : CurrentShape == (int)FishShape.Cube ? PinkTexture2 : PinkTexture;
                    break;
                default:
                    textureAsset = CurrentShape == (int)FishShape.Angel ? Texture3 : CurrentShape == (int)FishShape.Cube ? Texture2 : TextureAssets.Npc[Type];
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
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
        }
    }
}
