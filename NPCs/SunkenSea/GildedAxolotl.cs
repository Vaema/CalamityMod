using CalamityMod.BiomeManagers;
using CalamityMod.Items.Critters;
using CalamityMod.Items.Placeables.Banners;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using CalamityMod.Enums;
using System.Collections.Generic;
using CalamityMod.Buffs.StatBuffs;
using System;
using CalamityMod.Dusts;

namespace CalamityMod.NPCs.SunkenSea
{
    public class GildedAxolotl : SunkenSeaNPC
    {
        private enum AnimationState { Water, Land }
        private AnimationState Animation
        {
            get => (AnimationState)NPC.ai[1];
            set
            {
                if (value != Animation)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }

                NPC.ai[1] = (float)value;
            }
        }

        private enum PhaseType { Idle = 0, Flee = 1, Hunt = 2 }
        public ref float CurrentBehavior => ref NPC.ai[0];
        public ref float GroundMovementTimer => ref NPC.ai[2];

        public Vector2 targetPoint;

        public int IdleMinPathDistance = 400;
        public int IdleMaxPathDistance = 1800;
        public float FleeMaxSpeed = 8;
        public float HuntMaxSpeed = 6;
        public float IdleMaxSpeed = 3.2f;

        // Delays for effects triggering again after being executed
        private int FleeFXCooldown = 300;
        public int FleeTimer = 0;
        private int PassiveSoundCooldown = 240;
        public int PassiveSoundTimer = 0;
        private int NoticeHeldItemSoundCooldown = 180;
        public int NoticeHeldItemSoundTimer = 0;

        public bool PathfindToPlayer = false;

