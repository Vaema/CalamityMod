using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using System;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Leerslug : ModNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Rawr = 1,
            Jumps = 2,
            Dash = 3
        }

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];

        /// <summary>
        /// Handles the visual scale for the slug
        /// </summary>
        public Vector2 squish = new Vector2();
        public Player Target => Main.player[NPC.target];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 5;
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.width = 56;
            NPC.height = 30;
            NPC.defense = 3;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit50;
            NPC.DeathSound = SoundID.NPCDeath53;
            NPC.GravityIgnoresLiquid = true;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<LeerslugBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<BasaltGullyBiome>().Type };
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            squish = reader.ReadVector2();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(squish);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Leerslug")
            });
        }

        public override void AI()
        {
            NPC.Calamity().newAI[1]++;
            NPC.TargetClosest(false);
            Lighting.AddLight(NPC.Center, 0.5f, 0.2f, 0);
            if (NPC.direction == 0)
            {
                NPC.direction = Main.rand.NextBool().ToDirectionInt();
                squish = Vector2.One;
            }
            switch (CurrentPhase)
            {
                case (int)PhaseType.Idle:
                    {
                        // Flip direction and slow down if it's still moving from a previous cycle
                        if (Timer == 0 && Main.rand.NextBool())
                        {
                            NPC.direction *= -1;
                            NPC.velocity.X *= 0.9f;
                        }
                        // Move horizontally in short bursts
                        else if (Timer >= Main.rand.Next(30, 90) && NPC.velocity.Y == 0 && Math.Abs(NPC.velocity.X) < 1 && NPC.ai[2] == 0)
                        {
                            NPC.ai[2] = 1;
                            NPC.velocity.X = Main.rand.NextFloat(2, 4) * Main.rand.NextBool().ToDirectionInt();
                        }

                        // Increment the timer before the burst ends
                        if (NPC.ai[2] == 1)
                        {
                            NPC.ai[3]++;
                        }
                        // Slowdown after a burst
                        else if (NPC.ai[2] == 2 && NPC.velocity.Y == 0)
                        {
                            NPC.velocity.X *= 0.9f;
                            NPC.ai[3]++;
                        }

                        // Handle movement states
                        if (NPC.ai[3] > Main.rand.Next(20, 40))
                        {
                            // If the slug is moving, enter slowdown
                            if (NPC.ai[2] == 1)
                                NPC.ai[2] = 2;
                            // If the slug is in slowdown, reset
                            else if (NPC.ai[2] == 2)
                                ChangePhase((int)PhaseType.Idle);
                            NPC.ai[3] = 0;
                        }

                        NPC.direction = NPC.velocity.X.DirectionalSign();

                        // Aggro
                        if (Target.active && Target.Distance(NPC.Center) < 400 && NPC.HasSight(Target.Center))
                        {
                            ChangePhase((int)PhaseType.Rawr);
                        }                        
                    }
                    break;
                case (int)PhaseType.Rawr:
                    {
                        int roar = 40;
                        int startAI = 70;
                        NPC.velocity.X *= 0.9f;
                        // Roar in place with a lil jump
                        if (Timer == roar)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie7 with { Pitch = 0.7f }, NPC.Center);
                            if (NPC.velocity.Y == 0)
                            {
                                NPC.velocity.Y = -3;
                            }
                        }
                        NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                        if (Timer > startAI)
                            ChangePhase(Main.rand.NextBool() ? (int)PhaseType.Jumps : (int)PhaseType.Dash);
                    }
                    break;
                case (int)PhaseType.Jumps:
                    {
                        // Do a few hops towards the player
                        int maxJumps = 3;
                        int jumpHeight = Target.Bottom.Y < NPC.Top.Y ? 8 : 5;
                        if (NPC.ai[2] >= maxJumps - 1 && NPC.velocity.Y == 0)
                        {
                            ChangePhase((int)PhaseType.Dash);
                        }
                        // Jump
                        if (NPC.velocity.Y == 0)
                        {
                            NPC.velocity.Y = -jumpHeight;
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 4;
                            NPC.ai[2]++;
                        }
                        // Move towards the player if it fell in lava
                        else if (NPC.lavaWet)
                        {
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 4;
                            if (Main.rand.NextBool(20))
                            {
                                SoundEngine.PlaySound(SoundID.Zombie7 with { Pitch = 0.7f }, NPC.Center);
                            }
                        }
                        NPC.direction = NPC.velocity.X.DirectionalSign();
                    }
                    break;
                case (int)PhaseType.Dash:
                    {
                        // Squash and stretch to telegraph the dash
                        int squash = 30;
                        int stretch = 10;
                        Vector2 squashAmt = new Vector2(0.6f, 1.5f);
                        RawrAnimation(squash, stretch, squashAmt);
                        // Slow down before dashing
                        if (Timer < squash + 10)
                        {
                            NPC.velocity.X *= 0.95f;
                            NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                        }
                        // Dash
                        else if (Timer == squash + 10)
                        {
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 6;
                            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, NPC.Center);
                        }
                        // Leave behind some hot mist particles
                        else
                        {
                            if (NPC.velocity.X == 0)
                            {
                                ChangePhase((int)PhaseType.Jumps);
                                break;
                            }
                            if (Timer < squash + 60)
                            {
                                if (Timer % 2 == 0)
                                {
                                    MediumMistParticle particle = new MediumMistParticle(NPC.Center, -NPC.velocity, Color.Red, Color.Orange, Main.rand.NextFloat(0.8f, 1.2f), 255f, 0.2f);
                                    GeneralParticleHandler.SpawnParticle(particle);
                                }
                            }
                            NPC.direction = NPC.velocity.X.DirectionalSign();
                        }
                        if (Timer > 120 && NPC.velocity.Y == 0)
                            ChangePhase((int)PhaseType.Jumps);
                    }
                    break;
            }
            NPC.StepUpBlocks();
            NPC.spriteDirection = NPC.direction;
            // Bounce on lava
            if (NPC.lavaWet)
            {
                NPC.velocity.Y = MathHelper.Min(NPC.velocity.Y - 0.2f, -4);
            }
            Timer++;
        }

        public void RawrAnimation(int squashDuration, int stretchDuration, Vector2 squashAmt)
        {
            int squash = squashDuration;
            int stretch = squash + stretchDuration;
            // rawrxd
            if (Timer == squash)
            {
                SoundEngine.PlaySound(SoundID.Zombie7 with { Pitch = 0.7f }, NPC.Center);
            }
            // Squash
            if (Timer < squash)
            {
                squish.X = MathHelper.Lerp(1, squashAmt.X, Utils.GetLerpValue(0, squash, Timer, true));
                squish.Y = MathHelper.Lerp(1, squashAmt.Y, Utils.GetLerpValue(0, squash, Timer, true));
            }
            // Stretch
            else if (Timer >= squash && Timer < stretch)
            {
                squish.X = MathHelper.Lerp(squashAmt.X, 1, Utils.GetLerpValue(squash, stretch, Timer, true));
                squish.Y = MathHelper.Lerp(squashAmt.Y, 1, Utils.GetLerpValue(squash, stretch, Timer, true));
            }
        }

        public void ChangePhase(int phaseNum, bool resetai2 = true, bool resetai3 = true)
        {
            CurrentPhase = phaseNum;
            Timer = 0;
            if (resetai2)
                NPC.ai[2] = 0;
            if (resetai3)
                NPC.ai[3] = 0;
            NPC.netUpdate = true;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 1))
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LavaMoss, hit.HitDirection, -1f, 0, default, 1f);
            }
            CalamityUtils.SpawnGores(NPC, "Leerslug", 2);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 pos = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY;

            spriteBatch.Draw(tex, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale * squish, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);

            return false;
        }
    }
}
