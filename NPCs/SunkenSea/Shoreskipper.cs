using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;
using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Shoreskipper : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Roar = 1,
            Jumps = 2,
        }

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];
        public bool spawnedTackleHitbox = false;
        public bool notLandedFromWater = false; // When in water or just got out of water but not yet landed on solid ground
        public NPC Target; // Chosen target (can only be other shoreskippers)

        #region SunkenSea Fields 
        protected override List<int> PreyIDs =>
        [
            NPCType<Shoreskipper>(),
        ];
        protected override List<int> PredatorIDs =>
        [
            NPCType<Shoreskipper>(),
        ];
        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.TimelessShores;

        protected override bool NPCSearchFilter(NPC n)
        {
            // Only consider same-type NPCs within 450 pixels
            return n.active && n.type == Type && Vector2.DistanceSquared(NPC.Center, n.Center) < 450f * 450f;
        }
        protected override void OnPreyDetection(NPC prey)
        {
            if (prey.active && NPC.HasSight(prey.Center) && (Target == null || !Target.active))
            {
                ChangePhase((int)PhaseType.Roar);
                Target = prey;
            }
        }
        #endregion


        public override bool CanBeHitByNPC(NPC attacker) => true;
        public override bool CanHitNPC(NPC target) => target.type == NPC.type;
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;
        public override bool? CanBeHitByItem(Player player, Item item) => true;
        public override bool? CanBeHitByProjectile(Projectile projectile) => true;

        // If not a Shoreskipper tackle, nullify knockback
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type != ProjectileType<ShoreskipperTackle>())
                projectile.knockBack = 0f;
        }
        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) => modifiers.DisableKnockback();
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.DisableKnockback();

        #region Syncing 
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.ai[0]);
            writer.Write(NPC.ai[1]);
            writer.Write(NPC.ai[2]);
            writer.Write(NPC.ai[3]);
            writer.Write(NPC.Calamity().newAI[0]);
            writer.Write(NPC.Calamity().newAI[1]);
            writer.Write(spawnedTackleHitbox);
            writer.Write(notLandedFromWater);

            int targetWhoAmI = Target == null || !Target.active ? -1 : Target.whoAmI;
            writer.Write(targetWhoAmI);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.ai[0] = reader.ReadSingle();
            NPC.ai[1] = reader.ReadSingle();
            NPC.ai[2] = reader.ReadSingle();
            NPC.ai[3] = reader.ReadSingle();
            NPC.Calamity().newAI[0] = reader.ReadSingle();
            NPC.Calamity().newAI[1] = reader.ReadSingle();
            spawnedTackleHitbox = reader.ReadBoolean();
            notLandedFromWater = reader.ReadBoolean();

            int targetWhoAmI = reader.ReadInt32();
            if (targetWhoAmI >= 0 && targetWhoAmI < Main.maxNPCs)
                Target = Main.npc[targetWhoAmI];
            else
                Target = null;
        }
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 7;
            Main.npcCatchable[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.damage = 15;
            NPC.width = 64;
            NPC.height = 32;
            NPC.defense = 4;
            NPC.lifeMax = 80;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath9;
            NPC.GravityIgnoresLiquid = true;

            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ShoreskipperBanner>();

            NPC.catchItem = (short)ModContent.ItemType<ShoreskipperItem>();

            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToWater = false;

            SpawnModBiomes = new int[1] { ModContent.GetInstance<TimelessShoresBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Shoreskipper")
            });
        }
        public override bool? CanFallThroughPlatforms()
        {
            // Fall to try to reach target
            if (CurrentPhase == (int)PhaseType.Jumps && Target.Top.Y > NPC.Bottom.Y)
                return true;

            return false;
        }

        private bool HasActiveTackle() // Check for if their melee hitbox exists as to not spawn multiple at once
        {
            int tackleType = ProjectileType<ShoreskipperTackle>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == tackleType && proj.ai[0] == NPC.whoAmI)
                    return true;
            }
            return false;
        }

        public override void AI()
        {
            NPC.Calamity().newAI[1]++; // Mark as out of water 
            NPC.TargetClosest(false);

            if (NPC.direction == 0)
            {
                NPC.direction = Main.rand.NextBool().ToDirectionInt();
            }
            int frameHeight = TextureAssets.Npc[Type].Value.Height / Main.npcFrameCount[Type];


            if (NPC.velocity.Y == 0 && NPC.oldVelocity.Y != 0f && notLandedFromWater && !NPC.wet) // Do not maintain velocity from skipping on water
            {
                NPC.velocity.X = 0f;
                notLandedFromWater = false;
            }

            if (notLandedFromWater && !NPC.wet && NPC.velocity.Length() > 6.5f && Timer % 4 == 0) // When skipping on water at high enough velocity, spawns trailing rings
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center, -NPC.velocity * 0.33f, Color.DeepSkyBlue * 0.65f, new Vector2(0.5f, 1f), NPC.rotation, 0.12f, 0.24f, 18));
            

            switch (CurrentPhase)
            {
                case (int)PhaseType.Idle:
                    {
                        int frameDuration = 10;
                        int currentFrameIndex = (int)(NPC.frameCounter / frameDuration) % Main.npcFrameCount[Type];
                        // If it's the third frame
                        bool isAllowedToMove = NPC.frame.Y / frameHeight == 2;

                        // Move horizontally in short bursts
                        if (Timer >= 10 && NPC.velocity.Y == 0 && Math.Abs(NPC.velocity.X) < 1 && NPC.ai[2] == 0 && isAllowedToMove)
                        {
                            NPC.ai[2] = 1;
                            NPC.velocity.X = Main.rand.NextFloat(1.75f, 2.5f) * Main.rand.NextBool().ToDirectionInt();
                            if (Main.rand.NextBool(4))
                                SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/ShoreskipperGrunt", 2) { Volume = 0.9f, PitchVariance = 0.15f }, NPC.Center);
                        }

                        // Increment the timer before the slide ends
                        if (NPC.ai[2] == 1)
                        {
                            NPC.ai[3]++;
                        }

                        // Slow down quickly after sliding
                        else if (NPC.ai[2] == 2 && NPC.velocity.Y == 0)
                        {
                            NPC.velocity.X *= 0.835f;
                            NPC.ai[3]++;
                        }

                        // Handle movement states
                        if (NPC.ai[3] > Main.rand.Next(20, 40))
                        {
                            // If moving, enter slowdown
                            if (NPC.ai[2] == 1)
                                NPC.ai[2] = 2;
                            // If in slowdown, reset
                            else if (NPC.ai[2] == 2)
                                ChangePhase((int)PhaseType.Idle);
                            NPC.ai[3] = 0;
                        }

                        NPC.direction = NPC.velocity.X.DirectionalSign();
                    }
                    break;

                    // Based off of Leerslug's initial detection phase.
                case (int)PhaseType.Roar:
                    {
                        int roar = 20;
                        int startAI = 35;
                        NPC.velocity.X *= 0.9f;

                        // Roar in place with a lil jump
                        if (Timer == roar)
                        {
                            SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/ShoreskipperSighting") { Volume = 0.85f, Pitch = -0.1f, PitchVariance = 0.1f }, NPC.Center);
                            if (NPC.velocity.Y == 0)
                            {
                                NPC.velocity.Y = -2.5f;
                            }
                        }

                        NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();

                        if (Timer <= startAI && Timer >= roar && Timer % 5 == 0)
                            GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(NPC.Center + new Vector2((NPC.direction == 1 ? 28f : -28f), 0), Vector2.Zero, Color.Gray * 0.55f, new Vector2(1f, 1f), NPC.rotation, 0.05f, 0.6f, 16));

                        if (Timer > startAI)
                            ChangePhase((int)PhaseType.Jumps);

                        if ((!Target.active || Target == null || (Target.Distance(NPC.Center) > 580 && !NPC.HasSight(Target.Center)) || Target.Distance(NPC.Center) > 700) && (NPC.velocity.Y == 0 || NPC.wet)) // If target it dead/out of range and if this shoreskipper isnt midair
                        {
                            NPC.velocity.X *= 0.33f;
                            Target = null;
                            ChangePhase((int)PhaseType.Idle);
                        }
                    }
                    break;

                case (int)PhaseType.Jumps:
                    {
                        // Hops toward the target
                        int jumpHeight = Target.Bottom.Y < NPC.Top.Y ? 6 : 4;

                        // Reset hitbox status when grounded or right after exiting water
                        if (NPC.oldVelocity.Y != 0 && NPC.velocity.Y == 0 || (NPC.Calamity().newAI[1] == 1f))
                        {
                            spawnedTackleHitbox = false;
                            NPC.velocity.X = 0;
                        }

                        // Keep facing target while on the ground/water
                        if (NPC.velocity.Y == 0 || NPC.wet)
                            NPC.direction = NPC.DirectionTo(Target.Center).X.DirectionalSign();

                        // Check if the shoreskipper landed
                        bool isGrounded = NPC.velocity.Y == 0;

                        // Jump at the target on the 4th frame
                        bool isAtJumpFrame = NPC.frame.Y / frameHeight == 3;

                        if ((!spawnedTackleHitbox && (isGrounded && isAtJumpFrame)) || (NPC.Calamity().newAI[1] == 1f && HasActiveTackle() == false)) // Latter set of conditions allows spawning a hitbox while skipping on water
                        {
                            if (isGrounded)               
                                NPC.velocity.Y = -jumpHeight;

                            // Face and chase after target
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 4;

                            // Spawn hitbox while jumping
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<ShoreskipperTackle>(), (int) (NPC.damage * Main.rand.NextFloat(0.7f, 1.4f)), Main.rand.Next(3, 7), Main.myPlayer, NPC.whoAmI);
                                if (Main.rand.NextBool())
                                    SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/ShoreskipperGrunt", 2) { Volume = 1f, PitchVariance = 0.15f }, NPC.Center);

                                spawnedTackleHitbox = true;
                            }
                        }

                        if ((!Target.active || Target == null || (Target.Distance(NPC.Center) > 580 && !NPC.HasSight(Target.Center)) || Target.Distance(NPC.Center) > 700) && (NPC.velocity.Y == 0 || NPC.wet)) // If target it dead/out of range and if this shoreskipper isnt midair
                        {
                            NPC.velocity.X = 0f;
                            Target = null;
                            ChangePhase((int)PhaseType.Idle);
                        }
                    }
                    break;
            }

            NPC.StepUpBlocks();
            NPC.spriteDirection = NPC.direction;

            // In / on top of water
            if (NPC.wet)
            {
                if (NPC.Calamity().newAI[0] == 0f || Timer % 120 == 0)
                {
                    int newWaterDirection = NPC.direction;
                    if (Target != null && Target.active)
                        newWaterDirection = NPC.DirectionTo(Target.Center).X.DirectionalSign();
                    else if (Main.rand.NextBool(3))
                        newWaterDirection *= -1;

                    NPC.Calamity().newAI[0] = newWaterDirection;
                    NPC.Calamity().newAI[1] = 0f;
                }

                NPC.direction = (int)NPC.Calamity().newAI[0];
                NPC.velocity.Y = MathHelper.Min(NPC.velocity.Y - 0.2f, -4);
                notLandedFromWater = true;

                float waterAcceleration = 2.75f;
                // Accelerate up to the cap of 12
                if (Math.Abs(NPC.velocity.X) < 12f)
                    NPC.velocity.X += NPC.direction * waterAcceleration;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -12f, 12f);
            }

            else
            {
                // Reset the water direction AI slot when the NPC leaves the water
                NPC.Calamity().newAI[0] = 0f;

                if (NPC.Calamity().newAI[1] == 0f) // Just got out of water
                {
                    NPC.Calamity().newAI[1] = 1f; // Will spawn a hitbox if aggroed
                }
            }

            Timer++;
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

            // Reset anim progress
            NPC.frame.Y = 0;
            NPC.frameCounter = 0;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneTimelessShores && !spawnInfo.Water)
                    return SpawnCondition.Cavern.Chance * 0.6f;
                else if (spawnInfo.Player.Calamity().ZoneTimelessShores && spawnInfo.Water)
                    return SpawnCondition.Cavern.Chance * 0.25f;
            }
            return 0f;
        }

        #region FX + Drawing
        public override void FindFrame(int frameHeight)
        {
            bool isMidAir = NPC.velocity.Y != 0 && !NPC.wet;
            int currentFrameIndex = NPC.frame.Y / frameHeight;

            if (CurrentPhase == (int)PhaseType.Roar)
            {
                NPC.frame.Y = 0;
                return;
            }

            if (isMidAir && NPC.frame.Y >= frameHeight * 4)
            {
                NPC.frame.Y = frameHeight * 4; // Cannot progress from last airborne frame until grounded again
                return;
            }

            NPC.frameCounter++;
            if (NPC.frameCounter > (CurrentPhase == (int)PhaseType.Jumps ? 5 : 8)) // Progress faster when in jump phase
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
            for (int k = 0; k < 6; k++)
            {
                GeneralParticleHandler.SpawnParticle(new GenericBubbleParticle(NPC.Center, new Vector2(Main.rand.NextFloat(3f, 7f) * hit.HitDirection, Main.rand.NextFloat(-5f, 5f)).RotatedByRandom(1), Main.rand.NextFloat(0.22f, 0.52f), NPC.rotation, Main.rand.Next(7, 11)));
            }

            for (int k = 0; k < 6; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BloodWater, hit.HitDirection, -3f, 0, default, 0.8f);
            }
            SpawnGores(NPC, "Shoreskipper", 2);
        }

        public override void OnKill()
        {
            for (int k = 0; k < 10; k++)
            {
                GeneralParticleHandler.SpawnParticle(new GenericBubbleParticle(NPC.Center, new Vector2(Main.rand.NextFloat(3.25f, 8.5f), Main.rand.NextFloat(-5f, 5f)).RotatedByRandom(MathHelper.TwoPi), Main.rand.NextFloat(0.26f, 0.62f), NPC.rotation, Main.rand.Next(8, 13)));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 pos = NPC.Center - screenPos + Vector2.UnitY * NPC.gfxOffY;

            spriteBatch.Draw(tex, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0);

            return false;
        }
        #endregion
    }
}
