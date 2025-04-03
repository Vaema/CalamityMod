using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Tiles.SunkenSea.Ambient;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using rail;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using CalamityMod.Graphics.Metaballs;
using ReLogic.Content;
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
            NPC.value = Item.buyPrice(0, 0, 0, 0);
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit50;
            NPC.DeathSound = SoundID.NPCDeath53;
            NPC.GravityIgnoresLiquid = true;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<SearslugBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
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
                        if (Timer == 0 && Main.rand.NextBool())
                        {
                            NPC.direction *= -1;
                            NPC.velocity.X *= 0.9f;
                        }
                        else if (Timer >= Main.rand.Next(30, 90) && NPC.velocity.Y == 0 && Math.Abs(NPC.velocity.X) < 1 && NPC.ai[2] == 0)
                        {
                            NPC.ai[2] = 1;
                            NPC.velocity.X = Main.rand.NextFloat(2, 4) * Main.rand.NextBool().ToDirectionInt();
                        }

                        if (NPC.ai[2] == 1)
                        {
                            NPC.ai[3]++;
                        }
                        else if (NPC.ai[2] == 2 && NPC.velocity.Y == 0)
                        {
                            NPC.velocity.X *= 0.9f;
                            NPC.ai[3]++;
                        }

                        if (NPC.ai[3] > Main.rand.Next(20, 40))
                        {
                            if (NPC.ai[2] == 1)
                                NPC.ai[2] = 2;
                            else if (NPC.ai[2] == 2)
                                ChangePhase((int)PhaseType.Idle);
                            NPC.ai[3] = 0;
                        }

                        NPC.direction = NPC.velocity.X.DirectionalSign();

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
                        int maxJumps = 3;
                        if (NPC.ai[2] >= maxJumps - 1 && NPC.velocity.Y == 0)
                        {
                            ChangePhase((int)PhaseType.Dash);
                        }
                        if (NPC.velocity.Y == 0)
                        {
                            NPC.velocity.Y = -5;
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 4;
                            NPC.ai[2]++;
                        }
                        NPC.direction = NPC.velocity.X.DirectionalSign();
                    }
                    break;
                case (int)PhaseType.Dash:
                    {
                        int squash = 30;
                        int stretch = 10;
                        Vector2 squashAmt = new Vector2(0.6f, 1.5f);
                        RawrAnimation(squash, stretch, squashAmt);
                        if (Timer < squash + 10)
                        {
                            NPC.velocity.X *= 0.95f;
                            NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                        }
                        else if (Timer == squash + 10)
                        {
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 6;
                            SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot, NPC.Center);
                        }
                        else
                        {
                            if (Timer % 2 == 0)
                            {
                                MediumMistParticleAlphaBlend particle = new MediumMistParticleAlphaBlend(NPC.Center, -NPC.velocity, Color.Red, Color.Orange, Main.rand.NextFloat(0.8f, 1.2f), 80f, 0.2f);
                                GeneralParticleHandler.SpawnParticle(particle);
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
            Timer++;
        }

        public void RawrAnimation(int squashDuration, int stretchDuration, Vector2 squashAmt)
        {
            int squash = squashDuration;
            int stretch = squash + stretchDuration;
            if (Timer == squash)
            {
                SoundEngine.PlaySound(SoundID.Zombie7 with { Pitch = 0.7f }, NPC.Center);
            }
            if (Timer < squash)
            {
                squish.X = MathHelper.Lerp(1, squashAmt.X, Utils.GetLerpValue(0, squash, Timer, true));
                squish.Y = MathHelper.Lerp(1, squashAmt.Y, Utils.GetLerpValue(0, squash, Timer, true));
            }
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
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 pos = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY;

            spriteBatch.Draw(tex, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale * squish, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);

            return false;
        }
    }
}
