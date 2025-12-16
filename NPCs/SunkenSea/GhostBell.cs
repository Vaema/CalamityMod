using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Pathfinding;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace CalamityMod.NPCs.SunkenSea
{
    public class GhostBell : SunkenSeaNPC
    {
        public static Asset<Texture2D> PinkTexture;
        public static Asset<Texture2D> GreenTexture;

        public static int ElectrifyingPhaseDuration => 180;
        public static int ElectrifyingPhaseDischarge => 60;
        public static int ElectrifyingPhaseCooldown => 120;

        public ref float Phase => ref NPC.ai[0];

        public ref float Variant => ref NPC.ai[1];

        public ref float Timer => ref NPC.ai[3];

        public enum PhaseType
        {
            Idle = 0,
            Angry = 1,
            Electrifying = 2
        }

        public enum JellyColor
        {
            Blue = 0,
            Green = 1,
            Pink = 2
        }

        public List<List<VerletSimulatedSegment>> tentacles = [];

        protected override List<int> PreyIDs =>
        [
            ModContent.NPCType<PolypPanasea>(),
            ModContent.NPCType<SeaMinnow>(),
            ModContent.NPCType<SeaMinnowGold>(),
            ModContent.NPCType<AlphaSeaMinnow>(),
            ModContent.NPCType<AlphaSeaMinnowGold>(),
            ModContent.NPCType<PrismaticGuppy>(),
        ];

        protected override List<int> PredatorIDs => [];

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.GleamingBurrows | SunkenSeaBiomeFlags.PolypForest;

        public override void Load()
        {
            GreenTexture = ModContent.Request<Texture2D>(Texture + "Green");
            PinkTexture = ModContent.Request<Texture2D>(Texture + "Pink");
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 1;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 25;
            NPC.width = 74;
            NPC.height = 58;
            NPC.defense = 0;
            NPC.lifeMax = 120;
            NPC.knockBackResist = 0f;
            NPC.alpha = 100;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.HitSound = SoundID.NPCHit25;
            NPC.DeathSound = SoundID.NPCDeath28;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<GhostBellBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.GhostBell")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest();
            Variant = Main.rand.Next(0, 3);
            if (Phase == (int)PhaseType.Idle)
            {
                // 1-3 in the Polyp Forest, 3-4 in the burrows
                int jellyAmt = Main.player[NPC.target].Calamity().ZonePolypForest ? Main.rand.Next(2, 4) : Main.rand.Next(3, 5);
                // A swarm of babies spawns if the jelly is in the Polyp Forest
                if (Main.player[NPC.target].Calamity().ZonePolypForest)
                {
                    for (int i = 0; i < jellyAmt; i++)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int bebe = NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.Center.X + Main.rand.NextFloat(1f, 2f)), (int)(NPC.Center.Y + Main.rand.NextFloat(1f, 2f)), ModContent.NPCType<BabyGhostBell>(), ai0: -1);
                            Main.npc[bebe].rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
                        }
                    }
                }
                // Spawn more adults
                for (int i = 0; i < jellyAmt; i++)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.Center.X + Main.rand.NextFloat(1f, 2f)), (int)(NPC.Center.Y + Main.rand.NextFloat(1f, 2f)), Type, ai0: -1, ai2: Main.rand.Next(0, 60));
                    }
                }
            }
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(this);
                Acceleration = 0.1f;
                MaxSpeed = 1f;
            }
            CreateTentacles();
            NPC.ai[2]++;

            // Lite
            var lightColor = Variant switch
            {
                (int)JellyColor.Pink => Color.LightPink,
                (int)JellyColor.Green => Color.MediumSpringGreen,
                _ => Color.LightBlue,
            };
            Lighting.AddLight(NPC.Center, (lightColor.R - NPC.alpha) * 1f / 255f, (lightColor.G - NPC.alpha) * 1f / 255f, (lightColor.B - NPC.alpha) * 1f / 255f);

            NPC.chaseable = Phase > 0;
            NPC.noGravity = NPC.wet;

            NPC.netUpdate = true;
            NPC.netSpam = 0;

            Entity target = CurrentPrey;
            if (NPC.target > -1 && (int)Phase >= (int)PhaseType.Angry)
            {
                target = Main.player[NPC.target];
            }

            // Ghost Bells are usually in groups, so make them not overlap
            float SAImovement = 0.05f;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC otherFish = Main.npc[k];
                // Short circuits to make the loop as fast as possible
                if (!otherFish.active || k == NPC.whoAmI || otherFish.type != ModContent.NPCType<GhostBell>())
                    continue;

                float taxicabDist = Math.Abs(NPC.position.X - otherFish.position.X) + Math.Abs(NPC.position.Y - otherFish.position.Y);
                if (taxicabDist < NPC.width * 2f)
                {
                    if (NPC.position.X < otherFish.position.X)
                        NPC.velocity.X -= SAImovement;
                    else
                        NPC.velocity.X += SAImovement;

                    if (NPC.position.Y < otherFish.position.Y)
                        NPC.velocity.Y -= SAImovement;
                    else
                        NPC.velocity.Y += SAImovement;
                }
            }

            if (Phase <= (int)PhaseType.Idle)
            {
                NPC.velocity *= 0.8f;
            }

            if (target == null || !target.active || target.Distance(NPC.Center) > 1000)
            {
                Timer = 0;
                Phase = (int)PhaseType.Idle;
                NPC.netUpdate = true;
                return;
            }

            // Create an electrifying aura
            if (Phase == (int)PhaseType.Electrifying)
            {
                // Slow down
                NPC.velocity *= 0.9f;
                pathfinding.ClearResults();
                Timer++;
                // Create the aura
                if (NPC.ai[3] == 1)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GhostBellShock>(), 12, 0, ai1: NPC.whoAmI + 1);
                    }
                }
                // Reset
                if (Timer >= ElectrifyingPhaseDuration)
                {
                    Timer = -ElectrifyingPhaseCooldown;
                    Phase = (int)PhaseType.Angry;
                    NPC.netUpdate = true;
                }
            }
            // Move towards target
            else if (target != null)
            {
                pathfinding.DoPathfinding(new PathfindingParameters(this, NPC.Center, target.Center, SunkenSeaTileValiditySizeless));
                if (pathfinding.Path.Count > 0)
                    if (pathfinding.Path[^1].Distance(target.Center) > 300)
                    {
                        pathfinding.ClearResults();
                    }
                if (Phase == (int)PhaseType.Angry)
                {
                    if (target is Player && NPC.Distance(target.Center) < 300)
                    {
                        Timer++;
                    }
                    if (Timer > 120)
                    {
                        Timer = 0;
                        Phase = (int)PhaseType.Electrifying;
                        NPC.netUpdate = true;
                    }
                }
            }
        }

        public void CreateTentacles()
        {
            if (tentacles == null || tentacles.Count < 6)
            {
                // switches between tentacle indexes to determine how many segments should be on each
                // aims for a more squid-like distribution, i dont know if it applies to jellyfish too

                // an embarrassing amount of time was spent figuring out these numbers
                // i forgot they start at 0..
                int segmentCount = tentacles.Count switch
                {
                    0 or 5 => 9,
                    1 or 4 => 12,
                    _ => 8
                };
                List<VerletSimulatedSegment> segments = new List<VerletSimulatedSegment>(segmentCount);
                for (int i = 0; i < segmentCount; i++)
                {
                    VerletSimulatedSegment segment = new VerletSimulatedSegment(NPC.Center - Vector2.UnitY * i * 5);
                    segments.Add(segment);
                }
                segments[0].locked = true;
                tentacles.Add(segments);
            }
        }

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            GetPissed(player.whoAmI);
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.friendly && projectile.owner > -1)
            {
                GetPissed(projectile.owner);
            }
        }

        public void GetPissed(int player)
        {
            if (Phase <= (int)PhaseType.Idle)
            {
                Phase = (int)PhaseType.Angry;
                NPC.netUpdate = true;
            }
            NPC.target = player;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.type != Type)
                    continue;
                if (n.Distance(NPC.Center) > 400)
                    continue;
                if (n.ModNPC<GhostBell>().Phase <= (int)PhaseType.Idle)
                    n.ModNPC<GhostBell>().GetPissed(player);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZonePolypForest)
                    return SpawnCondition.CaveJellyfish.Chance * 0.5f;
                if (spawnInfo.Player.Calamity().ZoneGleamingBurrows)
                    return SpawnCondition.CaveJellyfish.Chance * 0.3f;

            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            // For bestiary animation
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.ai[2]++;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Underlying tentacle colour based on parent colour
            Color baseCol = Variant switch
            { // all of these colours are from the sprites but slightly darkened and desaturated
                (int)JellyColor.Green => new Color(29, 54, 39), // 27, 61, 40
                (int)JellyColor.Pink => new Color(79, 39, 53),  // 97, 38, 60
                _ => new Color(34, 50, 89)                     // 36, 55, 114
            };
            // Accent color is also based on parent color
            Color col = Variant switch
            { // these are also directly from the sprites, with no modifications; similar in luminance for visual consistency
                (int)JellyColor.Green => new Color(59, 215, 194),
                (int)JellyColor.Pink => new Color(255, 145, 238),
                _ => new Color(84, 215, 254)
            };

            Vector2 drawOffset = Vector2.Zero; // An added offset that gives the jellyfish a visual bobbing movement

            int fullTime = 180; // How long the bob animation lasts
            int localTimer = (int)(NPC.ai[2] % fullTime);
            float goUp = (int)(fullTime * 0.4f); // When should the jellyfish jet upwards
            int height = 30; // The vertical range the jellyfish moves

            int endElectricity = 60;

            Vector2 squash = new Vector2(1.1f, 0.9f);
            Vector2 stretch = new Vector2(0.8f, 1.2f);
            Vector2 finalScale;

            // Move up and down while squashing and stretching
            if (localTimer < goUp)
            {
                drawOffset.Y += MathHelper.Lerp(height, 0, CalamityUtils.CircOutEasing(Utils.GetLerpValue(0, goUp - 1, localTimer, true), 1));
                finalScale = Vector2.Lerp(stretch, squash, CalamityUtils.CircOutEasing(Utils.GetLerpValue(0, goUp - 1, localTimer, true), 1));
            }
            else
            {
                drawOffset.Y += MathHelper.Lerp(0, height, CalamityUtils.SineInEasing(Utils.GetLerpValue(goUp, fullTime - 1, localTimer, true), 1));
                finalScale = Vector2.Lerp(squash, stretch, CalamityUtils.SineInEasing(Utils.GetLerpValue(goUp, fullTime - 1, localTimer, true), 1));
            }
            // Use different squash n stretch when electrifying
            if (Phase == (int)PhaseType.Electrifying)
            {
                squash = new Vector2(1.4f, 0.8f);
                if (Timer < (ElectrifyingPhaseDuration - 80))
                    finalScale = Vector2.Lerp(Vector2.One, squash, CalamityUtils.CircOutEasing(Utils.GetLerpValue(0, ElectrifyingPhaseDischarge, Timer, true), 1));
                else
                    finalScale = Vector2.Lerp(squash, Vector2.One, CalamityUtils.SineInEasing(Utils.GetLerpValue(endElectricity, ElectrifyingPhaseDuration, Timer, true), 1));
            }
            // Keeps the hitbox centered
            drawOffset.Y -= height / 2;

            if (NPC.IsABestiaryIconDummy)
            {
                drawOffset.Y -= 20;
            }

            CreateTentacles();

            // Update the chains
            for (int i = 0; i < tentacles.Count; i++)
            {
                List<VerletSimulatedSegment> segments = tentacles[i];

                segments[0].oldPosition = segments[0].position;
                segments[0].position = NPC.Center + new Vector2(MathHelper.Lerp(-20, 20, (i + 1) / (float)tentacles.Count), 10) + drawOffset;

                // While electrifying, flail tentacles around frantically
                if (Phase == (int)PhaseType.Electrifying && Timer % 5 == 0 && Timer < endElectricity)
                {
                    for (int j = 0; j < segments.Count; j++)
                    {
                        int randomness = (int)MathHelper.Lerp(6, 18, (1 + j) / segments.Count);
                        segments[j].oldPosition = segments[j].position;
                        segments[j].position += Main.rand.NextVector2Circular(randomness, randomness);
                    }
                }
                tentacles[i] = VerletSimulatedSegment.SimpleSimulation(segments, 5, loops: 1, gravity: 0.6f);
            }

            // Draw tentacle chains
            for (int t = 0; t < tentacles.Count; t++)
            {
                List<VerletSimulatedSegment> segments = tentacles[t];
                for (int i = 0; i < segments.Count - 1; i++)
                {
                    VerletSimulatedSegment seg = segments[i];
                    float dist = i > 0 ? Vector2.Distance(seg.position, segments[i - 1].position) : 0;
                    if (dist <= 2)
                        dist = 2;
                    dist += 4;
                    if (i == segments.Count - 1)
                    {
                        dist = Vector2.Distance(seg.position, NPC.Center) + 2;
                    }
                    float rot = 0f;
                    if (i > 0)
                        rot = seg.position.DirectionTo(segments[i - 1].position).ToRotation();

                    // slight gradient to a lighter colour the higher the segment index
                    Color finalColor = baseCol;
                    finalColor *= 1 + (i * 0.15f);

                    // Last few segments are way brighter
                    if (i > segments.Count - 3)
                    {
                        finalColor = col * (0.05f + segments.Count * 0.1f);
                    }

                    // Color eases in and out while angry
                    if (Phase == (int)PhaseType.Angry || NPC.IsABestiaryIconDummy)
                    {
                        float mod = NPC.ai[2] % 30;
                        if (mod <= 10)
                            finalColor = Color.Lerp(finalColor, finalColor * 2, Utils.GetLerpValue(0, 10, mod, true));
                        else if (mod >= 20)
                            finalColor = Color.Lerp(finalColor * 2, finalColor, Utils.GetLerpValue(20, 30, mod, true));
                        else
                            finalColor *= 2;
                    }
                    // Color is at full brightness while electrifying
                    else if (Phase == (int)PhaseType.Electrifying)
                        finalColor *= 2;
                    // give the segments variable width (and an offset equal to half of the variance) to create more visual interest
                    // a side effect of this makes it look wiggly like some jellyfish tentacles are
                    SpriteEffects dir = NPC.Center.X > NPC.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(seg.position.X - i * 0.1f, seg.position.Y) - screenPos, new Rectangle(0, 0, (int)(3 + i * 0.2), 8), finalColor, rot + MathHelper.PiOver2, new Vector2(4, 4), new Vector2(1, dist / 8), dir, 0);
                }
            }

            Texture2D tex = Variant switch
            {
                (int)JellyColor.Pink => PinkTexture.Value,
                (int)JellyColor.Green => GreenTexture.Value,
                _ => TextureAssets.Npc[Type].Value
            };

            Main.EntitySpriteDraw(tex, NPC.Center - screenPos + drawOffset, null, Color.White, NPC.rotation, tex.Size() / 2, finalScale * NPC.scale, 0);
            return false;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
                target.AddBuff(ModContent.BuffType<StaticDischarge>(), 120);
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
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.JellyfishNecklace, 25);
            LeadingConditionRule postDS = npcLoot.DefineConditionalDropSet(DropHelper.PostDS());
            postDS.Add(ModContent.ItemType<VoltaicJelly>(), 5);
        }
    }
}
