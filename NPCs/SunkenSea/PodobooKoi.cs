using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.DataStructures;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;
using CalamityMod.Enums;
using System.Collections.Generic;
using Steamworks;
using System.IO;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using System.Linq;

namespace CalamityMod.NPCs.SunkenSea
{
    public class PodobooKoi : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Flee = 1,
            Hunt = 2,
            Hostile = 3
        }
        public enum VariantType
        {
            Normal = 0,
            Comet = 1,
            BubbleEye = 2,
            Oranda = 3
        }

        public ref float CurrentVariant => ref NPC.ai[0];

        public ref float CurrentBehavior => ref NPC.ai[1];

        public ref float ShootTimer => ref NPC.ai[2];

        public static int IdleMinPathDistance = 50;
        public static int IdleMaxPathDistance = 600;

        public static int FleeTileAnticipationDistance = 28;

        public Vector2 randomPathPoint;
        public Vector2 lavaLine = Vector2.Zero;

        public static Asset<Texture2D> cometTex;
        public static Asset<Texture2D> bubbleEyedTex;
        public static Asset<Texture2D> orandaTex;

        protected override List<int> PreyIDs => new List<int>()
        {

        };

        protected override List<int> PredatorIDs => new List<int>() {
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.BasaltGully;

        public override void Load()
        {
            cometTex = ModContent.Request<Texture2D>(Texture + "Comet");
            bubbleEyedTex = ModContent.Request<Texture2D>(Texture + "BubbleEyed");
            orandaTex = ModContent.Request<Texture2D>(Texture + "Oranda");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.damage = 30;
            NPC.width = 40;
            NPC.height = 32;
            NPC.defense = 10;
            NPC.lifeMax = 1000;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.lavaImmune = true;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToCold = true;
            NPC.Calamity().VulnerableToWater = true;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<PodobooKoiBanner>();
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.PodobooKoi")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 50% chance to be one of 3 variants
            if (Main.rand.NextBool())
            {
                CurrentVariant = Main.rand.Next(1, 4);
            }
            pathfinding = new PathfindingManager(NPC)
            {
                Acceleration = 0.5f,
                MaxSpeed = 4f,
            };
        }

        public override void AI()
        {
            if (NPC.ai[3] <= 0)
            {
                lavaLine = FindLavaLine();
                NPC.ai[3] = 180;
            }
            NPC.ai[3]--;
            if (NPC.lavaWet)
            {
                switch (CurrentBehavior)
                {
                    case (int)PhaseType.Idle:
                        IdleBehavior();
                        break;
                    case (int)PhaseType.Flee:
                        FleeBehavior();
                        break;
                    case (int)PhaseType.Hunt:
                        HuntBehavior();
                        break;
                    case (int)PhaseType.Hostile:
                        HostileBehavior();
                        break;
                }
            }
            else
            {
                BeachedBehavior();
            }
            if (CurrentBehavior != (int)PhaseType.Hostile)
            {
                int dir = NPC.velocity.X.DirectionalSign();
                NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
                NPC.spriteDirection = NPC.direction = dir;
            }
        }

        public void IdleBehavior()
        {
            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), LavaTileValiditySizeless));
        }

        public bool LavaTileValiditySizeless(Point point)
        {
            return !(Main.tile[point].IsTileSolidGround() || Main.tile[point].LiquidAmount < 255 || Main.tile[point].LiquidType != LiquidID.Lava);
        }

        public void FleeBehavior()
        {
            // If the predator is gone, go back to idling.
            if (CurrentPredator == null)
            {
                CurrentBehavior = (int)PhaseType.Idle;
                pathfinding.MaxSpeed = 4;
                return;
            }

            pathfinding.MaxSpeed = 8;

            // While it doesn't have any obstacles in front of it, run away in a straight line.
            // Try to manuever if there are any obstacles.
            if (!Main.tile[(NPC.Center + NPC.DirectionFrom(CurrentPredator.Center) * FleeTileAnticipationDistance).ToTileCoordinates()].IsTileSolid())
            {
                NPC.velocity += NPC.DirectionFrom(CurrentPredator.Center) * pathfinding.Acceleration;
                pathfinding.ClearResults();

                // Cap the speed if MaxSpeed has been surpassed.
                if (NPC.velocity.LengthSquared() > pathfinding.MaxSpeed * pathfinding.MaxSpeed)
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * pathfinding.MaxSpeed;
            }
            else
            {
                float distanceFromAvoided = Vector2.Distance(NPC.Center, CurrentPredator.Center);
                randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                NPC.netUpdate = true;
                pathfinding.DoPathfinding(new(NPC.Center, randomPathPoint, LavaTileValidity));
            }
        }

        public void HuntBehavior()
        {
            if (CurrentPrey == null)
            {
                pathfinding.MaxSpeed = 4;
                CurrentBehavior = (int)PhaseType.Idle;
                pathfinding.ClearResults();
                return;
            }
            pathfinding.MaxSpeed = 8;
            pathfinding.DoPathfinding(new(NPC.Center, CurrentPrey.Center, LavaTileValidity));
        }

        public void HostileBehavior()
        {
            // Reset
            if (CurrentPlayer == null || !NPC.HasSight(CurrentPlayer.Center))
            {
                pathfinding.MaxSpeed = 4;
                CurrentBehavior = (int)PhaseType.Idle;
                NPC.ai[2] = 0;
                pathfinding.ClearResults();
                // Sometimes the fish gets stuck in air, so make it dive down when losing interest
                NPC.velocity.Y = 4;
                return;
            }

            bool noLava = lavaLine == Vector2.Zero;
            // If this is true, use its lava behavior
            bool useLavaAI = CurrentPlayer.lavaWet && NPC.Distance(CurrentPlayer.Center) < 500;
            // If this is true, but the above is not use its "air" behavior. This is set to true provided the fish is close enough to the lava line
            bool useAirAI = false;

            // If the player is not in lava
            if (lavaLine != Vector2.Zero && !CurrentPlayer.lavaWet)
            {
                // Use air AI if the fish is close enough to the lava line 
                useAirAI = NPC.Distance(lavaLine) < 80;

                // If the player is too far from the lava swim towards the player
                bool farFromLava = CurrentPlayer.Distance(lavaLine) > 300;
                // Distance from the destination the fish needs to be
                float distNeeded = farFromLava ? 300 : 40;

                Vector2 destination = farFromLava ? new Vector2(CurrentPlayer.Center.X, lavaLine.Y) : lavaLine;

                // If it's still trying to chase the player, clear the pathfinding results
                if (NPC.Calamity().newAI[0] == 1)
                {
                    pathfinding.ClearResults();
                    NPC.Calamity().newAI[0] = 0;
                }

                // Go to the location if not close enough, otherwise slow down
                if (NPC.Distance(destination) > distNeeded)
                {
                    pathfinding.DoPathfinding(new(NPC.Center, destination));
                }
                else
                {
                    NPC.velocity *= 0.9f;
                    pathfinding.ClearResults();
                }
            }
            // If the player is in lava but far from the fish just swim to them
            else if (NPC.Distance(CurrentPlayer.Center) > 150)
            {
                if (NPC.position == NPC.oldPosition)
                {
                    NPC.velocity.Y = 4;
                }
                pathfinding.MaxSpeed = 6;
                pathfinding.DoPathfinding(new(NPC.Center, CurrentPlayer.Center, LavaTileValiditySizeless));
                // Mark the fish as currently trying to chase the player
                NPC.Calamity().newAI[0] = 1;
            }
            // If the player is in lava and close to the fish just kinda float about
            else
            {
                NPC.SimpleFlyMovement(Main.rand.NextVector2Circular(11, 11), 0.01f);
                pathfinding.ClearResults();
            }

            // Lava attack, directly shoot at the player
            if (useLavaAI)
            { 
                ShootTimer++;

                if (ShootTimer >= Main.rand.Next(50, 80))
                {
                    ShootTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.SafeDirectionTo(CurrentPlayer.Center, Vector2.UnitY) * 12, ModContent.ProjectileType<DrizzlefishFireball>(), NPC.damage, 0);
                        Main.projectile[p].hostile = true;
                        Main.projectile[p].friendly = false;
                    }
                }
            }
            // Air attack, shoot projectiles at the player in arches
            else if (useAirAI)
            {
                pathfinding.MaxSpeed = 4;
                ShootTimer++;

                if (ShootTimer >= Main.rand.Next(50, 80))
                {
                    ShootTimer = 0;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Scale the vertical velocity based on distance from the player
                        float upwards = 0.01f * MathF.Abs(CurrentPlayer.Center.Y - NPC.Center.Y);
                        int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(CurrentPlayer.Center) * 10 - Vector2.UnitY * upwards, ModContent.ProjectileType<DrizzlefishFireball>(), NPC.damage, 0);
                        Main.projectile[p].hostile = true;
                        Main.projectile[p].friendly = false;
                    }
                }
            }
            int dir = NPC.DirectionTo(CurrentPlayer.Center).X.DirectionalSign();
            NPC.rotation = NPC.DirectionTo(CurrentPlayer.Center).ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
            NPC.spriteDirection = NPC.direction = dir;
        }

        public Vector2 FindLavaLine()
        {
            Vector2? tileFoundPosition = null;
            Point npcPoint = NPC.Center.ToTileCoordinates();
            for (int i = npcPoint.Y; i > npcPoint.Y - 60; i--)
            {
                if (!Main.tile[npcPoint.X, i].HasTile && Main.tile[npcPoint.X, i].LiquidAmount < 255)
                {
                    tileFoundPosition = new Point(npcPoint.X, i).ToWorldCoordinates() - Vector2.UnitY * 8;
                    break;
                }
            }

            if (tileFoundPosition.HasValue)
                return tileFoundPosition.Value;

            return Vector2.Zero;
        }

        public void BeachedBehavior()
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity.X = NPC.velocity.X * 0.94f;
                if ((double)NPC.velocity.X > -0.2 && (double)NPC.velocity.X < 0.2)
                {
                    NPC.velocity.X = 0f;
                }
            }
            NPC.velocity.Y = NPC.velocity.Y + 0.3f;
            if (NPC.velocity.Y > 10f)
            {
                NPC.velocity.Y = 10f;
            }
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || n == CurrentPredator && Vector2.DistanceSquared(NPC.Center, n.Center) < 960f * 960f;
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            bool lavaFilter = p.lavaWet && Vector2.DistanceSquared(NPC.Center, p.Center) < 300f * 300f;
            bool airFilter = false;
            bool alreadyVisible = p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 600f * 600f;
            if (lavaLine != Vector2.Zero)
            {
                Vector2 lavaDist = lavaLine - p.Center;
                airFilter = !p.lavaWet && p.Center.Y <= lavaLine.Y && MathF.Abs(lavaDist.Y) < 200 && MathF.Abs(lavaDist.X) < 300;
            }
            return NPC.HasSight(p.Center) && (lavaFilter || airFilter || alreadyVisible);
        }

        protected override void OnPlayerDetection(Player player)
        {
            if (CurrentBehavior == (int)PhaseType.Idle)
            {
                CurrentBehavior = (int)PhaseType.Hostile;
                lavaLine = FindLavaLine();
            }
        }

        protected override void OnPreyDetection(NPC prey)
        {
            if (CurrentBehavior != (int)PhaseType.Flee)
                CurrentBehavior = (int)PhaseType.Hunt;
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            CurrentBehavior = (int)PhaseType.Flee;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 0.1f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            switch (CurrentVariant)
            {
                case (int)VariantType.Comet:
                    tex = cometTex.Value;
                    break;
                case (int)VariantType.Oranda:
                    tex = orandaTex.Value;
                    break;
                case (int)VariantType.BubbleEye:
                    tex = bubbleEyedTex.Value;
                    break;
            }
            spriteBatch.Draw(tex, NPC.Center - screenPos, null, NPC.GetAlpha(drawColor), NPC.rotation, tex.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Lava, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(randomPathPoint);
            writer.WriteVector2(lavaLine);
            writer.Write(NPC.Calamity().newAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
            lavaLine = reader.ReadVector2();
            NPC.Calamity().newAI[0] = reader.ReadSingle();
        }
    }
}
