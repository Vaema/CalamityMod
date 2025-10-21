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
using CalamityMod.Enums;
using System.Collections.Generic;
using System;
using CalamityMod.Projectiles.Ranged;

namespace CalamityMod.NPCs.SunkenSea
{
    public abstract class Pearlpod : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Eating = 1,
            Fleeing = 2,
            Hiding = 3
        }

        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>();

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.GleamingBurrows | SunkenSeaBiomeFlags.ClamDen;

        public abstract float SpawnRate { get; }
        public abstract int ItemType { get; }

        public abstract string GoreName { get; }

        public ref float CurrentPhase => ref NPC.Calamity().newAI[0];

        public ref float BiteCount => ref NPC.Calamity().newAI[1];

        public NPC clam => NPC.Calamity().newAI[2] == 0 ? null : Main.npc[(int)NPC.Calamity().newAI[2] - 1];

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.aiStyle = NPCAIStyleID.Snail;
            NPC.damage = 0;
            NPC.width = 30;
            NPC.height = 30;
            NPC.defense = 0;
            NPC.lifeMax = 20;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false; 
            NPC.HitSound = SoundID.NPCHit38;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.GravityIgnoresLiquid = true;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<PearlpodBanner>();
            NPC.catchItem = ItemType;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
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
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Pearlpod")
            });
        }

        public override void AI()
        {
            bool predatorAvailable = CurrentPredator != null || CurrentPlayer != null;
            Tile vine = CalamityUtils.ParanoidTileRetrieval((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
            if (CurrentPhase < 2)
            {
                // Set newAI[0] to 1 if the Pearlpod is inside of a vine
                if (vine.TileType == ModContent.TileType<DepthVines>() && !NPC.justHit)
                {
                    CurrentPhase = (int)PhaseType.Eating;
                }
                // Otherwise reset eating-related variables
                else
                {
                    BiteCount = 0;
                    CurrentPhase = (int)PhaseType.Idle;
                }
            }

            switch (CurrentPhase)
            {
                // :literallynothing:
                case (int)PhaseType.Idle:
                    {
                    }
                    break;
                // Eat kelp if found
                case (int)PhaseType.Eating:
                    {
                        NPC.velocity.X *= 0.1f;
                        // Play a crunch sound and spawn some grass dust randomly 
                        if (Main.rand.NextBool(20))
                        {
                            SoundEngine.PlaySound(SoundID.Item2 with { Volume = 0.4f, Pitch = 1.2f }, NPC.Center);
                            for (int i = 0; i < 4; i++)
                            {
                                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1), 40);
                            }
                            BiteCount++;
                        }
                        // After munching 10 times, the vine is broken and the Pearlpod continues about its day
                        if (BiteCount == 10)
                        {
                            if (vine.TileType == ModContent.TileType<DepthVines>())
                            {
                                WorldGen.KillTile((int)NPC.position.X / 16, (int)NPC.position.Y / 16);
                            }
                        }
                    }
                    break;
                // Run into a clam
                case (int)PhaseType.Fleeing:
                    {
                        BiteCount = 0;
                        bool clamValid = IsClamValid();
                        // Stop running if the threat is gone
                        if (!predatorAvailable)
                        {
                            CurrentPhase = (int)PhaseType.Idle;
                            NPC.ShowNameOnHover = true;
                            ResetClam();
                            break;
                        }
                        // Look for a clam if the current clam is no longer valid
                        if (!clamValid)
                        {
                            LookForClam();
                        }
                        else
                        {
                            // Run boy run!
                            NPC.velocity.X = NPC.DirectionTo(clam.Center).X.DirectionalSign() * 3;
                            NPC.direction = NPC.velocity.X.DirectionalSign();
                            NPC.StepUpBlocks();

                            // Minimum distances on both axes. Y is higher in case there's a ledge
                            Vector2 dif = NPC.Center - clam.Center;
                            bool xClose = Math.Abs(dif.X) < 20;
                            bool yClose = Math.Abs(dif.Y) < 60;

                            // Enter the clam if close enough
                            if (xClose && yClose)
                            {
                                CurrentPhase = (int)PhaseType.Hiding;
                            }
                            // If close enough horizontally, but not vertically, jump
                            else if (xClose && !yClose && NPC.Center.Y < clam.Center.Y && NPC.velocity.Y == 0)
                            {
                                NPC.velocity.Y -= 4;
                            }
                        }
                    }
                    break;
                // Hide inside a clam
                case (int)PhaseType.Hiding:
                    {
                        BiteCount = 0;
                        bool clamValid = IsClamValid();
                        // Exit the clam if the threat is gone
                        if (!predatorAvailable)
                        {
                            CurrentPhase = (int)PhaseType.Idle;
                            NPC.ShowNameOnHover = true;
                            ResetClam();
                            break;
                        }
                        // If the clam is gone, exit the dead clam
                        if (!clamValid)
                        {
                            LookForClam();
                            if (!IsClamValid())
                            {
                                CurrentPhase = (int)PhaseType.Idle;
                                NPC.ShowNameOnHover = true;
                                ResetClam();
                            }
                        }
                        // True hiding behaviour
                        else
                        {
                            // Become invisible
                            if (NPC.Opacity > 0)
                                NPC.Opacity -= 0.2f;
                            // When fully invisible lock into the clam's position
                            if (NPC.Opacity <= 0)
                                NPC.Center = clam.Center;
                            NPC.dontTakeDamage = true;
                            NPC.ShowNameOnHover = false;
                        }
                    }
                    break;
            }
            // Reset hiding stuff when not in danger
            if (CurrentPhase < 2)
            {
                if (predatorAvailable && !IsClamValid())
                    LookForClam();
            }
            if (CurrentPhase != (int)PhaseType.Hiding)
            {
                if (NPC.Opacity < 1)
                {
                    NPC.Opacity += 0.3f;
                    if (NPC.Opacity > 1)
                        NPC.Opacity = 1;
                }
                NPC.dontTakeDamage = false;
            }
        }
        
        /// <summary>
        /// Puts the clam back into idle state
        /// </summary>
        public void ResetClam()
        {
            if (IsClamValid())
            {
                clam.localAI[2] = 0;
                Clam clamMod = clam.ModNPC<Clam>();
                clamMod.ChangePhase((int)Clam.PhaseType.Idle);
                clamMod.ShellRotation = MathHelper.ToRadians(60);
                NPC.netUpdate = true;
                clam.netUpdate = true;
            }
        }

        /// <summary>
        /// Check if a clam is valid to hide in
        /// </summary>
        /// <returns></returns>
        public bool IsClamValid()
        {
            if (clam == null || !clam.active || clam.life < 0 || clam.type != ModContent.NPCType<Clam>() || clam.localAI[2] <= 0 || (clam.ai[0] > 0 && clam.ai[0] < 3))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Look for a clam
        /// </summary>
        public void LookForClam()
        {
            // Don't bother running the searches if no clams exist
            if (NPC.AnyNPCs(ModContent.NPCType<Clam>()))
            {
                NPC nearestClam = null;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    float dist = Vector2.Distance(NPC.Center, n.Center);
                    // Find the nearest clam in a 200px radius which the pearlpod is able to see and doesn't already have a pearlpod going after it
                    if (dist < 200f && NPC.HasSight(n.Center) && n.type == ModContent.NPCType<Clam>() && n.localAI[2] <= 0 && n.ai[0] == 0)
                    {
                        // if no clam has been attached yet and this is the nearest clam, mark it as a candidate
                        if (nearestClam == null || NPC.Distance(nearestClam.Center) < dist)
                        {
                            nearestClam = n;
                        }
                    }
                }
                // if a clam was found, update both NPCs' variables to have them cooperate
                if (nearestClam != null)
                {
                    nearestClam.localAI[2] = NPC.whoAmI + 1;
                    nearestClam.ai[0] = (int)Clam.PhaseType.Pod;
                    NPC.Calamity().newAI[2] = nearestClam.whoAmI + 1;
                    nearestClam.netUpdate = true;
                    NPC.netUpdate = true;
                }
            }
            if (clam != null)
            {
                clam.localAI[2] = NPC.whoAmI + 1;
            }
        }

        protected override void OnPlayerDetection(Player player)
        {
            CurrentPhase = (int)PhaseType.Fleeing;
            LookForClam();
        }

        protected override void OnPreyDetection(NPC prey)
        {
            CurrentPhase = (int)PhaseType.Fleeing;
            LookForClam();
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return !(CurrentPhase == (int)PhaseType.Hiding && !IsClamValid()) && (base.NPCSearchFilter(n) || n == CurrentPredator && Vector2.DistanceSquared(NPC.Center, n.Center) < 900f * 900f);
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return !(CurrentPhase == (int)PhaseType.Hiding && !IsClamValid()) && (base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 600f * 600f);
        }
        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * 3)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneGleamingBurrows && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * SpawnRate;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Coralstone, hit.HitDirection, -1f, 0, default, 1f);
            }
            CalamityUtils.SpawnGores(NPC, GoreName, 3);
        }
    }

    public class PearlpodWhite : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            Banner = ModContent.NPCType<PearlpodWhite>();
            BannerItem = ModContent.ItemType<PearlpodBanner>();
        }
        public override float SpawnRate => 0.6f;
        public override int ItemType => ModContent.ItemType<PearlpodItem>();

        public override string GoreName => "PearlpodWhite";
    }
    public class PearlpodPink : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            Banner = ModContent.NPCType<PearlpodWhite>();
            BannerItem = ModContent.ItemType<PearlpodBanner>();
        }
        public override float SpawnRate => 0.05f;
        public override int ItemType => ModContent.ItemType<PearlpodPinkItem>();

        public override string GoreName => "PearlpodPink";
    }
    public class PearlpodBlack : Pearlpod
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 4;
        }
        public override void SetDefaults()
        {
            Banner = ModContent.NPCType<PearlpodWhite>();
            BannerItem = ModContent.ItemType<PearlpodBanner>();
        }
        public override float SpawnRate => 0.2f;
        public override int ItemType => ModContent.ItemType<PearlpodBlackItem>();

        public override string GoreName => "PearlpodBlack";
    }
}
