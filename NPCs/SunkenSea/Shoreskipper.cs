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
using Terraria.ModLoader.Utilities;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Shoreskipper : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Rawr = 1,
            Jumps = 2,
        }

        public ref float CurrentPhase => ref NPC.ai[0];

        public ref float Timer => ref NPC.ai[1];

        public NPC Target;
        public bool spawnedTackleHitbox = false;

        #region SunkenSeaNPC Implementation

        // Can only do harm to its own species.
        protected override List<int> PreyIDs =>
        [
            NPCType<Shoreskipper>(),
        ];
        protected override List<int> PredatorIDs =>
        [
            NPCType<Shoreskipper>(),
        ];

        public override bool CanBeHitByNPC(NPC attacker) => true;

        public override bool CanHitNPC(NPC target) => target.type == NPC.type;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        // Player can hit Shoreskippers.
        public override bool? CanBeHitByItem(Player player, Item item) => true;
        public override bool? CanBeHitByProjectile(Projectile projectile) => true;

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.TimelessShores;
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

            NPC.value = Item.buyPrice(silver: 1);

            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ShoreskipperBanner>();

            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            // Only consider same-type NPCs within 450 pixels
            return n.active && n.type == Type && Vector2.DistanceSquared(NPC.Center, n.Center) < 450f * 450f;
        }
        protected override void OnPreyDetection(NPC prey)
        {
            if (prey.active && NPC.HasSight(prey.Center))
            {
                ChangePhase((int)PhaseType.Rawr);
                Target = prey;
            }
        }

        public override void AI()
        {
            NPC.Calamity().newAI[1]++;
            NPC.TargetClosest(false);
            Lighting.AddLight(NPC.Center, 0.5f, 0.2f, 0);
            if (NPC.direction == 0)
            {
                NPC.direction = Main.rand.NextBool().ToDirectionInt();
            }
            switch (CurrentPhase)
            {
                // IDLE AND ROAR LOGIC PLACEHOLDER. TAKEN FROM ANOTHER SSO NPC
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
                            ChangePhase((int)PhaseType.Jumps);

                        if (!Target.active || (Target.Distance(NPC.Center) > 700 && !NPC.HasSight(Target.Center)) && NPC.velocity.Y == 0) // If target it dead/out of range and if this shoreskipper is grounded
                        {
                            NPC.velocity.X = 0f;
                            ChangePhase((int)PhaseType.Idle);
                        }
                    }
                    break;

                case (int)PhaseType.Jumps:
                    {
                        // Hops toward the target
                        int jumpHeight = Target.Bottom.Y < NPC.Top.Y ? 6 : 4;

                        if (NPC.oldVelocity.Y != 0 && NPC.velocity.Y == 0)
                            spawnedTackleHitbox = false;

                        if (NPC.velocity.Y == 0 && !spawnedTackleHitbox)
                        {
                            NPC.velocity.Y = -jumpHeight;
                            NPC.velocity.X = NPC.DirectionTo(Target.Center).X.DirectionalSign() * 4;
                            NPC.ai[2]++;

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<ShoreskipperTackle>(), NPC.damage, 6f, Main.myPlayer, NPC.whoAmI);
                                spawnedTackleHitbox = true;
                            }
                        }

                        NPC.direction = NPC.velocity.X.DirectionalSign();

                        if (!Target.active || (Target.Distance(NPC.Center) > 700 && !NPC.HasSight(Target.Center)) && NPC.velocity.Y == 0) // If target it dead/out of range and if this shoreskipper is grounded
                        {
                            NPC.velocity.X = 0f;
                            ChangePhase((int)PhaseType.Idle);
                        }
                    }
                    break;
            }

            NPC.StepUpBlocks();
            NPC.spriteDirection = NPC.direction;

            // Bounce on water
            if (NPC.wet)
            {
                // Never fall below the surface
                NPC.velocity.Y = MathHelper.Min(NPC.velocity.Y - 0.2f, -4);

                // Gain speed quickly while skipping on shores (writing on fire)
                float waterAcceleration = 0.2f;
                NPC.velocity.X += NPC.direction * waterAcceleration;
                float maxWaterSpeed = 6f;

                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxWaterSpeed, maxWaterSpeed);
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
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneTimelessShores)
                    return SpawnCondition.Cavern.Chance * 0.9f;
            }
            return 0f;
        }

        // FX
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 8)
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
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Water, hit.HitDirection, -1f, 0, default, 1f);
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
    }
}
