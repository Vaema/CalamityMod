using System;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Projectiles.Melee;
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
namespace CalamityMod.NPCs.SunkenSea
{
    public class Clam : ModNPC
    {
        public enum PersonalityTypes
        {
            Reefs = 0,
            Burrows = 1,
            Den = 2
        }

        public enum PhaseType
        {
            Idle = 0,
            Attacking = 1,
            Squirt = 2,
            Pod = 3
        }

        public Player Target => Main.player[NPC.target];

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];

        public ref float Personality => ref NPC.ai[3];

        public ref float ShellRotation => ref NPC.localAI[0];

        public int originalDamage;

        #region Textures

        public static Asset<Texture2D> bottomJawTex;

        public static Asset<Texture2D> bottomJawTexAlgae;

        public static Asset<Texture2D> bottomJawTexCoral;

        public static Asset<Texture2D> algaeTex;

        public static Asset<Texture2D> coralTex;

        public static Asset<Texture2D> backTex;
        #endregion

        public override void Load()
        {
            backTex = ModContent.Request<Texture2D>(Texture + "Back");
            bottomJawTex = ModContent.Request<Texture2D>(Texture + "Bottom");
            bottomJawTexAlgae = ModContent.Request<Texture2D>(Texture + "BottomAlgae");
            bottomJawTexCoral = ModContent.Request<Texture2D>(Texture + "BottomCoral");
            algaeTex = ModContent.Request<Texture2D>(Texture + "Algae");
            coralTex = ModContent.Request<Texture2D>(Texture + "Coral");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
        }

