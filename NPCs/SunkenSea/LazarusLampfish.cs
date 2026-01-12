using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Enums;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Pathfinding;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class LazarusLampfish : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Attacking = 1,
            Fleeing = 2
        }

        public static Asset<Texture2D> cooldownTexture;
        public static Asset<Texture2D> attackTexture;

        public Vector2 randomPathPoint;

        public Entity currentTarget;

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 100;
        public static int IdleMaxPathDistance = 800;

        public static int FleeTileAnticipationDistance = 64;

        public ref float CurrentPhase => ref NPC.ai[0];
        public ref float Timer => ref NPC.ai[1];

        protected override List<int> PreyIDs => new List<int>()
        {
            ModContent.NPCType<BabyGhostBell>(),
            ModContent.NPCType<PrismaticGuppy>(),
            ModContent.NPCType<SeaMinnow>(),
            ModContent.NPCType<SeaMinnowGold>(),
            ModContent.NPCType<AlphaSeaMinnow>(),
            ModContent.NPCType<AlphaSeaMinnowGold>(),
        };

        public static List<int> PredatorList => new List<int>()
        {
            ModContent.NPCType<GhostBell>(),
        };

        protected override List<int> PredatorIDs => PredatorList;

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.GleamingBurrows | SunkenSeaBiomeFlags.ClamDen;

        public override void Load()
        {
            cooldownTexture = ModContent.Request<Texture2D>(Texture + "Cooldown");
            attackTexture = ModContent.Request<Texture2D>(Texture + "Attack");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.damage = 40;
            NPC.width = 50;
            NPC.height = 30;
            NPC.defense = 12;
            NPC.lifeMax = 400;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.1f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<LazarusLampfishBanner>();
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
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.LazarusLampfish")
            });
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(this);
                Acceleration = 0.2f;
                MaxSpeed = 3f;
            }
            if (NPC.direction == 0)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            // Fall and be useless if out of water
            if (!NPC.wet)
            {
                CurrentPhase = (int)PhaseType.Idle;
                Timer = 0;
                NPC.velocity.X *= 0.98f;
                NPC.noGravity = false;
                return;
            }
            NPC.noGravity = true;
            switch (CurrentPhase)
            {
                // Idle AI. Mostly sits still but occasionally moves in a random direction for a bit. 
                case (int)PhaseType.Idle:
                    NPC.chaseable = false;
                    NPC.ai[2]--;
                    if (CurrentPredator != null || CurrentPlayer != null)
                    {
                        EnterAttackMode();
                    }
                    if (CurrentPrey != null)
                    {
                        bool huntReady = NPC.ai[2] == 0;
                        if (huntReady)
                            NPC.ai[2] = Main.rand.Next(13, 30);

                        // With sight, just go straight at him. Without it, try to pathfind over them.
                        pathfinding.DoPathfinding(new(this, NPC.Center, CurrentPrey.Center, SunkenSeaTileValidity), forceNewTask: huntReady);
                    }
                    else
                    {
                        pathfinding.DoPathfinding(new(this, NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));
                    }
                    break;
                case (int)PhaseType.Attacking:
                    currentTarget = CurrentPredator != null ? CurrentPredator : CurrentPlayer;
                    if (currentTarget == null)
                    {
                        CurrentPhase = (int)PhaseType.Idle;
                        Timer = 0;
                        NPC.ai[2] = 0;
                        MaxSpeed = 3;
                        Acceleration = 0.2f;
                        break;
                    }
                    MaxSpeed = 8;
                    Acceleration = 0.3f;
                    NPC.chaseable = true;
                    bool hasSight = Collision.CanHitLine(NPC.Center, 1, 1, currentTarget.Center, 1, 1);
                    // If the target is too far from its shooting range or a tile is in the way, move closer
                    if (currentTarget.Distance(NPC.Center) > 200 || !hasSight)
                    {
                        bool restart = NPC.ai[2] == 0;
                        if (restart)
                            NPC.ai[2] = Main.rand.Next(13, 30);

                        // With sight, just go straight at him. Without it, try to pathfind over them.
                        pathfinding.DoPathfinding(new(this, NPC.Center, currentTarget.Center, SunkenSeaTileValidity), forceNewTask: restart);

                        NPC.ai[2]--;
                    }
                    // If close enough, start the timer for the flash
                    else if (Timer == 0)
                    {
                        Timer = 1;
                    }
                    // Flash!
                    if (Timer >= 1)
                    {
                        NPC.velocity *= 0.92f;
                        Timer++;
                        if (Timer == 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + (NPC.rotation - (NPC.spriteDirection == 1 ? 0 : MathHelper.Pi)).ToRotationVector2() * 40, Vector2.Zero, ModContent.ProjectileType<AnglerFlash>(), 20, 1);
                            }
                            SoundEngine.PlaySound(SoundID.NPCDeath7 with { Pitch = 0.4f }, NPC.Center);
                        }
                        if (Timer >= 120)
                        {
                            Timer = 0;
                            NPC.ai[2] = 0;
                            CurrentPhase = (int)PhaseType.Fleeing;
                        }
                    }
                    break;
                case (int)PhaseType.Fleeing:
                    {
                        // If the avoided entity is gone, go back to idling.
                        currentTarget = CurrentPredator != null ? CurrentPredator : CurrentPlayer;
                        if (currentTarget == null || Timer > 300)
                        {
                            CurrentPhase = currentTarget != null ? (int)PhaseType.Attacking : (int)PhaseType.Idle;
                            Timer = 0;
                            MaxSpeed = 3;
                            Acceleration = 0.2f;
                            NPC.ai[2] = currentTarget != null ? 0 : 120;
                            break;
                        }
                        if (Timer == 0)
                        {
                            NPC.spriteDirection = NPC.direction *= -1;
                        }
                        Timer++;
                        MaxSpeed = 8;
                        Acceleration = 0.4f;

                        if (Math.Abs(NPC.velocity.X) > 0)
                        {
                            NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
                        }
                        else
                        {
                            NPC.direction = NPC.Center.X > currentTarget.Center.X ? -1 : 1;
                        }

                        // While it doesn't have any obstacles in front of it, run away in a straight line.
                        // Try to manuever if there are any obstacles.
                        if (!Main.tile[(NPC.Center + NPC.DirectionFrom(currentTarget.Center) * FleeTileAnticipationDistance).ToTileCoordinates()].IsTileSolid())
                        {
                            NPC.velocity += NPC.DirectionFrom(currentTarget.Center) * Acceleration;
                            pathfinding.ClearResults();

                            // Cap the speed if MaxSpeed has been surpassed.
                            if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                                NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
                        }
                        else
                        {
                            float distanceFromAvoided = Vector2.Distance(NPC.Center, currentTarget.Center);
                            randomPathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                            NPC.netUpdate = true;
                            pathfinding.DoPathfinding(new(this, NPC.Center, randomPathPoint, SunkenSeaTileValidity));
                        }
                        break;
                    }
            }
            if (!(Timer >= 1 && CurrentPhase == (int)PhaseType.Attacking) && !(Timer < 3 && CurrentPhase == (int)PhaseType.Fleeing))
            {
                int dir = NPC.velocity.X.DirectionalSign();
                NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
                NPC.spriteDirection = NPC.direction = dir;
            }
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
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight)
            {
                NPC.frame.Y = 0;
            }
        }

        protected override void OnPlayerDetection(Player player)
        {
            EnterAttackMode();
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            EnterAttackMode();
        }

        public void EnterAttackMode()
        {
            CurrentPhase = (int)PhaseType.Attacking;
            Timer = 0;
            NPC.ai[2] = 0;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || (n == CurrentPredator || n == CurrentPrey) && Vector2.DistanceSquared(NPC.Center, n.Center) < 700f * 700f;
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || (p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 600f * 600f);
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void AwaitingPathBehavior()
        {
            bool hasSight = false;
            if (currentTarget != null)
                hasSight = Collision.CanHitLine(NPC.Center, 1, 1, currentTarget.Center, 1, 1);
            if (CurrentPhase == (int)PhaseType.Idle && CurrentPrey != null || CurrentPhase == (int)PhaseType.Attacking && currentTarget.Distance(NPC.Center) > 200 || !hasSight)
            {
                if (CurrentPrey != null)
                {
                    NPC.velocity += NPC.DirectionTo(CurrentPrey.Center) * Acceleration;

                    // Cap the speed if MaxSpeed has been surpassed.
                    if (NPC.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;
                }
                else
                    NPC.velocity *= 0.95f;
            }
            else
                base.AwaitingPathBehavior();
        }

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
            if ((spawnInfo.Player.Calamity().ZoneGleamingBurrows || spawnInfo.Player.Calamity().ZoneClamDen) && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.3f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
            CalamityUtils.SpawnGores(NPC, "LazarusLampfish", 3);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;
            Asset<Texture2D> tex = TextureAssets.Npc[Type];
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(tex.Width() / 2, tex.Height() / 2 / Main.npcFrameCount[Type]);

            if (CurrentPhase == (int)PhaseType.Attacking && Timer > 0)
            {
                tex = attackTexture;
                frame = tex.Frame(1, 11, 0, (int)MathHelper.Lerp(0, 10, Utils.GetLerpValue(0, 120, Timer, true)));
                origin = new Vector2(tex.Width() / 2, tex.Height() / 22);
            }
            if (CurrentPhase == (int)PhaseType.Fleeing)
            {
                tex = cooldownTexture;
                frame = tex.Frame(1, 4, 0, (int)MathHelper.Lerp(0, 3, Utils.GetLerpValue(0, 24, Timer % 24, true)));
                origin = new Vector2(tex.Width() / 2, tex.Height() / 8);
            }
            SpriteEffects fx = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(tex.Value, NPC.Center - Main.screenPosition + new Vector2(0, NPC.gfxOffY), frame, drawColor, NPC.rotation, origin, NPC.scale, fx, 0);
            return false;
        }
    }
}
