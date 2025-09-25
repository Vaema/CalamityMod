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
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using CalamityMod.Enums;
using System.Collections.Generic;
using System;
using CalamityMod.Particles;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CalamityMod.Items.Weapons.Ranged;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Steampod : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Hunting = 1,
            LaunchUpSide = 2
        }

        public static Asset<Texture2D> glowTex;

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];

        public ref float Jetting => ref NPC.ai[2];

        public ref float WalkTimer => ref NPC.Calamity().newAI[0];

        public ref float TurnTimer => ref NPC.Calamity().newAI[1];

        public ref float WalkOrStand => ref NPC.Calamity().newAI[2];

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.BasaltGully;

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<PodobooKoi>()
        };

        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<Searslug>()
        };

        public override void Load()
        {
            glowTex = ModContent.Request<Texture2D>(Texture + "Glow");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 17;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.aiStyle = -1;
            NPC.damage = 40;
            NPC.width = 44;
            NPC.height = 44;
            NPC.defense = 10;
            NPC.lifeMax = 200;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 20);
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.chaseable = false;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<SteampodBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToWater = true;
            NPC.Calamity().VulnerableToCold = true;
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 3; i++)
                NPC.Calamity().newAI[i] = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
                writer.Write(NPC.Calamity().newAI[i]);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Steampod")
            });
        }

        public override void AI()
        {
            switch (CurrentPhase)
            {
                case (int)PhaseType.Idle:
                    {
                        // Initialize its direction
                        if (NPC.direction == 0)
                            NPC.direction = Main.rand.NextBool() ? -1 : 1;
                        // Decide if it should walk or sit
                        float movementSpeed = CurrentPredator != null ? 2 : 1;
                        bool startMovement = false;
                        if (WalkTimer <= 0)
                        {
                            WalkTimer = Main.rand.Next(120, 270);
                            WalkOrStand = WalkOrStand <= 0 ? 1 : -1;
                            if (WalkOrStand == 1 && Main.rand.NextBool() && CurrentPrey == null)
                            {
                                NPC.direction *= -1;
                            }
                            startMovement = true;
                        }
                        bool lookingAtPredator = CurrentPredator != null && NPC.direction == Math.Sign(CurrentPredator.Center.X - NPC.Center.X);
                        bool pitCheck = FallCheck(16);
                        // If it bumps into something, jump or turn around
                        if ((!startMovement && TurnTimer <= 0 && (NPC.velocity.X == 0 || pitCheck) && WalkOrStand == 1) || lookingAtPredator)
                        {
                            // Jump if there are a couple tiles and a space above
                            if (!pitCheck && NPC.velocity.Y == 0 && !lookingAtPredator)
                            {
                                // Chance to do a jet
                                bool jet = Main.rand.NextBool(4) && Jetting == 0 && !NPC.lavaWet;
                                if (jet)
                                {
                                    if (JumpCheck(16))
                                    {
                                        TurnTimer = 0;
                                        WalkTimer = 0;
                                        WalkOrStand = 0;
                                        Timer = 0;
                                        CurrentPhase = (int)PhaseType.LaunchUpSide;
                                    }
                                }
                                else if (JumpCheck(6))
                                {
                                    NPC.velocity.Y -= 7;
                                    NPC.velocity.X = movementSpeed * NPC.direction;
                                    WalkOrStand = 1;
                                    TurnTimer = 10;
                                }
                            }
                            // Jet upwards if a pit is in the way
                            else if (Jetting == 0 && !NPC.lavaWet)
                            {
                                TurnTimer = 0;
                                WalkTimer = 0;
                                WalkOrStand = 0;
                                Timer = 0;
                                CurrentPhase = (int)PhaseType.LaunchUpSide;
                            }
                        }
                        // Attack 
                        Entity target = CurrentPrey != null ? CurrentPrey : CurrentPlayer;
                        if (target != null)
                        {
                            if (NPC.HasSight(target.Center) && Jetting == 0)
                            {
                                TurnTimer = 0;
                                WalkTimer = 0;
                                WalkOrStand = 0;
                                Timer = 0;
                                CurrentPhase = (int)PhaseType.Hunting;
                            }
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
                            if (CurrentPredator != null)
                            {
                                WalkOrStand = 1;
                                WalkTimer = Main.rand.Next(180, 340);
                            }
                        }
                        // Bounce on lava
                        if (NPC.lavaWet)
                        {
                            NPC.velocity.Y = MathHelper.Min(NPC.velocity.Y - 0.2f, -4);
                        }
                        CalamityUtils.StepUpBlocks(NPC);
                    }
                    break;
                case (int)PhaseType.LaunchUpSide:
                    {
                        int jumpUp = 60; // When to jet up
                        int jetSide = jumpUp + 40; // When to jet sideways
                        int jetRate = NPC.lavaWet ? 1 : 2; // How often particles spawn

                        // Reset if on ground or hit ceiling
                        if (NPC.velocity.Y == 0 && Jetting > 0 && Timer > 5)
                        {
                            NPC.velocity.Y = 0;
                            Timer = 0;
                            Jetting = -120;
                            CurrentPhase = (int)PhaseType.Idle;
                            NPC.rotation = 0;
                            break;
                        }

                        // Increment timer while on the ground
                        if (NPC.velocity.Y == 0 && Jetting == 0)
                        {
                            NPC.velocity.X *= 0.95f;
                            Timer++;
                        }

                        // Jump
                        if (Timer == jumpUp && Jetting == 0)
                        {
                            Jetting = 1;
                            NPC.velocity.Y = -12 * (NPC.lavaWet ? 1.5f : 1);
                            SoundEngine.PlaySound(DragonsBreath.FireballSound with { Pitch = 0.4f }, NPC.Center);
                        }
                        // Release particles
                        else if (Jetting == 1)
                        {
                            Timer++;
                            if (Timer > jumpUp)
                            {
                                NPC.velocity.Y += 0.01f;
                                if (Timer % jetRate == 0)
                                {
                                    SeaFoamParticle p = new SeaFoamParticle(NPC.Center, -NPC.velocity, Color.LightCyan, Color.Cyan, Main.rand.NextFloat(0.4f, 0.8f), 180f, 0.05f);
                                    GeneralParticleHandler.SpawnParticle(p);
                                }
                            }
                            if (Timer == jetSide)
                            {
                                Timer = 0;
                                Jetting = 2;
                            }
                        }
                        // Jet sideways
                        else if (Jetting == 2)
                        {
                            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                            Timer++;
                            if (Timer == 1)
                            {
                                SoundEngine.PlaySound(DragonsBreath.FireballSound with { Pitch = 0.4f }, NPC.Center);
                                NPC.velocity.X = NPC.direction * 16;
                                NPC.velocity.Y = -2;
                            }
                            else if (Timer % jetRate == 0)
                            {
                                SeaFoamParticle p = new SeaFoamParticle(NPC.Center, -NPC.velocity, Color.LightCyan, Color.Cyan, Main.rand.NextFloat(0.4f, 0.8f), 180f, 0.05f);
                                GeneralParticleHandler.SpawnParticle(p);
                            }
                            if (Timer > 1)
                            {
                                NPC.velocity.X += -NPC.direction * 0.01f;
                            }
                            // Failsafe if it gets trapped in jet mode
                            if (Timer > 300)
                            {
                                NPC.velocity.Y = 0;
                                Timer = 0;
                                Jetting = -120;
                                CurrentPhase = (int)PhaseType.Idle;
                                NPC.rotation = 0;
                                break;
                            }
                        }
                    }
                    break;
                case (int)PhaseType.Hunting:
                    {
                        int jumpUp = 60;
                        int jetSide = jumpUp + 20;
                        int jetRate = 1;

                        Entity target = CurrentPrey != null ? CurrentPrey : CurrentPlayer;

                        // If the target is gone, reset
                        if ((NPC.velocity.Y == 0 && Jetting > 0 && Timer > jetSide) || target == null)
                        {
                            NPC.velocity.Y = 0;
                            NPC.velocity.X = 0;
                            Timer = 0;
                            Jetting = -120;
                            CurrentPhase = (int)PhaseType.Idle;
                            NPC.rotation = 0;
                            break;
                        }

                        // Jet towards the target
                        if (NPC.velocity.Y == 0 && Jetting == 0)
                        {
                            NPC.velocity.X *= 0.95f;
                            Timer++;
                            if (Timer == jumpUp && Jetting == 0)
                            {
                                Jetting = 1;
                                NPC.velocity = NPC.SafeDirectionTo(target.Center) * 20;
                                SoundEngine.PlaySound(DragonsBreath.FireballSound with { Pitch = 0.4f }, NPC.Center);
                            }
                        }
                        // Release particles
                        if (Jetting == 1)
                        {
                            Timer++;
                            if (Timer % jetRate == 0)
                            {
                                SeaFoamParticle p = new SeaFoamParticle(NPC.Center, -NPC.velocity, Color.LightCyan, Color.Cyan, Main.rand.NextFloat(0.4f, 0.8f), 180f, 0.05f);
                                GeneralParticleHandler.SpawnParticle(p);
                            }
                            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                        }
                        // Failsafe reset
                        if (Timer > 300)
                        {
                            NPC.velocity.Y = 0;
                            Timer = 0;
                            Jetting = -120;
                            CurrentPhase = (int)PhaseType.Idle;
                            NPC.rotation = 0;
                            break;
                        }
                    }
                    break;
            }

            if (Jetting < 0)
                Jetting++;

            if (WalkTimer > 0)
                WalkTimer--;

            if (TurnTimer > 0)
                TurnTimer--;

            NPC.spriteDirection = NPC.direction;
        }

        // Checks if the horizontal position in front of it has lava or is a pit so that the pod can avoid it
        public bool FallCheck(int height)
        {
            Point startPos = NPC.direction == 1 ? NPC.Right.ToTileCoordinates() : NPC.Left.ToTileCoordinates();
            for (int i = 1; i < height; i++)
            {
                Tile t = CalamityUtils.ParanoidTileRetrieval(startPos.X + NPC.direction, startPos.Y + i);
                Tile above = CalamityUtils.ParanoidTileRetrieval(startPos.X + NPC.direction, startPos.Y + i - 2);
                // If there's a tile, wegud
                if (t.HasTile)
                    return false;
                // If there's liquid or a pit, we not gud
                if (t.LiquidAmount > 0 && above.LiquidAmount > 0)
                    return true;

            }
            return true;
        }

        // Checks if the horizontal position in front of the pod can be jumped up
        public bool JumpCheck(int height)
        {
            Point startPos = NPC.direction == 1 ? NPC.BottomRight.ToTileCoordinates() : NPC.BottomLeft.ToTileCoordinates();
            bool canJump = false;
            int add = NPC.direction == 1 ? 0 : -1;
            int jumpHeight = height;
            for (int i = jumpHeight; i > 0; i--)
            {
                Tile t = CalamityUtils.ParanoidTileRetrieval(startPos.X + add, startPos.Y - i);
                // If there are tiles obfuscating the jump position, return false
                if (i > (jumpHeight - 2) && t.HasTile)
                    return false;
                // If there is a tile to jump on, return true
                if (t.HasTile)
                {
                    canJump = true;
                    break;
                }
            }
            return canJump;
        }

        public override void FindFrame(int frameHeight)
        {
            bool still = (CurrentPhase == (int)PhaseType.Idle && NPC.velocity.X == 0);
            if (!still)
            {
                NPC.frameCounter++;
            }
            if (NPC.frameCounter > 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
            }
            // Jet
            if (Jetting > 0)
            {
                if (NPC.frame.Y >= 17 * frameHeight || NPC.frame.Y < 12 * frameHeight)
                {
                    NPC.frame.Y = 12 * frameHeight;
                }
            }
            // Curl up
            else if (CurrentPhase != (int)PhaseType.Idle)
            {
                if (NPC.frame.Y > 11 * frameHeight)
                {
                    NPC.frame.Y = 11 * frameHeight;
                }
                if (NPC.frame.Y < 7 * frameHeight)
                {
                    NPC.frame.Y = 7 * frameHeight;
                }
            }
            // Nothing
            else if (still)
            {
                NPC.frame.Y = 7 * frameHeight;
            }
            // Walk
            else
            {
                if (NPC.frame.Y >= 7 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || n == CurrentPrey && Vector2.DistanceSquared(NPC.Center, n.Center) < 960f * 960f;
        }

        protected override bool PlayerSearchFilter(Player n)
        {
            return base.PlayerSearchFilter(n) || n == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, n.Center) < 960f * 960f;
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneBasaltGully && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.5f;
            }
            return 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage <= 0)
                return;
            target.AddBuff(BuffID.OnFire, 120);
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Lava, hit.HitDirection, -1f, 0, default, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            spriteBatch.Draw(tex, NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);
            spriteBatch.Draw(glowTex.Value, NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY, NPC.frame, Color.White, NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);

            return false;
        }
    }
}
