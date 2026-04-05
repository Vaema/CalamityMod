using System;
using CalamityMod.Effects;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.NormalNPCs
{
    public class Piggy : ModNPC
    {
        public enum BehaviorState
        {
            IdleAndWalk,
            Running,
        }

        public Vector2 SquashVector;

        public static float MaxAcceleration_Walking => 0.035f;
        public static float MaxAcceleration_Running => 0.085f;
        public static float MaxSpeed_Walking => 1.2f;
        public static float MaxSpeed_Running => 3.6f;

        public ref float Timer => ref NPC.ai[0];

        public ref float AIState => ref NPC.ai[1];

        public ref float LocalAIState => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
            NPCID.Sets.NormalGoldCritterBestiaryPriority.Insert(NPCID.Sets.NormalGoldCritterBestiaryPriority.IndexOf(NPCID.GoldBunny) + 1, Type);
        }

        public override void SetDefaults()
        {
            NPC.damage = 0;
            NPC.width = 26;
            NPC.height = 26;
            NPC.lifeMax = 2000;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 1.15f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.catchItem = (short)ModContent.ItemType<PiggyItem>();
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Piggy")
            });
        }

        public override void AI()
        {
            // Force bestiary unlock
            //if (Main.netMode != NetmodeID.MultiplayerClient && Main.BestiaryTracker.Kills.GetKillCount(NPC) <= 0)
            //{
            //    Main.BestiaryTracker.Kills.RegisterKill(NPC);
            //}

            if (NPC.direction == 0)
                NPC.direction = Utils.SelectRandom(Main.rand, -1, 1);

            switch ((BehaviorState)AIState)
            {
                case BehaviorState.IdleAndWalk:
                    MainBehavior_IdleAndWalk();
                    break;

                case BehaviorState.Running:
                    MainBehavior_Running();
                    break;
            }
        
            NPC.StepUpBlocks();

            SquashVector = Vector2.Lerp(SquashVector, Vector2.One, 0.065f);
            Timer++;
        }

        private void MainBehavior_IdleAndWalk()
        {
            // Idling.
            if (LocalAIState == 0f)
            {
                if (Timer > 0f)
                {
                    if (Timer % 60f == 0f && Main.rand.NextBool(6))
                    {
                        Timer = 0f;
                        LocalAIState = 1f;
                        NPC.netUpdate = true;
                    }

                    if (Timer % 60f == 0f && Main.rand.NextBool(12))
                    {
                        AIState = (int)BehaviorState.Running;
                        Timer = 0f;
                        LocalAIState = 0f;
                        NPC.netUpdate = true;
                    }
                }

                // Stop moving and occasionally switch directions.
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X *= 0.8f;
                    if (Timer % 15f == 0f && Main.rand.NextBool(12))
                        NPC.direction *= -1;
                }
            }

            // Walking.
            if (LocalAIState == 1f)
            {
                if (Timer > 120f && Timer % 60f == 0f && Main.rand.NextBool(5))
                {
                    Timer = 0f;
                    LocalAIState = 0f;
                    NPC.netUpdate = true;
                }

                if (MathF.Abs(NPC.velocity.X) < MaxSpeed_Walking)
                    NPC.velocity.X += MaxAcceleration_Walking * NPC.direction;

                if (NPC.collideX && NPC.velocity.Y == 0f)
                    NPC.velocity.Y -= 6f;
            }

            NPC.spriteDirection = NPC.direction;
            float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.175f * (NPC.velocity.Y < 0).ToDirectionInt() : 0f;
            NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.075f);
        }

        private void MainBehavior_Running()
        {
            // Run in a random direction until collision with a wall is made.
            if (LocalAIState == 0f)
            {
                if (MathF.Abs(NPC.velocity.X) < MaxSpeed_Running)
                    NPC.velocity.X += MaxAcceleration_Running * NPC.direction;
                NPC.spriteDirection = NPC.direction;

                // Spawn particles when running at max speed.
                if (NPC.velocity.Y == 0f && MathF.Abs(NPC.velocity.X) >= MaxSpeed_Running)
                {
                    int dustType = NPC.type == ModContent.NPCType<PiggyGold>() ? DustID.Enchanted_Gold : DustID.Cloud;
                    Vector2 dustPosition = new(NPC.Bottom.X + Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f), NPC.Bottom.Y);
                    Dust.NewDustPerfect(dustPosition, dustType, new Vector2(NPC.velocity.X * 0.2f, Main.rand.NextFloat(-0.3f, 0.3f)), 0, default, Main.rand.NextFloat(1f, 1.2f));
                    if (Timer % 7 == 0f)
                        SoundEngine.PlaySound(SoundID.Run with { Pitch = 0.3f, Volume = 0.7f, Identifier = "Piggy Run" }, NPC.Center);
                }

                if (HoleBelow() && NPC.velocity.Y == 0f)
                    NPC.velocity.Y -= 6f;

                if (NPC.collideX)
                {
                    SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.7f }, NPC.Center);
                    SquashVector = new Vector2(0.6f, 1f);

                    NPC.velocity.X = NPC.oldVelocity.X * -0.86f;
                    NPC.velocity.Y -= 3f;
                    Timer = 0f;
                    LocalAIState = 1f;
                    NPC.netUpdate = true;
                }
                // Stop running and go back to idling if 5 seconds has passed without collision.
                else if (Timer >= 300f)
                {
                    AIState = (int)BehaviorState.IdleAndWalk;
                    LocalAIState = Main.rand.Next(1);
                    Timer = 0f;
                    NPC.netUpdate = true;
                }
            }

            if (LocalAIState == 1f)
            {
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.9f;

                if (Timer >= 120f)
                {
                    AIState = (int)BehaviorState.IdleAndWalk;
                    LocalAIState = Main.rand.Next(1);
                    Timer = 0f;
                    NPC.netUpdate = true;
                }
            }

            float targetAngle = (NPC.velocity.Y != 0f) ? NPC.velocity.X * 0.175f * (NPC.velocity.Y > 0).ToDirectionInt() : 0f;
            NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.075f);
        }

        private bool HoleBelow()
        {
            int npcWidthInTiles = NPC.width / 16;
            int tileX = (int)(NPC.Center.X / 16f) - npcWidthInTiles;
            if (NPC.velocity.X > 0)
                tileX += npcWidthInTiles;

            int tileY = (int)((NPC.position.Y + NPC.height) / 16f);
            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + npcWidthInTiles; x++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSulphur || spawnInfo.Player.Calamity().ZoneSunkenSea)
            {
                return 0f;
            }
            return SpawnCondition.TownCritter.Chance * 0.005f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemID.Bacon);

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            //if (NPC.velocity.Y == 0f)
            //{
            //    if (!NPC.IsABestiaryIconDummy)
            //    {
            //        if (NPC.velocity.X == 0f)
            //        {
            //            NPC.frame.Y = 0;
            //            NPC.frameCounter = 0.0;
            //            return;
            //        }
            //    }
            //    NPC.frameCounter += NPC.IsABestiaryIconDummy ? 0.6f : Math.Abs(NPC.velocity.X) * 0.25f;
            //    NPC.frameCounter += 1.0;
            //    if (NPC.frameCounter > 12.0)
            //    {
            //        NPC.frame.Y = NPC.frame.Y + frameHeight;
            //        NPC.frameCounter = 0.0;
            //    }
            //    if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[Type] - 1)
            //    {
            //        NPC.frame.Y = frameHeight;
            //    }
            //}
            //else
            //{
            //    NPC.frameCounter = 0.0;
            //    NPC.frame.Y = frameHeight * 2;
            //}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            Texture2D baseTexture = TextureAssets.Npc[Type].Value;
            Vector2 scale = SquashVector * NPC.scale;
            SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(baseTexture, NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, scale, spriteEffects, 0f);
            return false;
        }
    }
}