        public override void SetDefaults()
        {
            originalDamage = NPC.damage = Main.hardMode ? 60 : 30;
            NPC.width = 50;
            NPC.height = 30;
            NPC.defense = 9999;
            NPC.lifeMax = Main.hardMode ? 300 : 150;
            if (Main.expertMode)
            {
                NPC.lifeMax *= 2;
            }
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Main.hardMode ? Item.buyPrice(silver: 5) : Item.buyPrice(silver: 1);
            NPC.HitSound = SoundID.NPCHit4;
            NPC.knockBackResist = 0;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ClamBanner>();
            NPC.GravityIgnoresLiquid = true;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[3] { ModContent.GetInstance<RadiantReefsBiome>().Type, ModContent.GetInstance<GleamingBurrowsBiome>().Type, ModContent.GetInstance<ClamDenBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Clam")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest();

            NPC.direction = Main.rand.NextBool().ToDirectionInt();
            NPC.localAI[1] = Main.rand.Next(0, 4);

            if (Target.Calamity().ZoneClamDen)
                Personality = (int)PersonalityTypes.Den;
            else if (Target.Calamity().ZoneGleamingBurrows)
                Personality = (int)PersonalityTypes.Burrows;
            else
                Personality = (int)PersonalityTypes.Reefs;
        }

        public override void AI()
        {
            // Rotation at its peak
            float maxRotation = MathHelper.ToRadians(60);
            NPC.TargetClosest(false);
            // Always be mad during Clamity
            if (Main.player[NPC.target].Calamity().clamity)
            {
                Personality = (int)PersonalityTypes.Burrows;
            }
            switch (CurrentPhase)
            {
                case (int)PhaseType.Idle:
                    {
                        NPC.damage = 0;
                        NPC.chaseable = false;
                        NPC.velocity.X *= 0.9f;
                        if (ShellRotation > 0)
                        {
                            ShellRotation -= 0.2f;
                            if (ShellRotation < 0)
                                ShellRotation = 0;
                        }
                        else if (ShellRotation < 0)
                        {
                            ShellRotation += 0.03f;
                            if (ShellRotation > 0)
                            {
                                ShellRotation = 0;
                            }
                        }
                        switch (Personality)
                        {
                            // Aggro immediately
                            case (int)PersonalityTypes.Den:
                                {
                                    ChangePhase((int)PhaseType.Attacking);
                                }
                                break;
                            // Aggro if the player is near
                            case (int)PersonalityTypes.Burrows:
                                {
                                    if (Target.Distance(NPC.Center) < 600 && NPC.HasSight(Target.Center))
                                    {
                                        ChangePhase((int)PhaseType.Attacking);
                                    }
                                }
                                break;
                            // Aggro if hurt
                            default:
                                {
                                    if (NPC.life < (NPC.lifeMax - 2))
                                    {
                                        ChangePhase((int)PhaseType.Attacking);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case (int)PhaseType.Attacking:
                    {
                        NPC.damage = 0;
                        NPC.knockBackResist = 0.05f;
                        NPC.defense = Main.hardMode ? 15 : 6;
                        NPC.chaseable = true;
                        Timer++;
                        if (NPC.velocity.Y == 0)
                        {
                            NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                            NPC.velocity.X = 0;
                            NPC.ai[2]++;
                            if (NPC.ai[2] > Main.rand.Next(30, 60))
                            {
                                NPC.velocity.Y = -4;
                                NPC.velocity.X = NPC.direction * 6;
                                NPC.ai[2] = 0;
                            }
                        }
                        else
                        {
                            NPC.damage = originalDamage;

                            if (NPC.velocity.Y < 0)
                            {
                                ShellRotation += 0.065f;
                                if (ShellRotation > maxRotation)
                                    ShellRotation = maxRotation;
                            }
                            NPC.velocity.X *= 0.99f;
                        }

                        if (NPC.position.Distance(NPC.oldPosition) < 8 && NPC.velocity.Y >= 0)
                        {
                            ShellRotation -= 0.065f;
                            if (ShellRotation < 0)
                                ShellRotation = 0;
                        }

                        // Squirt
                        if (Timer > Main.rand.Next(220, 260) && NPC.HasSight(Target.Center) && ShellRotation == 0)
                        {
                            ChangePhase((int)PhaseType.Squirt);
                            NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                        }
                        NPC.StepUpBlocks();
                    }
                    break;
                case (int)PhaseType.Squirt:
                    {
                        NPC.damage = 0;
                        NPC.chaseable = true;
                        // Slow down. Once the clam is rested, start incrementing Timer
                        if (NPC.velocity.Y == 0)
                        {
                            NPC.velocity.X *= 0.8f;
                            // Set ai[2] to 1
                            NPC.ai[2] = 1;
                        }

                        // Increment Timer is ai[2] is 1
                        if (NPC.ai[2] == 1)
                        {
                            Timer++;
                        }

                        // When to start opening
                        float startOpen = 30;
                        // When to end opening
                        float endOpen = startOpen + 30;
                        // When to start closing
                        float startClose = endOpen + 20;
                        // When to stop closing
                        float endClose = startClose + 5;
                        // When to go to the next attack
                        float reset = endClose + 60;

                        // Direction the player is relative to the clam
                        int playerPosition = NPC.DirectionTo(Target.Center).X.DirectionalSign();

                        // Shell animation
                        if (Timer >= startOpen && Timer <= endOpen)
                        {
                            ShellRotation = (float)Utils.AngleLerp(0, maxRotation, CalamityUtils.SineOutEasing(Utils.GetLerpValue(startOpen, endOpen, Timer, true), 0));
                        }
                        else if (Timer >= startClose && Timer <= endClose)
                        {
                            ShellRotation = (float)Utils.AngleLerp(maxRotation, 0, CalamityUtils.SineOutEasing(Utils.GetLerpValue(startClose, endClose, Timer, true), 0));
                        }

                        // Fire the projectile
                        if (Timer == (endClose - 5))
                        {
                            NPC.damage = originalDamage;
                            Vector2 velocity = NPC.SafeDirectionTo(Target.Center, Vector2.UnitY) * 5;

                            // If the player is on the other side of the clam, flip the jet so that it doesn't fire backwards
                            if (playerPosition != NPC.direction)
                                velocity.X *= -1;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<ClamBubbleBlast>(), Main.hardMode ? 30 : 15, 1);
                            }
                            for (int i = 0; i < 9; i++)
                            {
                                GenericBubbleParticle waterFlavored = new GenericBubbleParticle(NPC.Center, Main.rand.NextFloat(16, 22) * velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-45, 45))), Main.rand.NextFloat(0.3f, 0.5f), Main.rand.NextFloat(-4, 4), 5);
                                waterFlavored.AffectedByLight = true;
                                GeneralParticleHandler.SpawnParticle(waterFlavored);
                            }

                            SoundEngine.PlaySound((Main.rand.NextBool(2) ? SoundID.Item85 : SoundID.Item86).WithPitchOffset(-0.5f), NPC.Center);
                            SoundEngine.PlaySound(SoundID.NPCDeath14.WithPitchOffset(1f), NPC.Center);
                        }

                        // Go back to melee
                        if (Timer >= reset)
                        {
                            ChangePhase((int)PhaseType.Attacking);
                        }
                    }
                break;
                case (int)PhaseType.Pod:
                    {
                        NPC.damage = 0;

                        NPC.chaseable = true;
                        NPC pod = Main.npc[(int)NPC.localAI[2] - 1];
                        // If the Pearlpod is invalid, go back to idling
                        if (pod == null || !pod.active || pod.life < 0 || pod.ModNPC == null || pod.ModNPC is not Pearlpod)
                        {
                            NPC.localAI[2] = 0;
                            ChangePhase((int)PhaseType.Idle);
                        }
                        else
                        {
                            // Face towards the Pearlpod while it's still out
                            if (pod.Opacity == 1)
                                NPC.direction = NPC.DirectionTo(pod.Center).X.DirectionalSign();
                            // Start closing when the Pearlpod is nearby
                            if (pod.Distance(NPC.Center) > 30)
                            {
                                ShellRotation += 0.05f;
                                if (ShellRotation > maxRotation)
                                    ShellRotation = maxRotation;
                            }
                            // Otherwise open up!
                            else
                            {
                                ShellRotation -= 0.2f;
                                if (ShellRotation < -0.2f)
                                    ShellRotation = -0.2f;
                            }
                        }
                    }
                break;
            }
            NPC.spriteDirection = NPC.direction;
        }