        public float auraFade = 0;

        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);
        public override bool CanHitNPC(NPC target) => PreyIDs.Contains(target.type);
        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public static readonly SoundStyle noticedBaitSound = new("CalamityMod/Sounds/Custom/GildedAxolotlNeuronActivation");
        public static readonly SoundStyle ambientNoise = new("CalamityMod/Sounds/Custom/GildedAxolotlVocalStim", 2);
        public static readonly SoundStyle alert = new("CalamityMod/Sounds/Custom/GildedAxolotlAlert");

        #region SunkenSea Fields

        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs;
        protected override List<int> PreyIDs => new()
        {
            ModContent.NPCType<Probesnout>(),
            ModContent.NPCType<Slugbun>(),
            ModContent.NPCType<SeaMinnow>(),
            ModContent.NPCType<SeaMinnowGold>(),
            ModContent.NPCType<AlphaSeaMinnow>(),
            ModContent.NPCType<AlphaSeaMinnowGold>()
        };

        protected override List<int> PredatorIDs => new()
        {
            ModContent.NPCType<Sharkoon>(),
            ModContent.NPCType<Polyperil>(),
            // ModContent.NPCType<CrestedStalker>(),
            // ModContent.NPCType<Hermititan>()
        };

        #endregion

        protected List<int> ItemsOfInterest => new()
        {
            ModContent.ItemType<ProbesnoutItem>(),
            ModContent.ItemType<ProbesnoutGoldItem>(),
            ModContent.ItemType<SlugbunItem>(),
            ModContent.ItemType<SlugbunBurrowsItem>(),
            ModContent.ItemType<SlugbunPolypItem>(),
            ModContent.ItemType<SlugbunRadiantItem>(),
            ModContent.ItemType<SeaMinnowItem>(),
            ModContent.ItemType<SeaMinnowGoldItem>(),
            ModContent.ItemType<AlphaSeaMinnowItem>(),
            ModContent.ItemType<AlphaSeaMinnowGoldItem>()
        };

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.npcFrameCount[Type] = 7;
            NPCID.Sets.CountsAsCritter[Type] = true;
            Main.npcCatchable[Type] = true;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.aiStyle = -1;
            NPC.width = 84;
            NPC.height = 42;

            NPC.damage = 7;
            NPC.defense = 7;
            NPC.lifeMax = 77;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.GravityIgnoresLiquid = true;
            NPC.chaseable = false;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath9;

            Banner = NPC.type;
            BannerItem = ModContent.ItemType<GildedAxolotlBanner>();

            NPC.catchItem = ModContent.ItemType<GildedAxolotlItem>();

            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToElectricity = false;
            NPC.Calamity().VulnerableToWater = false;
            NPC.Calamity().VulnerableToCold = true;

            SpawnModBiomes = new int[1] { ModContent.GetInstance<RadiantReefsBiome>().Type };
        }

        #region Syncing

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.ai[0] = reader.ReadSingle(); // CurrentBehavior
            NPC.ai[1] = reader.ReadSingle(); // Anim state
            NPC.ai[2] = reader.ReadSingle(); // Ground movement timer
            targetPoint = reader.ReadVector2(); // For pathfinding points
            auraFade = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.ai[0]); // CurrentBehavior
            writer.Write(NPC.ai[1]); // Anim state
            writer.Write(NPC.ai[2]); // Ground movement timer
            writer.WriteVector2(targetPoint); // For pathfinding points
            writer.Write(auraFade);
        }

        #endregion

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.GildedAxolotl")
            });
        }

        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    Acceleration = 0.4f,
                    MaxSpeed = IdleMaxSpeed,
                    MinimumPointDistance = 60f
                };
            }

            // Find nearby players
            NPC.TargetClosest(false);
            Player nearestPlayer = null;
            float nearestPlayerDistance = 10000f;

            float detectionRange = 700f;
            float buffRange = 290f;

            Player nearestPlayerForVisuals = null;
            float nearestPlayerDistanceForVisuals = 10000f;

            // Handle aura buff for any nearby player
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                float distance = Vector2.Distance(NPC.Center, p.Center);

                if (distance < buffRange && distance < nearestPlayerDistanceForVisuals)
                {
                    nearestPlayerForVisuals = p;
                    nearestPlayerDistanceForVisuals = distance;
                }

                if (distance < buffRange)
                {
                    p.AddBuff(ModContent.BuffType<FortunesFavor>(), 60, true);
                }

                // Track the nearest player holding a target item
                if (distance < detectionRange && ItemsOfInterest.Contains(p.HeldItem.type))
                {
                    if (distance < nearestPlayerDistance)
                    {
                        nearestPlayer = p;
                        nearestPlayerDistance = distance;
                    }
                }
            }

            // Handle aura vfx
            float distanceForVisual = nearestPlayerForVisuals != null ? nearestPlayerDistanceForVisuals : buffRange;
            float maxDistance = buffRange * 1.5f;
            float minDistance = 120f;
            auraFade = Utils.GetLerpValue(maxDistance, minDistance, distanceForVisual, clamped: true);

            // Allow NPCs to be buffed while in the aura, including this npc.
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC targetNPC = Main.npc[i];
                if (!targetNPC.active)
                    continue;

                float distance = Vector2.Distance(NPC.Center, targetNPC.Center);

                if (distance < buffRange)
                {
                    targetNPC.AddBuff(ModContent.BuffType<FortunesFavor>(), 60, true);
                }
            }

            // Handle dust which dynamically changes alpha based on distance and time
            if (auraFade > 0f)
            {
                float pulse = 0.6f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
                int baseAlpha = (int)(255f * (1f - auraFade * 0.5f * pulse));
                if (baseAlpha < 0) baseAlpha = 0;
                if (baseAlpha > 255) baseAlpha = 255;


                int innerDustCount = Main.rand.Next(0, 2);

                for (int i = 0; i < innerDustCount; i++)
                {
                    float areaSize = 255f;
                    Vector2 spawnSpot = NPC.Center + Main.rand.NextVector2Circular(areaSize, areaSize);

                    Dust dust = Dust.NewDustPerfect(spawnSpot, ModContent.DustType<LightDust>());
                    dust.scale = Main.rand.NextFloat(0.6f, 1.1f);
                    dust.noGravity = true;
                    dust.alpha = baseAlpha;
                    dust.color = (Main.rand.NextBool(2) ? Color.Silver : Color.Gold) * 2f;
                    dust.noLight = true;
                    dust.noLightEmittence = true;
                    dust.velocity *= 0.05f;
                }
            }

            // When detecting a player holding of one of the Axolotl's prey
            if (nearestPlayer != null)
            {
                if (CurrentBehavior == (float)PhaseType.Idle)
                {
                    if (NoticeHeldItemSoundTimer <= 0)
                    {
                        SoundEngine.PlaySound(noticedBaitSound with { Volume = 0.7f, Pitch = 0.1f, PitchVariance = 0.1f }, NPC.Center);
                        NoticeHeldItemSoundTimer = NoticeHeldItemSoundCooldown;
                    }

                    CurrentBehavior = (float)PhaseType.Hunt;
                    PathfindToPlayer = true;
                    NPC.netUpdate = true;
                    pathfinding.ClearResults();
                }
            }

            else
            {
                if (CurrentBehavior == (float)PhaseType.Hunt && PathfindToPlayer)
                    CurrentBehavior = CurrentPrey != null ? (float)PhaseType.Hunt : (float)PhaseType.Idle;

                PathfindToPlayer = false;
            }

            Lighting.AddLight(NPC.Center, Color.Gold.ToVector3() * (auraFade * 0.8f)); // Passively emit light based on auraFade

            if (NPC.wet)
            {
                NPC.noGravity = true;
                Animation = AnimationState.Water;

                switch ((PhaseType)CurrentBehavior)
                {
                    case PhaseType.Idle:
                        IdleBehavior();
                        break;
                    case PhaseType.Flee:
                        FleeBehavior();
                        break;
                    case PhaseType.Hunt:
                        HuntBehavior();
                        break;
                }

                if (NPC.velocity != Vector2.Zero)
                {
                    int dir = NPC.velocity.X.DirectionalSign();
                    if (dir != 0)
                        NPC.spriteDirection = NPC.direction = dir;

                    float targetRotation = NPC.velocity.ToRotation();
                    if (NPC.spriteDirection == -1)
                        targetRotation = MathHelper.WrapAngle(targetRotation - MathHelper.Pi);

                    NPC.rotation = MathHelper.Clamp(targetRotation, -MathHelper.PiOver2, MathHelper.PiOver2);
                }
                else
                    NPC.rotation = 0f;
            }

            else // Refer to BeachedBehavior for grounded movement
            {
                Animation = AnimationState.Land;
                BeachedBehavior();
                pathfinding.ClearResults();
            }

            // Timers
            if (FleeTimer > 0) 
                FleeTimer--;
            if (PassiveSoundTimer > 0) 
                PassiveSoundTimer--;
            if (NoticeHeldItemSoundTimer > 0) 
                NoticeHeldItemSoundTimer--;
        }

        public void IdleBehavior()
        {
            pathfinding.MaxSpeed = IdleMaxSpeed;

            // Always pathfind to a random point when idling
            pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(IdleMinPathDistance, IdleMaxPathDistance), SunkenSeaTileValidity));

            if (Main.rand.NextBool(300) && PassiveSoundTimer <= 0) // Randomly emit a noise
            {
                SoundEngine.PlaySound(ambientNoise with { Volume = 0.66f, Pitch = 0.1f, PitchVariance = 0.15f }, NPC.Center);
                PassiveSoundTimer = PassiveSoundCooldown;
            }
        }

        public void HuntBehavior()
        {
            pathfinding.MaxSpeed = HuntMaxSpeed;

            if (PathfindToPlayer) // Player is holding an item of interest
            {
                Player target = Main.player[NPC.target];
                targetPoint = target.Center;
            }
            else
            {
                // If prey is gone, go back to idling
                if (CurrentPrey == null)
                {
                    CurrentBehavior = (float)PhaseType.Idle;
                    return;
                }
                targetPoint = CurrentPrey.Center;
            }

            pathfinding.DoPathfinding(new(NPC.Center, targetPoint, SunkenSeaTileValidity));
        }

        public void FleeBehavior()
        {
            if (CurrentPredator == null)
            {
                CurrentBehavior = CurrentPrey != null ? (float)PhaseType.Hunt : (float)PhaseType.Idle;
                FleeTimer = 0;
                return;
            }

            if (FleeTimer <= 0) // Specifically for VFX and SFX when fleeing
            {
                SoundEngine.PlaySound(alert with { Volume = 0.9f, PitchVariance = 0.1f }, NPC.Center);

                if (!Main.dedServ)
                {
                    var emoteDirection = -Vector2.UnitY * Main.rand.NextFloat(2f, 3f);
                    GeneralParticleHandler.SpawnParticle(new EmoteExpressionParticle(NPC.Center + emoteDirection * 2f, emoteDirection, 2.2f, Color.Yellow, Main.rand.Next(30, 46), EmoteExpressionParticle.EmoteType.Exclamation));
                }

                FleeTimer = FleeFXCooldown;
            }

            pathfinding.MaxSpeed = FleeMaxSpeed;

            // Try to find a safe point from predator
            float distanceFromAvoided = Vector2.Distance(NPC.Center, CurrentPredator.Center);
            Vector2 runDirection = NPC.DirectionFrom(CurrentPredator.Center);
            targetPoint = NPC.Center + runDirection * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);

            pathfinding.DoPathfinding(new(NPC.Center, targetPoint, SunkenSeaTileValidity));
            NPC.netUpdate = true;
        }

        public void BeachedBehavior()
        {
            NPC.noGravity = false; // Gravity on while not swimming
            if (NPC.collideY)
            {
                if (GroundMovementTimer <= 0)
                {
                    NPC.velocity.X *= 0.94f; // Slow down
                    if (Math.Abs(NPC.velocity.X) < 0.2f)
                        NPC.velocity.X = 0f;

                    if (Main.rand.NextBool(80))
                    {
                        if (Main.rand.NextBool())
                            NPC.direction = -1;
                        else
                            NPC.direction = 1;

                        NPC.spriteDirection = NPC.direction;
                        GroundMovementTimer = Main.rand.Next(30, 70);
                        NPC.velocity.X = 1.2f * NPC.direction;
                        NPC.netUpdate = true;
                    }
                }

                else
                    GroundMovementTimer--;
            }

            if (Math.Abs(NPC.velocity.X) > 0.01f) // Face chosen direction while moving
            {
                int dir = NPC.velocity.X.DirectionalSign();
                if (dir != 0)
                    NPC.spriteDirection = NPC.direction = dir;
            }

            if (Main.rand.NextBool(300) && PassiveSoundTimer <= 0) // Randomly emit a noise
            {
                SoundEngine.PlaySound(ambientNoise with { Volume = 0.66f, Pitch = 0.1f, PitchVariance = 0.15f }, NPC.Center);
                PassiveSoundTimer = PassiveSoundCooldown;
            }

            NPC.rotation = 0f; // Rotation not needed when not in water
        }

        protected override void OnPredatorDetection(NPC predator)
        {
            CurrentBehavior = (float)PhaseType.Flee; // Hit the bricks regardless of what the current action is
            NPC.netUpdate = true;
        }

        protected override void OnPreyDetection(NPC prey)
        {
            // Only start hunting if not fleeing from a pred
            if ((PhaseType)CurrentBehavior != PhaseType.Flee)
            {
                CurrentBehavior = (float)PhaseType.Hunt;
                NPC.netUpdate = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            int frameGate;

            if (Animation == AnimationState.Water)
            {
                frameGate = (NPC.velocity.Length() > 3.5f ? 5 : 7); // Flap limbs faster when going fast
            }

            else // Land
            {
                if (Math.Abs(NPC.velocity.X) > 0.1f)
                {
                    frameGate = 6;
                }
                else
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                    return;
                }
            }

            if (NPC.frameCounter > frameGate)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 1))
                {
                    NPC.frame.Y = 0; // Loop anim
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneRadiantReefs && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.Cavern.Chance * 0.1777f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 7; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCoin, hit.HitDirection, -1f, 0, default, 1f);
            }
            CalamityUtils.SpawnGores(NPC, "GildedAxolotl", 3);
        }

        public override void OnKill()
        {
            for (int k = 0; k < 7; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<LightDust>(), Main.rand.NextFloat(0, MathHelper.TwoPi), Main.rand.NextFloat(-1f, 3f), 0, default, 1f);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return true;

            // Draw aura circle behind the axolotl
            float auraOpacity = auraFade * 0.4f;
            if (auraOpacity > 0f)
            {
                Texture2D auraTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
                Main.EntitySpriteDraw(auraTexture, NPC.Center - screenPos, null, (Color.Gold * auraOpacity) with { A = 0 }, NPC.rotation, auraTexture.Size() * 0.5f, 7.77f, SpriteEffects.None, 0);
            }

            // Main NPC drawing
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(tex.Width / 4, tex.Height / (Main.npcFrameCount[Type] * 2));

            float yOffset = Animation == AnimationState.Land ? 2f : 0f; // Draws 2 pixels down out of water. The way it's sheeted makes it best to handle it this way as to not edit the hitbox.
            Vector2 drawPos = NPC.Center - screenPos + Vector2.UnitY * (NPC.gfxOffY + yOffset);

            Rectangle frame = tex.Frame(2, Main.npcFrameCount[Type], (int)Animation, NPC.frame.Y / NPC.height);

            // Draw backglow that scales with auraFade
            if (auraFade > 0f)
            {
                float glowScale = NPC.scale * (1f + (0.0777f * auraFade));
                float pulse = 0.8f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f);
                Color glowColor = (Color.Gold * auraFade * pulse) with { A = 0 };

                spriteBatch.Draw(tex, drawPos, frame, glowColor, NPC.rotation, origin, glowScale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(tex, drawPos, frame, NPC.GetAlpha(drawColor * (1f + auraFade * 3)), NPC.rotation, origin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }
    }
}
