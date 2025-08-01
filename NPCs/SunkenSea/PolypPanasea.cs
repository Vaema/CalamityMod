using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using System.Collections.Generic;
using CalamityMod.Enums;
using System.IO;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PolypPanasea : SunkenSeaNPC
    {
        public enum FishColor
        {
            Red = 0,
            Turquoise = 1,
            Green = 2,
            Purple = 3,
            Gold = 4,
            Radiant = 5
        }
        public enum PhaseType
        {
            Idle = 0,
            Flee = 1,
            Hiding = 2
        }
        public ref float CurrentBehavior => ref NPC.ai[0];
        public ref float Variant => ref NPC.ai[1];
        public ref float PanaceaTimer => ref NPC.ai[2];

        #region Textures
        // Welcome to the fish texture wall, have a nice stay, or just collapse this region, either works
        public static Asset<Texture2D> RadiantTexture;
        public static Asset<Texture2D> GreenTexture;
        public static Asset<Texture2D> PurpleTexture;
        public static Asset<Texture2D> TurquoiseTexture;
        public static Asset<Texture2D> GoldTexture;
        public static Asset<Texture2D> TextureCoated;
        public static Asset<Texture2D> RadiantTextureCoated;
        public static Asset<Texture2D> GreenTextureCoated;
        public static Asset<Texture2D> PurpleTextureCoated;
        public static Asset<Texture2D> TurquoiseTextureCoated;
        public static Asset<Texture2D> GoldTextureCoated;
        #endregion

        public int PolypIndex = -1;

        public Vector2 randomPathPoint;

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 600;
        public static int IdleMaxPathDistance = 1200;

        public static int FleeTileAnticipationDistance = 64;

        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<SandProwler>(),
            ModContent.NPCType<SandProwlerNested>(),
            ModContent.NPCType<GhostBell>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.PolypForest;

        public override void Load()
        {
            RadiantTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiant");
            GoldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGold");
            GreenTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreen");
            PurpleTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurple");
            TurquoiseTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoise");
            TextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRedCoated");
            RadiantTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaRadiantCoated");
            GoldTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGoldCoated");
            GreenTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaGreenCoated");
            PurpleTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaPurpleCoated");
            TurquoiseTextureCoated = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/PolypPanaseaTurquoiseCoated");
            base.Load();
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
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
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PolypPanaseaBanner>();
            NPC.chaseable = false;
            NPC.catchItem = (short)ModContent.ItemType<PolypPanaseaItem>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Panaseas released by the player do not randomize when spawned
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            // Randomize the color of the fish
            Variant = Main.rand.Next(0, 4);
            // 1 in 30 chance for a rare fish variant (rfv)
            if (Main.rand.NextBool(30))
            {
                Variant = Main.rand.Next(4, 6);
            }
            // 1 in 5 chance for a Panasea to be coated
            if (Main.rand.NextBool(5))
            {
                PanaceaTimer = 61;
            }
            switch (Variant)
            {
                case (int)FishColor.Purple:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaPurpleItem>();
                    break;
                case (int)FishColor.Green:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaGreenItem>();
                    break;
                case (int)FishColor.Turquoise:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaTurquoiseItem>();
                    break;
                case (int)FishColor.Gold:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaGoldItem>();
                    break;
                case (int)FishColor.Radiant:
                    NPC.catchItem = ModContent.ItemType<PolypPanaseaRadiantItem>();
                    break;
            }
            if (Variant == (int)FishColor.Gold || Variant == (int)FishColor.Radiant)
            {
                NPC.rarity = 3;
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PolypPanasea")
            });
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    Acceleration = 0.5f,
                    MaxSpeed = 4f,
                };
            }
            // Reset polyp index in case the polyp dies
            if (PolypIndex > -1)
            {
                if (Main.npc[PolypIndex].type != ModContent.NPCType<Polyperil>() || !Main.npc[PolypIndex].active || Main.npc[PolypIndex].life <= 0 || Main.npc[PolypIndex].Distance(NPC.Center) > 1000)
                {
                    PolypIndex = -1;
                }
            }
            if (NPC.wet)
            {
                switch (CurrentBehavior)
                {
                    case (int)PhaseType.Idle:
                        IdleBehavior();
                        break;
                    case (int)PhaseType.Flee:
                        FleeBehavior();
                        break;
                    case (int)PhaseType.Hiding:
                        HideBehavior();
                        break;
                }
            }
            else
            {
                BeachedBehavior();
            }

            int dir = NPC.velocity.X.DirectionalSign();
            if (CurrentBehavior != (int)PhaseType.Hiding)
            {
                NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
            }
            else
            {
                NPC.rotation = 0;
            }
            NPC.spriteDirection = NPC.direction = dir;
            // Assure it cant be caught when collecting panacea
            if (PanaceaTimer <= 60 && PanaceaTimer > 0)
            {
                PanaceaTimer--;
            }
            if (Variant == (int)FishColor.Gold)
            {
                NPC.ProduceGoldCritterDust();
            }
        }

        public void IdleBehavior()
        {
            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(100, IdleMaxPathDistance), SunkenSeaTileValidity));
        }
        public void HideBehavior()
        {
            // Once the threat is gone or if the polyp is destroyed, go out of hiding
            if (PolypIndex <= -1 || (CurrentPredator == null && Main.rand.NextBool(120)))
            {
                NPC.dontTakeDamage = false;
                CurrentBehavior = (int)PhaseType.Idle;
                pathfinding.ClearResults();
                return;
            }

            // Become invincible while hiding
            NPC.dontTakeDamage = true;
            pathfinding.MaxSpeed = 2;

            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.DoPathfinding(new(NPC.Center, Main.npc[PolypIndex].Center + Main.rand.NextVector2Unit() * Main.rand.Next(20, 30), SunkenSeaTileValidity));
        }

        public void FleeBehavior()
        {
            // If the predator is gone, go back to idling.
            if (CurrentPredator == null && PolypIndex <= -1)
            {
                CurrentBehavior = (int)PhaseType.Idle;
                pathfinding.MaxSpeed = 4;
                return;
            }

            pathfinding.MaxSpeed = 6;

            // If a polyp is found, try to run into it
            if (PolypIndex > -1)
            {
                NPC.netUpdate = true;
                Vector2 polypPos = Main.npc[PolypIndex].position;
                pathfinding.DoPathfinding(new(NPC.Center, polypPos, SunkenSeaTileValidity));

                if (NPC.Distance(Main.npc[PolypIndex].Center) < 50)
                {
                    CurrentBehavior = (int)PhaseType.Hiding;
                    pathfinding.ClearResults();
                }
            }
            // While it doesn't have any obstacles in front of it, run away in a straight line.
            // Try to manuever if there are any obstacles.
            else if (!Main.tile[(NPC.Center + NPC.DirectionFrom(CurrentPredator.Center) * FleeTileAnticipationDistance).ToTileCoordinates()].IsTileSolid())
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

        public void BeachedBehavior()
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

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || n == CurrentPredator && Vector2.DistanceSquared(NPC.Center, n.Center) < 1960f * 1960f;
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 960f * 960f;
        }
        protected override void OnPredatorDetection(NPC predator)
        {
            if (CurrentBehavior == (int)PhaseType.Idle)
            {
                pathfinding.ClearResults();
                CurrentBehavior = (int)PhaseType.Flee;
                AlertPolyp();
            }
        }

        public void AlertPolyp()
        {
            // Look for a nearby polyp to hide in
            if (PolypIndex <= -1)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    bool nCloser = PolypIndex <= -1 ? true : n.Distance(NPC.Center) < Main.npc[PolypIndex].Distance(NPC.Center);
                    if (n.type == ModContent.NPCType<Polyperil>() && nCloser)
                    {
                        PolypIndex = n.whoAmI;
                    }
                }
            }
            // If a polyp is found, cause both NPCs to signal each other with a !
            if (PolypIndex > -1)
            {
                SoundEngine.PlaySound(SoundID.NPCHit37 with { Pitch = 1 }, NPC.Center);

                if (!Main.dedServ)
                {
                    var emoteDirection = -Vector2.UnitY * Main.rand.NextFloat(2f, 3f);
                    Particle emote = new EmoteExpressionParticle(
                        NPC.Center + emoteDirection * 2f,
                        emoteDirection,
                        2.2f,
                        Color.Red,
                        Main.rand.Next(30, 46),
                        EmoteExpressionParticle.EmoteType.Exclamation);
                    GeneralParticleHandler.SpawnParticle(emote);
                    Particle emote2 = new EmoteExpressionParticle(
                        Main.npc[PolypIndex].Center + emoteDirection * 2f,
                        emoteDirection,
                        2.2f,
                        Color.Yellow,
                        Main.rand.Next(30, 46),
                        EmoteExpressionParticle.EmoteType.Exclamation);
                    GeneralParticleHandler.SpawnParticle(emote2);
                }
            }
        }

        public override void ModifyTypeName(ref string typeName)
        {
            if (Variant == (int)FishColor.Radiant)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.RadiantPolypPanasea");
            }
            if (Variant == (int)FishColor.Gold)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.GoldPolypPanasea");
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 0.1f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
            NPC.ai[3] = frame;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZonePolypForest && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.8f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            bool Coated = PanaceaTimer >= 61;

            Texture2D texture = TextureAssets.Npc[Type].Value;
            switch (Variant)
            {
                case (int)FishColor.Radiant:
                    texture = Coated ? RadiantTextureCoated.Value : RadiantTexture.Value;
                    break;
                case (int)FishColor.Gold:
                    texture = Coated ? GoldTextureCoated.Value : GoldTexture.Value;
                    break;
                case (int)FishColor.Purple:
                    texture = Coated ? PurpleTextureCoated.Value : PurpleTexture.Value;
                    break;
                case (int)FishColor.Green:
                    texture = Coated ? GreenTextureCoated.Value : GreenTexture.Value;
                    break;
                case (int)FishColor.Turquoise:
                    texture = Coated ? TurquoiseTextureCoated.Value : TurquoiseTexture.Value;
                    break;
                case (int)FishColor.Red:
                    texture = Coated ? TextureCoated.Value : TextureAssets.Npc[Type].Value;
                    break;
            }
            Vector2 origin = new Vector2((float)(texture.Width / 2), (float)(texture.Height / Main.npcFrameCount[Type] / 2));
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2((float)texture.Width, (float)(texture.Height / Main.npcFrameCount[Type])) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            Rectangle frame = texture.Frame(1, 6, 0, (int)NPC.ai[3]);
            spriteBatch.Draw(texture, npcOffset, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }
        public override bool? CanBeCaughtBy(Item item, Player player)
        {
            if (PanaceaTimer > 0)
            {
                if (PanaceaTimer > 60)
                {
                    // TODO: add actual panacea item
                    Item.NewItem(NPC.GetSource_CatchEntity(NPC), (int)NPC.Center.X, (int)NPC.Center.Y, 1, 1, ItemID.FlaskofPoison);
                    PanaceaTimer = 60;
                }
                return false;
            }            
            return null;
        }
        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (attacker.type == ModContent.NPCType<Polyperil>() || attacker.type == ModContent.NPCType<PolyperilTentacle>())
                return false;
            return PredatorIDs.Contains(attacker.type);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(randomPathPoint);
            writer.Write(PolypIndex);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
            PolypIndex = reader.ReadInt32();
        }
    }
}
