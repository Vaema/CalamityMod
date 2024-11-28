using CalamityMod.BiomeManagers;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Scavenger : ModNPC
    {
        public static Asset<Texture2D> walkTexture;

        public static Asset<Texture2D> inspectTexture;

        public static Asset<Texture2D> giveTexture;

        // Items that can be given to the Scavenger
        public static Dictionary<int, int> currencies = new Dictionary<int, int>();

        // Items that can be received form the Scavenger
        public static Dictionary<int, float> rewards = new Dictionary<int, float>();

        // Ditto the above but in a different format that allows for clean rng rolls
        public static WeightedRandom<int> rewardsRoll = new WeightedRandom<int>();

        // The in world index of the item the crab is going after
        public int HeldItemIndex
        {
            get => (int)NPC.ai[1] - 1;
            set => NPC.ai[1] = value + 1;
        }

        // The type of currency the crab is currently holding
        public int HeldItemType
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        // The crab's current behaviour
        public ref float Phase => ref NPC.ai[0];

        // Used during trading and as a cooldown
        public ref float TradeTimer => ref NPC.ai[3];

        public ref float WalkTimer => ref NPC.Calamity().newAI[0];

        public ref float TurnTimer => ref NPC.Calamity().newAI[1];

        public ref float WalkOrStand => ref NPC.Calamity().newAI[2];

        public bool ShouldUseWalkingFrames => NPC.velocity.X != 0 && ((WalkOrStand == 1 && Phase == (int)PhaseType.Idle) || (Phase == (int)PhaseType.FoundItem));

        public bool ShouldUseInspectionFrames => Phase == (int)PhaseType.Bartering && TradeTimer < 80;

        public bool ShouldUseGivingFrames => Phase == (int)PhaseType.Bartering && TradeTimer >= 80;


        public enum PhaseType
        {
            Idle = 0,
            FoundItem = 1,
            Bartering = 2
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            Main.npcFrameCount[Type] = 7;

            // Fill the currency list
            // The key is the item type, the value is how many rolls the item provides
            currencies.Add((int)ItemID.WhitePearl, 1);
            currencies.Add((int)ItemID.BlackPearl, 2);
            currencies.Add((int)ItemID.PinkPearl, 5);

            // Fill the rewards list
            // The key is the item type, the value is the item's rarity
            rewards.Add(ModContent.ItemType<Driftwood>(), 0.5f);
            rewards.Add(ModContent.ItemType<BurntSienna>(), 15f);
            rewards.Add(ModContent.ItemType<Runestone>(), 0.2f);
            rewards.Add(ModContent.ItemType<RuneSand>(), 0.2f);
            rewards.Add(ModContent.ItemType<AmidiasSpark>(), 15f);
            rewards.Add(ItemID.PalmWoodBreastplate, 10f);
            rewards.Add(ItemID.PalmWoodHelmet, 10f);
            rewards.Add(ItemID.PalmWoodGreaves, 10f);

            // Fill the drop pool
            foreach (var v in rewards)
            {
                // Have the value act as a divisor since WeightedRandom prioritizes higher values
                rewardsRoll.Add(v.Key, 1 / v.Value);
            }

            if (!Main.dedServ)
            {
                walkTexture = ModContent.Request<Texture2D>(Texture + "Walking");
                inspectTexture = ModContent.Request<Texture2D>(Texture + "Inspecting");
                giveTexture = ModContent.Request<Texture2D>(Texture + "Giving");
            }
        }

        public override void SetDefaults()
        {
            NPC.damage = 20;
            NPC.width = 48;
            NPC.height = 48;
            NPC.defense = 5;
            NPC.lifeMax = 350;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<ScavengerBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Scavenger")
            });
        }

        public override void AI()
        {
            // Check to make sure the target item still exists
            if (HeldItemIndex > 0)
            {
                Item targetItem = Main.item[HeldItemIndex];
                // If the item doesn't exist, reset its held item
                if (targetItem == null || !targetItem.active)
                {
                    HeldItemIndex = -1;
                }
            }

            // Initialize its direction
            if (NPC.direction == 0)
                NPC.direction = Main.rand.NextBool() ? -1 : 1;

            switch (Phase)
            {
                // Do idle stuff, actual behavior not determined rn
                case (int)PhaseType.Idle:

                    // Decide if it should walk or sit
                    float movementSpeed = 1f;
                    if (WalkTimer <= 0)
                    {
                        WalkTimer = Main.rand.Next(180, 340);
                        WalkOrStand = WalkOrStand <= 0 ? 1 : -1;
                    }
                    // If it bumps into something, turn around
                    if (TurnTimer <= 0 && NPC.velocity.X == 0 && WalkOrStand == 1)
                    {
                        NPC.direction *= -1;
                        TurnTimer = 30;
                    }
                    // Move
                    if (!NPC.justHit)
                    {
                        if (WalkOrStand == 1)
                            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * movementSpeed, 0.05f);
                        else if (NPC.velocity.Y == 0)
                            NPC.velocity.X *= 0.95f;
                    }
                    else if (WalkOrStand == -1)
                    {
                        WalkTimer = 0;
                    }

                    StepUp();

                    // If the trade timer is 0 and the crab isn't looking ofr an item, look for an item
                    if (TradeTimer >= 0 && HeldItemIndex <= -1)
                    {
                        float curDist = 0;
                        int currencyRarity = 0;
                        foreach (Item i in Main.ActiveItems)
                        {
                            if (!i.active)
                                continue;
                            if (i.beingGrabbed)
                                continue;
                            float distance = i.Distance(NPC.Center);
                            if (distance > 460)
                                continue;

                            // Check if the item is a valid currency and is the closest possible currency
                            if (currencies.ContainsKey(i.type))
                            {
                                if (distance < curDist || curDist == 0)
                                {
                                    HeldItemIndex = i.whoAmI;
                                    currencyRarity = currencies[i.type];
                                }
                                curDist = distance;
                            }
                        }
                        // If an item was found, go after it
                        if (curDist != 0)
                        {
                            NPC.netUpdate = true;
                            Phase = (int)PhaseType.FoundItem;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            if (NPC.velocity.Y == 0)
                                NPC.velocity.Y = -5;

                            SoundEngine.PlaySound(SoundID.NPCHit51 with { Pitch = -0.4f }, NPC.Center);

                            EmoteExpressionParticle.EmoteType eType = currencyRarity >= 5 ? EmoteExpressionParticle.EmoteType.DoubleExclamation : EmoteExpressionParticle.EmoteType.Exclamation;
                            
                            var emoteDirection = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 3f);
                            Particle emote = new EmoteExpressionParticle(
                                NPC.Center + emoteDirection * 2f,
                                emoteDirection,
                                2.2f,
                                Color.YellowGreen,
                                Main.rand.Next(30, 46),
                                eType);
                            GeneralParticleHandler.SpawnParticle(emote);
                        }
                    }
                    // Increment the trade timer back to zero if it is below zero
                    if (TradeTimer < 0)
                        TradeTimer++;
                    break;
                // Go to the item
                case (int)PhaseType.FoundItem:
                    {
                        NPC.direction = NPC.velocity.X.DirectionalSign();

                        // If the item is no longer valid, go back to idle behaviour
                        if (HeldItemIndex <= -1)
                        {
                            NPC.netUpdate = true;
                            Phase = (int)PhaseType.Idle;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            return;
                        }
                        
                        // If the item is suddenly no longer valid for some reason, go back to idle behaviur
                        Item targetItem = Main.item[HeldItemIndex];
                        if (!targetItem.active || !currencies.ContainsKey(targetItem.type))
                        {
                            NPC.netUpdate = true;
                            HeldItemIndex = -1;
                            NPC.velocity = Vector2.Zero;
                            Phase = (int)PhaseType.Idle;
                            TurnTimer = 0;
                            WalkTimer = 0;
                            break;
                        }

                        // If it gets stuck, jump
                        if (NPC.velocity.X == 0)
                        {
                            WalkTimer--;
                            if (WalkTimer <= -30 && NPC.velocity.Y == 0)
                            {
                                WalkTimer = 120;
                                NPC.velocity.Y = -6;
                                TurnTimer--;
                            }
                        }

                        // Movement goes here
                        NPC.velocity.X = NPC.DirectionTo(targetItem.Center).X * 3;
                        StepUp();

                        // If 3 jumps fail, give up
                        if (TurnTimer <= -3)
                        {
                            TurnTimer = 0;
                            WalkTimer = 0;
                            NPC.netUpdate = true;
                            HeldItemIndex = -1;
                            Phase = (int)PhaseType.Idle;
                            TradeTimer = -CalamityUtils.SecondsToFrames(4);
                        }

                        int grabRangeX = 5;
                        Rectangle itemGrabHitbox = new Rectangle((int)NPC.position.X - grabRangeX, (int)NPC.Center.Y, (int)NPC.width + grabRangeX * 2, (int)(NPC.height * 0.5f) + 20);

                        // Grab the item if close enough
                        if (itemGrabHitbox.Distance(targetItem.position) < 5)
                        {
                            NPC.netUpdate = true;
                            Phase = (int)PhaseType.Bartering;
                            // If the item's stack is 1, despawn the item. Otherwise decrement its stack by 1.
                            if (targetItem.stack == 1)
                                targetItem.active = false;
                            else
                                targetItem.stack--;
                            // Set the crab's held item type
                            HeldItemType = targetItem.type;
                            TurnTimer = 0;
                            WalkTimer = 0;
                        }
                    }
                    break;
                // Ponder the held item then give a reward back
                case (int)PhaseType.Bartering:
                    {
                        NPC.velocity.X *= NPC.velocity.Y != 0 ? 0.96f : 0.9f;
                        TradeTimer++;
                        // Calculate the reward
                        if (TradeTimer == 132)
                        {
                            NPC.netUpdate = true;
                            HeldItemType = CalculateReward();
                        }

                        // After some time, spit out a reward and go back to idle with a cooldown
                        if (TradeTimer > 160)
                        {
                            NPC.netUpdate = true;
                            HeldItemIndex = -1;
                            Phase = (int)PhaseType.Idle;
                            // This timer increments during its idle phase so it's set to a negative value
                            // Once the value hits zero, the crab will be able to trade again
                            TradeTimer = -CalamityUtils.SecondsToFrames(8);

                            // Spawn the reward
                            if (HeldItemType > 0)
                            {
                                int i = Item.NewItem(NPC.GetSource_FromThis(), new Rectangle((int)NPC.Center.X + NPC.direction * 20, (int)NPC.Center.Y - 20, NPC.width, NPC.height), HeldItemType);
                                Main.item[i].velocity = new Vector2(NPC.direction * 4, -1);
                            }
                            HeldItemType = 0;
                        }
                    }
                    break;
            }

            if (WalkTimer > 0)
                WalkTimer--;

            if (TurnTimer > 0)
                TurnTimer--;

            NPC.spriteDirection = NPC.direction;
        }
        
        public int CalculateReward()
        {
            // Make sure the held item type is valid
            if (currencies.TryGetValue(HeldItemType, out int value))
            {
                // If the held item has no roll value, immediately return
                if (value == 0)
                    return 0;

                // The reward's item type
                int currentItem = 0;
                // The rarity value of the reward
                float currentValue = 0;

                // Roll based on the held item's roll value
                for (int i = 0; i < value; i++)
                {
                    // Grab an item from the pool and its value
                    int newItem = rewardsRoll.Get();
                    float newValue = rewards[newItem];
                    // If the rarity of the reward is larger than the current reward, have it take priority
                    // This also applies if currentValue is at its default
                    if (newValue > currentValue || currentValue == 0)
                    {
                        currentValue = newValue;
                        currentItem = newItem;
                    }
                }
                return currentItem;
            }
            else
            {
                return 0;
            }
        }

        public static bool IsPassableTile(int x, int y)
        {
            return (!Main.tile[x, y].HasUnactuatedTile ||
                !Main.tileSolid[(int)Main.tile[x, y].TileType] || Main.tileSolidTop[(int)Main.tile[x, y].TileType]);
        }

        public void StepUp()
        {
            Vector2 position = NPC.position;
            position.X += NPC.velocity.X;
            int x = (int)((position.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1)) * NPC.direction) / 16f);
            int y = (int)((position.Y + (float)NPC.height - 1f) / 16f);

            if ((float)(x * 16) >= position.X + (float)NPC.width || (float)(x * 16 + 16) <= position.X)
                return;

            bool nextTileValid = Main.tile[x, y].HasUnactuatedTile && !Main.tile[x, y].TopSlope && !Main.tile[x, y - 1].TopSlope && Main.tileSolid[(int)Main.tile[x, y].TileType] && !Main.tileSolidTop[(int)Main.tile[x, y].TileType];
            bool aboveTileHalfBlock = Main.tile[x, y - 1].IsHalfBlock && Main.tile[x, y - 1].HasUnactuatedTile;
            bool aboveTileHasRoom = Main.tile[x, y - 1].IsHalfBlock && IsPassableTile(x, y - 4);
            bool aboveTileEmpty = !Main.tile[x, y - 1].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x, y - 1].TileType] || Main.tileSolidTop[(int)Main.tile[x, y - 1].TileType] || aboveTileHasRoom;
            bool tile3AbovePassable = !Main.tile[x - NPC.direction, y - 3].HasUnactuatedTile || !Main.tileSolid[(int)Main.tile[x - NPC.direction, y - 3].TileType];

            if ((nextTileValid || aboveTileHalfBlock) && aboveTileEmpty && IsPassableTile(x, y - 2) && IsPassableTile(x, y - 3) && tile3AbovePassable)
            {
                float npcBottom = (float)(y * 16);
                if (Main.tile[x, y].IsHalfBlock)
                {
                    npcBottom += 8f;
                }
                if (Main.tile[x, y - 1].IsHalfBlock)
                {
                    npcBottom -= 8f;
                }
                if (npcBottom < position.Y + (float)NPC.height)
                {
                    float percentageTileRisen = position.Y + (float)NPC.height - npcBottom;
                    if (percentageTileRisen <= 16.1f)
                    {
                        NPC.gfxOffY += NPC.position.Y + (float)NPC.height - npcBottom;
                        NPC.position.Y = npcBottom - (float)NPC.height;
                        if (percentageTileRisen < 9f)
                        {
                            NPC.stepSpeed = 1f;
                        }
                        else
                        {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            float frameCount = Main.npcFrameCount[Type] - 1;

            if (ShouldUseWalkingFrames)
                frameCount = 5;
            else if (ShouldUseInspectionFrames)
                frameCount = 23;
            else if (ShouldUseGivingFrames)
                frameCount = 12;

            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frame.Y++;
                NPC.frameCounter = 0;
            }

            // Reset frame when transitioning to giving animation
            if (NPC.frame.Y >= frameCount || (Phase == (int)PhaseType.Bartering && TradeTimer == 80))
            {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);

            return !spawnInfo.Player.Calamity().clamity && tile.WallType == WallID.CrimstoneUnsafe ? 0.05f : 0f;

            //fuck this
            //if (spawnInfo.Player.Calamity().ZoneSunkenSeaShores && !spawnInfo.Player.Calamity().clamity && tile.WallType == ModContent.WallType<RunestoneWall>())
            //{
            //    return 0.05f;
            //}
            //return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 10; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, Color.DarkGray * 0.2f, 1f);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            int frameCount = Main.npcFrameCount[Type];
            float extraPosOffset = 0;
            if (ShouldUseWalkingFrames)
            {
                texture = walkTexture.Value;
                frameCount = 6;
            }
            else if (ShouldUseInspectionFrames)
            {
                texture = inspectTexture.Value;
                frameCount = 24;
                extraPosOffset = 6;
            }
            else if (ShouldUseGivingFrames)
            {
                texture = giveTexture.Value;
                frameCount = 13;
            }
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / frameCount / 2);
            Vector2 npcOffset = NPC.Center - screenPos + Vector2.UnitY * extraPosOffset;
            npcOffset -= new Vector2(texture.Width, texture.Height / frameCount) * NPC.scale / 2f;
            npcOffset += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            spriteBatch.Draw(texture, npcOffset, texture.Frame(1, frameCount, 0, NPC.frame.Y), NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            // my dreams devoured
            // legacy code for visually holding an item
            /*Texture2D item = TextureAssets.Item[HeldItemType].Value;
            Vector2 itemOffset = new Vector2(NPC.direction == 1 ? 8 : -8, 20);
            if (TradeTimer < 80)
                spriteBatch.Draw(item, npcOffset + itemOffset, null, drawColor, 0, new Vector2(item.Width / 2, item.Height), 1f, spriteEffects, 0);
            */
            return false;
        }
    }
}
