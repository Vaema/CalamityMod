using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.World;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class KelpieSeadragon : SunkenSeaNPC
    {
        public ref float SquishX => ref NPC.localAI[0];
        public ref float SquishY => ref NPC.localAI[1];

        public Vector2 randomPathPoint;

        public Entity currentTarget;

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 400;
        public static int IdleMaxPathDistance = 800;

        public static int FleeTileAnticipationDistance = 64;

        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<Polyperil>(),
            ModContent.NPCType<Slugbun>()
        };

        protected override List<int> PredatorIDs => new List<int>()
        {
            ModContent.NPCType<PolypPanasea>(),
            ModContent.NPCType<SandProwler>(),
            //ModContent.NPCType<PulseRaptor>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.PolypForest;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 13;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.damage = 10;
            NPC.width = 20;
            NPC.height = 58;
            NPC.defense = 5;
            NPC.lifeMax = 150;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 5);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.15f;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<KelpieSeadragonBanner>();
            NPC.chaseable = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.KelpieSeadragon")
            });
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    Acceleration = 0.3f,
                    MaxSpeed = 3f,
                };
            }
            if (NPC.direction == 0)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            // Fall and be useless if out of water
            if (!NPC.wet)
            {
                NPC.ai[0] = 0;
                NPC.ai[1] = 0;
                NPC.velocity.X *= 0.98f;
                NPC.noGravity = false;
                NPC.rotation = MathHelper.Lerp(NPC.rotation, MathHelper.PiOver2, 0.1f);
                NPC.gfxOffY += 5;
                return;
            }
            NPC.noGravity = true;
            switch (NPC.ai[0])
            {
                // Idle AI. Mostly sits still but occasionally moves in a random direction for a bit. 
                case 0:
                    NPC.chaseable = false;
                    if (NPC.velocity.Length() < 0.1f)
                    {
                        NPC.ai[1]++;
                        // Randomly switch direction
                        if (Main.rand.NextBool(120))
                        {
                            NPC.direction *= -1;
                        }
                        // Move in a random direction towards the direction the horse is facing
                        if (NPC.ai[1] > 120 || Main.rand.NextBool(90))
                        {
                            Vector2 direction = new Vector2(NPC.direction * 30, Main.rand.Next(-30, 30));
                            direction = direction.SafeNormalize(Vector2.Zero);
                            NPC.velocity = direction * 2;
                            NPC.ai[1] = 0;
                        }
                    }
                    // Reset any rotation from aggressive AI
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.1f);
                    NPC.velocity *= 0.99f;
                    break;
                case 1:
                    currentTarget = CurrentPrey != null ? CurrentPrey : CurrentPlayer;
                    if (currentTarget == null)
                    {
                        NPC.ai[0] = 0;
                        NPC.ai[1] = 0;
                        SquishX = 0;
                        SquishY = 0;
                        break;
                    }
                    if (currentTarget is Player)
                        NPC.chaseable = true;
                    bool hasSight = Collision.CanHitLine(NPC.Center, 1, 1, target.Center, 1, 1);
                    // If the target is too far from its shooting range or a tile is in the way, move closer
                    if ((currentTarget.Distance(NPC.Center) > 300 || !hasSight) || currentTarget is NPC)
                    {
                        NPC.ai[1] = 0;

                        bool huntReady = NPC.ai[2] == 0;
                        if (huntReady)
                            NPC.ai[2] = Main.rand.Next(13, 30);

                        // With sight, just go straight at him. Without it, try to pathfind over them.
                        pathfinding.DoPathfinding(new(NPC.Center, currentTarget.Center, SunkenSeaTileValidity), forceNewTask: huntReady);
                        pathfinding.CustomIdleBehavior = () =>
                        {
                            if (currentTarget != null)
                            {
                                NPC.velocity += NPC.DirectionTo(currentTarget.Center) * pathfinding.Acceleration;

                                // Cap the speed if MaxSpeed has been surpassed.
                                if (NPC.velocity.LengthSquared() > pathfinding.MaxSpeed * pathfinding.MaxSpeed)
                                    NPC.velocity = Vector2.Normalize(NPC.velocity) * pathfinding.MaxSpeed;
                            }
                            else
                                NPC.velocity *= 0.95f;
                        };

                        NPC.ai[2]--;

                        NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.direction * MathHelper.PiOver4 / 2, 0.05f);
                    }
                    else
                    {
                        // Otherwise sit at a distance and fire projectiles
                        NPC.velocity *= 0.9f;
                        NPC.ai[1]++;
                        int fireRate = 30; // Do not change this without adjusting the frame rate as well
                        float currentTime = NPC.ai[1] % 36;
                        if (currentTime == fireRate)
                        {
                            SoundEngine.PlaySound(Sounds.CommonCalamitySounds.ExoPlasmaShootSound with { Volume = 0.2f, Pitch = 1.8f }, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 spawnPos = new Vector2(NPC.Center.X + NPC.direction * 18, NPC.position.Y + 22);
                                Vector2 projSpeed = spawnPos.DirectionTo(currentTarget.Center).SafeNormalize(Vector2.Zero) * 6;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), spawnPos, projSpeed, ModContent.ProjectileType<HorsPoisonBlast>(), 10, 0f);
                            }
                        }
                        // Squash and stretch
                        int shotTime = 24; // When to squash 
                        if (currentTime < 24)
                        {
                            SquishX = MathHelper.Lerp(SquishX, 0.9f, currentTime / shotTime);
                            SquishY = MathHelper.Lerp(SquishY, 1.05f, currentTime / shotTime);
                        }
                        else
                        {
                            SquishX = MathHelper.Lerp(SquishX, 1.35f, (currentTime - shotTime) / (fireRate - shotTime));
                            SquishY = MathHelper.Lerp(SquishY, 0.85f, (currentTime - shotTime) / (fireRate - shotTime));
                        }

                        NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.1f);
                    }
                    if (Math.Abs(NPC.velocity.X) > 0)
                    {
                        NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                    }
                    else
                    {
                        NPC.direction = NPC.Center.X > currentTarget.Center.X ? -1 : 1;
                    }
                    break;
                case 2:
                    {
                        // If the avoided entity is gone, go back to idling.
                        if (CurrentPredator == null)
                        {
                            NPC.ai[0] = 0;
                            break;
                        }

                        if (Math.Abs(NPC.velocity.X) > 0)
                        {
                            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                        }
                        else
                        {
                            NPC.direction = NPC.Center.X > CurrentPredator.Center.X ? -1 : 1;
                        }

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
                            pathfinding.DoPathfinding(new(NPC.Center, randomPathPoint, SunkenSeaTileValidity));
                        }
                        break;
                    }
            }
            NPC.spriteDirection = NPC.direction;
        }

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.wet && !NPC.IsABestiaryIconDummy)
                return;
            NPC.frameCounter++;
            if (NPC.frameCounter > 5)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0;
            }
            // Anger
            if (NPC.ai[0] == 1 && NPC.ai[1] > 0)
            {
                if (NPC.frame.Y > 12 * frameHeight || NPC.frame.Y < frameHeight * 7)
                {
                    NPC.frame.Y = frameHeight * 7;
                }
            }
            // Idle
            else
            {
                if (NPC.frame.Y > 6 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        protected override void OnPlayerDetection(Player player)
        {
            if (CurrentPrey is null && CurrentPredator is null)
            { 
                EnterAttackMode();
                currentTarget = player;
            }
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            NPC.ai[0] = 2;
            NPC.ai[1] = 0;
            currentTarget = null;
        }

        protected override void OnPreyDetection(NPC prey)
        {
            if (CurrentPredator == null)
            {
                EnterAttackMode();
                currentTarget = prey;
            }
        }

        public void EnterAttackMode()
        {
            NPC.ai[0] = 1;
            NPC.ai[1] = 0;
            SquishX = 0.9f;
            SquishY = 1.05f;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || (n == CurrentPredator || n == CurrentPrey) && Vector2.DistanceSquared(NPC.Center, n.Center) < 900f * 900f;
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 600f * 600f;
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type) || attacker.type == ModContent.NPCType<PolyperilTentacle>();

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(randomPathPoint);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZonePolypForest && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.7f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;
            Asset<Texture2D> tex = TextureAssets.Npc[Type];
            SpriteEffects fx = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 stretch = NPC.ai[0] == 1 && NPC.ai[1] > 0 ? new Vector2(SquishX, SquishY): Vector2.One;
            spriteBatch.Draw(tex.Value, NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY), NPC.frame, drawColor, NPC.rotation, new Vector2(tex.Width() / 2, tex.Height() / 2 / Main.npcFrameCount[Type]), NPC.scale * stretch, fx, 0); 
            return false;
        }
    }
}
