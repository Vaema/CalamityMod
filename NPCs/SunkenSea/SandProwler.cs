using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class SandProwler : SunkenSeaNPC
    {
        public const int maxLength = 14;
        public float speed = 3f;
        public float turnSpeed = 0.0625f;
        bool TailSpawned = false;
        #region Textures
        public static Asset<Texture2D> BodySprite1;
        public static Asset<Texture2D> BodySprite2;
        public static Asset<Texture2D> BodySprite3;
        public static Asset<Texture2D> BodySprite4;
        public static Asset<Texture2D> BodySprite5;
        public static Asset<Texture2D> BodySprite6;
        public static Asset<Texture2D> BodySprite7;
        public static Asset<Texture2D> TailSprite;
        #endregion

        public enum AnimType
        {
            None = 0,
            Blink = 1,
            Bite = 2
        }

        public bool IsHead => NPC.ai[3] == 0;

        public bool IsTail => NPC.ai[3] == 8;

        public ref float CurrentAnimation => ref NPC.localAI[1];

        public ref float CurrentFrame => ref NPC.localAI[0];

        public ref float BlinkTimer => ref NPC.localAI[2];
        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<PolypPanasea>(),
            ModContent.NPCType<PrismaticGuppy>(),
            ModContent.NPCType<Slugbun>(),
        };

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<Polyperil>(),
            ModContent.NPCType<PolyperilTentacle>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.PolypForest;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                PortraitPositionXOverride = 40,
                PortraitPositionYOverride = 20
            };
            value.Position.Y += 20;
            value.Position.X += 40;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true; // It will only eat coins that the AI says it can, and when it does, you aren't getting them back
            if (!Main.dedServ)
            {
                BodySprite1 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler2", AssetRequestMode.AsyncLoad);
                BodySprite2 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler3", AssetRequestMode.AsyncLoad);
                BodySprite3 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler4", AssetRequestMode.AsyncLoad);
                BodySprite4 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler5", AssetRequestMode.AsyncLoad);
                BodySprite5 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler6", AssetRequestMode.AsyncLoad);
                BodySprite6 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler7", AssetRequestMode.AsyncLoad);
                BodySprite7 = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler8", AssetRequestMode.AsyncLoad);
                TailSprite = ModContent.Request<Texture2D>("CalamityMod/NPCs/SunkenSea/SandProwler9", AssetRequestMode.AsyncLoad);
            }
            Main.npcFrameCount[Type] = 11;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.damage = 50;
            NPC.width = 30;
            NPC.height = 24; 
            NPC.defense = 10;
            NPC.lifeMax = 300;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 20);
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.netAlways = true;
            NPC.chaseable = false;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<SandProwlerBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            NPC.waterMovementSpeed = 1;
            NPC.GravityIgnoresLiquid = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.SandProwler")
            });
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // only the head gets a healthbar
            return IsHead;
        }


        public override void AI()
        {
            if (IsHead)
                HeadAI();
            else
                SegmentAI();
        }
        public void HeadAI()
        {
            Point point = NPC.Center.ToTileCoordinates();
            Tile tileSafely = Framing.GetTileSafely(point);
            bool createDust = tileSafely.HasUnactuatedTile && NPC.Distance(Main.player[NPC.target].Center) < 800f;
            if (createDust)
            {
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 204, 0f, 0f, 150, default(Color), 0.3f);
                    dust.fadeIn = 0.75f;
                    dust.velocity *= 0.1f;
                    dust.noLight = true;
                }
            }

            if (NPC.ai[2] > 0f)
            {
                NPC.realLife = (int)NPC.ai[2];
            }
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!TailSpawned && NPC.ai[0] == 0f)
                {
                    int Previous = NPC.whoAmI;
                    for (int segment = 0; segment < maxLength; segment++)
                    {
                        int lol = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<SandProwler>(), NPC.whoAmI);
                        Main.npc[lol].realLife = NPC.whoAmI;
                        Main.npc[lol].ai[2] = (float)NPC.whoAmI;
                        Main.npc[lol].ai[1] = (float)Previous;
                        Main.npc[Previous].ai[0] = (float)lol;
                        float spriteUse = segment % 2 == 0 ? 3 : 4; // Looping body 3 and 4
                        int width = 18;
                        switch (segment)
                        {
                            case maxLength - 1:
                                spriteUse = 8; // Tail
                                break;
                            case maxLength - 2:
                                spriteUse = 7; // Body 7
                                width = 14 ;
                                break;
                            case maxLength - 3:
                                spriteUse = 6; // Body 6
                                width = 16;
                                break;
                            case maxLength - 4:
                                spriteUse = 5; // Body 5
                                break;
                            case 0:
                                spriteUse = 1; // Body 1
                                break;
                            case 1:
                                spriteUse = 2; // Body 2
                                break;
                        }
                        Main.npc[lol].ai[3] = spriteUse;
                        Main.npc[lol].npcSlots = 0;
                        Main.npc[lol].dontCountMe = true;
                        Main.npc[lol].width = width;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, lol, 0f, 0f, 0f, 0);
                        Previous = lol;
                    }
                    TailSpawned = true;
                }
            }
            if (NPC.velocity.X < 0f)
            {
                NPC.spriteDirection = -1;
            }
            else if (NPC.velocity.X > 0f)
            {
                NPC.spriteDirection = 1;
            }
            if (Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(false);
            }
            NPC.alpha -= 42;
            if (NPC.alpha < 0)
            {
                NPC.alpha = 0;
            }
            if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 5600f)
            {
                NPC.active = false;
            }
            float currentSpeed = speed;
            float currentTurnSpeed = turnSpeed;
            Vector2 segmentPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
            Entity targ = CurrentPrey != null ? CurrentPrey : Main.player[NPC.target];
            float targetXDist = targ.position.X + (float)(targ.width / 2);
            float targetYDist = targ.position.Y + (float)(targ.height / 2);
            bool coinTarget = false;
            // fucking run if it notices a predator
            if (CurrentPredator != null)
            {
                Vector2 dirToPred = NPC.DirectionTo(CurrentPredator.Center);
                targetXDist = -dirToPred.X * 300;
                targetYDist = -dirToPred.Y * 300;
                currentSpeed *= 2f;
                currentTurnSpeed *= 11.5f;
            }
            else
            {
                // Look for silver and gold coins to eat
                for (int i = 0; i < Main.maxItems; i++)
                {
                    Item item = Main.item[i];
                    // continue if not an active silver or gold coin
                    if (item == null || !item.active || (item.type != ItemID.SilverCoin && item.type != ItemID.GoldCoin))
                        continue;
                    // can only look for coins in a 75 tile radius
                    if (item.Distance(NPC.Center) > 1200)
                        continue;
                    // if its head touches the coin, eat it
                    if (item.getRect().Intersects(NPC.getRect()))
                    {
                        SoundEngine.PlaySound(SoundID.Item2 with { Pitch = 1.2f, Volume = 0.8f }, NPC.Center);
                        SoundEngine.PlaySound(SoundID.CoinPickup, NPC.Center);
                        int dustType = item.type == ItemID.SilverCoin ? DustID.SilverCoin : DustID.GoldCoin;
                        for (int j = 0; j < 4; j++)
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                        }
                        item.active = false;
                        break;
                    }
                    // if it isn't touching the coin, go to it
                    targetXDist = item.Center.X;
                    targetYDist = item.Center.Y;
                    coinTarget = true;
                    break;
                }
            }
            // aggro on coins takes priority over players
            if (CurrentPredator == null && (NPC.life > NPC.lifeMax * 0.99 && targ is Player) && !coinTarget)
            {
                targetYDist += 300;
                if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) < 250f)
                {
                    if (NPC.velocity.X > 0f)
                    {
                        targetXDist = Main.player[NPC.target].Center.X + 300f;
                    }
                    else
                    {
                        targetXDist = Main.player[NPC.target].Center.X - 300f;
                    }
                }

                // Blink every so often
                if (Main.rand.NextBool(600) && CurrentAnimation == (int)AnimType.None && BlinkTimer <= 0)
                {
                    CurrentAnimation = (int)AnimType.Blink;
                    BlinkTimer = Main.rand.NextBool(4) ? 48 : 24;
                }
            }
            else if (CurrentPredator == null)
            {
                currentSpeed *= 1.5f;
                currentTurnSpeed *= 1.5f;
            }
            float maxCurrentSpeed = currentSpeed * 1.3f;
            float minCurrentSpeed = currentSpeed * 0.7f;
            float speedCompare = NPC.velocity.Length();
            if (speedCompare > 0f)
            {
                if (speedCompare > maxCurrentSpeed)
                {
                    NPC.velocity.Normalize();
                    NPC.velocity *= maxCurrentSpeed;
                }
                else if (speedCompare < minCurrentSpeed)
                {
                    NPC.velocity.Normalize();
                    NPC.velocity *= minCurrentSpeed;
                }
            }
            targetXDist = (float)((int)(targetXDist / 16f) * 16);
            targetYDist = (float)((int)(targetYDist / 16f) * 16);
            segmentPosition.X = (float)((int)(segmentPosition.X / 16f) * 16);
            segmentPosition.Y = (float)((int)(segmentPosition.Y / 16f) * 16);
            targetXDist -= segmentPosition.X;
            targetYDist -= segmentPosition.Y;
            float targetDistance = (float)System.Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
            float absoluteTargetX = System.Math.Abs(targetXDist);
            float absoluteTargetY = System.Math.Abs(targetYDist);
            float timeToReachTarget = currentSpeed / targetDistance;
            targetXDist *= timeToReachTarget;
            targetYDist *= timeToReachTarget;
            if (targetDistance < 128 && targ is NPC || (NPC.life <= NPC.lifeMax * 0.99 && targ is Player) || coinTarget)
            {
                CurrentAnimation = (int)AnimType.Bite;
            }
            else if (CurrentAnimation != (int)AnimType.Blink)
            {
                CurrentAnimation = (int)AnimType.None;
            }
            BlinkTimer--;
            if (BlinkTimer <= 0)
            {
                BlinkTimer = 0;
                if (CurrentAnimation == (int)AnimType.Blink)
                {
                    CurrentAnimation = (int)AnimType.None;
                }
            }
            if ((NPC.velocity.X > 0f && targetXDist > 0f) || (NPC.velocity.X < 0f && targetXDist < 0f) || (NPC.velocity.Y > 0f && targetYDist > 0f) || (NPC.velocity.Y < 0f && targetYDist < 0f))
            {
                if (NPC.velocity.X < targetXDist)
                {
                    NPC.velocity.X = NPC.velocity.X + currentTurnSpeed;
                }
                else
                {
                    if (NPC.velocity.X > targetXDist)
                    {
                        NPC.velocity.X = NPC.velocity.X - currentTurnSpeed;
                    }
                }
                if (NPC.velocity.Y < targetYDist)
                {
                    NPC.velocity.Y = NPC.velocity.Y + currentTurnSpeed;
                }
                else
                {
                    if (NPC.velocity.Y > targetYDist)
                    {
                        NPC.velocity.Y = NPC.velocity.Y - currentTurnSpeed;
                    }
                }
                if ((double)System.Math.Abs(targetYDist) < (double)currentSpeed * 0.2 && ((NPC.velocity.X > 0f && targetXDist < 0f) || (NPC.velocity.X < 0f && targetXDist > 0f)))
                {
                    if (NPC.velocity.Y > 0f)
                    {
                        NPC.velocity.Y = NPC.velocity.Y + currentTurnSpeed * 2f;
                    }
                    else
                    {
                        NPC.velocity.Y = NPC.velocity.Y - currentTurnSpeed * 2f;
                    }
                }
                if ((double)System.Math.Abs(targetXDist) < (double)currentSpeed * 0.2 && ((NPC.velocity.Y > 0f && targetYDist < 0f) || (NPC.velocity.Y < 0f && targetYDist > 0f)))
                {
                    if (NPC.velocity.X > 0f)
                    {
                        NPC.velocity.X = NPC.velocity.X + currentTurnSpeed * 2f; //changed from 2
                    }
                    else
                    {
                        NPC.velocity.X = NPC.velocity.X - currentTurnSpeed * 2f; //changed from 2
                    }
                }
            }
            else
            {
                if (absoluteTargetX > absoluteTargetY)
                {
                    if (NPC.velocity.X < targetXDist)
                    {
                        NPC.velocity.X = NPC.velocity.X + currentTurnSpeed * 1.1f; //changed from 1.1
                    }
                    else if (NPC.velocity.X > targetXDist)
                    {
                        NPC.velocity.X = NPC.velocity.X - currentTurnSpeed * 1.1f; //changed from 1.1
                    }
                    if ((double)(System.Math.Abs(NPC.velocity.X) + System.Math.Abs(NPC.velocity.Y)) < (double)currentSpeed * 0.5)
                    {
                        if (NPC.velocity.Y > 0f)
                        {
                            NPC.velocity.Y = NPC.velocity.Y + currentTurnSpeed;
                        }
                        else
                        {
                            NPC.velocity.Y = NPC.velocity.Y - currentTurnSpeed;
                        }
                    }
                }
                else
                {
                    if (NPC.velocity.Y < targetYDist)
                    {
                        NPC.velocity.Y = NPC.velocity.Y + currentTurnSpeed * 1.1f;
                    }
                    else if (NPC.velocity.Y > targetYDist)
                    {
                        NPC.velocity.Y = NPC.velocity.Y - currentTurnSpeed * 1.1f;
                    }
                    if ((double)(System.Math.Abs(NPC.velocity.X) + System.Math.Abs(NPC.velocity.Y)) < (double)currentSpeed * 0.5)
                    {
                        if (NPC.velocity.X > 0f)
                        {
                            NPC.velocity.X = NPC.velocity.X + currentTurnSpeed;
                        }
                        else
                        {
                            NPC.velocity.X = NPC.velocity.X - currentTurnSpeed;
                        }
                    }
                }
            }
            NPC.rotation = (float)System.Math.Atan2((double)NPC.velocity.Y, (double)NPC.velocity.X) + 1.57f;
        }

        public void SegmentAI()
        {
            if (NPC.ai[2] > 0f)
                NPC.realLife = (int)NPC.ai[2];

            // Check if other segments are still alive, if not, die
            bool shouldDespawn = true;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<SandProwler>() && Main.npc[i].ai[3] == 0)
                {
                    shouldDespawn = false;
                    break;
                }
            }
            if (!shouldDespawn)
            {
                if (NPC.ai[1] <= 0f)
                    shouldDespawn = true;
                else if (Main.npc[(int)NPC.ai[1]].life <= 0)
                    shouldDespawn = true;
            }
            if (shouldDespawn)
            {
                NPC.life = 0;
                NPC.HitEffect(0, 10.0);
                NPC.checkDead();
                NPC.active = false;
            }

            if (Main.npc[(int)NPC.ai[1]].alpha < 128)
            {
                NPC.alpha -= 42;
                if (NPC.alpha < 0)
                    NPC.alpha = 0;
            }

            Vector2 segmentPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
            float targetXDist = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2);
            float targetYDist = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2);
            targetXDist = (float)((int)(targetXDist / 16f) * 16);
            targetYDist = (float)((int)(targetYDist / 16f) * 16);
            segmentPosition.X = (float)((int)(segmentPosition.X / 16f) * 16);
            segmentPosition.Y = (float)((int)(segmentPosition.Y / 16f) * 16);
            targetXDist -= segmentPosition.X;
            targetYDist -= segmentPosition.Y;
            float targetDistance = (float)System.Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
            if (NPC.ai[1] > 0f && NPC.ai[1] < (float)Main.npc.Length)
            {
                try
                {
                    segmentPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                    float headDivisor = 1;
                    targetXDist = Main.npc[(int)NPC.ai[1]].position.X + (float)(Main.npc[(int)NPC.ai[1]].width / 2 / headDivisor) - segmentPosition.X;
                    targetYDist = Main.npc[(int)NPC.ai[1]].position.Y + (float)(Main.npc[(int)NPC.ai[1]].height / 2) - segmentPosition.Y;
                }
                catch
                {
                }
                NPC.rotation = (float)System.Math.Atan2((double)targetYDist, (double)targetXDist) + 1.57f;
                targetDistance = (float)System.Math.Sqrt((double)(targetXDist * targetXDist + targetYDist * targetYDist));
                int segmentWidth = NPC.width;
                targetDistance = (targetDistance - (float)segmentWidth) / targetDistance;
                targetXDist *= targetDistance;
                targetYDist *= targetDistance;
                NPC.velocity = Vector2.Zero;
                NPC.position.X = NPC.position.X + targetXDist;
                NPC.position.Y = NPC.position.Y + targetYDist;

                if (targetXDist < 0f)
                    NPC.spriteDirection = -1;
                else if (targetXDist > 0f)
                    NPC.spriteDirection = 1;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (IsTail)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 6)
                {
                    NPC.localAI[0]++;
                    NPC.frameCounter = 0;
                }
                if (NPC.localAI[0] > 7)
                {
                    NPC.localAI[0] = 0;
                }
            }
            if (IsHead)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 6)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0;
                }
                switch (CurrentAnimation)
                {
                    case (int)AnimType.None:
                            NPC.frame.Y = 0;
                        break;
                    case (int)AnimType.Bite:
                        if (NPC.frame.Y < frameHeight * 5 || NPC.frame.Y > frameHeight * 10)
                        {
                            NPC.frame.Y = frameHeight * 5;
                        }
                        break;
                    case (int)AnimType.Blink:
                        if (NPC.frame.Y < frameHeight || NPC.frame.Y >  frameHeight * 4)
                        {
                            NPC.frame.Y = frameHeight;
                        }
                        break;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.hardMode && spawnInfo.Player.Calamity().ZonePolypForest && spawnInfo.Water &&
                !NPC.AnyNPCs(ModContent.NPCType<SandProwler>()) && !spawnInfo.Player.Calamity().clamity && !spawnInfo.PlayerSafe)
                return SpawnCondition.CaveJellyfish.Chance * 0.3f;

            return 0f;
        }

        public override bool CheckActive()
        {
            return IsHead;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            float huntRange = 600f;
            float avoidRange = 200f;
            bool preyFilter = Vector2.DistanceSquared(NPC.Center, n.Center) < huntRange * huntRange && PreyIDs.Contains(n.type);
            bool predFilter = Vector2.DistanceSquared(NPC.Center, n.Center) < avoidRange * avoidRange && PredatorIDs.Contains(n.type);
            bool hidingGuppy = n.type == ModContent.NPCType<PrismaticGuppy>() && n.alpha > 0;
            return !hidingGuppy && (preyFilter || predFilter);
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override bool CanHitNPC(NPC target)
        {
            return IsHead;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            PlayerHurt();
        }

        public void PlayerHurt()
        {
            if (IsHead)
                NPC.chaseable = true;
            else
                Main.npc[NPC.realLife].chaseable = true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => DefineSandProwlerLoot(npcLoot);

        public static void DefineSandProwlerLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<Serpentine>(), 4);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SeaSerpentGore1").Type, 1f);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D segmentSprite = TextureAssets.Npc[Type].Value;
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frame = segmentSprite.Frame();
                // Buffers the segment position and rotations
                float offset = -0.2f;
                float startX = 0;
                float startY = 0;
                int segmentSpacing = 16;
                int animationSpeed = 5;
                float wormTimer = NPC.Calamity().bestiaryWormTimer;
                // Draw the body segments
                for (int i = 7; i > 0; i--)
                {
                    // The first segment is slightly closer to keep up with the head
                    float bodyOffset = i == 1 ? i * segmentSpacing * 0.4f : i * segmentSpacing - segmentSpacing * 0.5f;

                    Texture2D toUse = i == 1 ? BodySprite1.Value : i == 2 ? BodySprite2.Value : i % 2 == 0 ? BodySprite3.Value : BodySprite4.Value;
                    spriteBatch.Draw(toUse, NPC.position + new Vector2(startX + bodyOffset, MathF.Sin((wormTimer + offset * i) * animationSpeed) * 2 + startY), toUse.Frame(1, 1, 0, 0), NPC.GetAlpha(drawColor), NPC.rotation - MathHelper.PiOver2 - MathF.Cos((wormTimer + offset * i) * animationSpeed) * MathHelper.PiOver4 * 0.075f, new Vector2(toUse.Width / 2, toUse.Width / 2), NPC.scale, SpriteEffects.None, 0f);
                }
                // Draw the head
                spriteBatch.Draw(segmentSprite, NPC.position + new Vector2(startX, MathF.Sin(wormTimer * animationSpeed) * 2 + startY), segmentSprite.Frame(1, 11, 0, 0), NPC.GetAlpha(drawColor), NPC.rotation - MathHelper.PiOver2 - MathF.Cos(wormTimer * animationSpeed) * MathHelper.PiOver4 * 0.075f, new Vector2(segmentSprite.Width * 0.5f, segmentSprite.Height / 11), NPC.scale, SpriteEffects.None, 0f);

                return false;
            }
            switch (NPC.ai[3])
            {
                case 0:
                    break;
                case 1:
                    segmentSprite = BodySprite1.Value;
                    break;
                case 2:
                    segmentSprite = BodySprite2.Value;
                    break;
                case 3:
                    segmentSprite = BodySprite3.Value;
                    break;
                case 4:
                    segmentSprite = BodySprite4.Value;
                    break;
                case 5:
                    segmentSprite = BodySprite5.Value;
                    break;
                case 6:
                    segmentSprite = BodySprite6.Value;
                    break;
                case 7:
                    segmentSprite = BodySprite7.Value;
                    break;
                case 8:
                    segmentSprite = TailSprite.Value;
                    break;
                default:
                    segmentSprite = NPC.ai[3] % 2 == 0 ? BodySprite4.Value : BodySprite3.Value;
                    break;
            }
            float frameDivisor = IsTail ? 8 : IsHead ? Main.npcFrameCount[Type] : 1;
            Vector2 origin = new Vector2(segmentSprite.Width / 2, segmentSprite.Height / 2 / frameDivisor);
            Vector2 npcOffset = NPC.Center - screenPos;
            npcOffset -= new Vector2(segmentSprite.Width, segmentSprite.Height / frameDivisor) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            SpriteEffects fx = NPC.oldPos[1].X < NPC.position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Rectangle frame = segmentSprite.Frame(1, 1, 0, 0);
            if (IsHead)
            {
                frame = NPC.frame;
            }
            else if (IsTail)
            {
                frame = segmentSprite.Frame(1, 8, 0, (int)NPC.localAI[0]);
            }
            spriteBatch.Draw(segmentSprite, npcOffset, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, fx, 0f);
            return false;
        }
    }
}
