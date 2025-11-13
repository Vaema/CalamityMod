using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.SunkenSea
{
    public class EutrophicRay : SunkenSeaNPC
    {
        public static Asset<Texture2D> GlowTexture;
        public static Asset<Texture2D> TailTexture;
        public static Asset<Texture2D> TailGlowTexture;
        #region Fields
        protected override List<int> PreyIDs => [];
        protected override List<int> PredatorIDs =>
        [
            ModContent.NPCType<Sharkoon>(),
            ModContent.NPCType<Polyperil>(),
            //ModContent.NPCType<Hermititan>(),
            //ModContent.NPCType<SunkenScourgeHead>()
        ];
        protected override SunkenSeaBiomeFlags BiomeDesignation => SunkenSeaBiomeFlags.RadiantReefs | SunkenSeaBiomeFlags.GleamingBurrows;

        private bool hasBeenHit = false;
        private bool shouldAttack => NPC.life < (int)(NPC.lifeMax * 0.6f);
        private Entity AttackingEntity;
        private Entity AvoidedEntity;
        private Action _currentBehavior;
        private Action CurrentBehavior
        {
            get => _currentBehavior;
            set
            {
                if (value != _currentBehavior)
                    OnBehaviorChange(value);
                _previousBehavior = _currentBehavior;
                _currentBehavior = value;
            }
        }
        private Action _previousBehavior;
        #endregion

        public override void Load()
        {
            GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow");
            TailTexture = ModContent.Request<Texture2D>(Texture + "Tail");
            TailGlowTexture = ModContent.Request<Texture2D>(Texture + "TailGlow");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 7;
            NPCID.Sets.TrailingMode[Type] = 3;
            NPCID.Sets.TrailCacheLength[Type] = 15;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 54;
            NPC.height = 68;
            NPC.damage = 20;
            NPC.defense = 5;
            NPC.lifeMax = 200;
            NPC.scale = 0.85f;

            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(silver: 1);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath55;
            NPC.knockBackResist = 0.5f;

            Banner = NPC.type;
            BannerItem = ModContent.ItemType<EutrophicRayBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.EutrophicRay")
            });
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.chaseable);
            writer.Write(hasBeenHit);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.chaseable = reader.ReadBoolean();
            hasBeenHit = reader.ReadBoolean();
        }

        public override void OnSpawn(IEntitySource source)
        {
            pathfinding = new PathfindingManager(NPC)
            {
                MaxSpeed = 4.85f,
                Acceleration = 0.5f
            };
            CurrentBehavior = IdleBehavior;
        }

        #region AI Functions
        public override void AI()
        {
            if (pathfinding == null)
            {
                pathfinding = new PathfindingManager(NPC)
                {
                    MaxSpeed = 4.85f,
                    Acceleration = 0.5f
                };
            }
            CurrentBehavior?.Invoke();

            // If out of water, act like you're out of water.
            if ((NPC.noTileCollide ? !(Collision.WetCollision(NPC.position, NPC.width, NPC.height) || Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) : !NPC.wet) && CurrentBehavior != OutOfWaterBehavior)
                CurrentBehavior = OutOfWaterBehavior;

            // Used for determining if it should run from players.
            if (NPC.justHit && !hasBeenHit)
                hasBeenHit = true;
            NPC.chaseable = hasBeenHit;

            float curRot = NPC.rotation;
            float newRot = NPC.velocity.ToRotation() + MathHelper.Pi;
            if (Math.Abs(MathHelper.WrapAngle(curRot) - MathHelper.WrapAngle(newRot)) > MathHelper.PiOver4)
            {
                NPC.rotation = Utils.AngleLerp(curRot, newRot, 0.1f);
            }
            else
            {
                NPC.rotation = newRot;
            }
        }
        private void OnBehaviorChange(Action newBehavior)
        {
            if (newBehavior == OutOfWaterBehavior)
                NPC.noGravity = false;

            pathfinding.MaxSpeed = newBehavior == FleeBehavior ? 6.5f : 5f;
        }

        private void IdleBehavior()
        {
            // Don't deal damage while idling.
            NPC.damage = 0;
            // If inside solid tiles, attempt to get out of them by occasionally bursting in a random direction.
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            {
                if (Main.rand.NextBool(150))
                    NPC.velocity = Main.rand.NextVector2CircularEdge(7.5f, 7.5f);

                NPC.velocity *= 0.98f;
            }
            pathfinding.DoPathfinding(new(NPC.Center, NPC.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(200f, 800f), SunkenSeaTileValidity));
        }
        private void AttackBehavior()
        {
            // Check what specifically the hunted entity is. If there is none, go back to idling.
            AttackingEntity = AttackingEntity is NPC ? CurrentPrey : CurrentPlayer;
            if (AttackingEntity == null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            // Set damage while attacking. Clear idle behavior pathfinding to prevent it continuously trying to return to that path when attacking.
            NPC.damage = NPC.defDamage;
            pathfinding.ClearResults();
            NPC.noTileCollide = true;

            float maxSpeedX = 8.2f;
            float maxSpeedY = 3.2f;
            float accelX = 0.25f;
            float accelY = 0.3f;
            int directionX = (NPC.Center.X < AttackingEntity.Center.X).ToDirectionInt();
            int directionY = (NPC.Center.Y < AttackingEntity.Center.Y).ToDirectionInt();

            NPC.velocity.X += accelX * directionX;
            if (NPC.velocity.X > maxSpeedX)
                NPC.velocity.X = maxSpeedX;
            if (NPC.velocity.X < -maxSpeedX)
                NPC.velocity.X = -maxSpeedX;

            NPC.velocity.Y += accelY * directionY;
            if (NPC.velocity.Y > maxSpeedY)
                NPC.velocity.Y = maxSpeedY;
            if (NPC.velocity.Y < -maxSpeedY)
                NPC.velocity.Y = -maxSpeedY;
        }
        private void FleeBehavior()
        {
            // Check what specifically the predator is. If there is none, go back to idling.
            AvoidedEntity = AvoidedEntity is NPC ? CurrentPredator : CurrentPlayer;
            if (AvoidedEntity == null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            // If you reach the attack threshold, start attacking.
            if (shouldAttack && AvoidedEntity is Player)
            {
                CurrentBehavior = AttackBehavior;
                AttackingEntity = AvoidedEntity;
                AvoidedEntity = null;
                return;
            }

            // Don't deal damage while fleeing.
            NPC.damage = 0;
            NPC.noTileCollide = true;

            // Flee in a straight line if possible, otherwise pathfind away from the attacker.
            if (!Main.tile[(NPC.Center + NPC.DirectionFrom(AvoidedEntity.Center) * 64f).ToTileCoordinates()].IsTileSolid())
            {
                NPC.velocity += NPC.DirectionFrom(AvoidedEntity.Center) * pathfinding.Acceleration;
                pathfinding.ClearResults();

                // Cap the speed if MaxSpeed has been surpassed.
                if (NPC.velocity.LengthSquared() > pathfinding.MaxSpeed * pathfinding.MaxSpeed)
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * pathfinding.MaxSpeed;
            }
            else
            {
                float distanceFromAvoided = Vector2.Distance(NPC.Center, AvoidedEntity.Center);
                Vector2 pathPoint = NPC.Center + Main.rand.NextVector2Unit() * Utils.Remap(distanceFromAvoided, 0f, 960f, 80f, 3200f);
                NPC.netUpdate = true;
                pathfinding.DoPathfinding(new(NPC.Center, pathPoint, SunkenSeaTileValidity));
            }
        }
        private void OutOfWaterBehavior()
        {
            NPC.damage = 0;
            NPC.velocity.X *= 0.985f;
            if (NPC.noTileCollide ? (Collision.WetCollision(NPC.position, NPC.width, NPC.height) || Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) : NPC.wet)
            {
                NPC.noGravity = true;
                CurrentBehavior = _previousBehavior;
            }
        }
        #endregion

        #region Creature Detection and Hit Logic
        protected override bool PlayerSearchFilter(Player p) => (Vector2.DistanceSquared(NPC.Center, p.Center) < 262144f && hasBeenHit) || shouldAttack;
        protected override bool NPCSearchFilter(NPC n) => Vector2.DistanceSquared(NPC.Center, n.Center) < 102400f && (PreyIDs.Contains(n.type) || PredatorIDs.Contains(n.type));
        protected override void OnPreyDetection(NPC prey)
        {
            if (CurrentPredator is not null)
            {
                CurrentBehavior = AttackBehavior;
                NPC.noTileCollide = true;
                AttackingEntity = prey;
            }
        }
        public override bool CanHitNPC(NPC target) => PreyIDs.Contains(target.type);
        protected override void OnPredatorDetection(NPC predator)
        {
            CurrentBehavior = FleeBehavior;
            NPC.noTileCollide = false;
            AvoidedEntity = predator;
        }
        public override bool CanBeHitByNPC(NPC attacker) => PredatorIDs.Contains(attacker.type);
        protected override void OnPlayerDetection(Player player)
        {
            if (CurrentPredator is not null && !hasBeenHit)
                return;

            if (shouldAttack)
            {
                CurrentBehavior = AttackBehavior;
                AttackingEntity = player;
            }
            else if (hasBeenHit)
            {
                CurrentBehavior = FleeBehavior;
                AvoidedEntity = player;
            }
            NPC.noTileCollide = shouldAttack;
        }
        #endregion

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.15f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new Vector2(tex.Width / 2, tex.Height / 2 / Main.npcFrameCount[Type]);
            Vector2 vector = NPC.Center - screenPos;
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0);
            spriteBatch.Draw(tex, vector, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, 0, 0f);
            spriteBatch.Draw(GlowTexture.Value, vector, NPC.frame, color, NPC.rotation, halfSizeTexture, NPC.scale, 0, 0f);

            // Position where the tail starts
            Vector2 tailOffset = Vector2.UnitX.RotatedBy(NPC.rotation) * (tex.Width / 2 - 4);

            if (NPC.IsABestiaryIconDummy)
            {
                for (int i = 0; i < NPC.oldRot.Length; i++)
                {
                    NPC.oldRot[i] = 0;
                }
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    NPC.oldPos[i] = Vector2.UnitX.RotatedBy(NPC.rotation) * ((tex.Width / 2 - 4) + i * 8);
                }
            }

            float currentSegmentRotation = NPC.rotation;
            List<Vector2> tailDrawPositions = new List<Vector2>();
            int segAmt = 7;
            int tailLength = 40;
            for (int i = 0; i < segAmt; i++)
            {
                float tailCompletionRatio = i / (float)segAmt;
                float wrappedAngularOffset = MathHelper.WrapAngle(NPC.oldRot[i + 1] - currentSegmentRotation) * 0.6f;
                float segmentRotationOffset = MathHelper.Clamp(wrappedAngularOffset, -0.24f, 0.24f);

                Vector2 Offset = Vector2.UnitX.RotatedBy(NPC.rotation);
                Vector2 tailSegmentOffset = Vector2.UnitX.RotatedBy(currentSegmentRotation) * tailCompletionRatio * tailLength + Offset;
                tailDrawPositions.Add(NPC.Center + tailSegmentOffset + tailOffset);

                currentSegmentRotation += segmentRotationOffset;
            }
            for (int i = 0; i < tailDrawPositions.Count; i++)
            {
                Vector2 pos = tailDrawPositions[i];
                float rot = pos.DirectionTo(NPC.Center + Vector2.UnitX.RotatedBy(NPC.rotation)).ToRotation();
                if (i > 0)
                {
                    Vector2 oldPos = tailDrawPositions[i - 1];
                    rot = pos.DirectionTo(oldPos).ToRotation();
                }
                int frame = i switch
                {
                    1 => 10,
                    5 => 20,
                    6 => 32,
                    _ => 0
                };
                int height = 8;
                if (i == 6)
                    height = 10;
                rot += MathHelper.PiOver2;

                spriteBatch.Draw(TailTexture.Value, pos - screenPos, new Rectangle(0, frame, 10, height), NPC.GetAlpha(drawColor), rot, new Vector2(6, 4), NPC.scale, 0, 0f);
                spriteBatch.Draw(TailGlowTexture.Value, pos - screenPos, new Rectangle(0, frame, 10, height), color, rot, new Vector2(6, 4), NPC.scale, 0, 0f);
            }

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                if (spawnInfo.Player.Calamity().ZoneRadiantReefs || spawnInfo.Player.Calamity().ZoneGleamingBurrows)
                    return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
            CalamityUtils.SpawnGores(NPC, "EutrophicRay", 2);
        }
    }
}
