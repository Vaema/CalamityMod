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
using CalamityMod.Enums;
using System.Collections.Generic;
using Steamworks;
using System.IO;

namespace CalamityMod.NPCs.SunkenSea
{
    public class AlphaSeaMinnow : SunkenSeaNPC
    {
        public enum PhaseType
        {
            Idle = 0,
            Flee = 1
        }

        public ref float CurrentBehavior => ref NPC.ai[1];

        public static int IdleRandomMovementUnlikeliness = 250;
        public static int IdleMinPathDistance = 100;
        public static int IdleMaxPathDistance = 1200;

        public static int FleeTileAnticipationDistance = 64;

        public Vector2 randomPathPoint;

        public bool instantiated = false;

        protected override List<int> PreyIDs => new List<int>();

        protected override List<int> PredatorIDs => new List<int>() {
            ModContent.NPCType<Sharkoon>(),
            ModContent.NPCType<Polyperil>(),
            ModContent.NPCType<PolyperilTentacle>(),
            ModContent.NPCType<LazarusLampfish>(),
            ModContent.NPCType<GhostBell>()
        };

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.GleamingBurrows;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.npcSlots = 0.5f;
            NPC.noGravity = true;
            NPC.damage = 0;
            NPC.width = 40;
            NPC.height = 32;
            NPC.defense = 0;
            NPC.lifeMax = 5;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            //Banner = NPC.type;
            //BannerItem = ModContent.ItemType<AlphaSeaMinnowBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            NPC.chaseable = false;
            NPC.catchItem = (short)ModContent.ItemType<AlphaSeaMinnowItem>();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.AlphaSeaMinnow")
            });
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Alphas released by the player do not spawn a shoal of minnows
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Player)
            {
                return;
            }
            NPC.TargetClosest(false);
            // Spawn a shoal of minnows
            int fishCount = Main.rand.Next(2, 7);
            // More spawn in the Radiant Reefs
            if (NPC.HasPlayerTarget)
            {
                if (Main.player[NPC.target].Calamity().ZoneRadiantReefs)
                    fishCount += 3;
            }
            for (int i = 0; i < fishCount; i++)
            {
                int goldchance = NPC.type == ModContent.NPCType<AlphaSeaMinnowGold>() ? 20 : 50;
                int minnowtype = Main.rand.NextBool(goldchance) ? ModContent.NPCType<SeaMinnowGold>() : ModContent.NPCType<SeaMinnow>();
                int n = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, minnowtype);
                Main.npc[n].ai[2] = NPC.whoAmI; // makes the spawned minnow recognize this one as the alpha
            }
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    Acceleration = 0.6f,
                    MaxSpeed = 4f,
                };
            }
            if (NPC.wet)
            {
                switch (CurrentBehavior)
                {
                    case (int)PhaseType.Idle:
                        IdleBehavior();
                        break;
                    case (int)PhaseType.Flee:
                        FleeBehavior();
                        break;
                }
            }
            else
            {
                BeachedBehavior();
            }
            int dir = NPC.velocity.X.DirectionalSign();
            NPC.rotation = NPC.velocity.ToRotation() + (dir == 1 ? 0 : MathHelper.Pi);
            NPC.spriteDirection = NPC.direction = dir;
            if (NPC.type == ModContent.NPCType<AlphaSeaMinnowGold>())
            {
                NPC.ProduceGoldCritterDust();
            }
        }

        public void IdleBehavior()
        {
            // At random, the mob will choose a random nearby point and pathfind there.
            pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.Next(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));
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
            Point lookAheadPosition = (NPC.Center + NPC.DirectionFrom(CurrentPredator.Center) * FleeTileAnticipationDistance).ToTileCoordinates();
            if (!CalamityUtils.ParanoidTileRetrieval(lookAheadPosition.X, lookAheadPosition.Y).IsTileSolid())
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
            NPC.ai[0] = 1f;
        }

        protected override bool NPCSearchFilter(NPC n)
        {
            return base.NPCSearchFilter(n) || n == CurrentPredator && Vector2.DistanceSquared(NPC.Center, n.Center) < 960f * 960f;
        }

        protected override bool PlayerSearchFilter(Player p)
        {
            return base.PlayerSearchFilter(p) || p == CurrentPlayer && Vector2.DistanceSquared(NPC.Center, p.Center) < 960f * 960f;
        }
        protected override void OnPredatorDetection(NPC predator)
        {
            CurrentBehavior = (int)PhaseType.Flee;
        }

        public override void ModifyTypeName(ref string typeName)
        {
            // Holy mackerel is that an Ultrakill reference!?!?
            // (I have barely played the game as of writing this _ YuH)
            if (Main.zenithWorld)
            {
                typeName = CalamityUtils.GetTextValue("NPCs.MinnowPrime");
            }
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

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneRadiantReefs)
                    return SpawnCondition.CaveJellyfish.Chance * 0.6f;
                if (spawnInfo.Player.Calamity().ZoneGleamingBurrows)
                    return SpawnCondition.CaveJellyfish.Chance * 0.3f;

            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 68, hit.HitDirection, -1f, 0, default, 1f);
            }
            CalamityUtils.SpawnGores(NPC, "AlphaSeaMinnow", 2);
        }
        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(randomPathPoint);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            randomPathPoint = reader.ReadVector2();
        }
    }
    public class AlphaSeaMinnowGold : AlphaSeaMinnow
    {
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.rarity = 3;
            NPC.catchItem = ModContent.ItemType<AlphaSeaMinnowGoldItem>();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneRadiantReefs)
                    return SpawnCondition.CaveJellyfish.Chance * 0.2f;
                if (spawnInfo.Player.Calamity().ZoneGleamingBurrows)
                    return SpawnCondition.CaveJellyfish.Chance * 0.05f;

            }
            return 0f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f, 0, default, 1f);
            }
        }
    }
}