        public void ChangePhase(int phaseNum, bool resetai2 = true)
        {
            CurrentPhase = phaseNum;
            Timer = 0;
            if (resetai2)
                NPC.ai[2] = 0;
            NPC.netUpdate = true;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return CurrentPhase > 0;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return CurrentPhase > 0;
            }
            return null;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > 4.0)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y = NPC.frame.Y + frameHeight;
            }
            if (CurrentPhase == 0)
            {
                NPC.frame.Y = frameHeight * 4;
            }
            else
            {
                if (NPC.frame.Y > frameHeight * 3)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water)
            {
                if (spawnInfo.Player.Calamity().ZoneRadiantReefs)
                    return SpawnCondition.CaveJellyfish.Chance * 0.8f;

                if (spawnInfo.Player.Calamity().ZoneGleamingBurrows)
                    return SpawnCondition.CaveJellyfish.Chance * 1f;

                if (spawnInfo.Player.Calamity().ZoneClamDen)
                    return SpawnCondition.CaveJellyfish.Chance * 1.2f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Obsidian, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Obsidian, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Clam1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Clam2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Clam3").Type, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<Navystone>(), 1, 8, 12);
            npcLoot.Add(ItemID.WhitePearl, 8);
            npcLoot.Add(ItemID.BlackPearl, 16);
            npcLoot.Add(ItemID.PinkPearl, 40);
            npcLoot.AddIf(() => Main.hardMode, ModContent.ItemType<MolluskHusk>(), 2);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Texture2D jaw = bottomJawTex.Value;

            switch (NPC.localAI[1])
            {
                case 1:
                    {
                        tex = algaeTex.Value;
                        jaw = bottomJawTexAlgae.Value;
                    }
                    break;
                case 2:
                    {
                        tex = coralTex.Value;
                        jaw = bottomJawTexCoral.Value;
                    }
                    break;
            }

            Vector2 drawOffset = Vector2.UnitY * 8;
            Vector2 topDrawOffset = drawOffset + new Vector2(NPC.spriteDirection * -22, -3);
            bool facingRight = NPC.spriteDirection == 1;
            if (facingRight)
            {
                topDrawOffset.X += 6;
            }
            Vector2 backOffset = topDrawOffset;
            float trueShellRotation = ShellRotation * -NPC.spriteDirection;
            if (ShellRotation == 0)
                spriteBatch.Draw(backTex.Value, NPC.Center - screenPos + backOffset, null, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(facingRight ? 0 : tex.Width, tex.Height), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(tex, NPC.Center - screenPos + topDrawOffset, null, NPC.GetAlpha(drawColor), trueShellRotation, new Vector2(facingRight ? 0 : tex.Width, tex.Height), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(jaw, NPC.Center - screenPos + drawOffset, null, NPC.GetAlpha(drawColor), NPC.rotation, tex.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
    }
}
