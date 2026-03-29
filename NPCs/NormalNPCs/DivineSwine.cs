using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Effects;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Ammo;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Potions.Food;
using CalamityMod.Particles;
using CalamityMod.Systems.Graphic.PixelationSystem;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class DivineSwine : ModNPC
    {
        public enum BehaviorState
        {
            IdleAndFly,
            DivineMeatGrant,
        }

        private static Asset<Texture2D> BloomCircle;
        private static Asset<Texture2D> BloomFlare;
        private static Asset<Texture2D> ShineFlare;
        private static Asset<Texture2D> MagicStarCircle;
        private static Asset<Texture2D> FadedStarRing;
        private static Asset<Texture2D> DistortionTexture;

        private static SoundStyle DivineSwine_Idle = new("CalamityMod/Sounds/Custom/DivineSwine/DivineSwine_Idle", 4);
        private static SoundStyle DivineSwine_NearbyLoop = new("CalamityMod/Sounds/Custom/DivineSwine/DivineSwine_NearbyLoop")
        {
            IsLooped = true,
            PauseBehavior = PauseBehavior.PauseWithGame,
        };

        public bool ShouldTurnAway;

        public Vector2 IdleMovementVelocity;

        public Vector2 DivineMeatSpawnLocation;

        public Vector2 SquashVector;

        private SlotId SoundSlot;

        public static float MaxSpeed_Hovering => 0.2f;
        public static float MaxSpeed_Flying => 1.2f;
        public static float MaxAcceleration => 0.03f;

        public static Color DivineBlue => new(166, 238, 247);
        public static Color DivineYellow => new(247, 242, 166);

        public ref float Timer => ref NPC.ai[0];

        public ref float AIState => ref NPC.ai[1];

        public ref float LocalAIState => ref NPC.ai[2];

        public ref float IdleMovementTimer => ref NPC.ai[3];

        public override void Load()
        {
            On_Main.HoverOverNPCs += DivineSwineRightClickInteraction;
            if (!Main.dedServ)
            {
                BloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
                BloomFlare = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BloomFlare");
                ShineFlare = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ShineFlare");
                MagicStarCircle = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/MagicStarCircle");
                FadedStarRing = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/FadedStarRing");
                DistortionTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Smudges");
            }
        }

        public override void SetStaticDefaults()
        {
            //Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(NPCID.GoldBunny) + 3, Type);
        }

        public override void SetDefaults()
        {
            NPC.damage = 0;
            NPC.width = 26;
            NPC.height = 26;
            NPC.lifeMax = 999999;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0.1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //NPC.catchItem = (short)ModContent.ItemType<PiggyItem>();
            NPC.immortal = true;
            NPC.noGravity = true;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PiggyBanner>();
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToSickness = true;

            SquashVector = Vector2.One;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Piggy")
            });
        }

        public override void AI()
        {
            switch ((BehaviorState)AIState)
            {
                case BehaviorState.IdleAndFly:
                    MainBehavior_IdleAndFly();
                    break;

                case BehaviorState.DivineMeatGrant:
                    MainBehavior_DivineMeatGrant();
                    break;
            }

            SoundEffects();

            Lighting.AddLight(NPC.Center, DivineYellow.ToVector3() * NPC.scale * 0.825f);

            SquashVector = Vector2.Lerp(SquashVector, Vector2.One, 0.125f);
            ShouldTurnAway = NPC.ArcCollisionCheck(-0.3f, 0.3f, 0.05f, optionalCollisionCheckOverride: (arcBasePoint, arcPoint) =>
            {
                return !Collision.CanHitLine(arcBasePoint, 1, 1, arcPoint, 1, 1) || Collision.WetCollision(arcPoint, 1, 1);
            });

            NPC.spriteDirection = (NPC.velocity.X > 0).ToDirectionInt();
            NPC.rotation = NPC.velocity.X * 0.12f;
            Timer++;
        }

        public void MainBehavior_IdleAndFly()
        {
            // Hovering very slowly in random directions.
            if (LocalAIState == 0f)
            {
                if (Timer > 180f && Timer % 15f == 0f && Main.rand.NextBool(6))
                {
                    Timer = 0f;
                    LocalAIState = 1f;
                    NPC.netUpdate = true;
                }

                if (NPC.velocity.Length() > MaxSpeed_Hovering)
                    NPC.velocity *= 0.9f;

                if (ShouldTurnAway)
                {
                    HelperBehavior_AvoidTileCollision(MaxSpeed_Hovering + 0.6f);
                }
                else
                {
                    if (Timer % 75f == 0f && Main.rand.NextBool(2))
                        NPC.velocity = Main.rand.NextVector2Circular(MaxSpeed_Hovering, MaxSpeed_Hovering);
                }
            }

            if (LocalAIState == 1f)
            {
                if (ShouldTurnAway)
                {
                    HelperBehavior_AvoidTileCollision(MaxSpeed_Flying + 0.6f);
                    IdleMovementVelocity = Vector2.Zero;
                    IdleMovementTimer = 0f;
                }
                else
                {
                    if (IdleMovementTimer == 0f)
                    {
                        IdleMovementTimer = Main.rand.Next(100, 201);
                        IdleMovementVelocity = Main.rand.NextVector2Circular(MaxSpeed_Flying, MaxSpeed_Flying);
                        NPC.netUpdate = true;
                    }

                    NPC.velocity = Vector2.Lerp(NPC.velocity, IdleMovementVelocity, 0.075f);
                    IdleMovementTimer--;
                }
            }
        }

        public void MainBehavior_DivineMeatGrant()
        {
            if (Timer == 0f)
            {
                DivineMeatSpawnLocation = NPC.Center + new Vector2(0f, -78f);
                NPC.netUpdate = true;
            }

            // Spawn particles and do a lil animation and whateva
            if (Timer <= 180f)
            {
                float lightSpawnDistance = MathHelper.Lerp(32f, 72f, Timer / 180f);
                float glowRingScale = MathHelper.Lerp(0.3f, 0.9f, Timer / 180f);
                int glowRingSpawnInterval = (int)MathHelper.Lerp(30f, 15f, Timer / 180f);

                int lightAmt = Main.rand.Next(1, 2);
                for (int i = 0; i < lightAmt; i++)
                {
                    Vector2 lightSpawnPosition = DivineMeatSpawnLocation + Main.rand.NextVector2Unit() * lightSpawnDistance * Main.rand.NextFloat(0.6f, 1f);
                    Vector2 lightVelocity = (DivineMeatSpawnLocation - lightSpawnPosition).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2f, 3f);
                    float lightScale = Main.rand.NextFloat(0.6f, 0.8f) * MathHelper.Clamp(Timer / 180f, 0f, 1f);
                    Color lightColor = Color.Lerp(DivineBlue, DivineYellow, Main.rand.NextFloat());
                    SquishyLightParticle meatLight = new(lightSpawnPosition, lightVelocity, lightScale, lightColor, Main.rand.Next(30, 45));
                    GeneralParticleHandler.SpawnParticle(meatLight, true);
                }

                if (Timer % 15f == 0f)
                {
                    CustomPulse meatLightRing = new(DivineMeatSpawnLocation, Vector2.Zero, DivineBlue, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, glowRingScale, 0f, 25);
                    GeneralParticleHandler.SpawnParticle(meatLightRing, true);
                }
            }

            // Meat granted
            if (Timer == 180f)
            {
                SoundEngine.PlaySound(SoundID.Item29, DivineMeatSpawnLocation);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int meat = Item.NewItem(NPC.GetSource_GiftOrReward(), DivineMeatSpawnLocation, ModContent.ItemType<DeliciousMeat>());
                    Main.item[meat].velocity = Vector2.UnitY * -3f;
                }

                CustomPulse meatLightRing = new(DivineMeatSpawnLocation, Vector2.Zero, DivineBlue, "CalamityMod/Particles/BloomRing", Vector2.One, 0f, 0f, 1.5f, 60);
                GeneralParticleHandler.SpawnParticle(meatLightRing, true);
                for (int i = 0; i < 15; i++)
                {
                    Color lightColor = Color.Lerp(DivineBlue, DivineYellow, Main.rand.NextFloat());
                    SquishyLightParticle meatLight = new(DivineMeatSpawnLocation, Main.rand.NextVector2Circular(5f, 5f), Main.rand.NextFloat(0.8f, 1.2f), lightColor, Main.rand.Next(45, 60));
                    GeneralParticleHandler.SpawnParticle(meatLight, true);
                }
            }

            if (Timer >= 240f)
            {
                AIState = (int)BehaviorState.IdleAndFly;
                Timer = 0f;
                LocalAIState = Utils.SelectRandom(Main.rand, -1, 1);
                NPC.velocity = Main.rand.NextVector2Circular(MaxSpeed_Hovering, MaxSpeed_Hovering);
                NPC.netUpdate = true;
            }

            NPC.velocity *= 0.9f;
        }

        public void HelperBehavior_AvoidTileCollision(float maxSpeed, float turnAwayStrength = 0.125f)
        {
            float distanceToCollisionLeft = CalamityUtils.DistanceToTileCollisionHit(NPC.Center, NPC.velocity.RotatedBy(MathHelper.PiOver2), 32, ShouldAvoidTile) ?? 10000f;
            float distanceToCollisionRight = CalamityUtils.DistanceToTileCollisionHit(NPC.Center, NPC.velocity.RotatedBy(-MathHelper.PiOver2), 32, ShouldAvoidTile) ?? 10000f;
            int directionToMove = (distanceToCollisionLeft > distanceToCollisionRight).ToDirectionInt();

            Vector2 idealVelocity = NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * directionToMove) * (maxSpeed - NPC.velocity.Length());
            NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, turnAwayStrength);
        }

        public void SoundEffects()
        {
            if (NPC.soundDelay == 0 && Main.rand.NextBool(100))
            {
                SoundEngine.PlaySound(DivineSwine_Idle, NPC.Center);
                SquashVector = new Vector2(1.2f, 0.8f);
                NPC.soundDelay = Main.rand.Next(60, 120);
            }

            if (!SoundEngine.TryGetActiveSound(SoundSlot, out _))
                SoundSlot = SoundEngine.PlaySound(DivineSwine_NearbyLoop, NPC.Center, SoundCallbackMethod);

            // Fade the music depending on the distance between the player and Divine Swine.
            float musicVolumeInterpolant = Utils.Remap(NPC.Distance(Main.LocalPlayer.Center), 600f, 100f, 1f, 0.2f, true);
            Main.musicFade[Main.curMusic] = musicVolumeInterpolant;
        }

        private bool SoundCallbackMethod(ActiveSound soundInstance)
        {
            soundInstance.Position = NPC.Center;

            float idealPitch = 0f;
            if (AIState == (int)BehaviorState.DivineMeatGrant && Timer <= 180f)
                idealPitch = 0.4f;

            float volumeInterpolant = Utils.Remap(NPC.Distance(Main.LocalPlayer.Center), 600f, 100f, 0.1f, 0.7f, true);
            soundInstance.Volume = volumeInterpolant;
            soundInstance.Pitch = MathHelper.Lerp(soundInstance.Pitch, idealPitch, 0.075f);
            return NPC.active;
        }

        public void SwitchToDivineGrant()
        {
            AIState = (int)BehaviorState.DivineMeatGrant;
            Timer = 0f;
            LocalAIState = 0f;
            NPC.netUpdate = true;
            NetMessage.SendData(MessageID.WorldData);
        }

        private bool ShouldAvoidTile(Tile tile) => WorldGen.SolidTile(tile) || (tile.HasUnactuatedTile && Main.tileSolidTop[tile.TileType]) || tile.LiquidAmount >= 255;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                DrawBestiaryPortrait(spriteBatch);
                return false;
            }

            Texture2D baseTexture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY;
            SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            var device = Main.graphics.GraphicsDevice;
            using var sunSigilLease = RenderTargetPool.Shared.Rent(device, (int)(Main.screenWidth * 0.5f), (int)(Main.screenHeight * 0.5f), RenderTargetDescriptor.Default);
            //using var outerRingLease = RenderTargetPool.Shared.Rent(device, (int)(Main.screenWidth * 0.5f), (int)(Main.screenHeight * 0.5f), RenderTargetDescriptor.Default);

            spriteBatch.End(out var snapshot);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Buncha yellow-colored bloom stuff to look like a glowing sun.
            using (sunSigilLease.Scope(clearColor: Color.Black))
            {
                float bloomFlareScale = MathHelper.Lerp(0.4f, 0.7f, MathF.Sin((float)Main.timeForVisualEffects / 60f) * 0.5f + 0.5f);
                spriteBatch.Draw(BloomFlare.Value, drawPosition, null, NPC.GetAlpha(DivineYellow) * 0.8f, MathHelper.PiOver4, BloomFlare.Size() * 0.5f, bloomFlareScale, 0, 0f);

                spriteBatch.Draw(ShineFlare.Value, drawPosition, null, NPC.GetAlpha(DivineYellow), 0f, MagicStarCircle.Size() * 0.5f, bloomFlareScale + 0.25f, 0, 0f);
                spriteBatch.Draw(MagicStarCircle.Value, drawPosition, null, NPC.GetAlpha(DivineYellow) * 0.8f, 0f, MagicStarCircle.Size() * 0.5f, 0.5f, 0, 0f);
            }

            spriteBatch.End();

            // Apply a chromatic abberation effect to the largest faded ring.
            //Effect chromaAbberShader = CalamityShaders.ChromaticAbberationShader.Value;
            //chromaAbberShader.Parameters["abberationStrength"].SetValue(10f);
            //chromaAbberShader.Parameters["impactPosition"].SetValue(drawPosition);

            //spriteBatch.End();
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, chromaAbberShader, Main.GameViewMatrix.TransformationMatrix);

            //using (outerRingLease.Scope(clearColor: Color.Transparent))
            //{
            //    float starRingScale = MathHelper.Lerp(0.7f, 1f, MathF.Sin((float)Main.timeForVisualEffects / 180f) * 0.5f + 0.5f);
            //    spriteBatch.Draw(FadedStarRing.Value, drawPosition, null, NPC.GetAlpha(DivineBlue) * 0.7f, (float)(Main.timeForVisualEffects / 720f) + NPC.whoAmI, FadedStarRing.Size() * 0.5f, starRingScale, 0, 0f);
            //}


            //Effect distortionShader = CalamityShaders.BasicTextureDistortionShader.Value;
            //distortionShader.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly);
            //distortionShader.Parameters["noiseScale"].SetValue(0.3f);
            //distortionShader.Parameters["distortionStrength"].SetValue(0f);
            //distortionShader.Parameters["timeOffset"].SetValue(new Vector2(-0.02f, 0.01f));

            //device.Textures[1] = DistortionTexture.Value;
            //device.SamplerStates[1] = SamplerState.LinearWrap;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(sunSigilLease.Target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            // Subtractive backing images.
            spriteBatch.SetBlendState(CalamityUtils.SubtractiveBlending);

            int backglowCount = 3;
            for (int i = 0; i < backglowCount; i++)
            {
                float rotation = (float)(Main.timeForVisualEffects / MathHelper.Pi * 0.08f) + NPC.whoAmI;
                Vector2 backglowDrawPosition = drawPosition + Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / backglowCount + rotation) * 6f;
                spriteBatch.Draw(baseTexture, backglowDrawPosition, NPC.frame, NPC.GetAlpha(Color.White) * 0.9f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
            }

            int backglowCount2 = 6;
            for (int i = 0; i < backglowCount2; i++)
            {
                float rotation = (float)(Main.timeForVisualEffects / MathHelper.Pi * 0.06f) + NPC.whoAmI;
                Vector2 backglowDrawPosition = drawPosition + Vector2.UnitX.RotatedBy(i * MathHelper.TwoPi / backglowCount2 + rotation) * 12f;
                spriteBatch.Draw(baseTexture, backglowDrawPosition, NPC.frame, NPC.GetAlpha(Color.White) * 0.7f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, spriteEffects, 0f);
            }

            // Actual Divine Swine.
            spriteBatch.SetBlendState(BlendState.AlphaBlend);
            spriteBatch.Draw(baseTexture, drawPosition, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() * 0.5f, SquashVector * NPC.scale, spriteEffects, 0f);

            spriteBatch.End();
            spriteBatch.Begin(snapshot);

            return false;
        }

        public void DrawBestiaryPortrait(SpriteBatch spriteBatch)
        {
            spriteBatch.End(out var snapshot);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Texture2D baseTexture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center + Vector2.UnitY * MathHelper.Lerp(-6f, 6f, MathF.Sin((float)Main.timeForVisualEffects / 60f) * 0.5f + 0.5f);
            Color backglowColor = new(166, 238, 247);
            Color bloomCircleColor = new(247, 242, 166);
            spriteBatch.Draw(BloomCircle.Value, drawPosition, null, NPC.GetAlpha(backglowColor) * 0.8f, 0f, BloomCircle.Size() * 0.5f, 0.8f, 0, 0f);
            spriteBatch.Draw(BloomCircle.Value, drawPosition, null, NPC.GetAlpha(bloomCircleColor) * 0.75f, 0f, BloomCircle.Size() * 0.5f, 1.6f, 0, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, CalamityUtils.SubtractiveBlending, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            spriteBatch.Draw(BloomCircle.Value, drawPosition, null, NPC.GetAlpha(Color.White) * 0.2f, 0f, BloomCircle.Size() * 0.5f, 0.6f, 0, 0f);

            int backglowCount = 4;
            for (int i = 0; i < backglowCount; i++)
            {
                float rotation = (float)(Main.timeForVisualEffects / MathHelper.Pi * 0.08f) + NPC.whoAmI;
                Vector2 backglowDrawPosition = drawPosition + Vector2.UnitX.RotatedBy((i * MathHelper.TwoPi / backglowCount) + rotation) * 8f;
                spriteBatch.Draw(baseTexture, backglowDrawPosition, NPC.frame, NPC.GetAlpha(Color.White) * 0.6f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, 0, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            spriteBatch.Draw(baseTexture, drawPosition, NPC.frame, NPC.GetAlpha(Color.White), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, 0, 0f);

            spriteBatch.End();
            spriteBatch.Begin(snapshot);
        }

        private static void DivineSwineRightClickInteraction(On_Main.orig_HoverOverNPCs orig, Main self, Rectangle mouseRectangle)
        {
            orig(self, mouseRectangle);

            Player player = Main.LocalPlayer;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type != ModContent.NPCType<DivineSwine>())
                    continue;

                if ((!npc.ShowNameOnHover || !(npc.active & (npc.shimmerTransparency == 0f || npc.CanApplyHunterPotionEffects()))))
                    continue;

                Main.instance.LoadNPC(npc.whoAmI);
                npc.position += npc.netOffset;

                Rectangle npcRectangle = new((int)(npc.Bottom.X - npc.frame.Width * 0.5f), (int)(npc.Bottom.Y - npc.frame.Height), npc.frame.Width, npc.frame.Height);
                NPCLoader.ModifyHoverBoundingBox(npc, ref npcRectangle);

                bool hoveringOverHitbox = mouseRectangle.Intersects(npcRectangle);
                bool canBeInteractedWith = hoveringOverHitbox || (Main.SmartInteractShowingGenuine && Main.SmartInteractNPC == npc.whoAmI);
                if (canBeInteractedWith && npc.ai[1] != 1)
                {
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = ItemID.PlatinumCoin;
                    player.cursorItemIconText = "";
                    player.noThrow = 2;

                    PlayerInput.SetZoom_MouseInWorld();
                    if (Main.mouseRight && Main.npcChatRelease)
                    {
                        Main.npcChatRelease = false;
                        if (PlayerInput.UsingGamepad)
                            player.releaseInventory = false;

                        if (player.talkNPC != npc.whoAmI && !player.tileInteractionHappened && TryOfferingPlatinumToSwine())
                        {
                            npc.ModNPC<DivineSwine>().SwitchToDivineGrant();
                            SoundEngine.PlaySound(SoundID.Coins);
                        }
                    }
                }
            }
        }

        private static bool TryOfferingPlatinumToSwine()
        {
            Player player = Main.LocalPlayer;
            bool inPiggyBank = false;

            int inventoryIndexSlot = player.FindItem(ItemID.PlatinumCoin);
            int piggyBankIndexSlot = player.FindItem(ItemID.PlatinumCoin, player.bank.item);
            if (piggyBankIndexSlot != -1)
                inPiggyBank = true;

            if (inventoryIndexSlot == -1 && piggyBankIndexSlot == -1)
                return false;

            Item foundItem = inPiggyBank ? player.bank.item[piggyBankIndexSlot] : player.inventory[inventoryIndexSlot];
            if (--foundItem.stack <= 0)
                foundItem.TurnToAir();

            Recipe.FindRecipes();
            return true;
        }
    }
}
