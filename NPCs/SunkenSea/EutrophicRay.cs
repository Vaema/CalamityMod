using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Enums;
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

        public override void SetStaticDefaults()
        {
            //Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers();
            value.Position.X += 24f;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = value;
            if (!Main.dedServ)
            {
                GlowTexture = ModContent.Request<Texture2D>(Texture + "Glow", AssetRequestMode.AsyncLoad);
            }
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            NPC.damage = 20;
            NPC.width = 116;
            NPC.height = 34;
            NPC.defense = 5;
            NPC.DR_NERD(0.05f);
            NPC.lifeMax = 200;
            NPC.noGravity = true;
            NPC.value = Item.buyPrice(0, 0, 1, 0);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath55;
            NPC.knockBackResist = 0.5f;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<EutrophicRayBanner>();
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };

            // Scale stats in Expert and Master
            CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
            CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
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
                MaxSpeed = 5f,
                Acceleration = 0.5f
            };
            CurrentBehavior = IdleBehavior;
        }

        #region AI Functions
        public override void AI()
        {
            CurrentBehavior?.Invoke();

            if (NPC.velocity.X > 0.25f)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < 0.25f)
                NPC.spriteDirection = -1;

            if (!NPC.wet)
            {
                Main.NewText("h");
                CurrentBehavior = OutOfWaterBehavior;
            } 

            if (NPC.justHit && !hasBeenHit)
            {
                hasBeenHit = true;
            }

            NPC.chaseable = hasBeenHit;

            NPC.rotation = NPC.velocity.X * 0.04f;
            if (NPC.rotation < -0.1f)
                NPC.rotation = -0.1f;
            if (NPC.rotation > 0.1f)
                NPC.rotation = 0.1f;                
        }
        private void OnBehaviorChange(Action newBehavior)
        {
            if (newBehavior == OutOfWaterBehavior)
                NPC.noGravity = false;

            pathfinding.MinimumPointDistance = newBehavior == AttackBehavior ? 20f : 48f;
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
            if (AttackingEntity is null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            // Set damage while attacking. Clear idle behavior pathfinding to prevent it continuously trying to return to that path when attacking.
            NPC.damage = NPC.defDamage;
            pathfinding.ClearResults();

            float maxSpeedX = 8f;
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

            NPC.rotation = NPC.velocity.Y * 0.05f;
            if (NPC.rotation < -0.1f)
                NPC.rotation = -0.1f;
            if (NPC.rotation > 0.1f)
                NPC.rotation = 0.1f;
        }
        private void FleeBehavior()
        {
            // Check what specifically the predator is. If there is none, go back to idling.
            AvoidedEntity = AvoidedEntity is NPC ? CurrentPredator : CurrentPlayer;
            if (AvoidedEntity is null)
            {
                CurrentBehavior = IdleBehavior;
                return;
            }

            // Don't deal damage while fleeing. Clear idle behavior pathfinding to prevent it continuously trying to return to that path at the edge of the flee radius.
            NPC.damage = 0;
            pathfinding.ClearResults();

            float maxSpeedX = 5f;
            float maxSpeedY = 2.5f;
            float accelX = 0.25f;
            float accelY = 0.3f;
            int directionX = (NPC.Center.X > AvoidedEntity.Center.X).ToDirectionInt();
            int directionY = (NPC.Center.Y > AvoidedEntity.Center.Y).ToDirectionInt();

            Vector2 fleeDirection = Utils.DirectionTo(NPC.Center, AvoidedEntity.Center);

            NPC.velocity.X += Math.Min(Math.Abs(fleeDirection.X), accelX) * directionX;
            if (NPC.velocity.X > maxSpeedX)
                NPC.velocity.X = maxSpeedX;
            if (NPC.velocity.X < -maxSpeedX)
                NPC.velocity.X = -maxSpeedX;
            NPC.velocity.Y += Math.Min(Math.Abs(fleeDirection.Y), accelY) * directionY;
            if (NPC.velocity.Y > maxSpeedY)
                NPC.velocity.Y = maxSpeedY;
            if (NPC.velocity.Y < -maxSpeedY)
                NPC.velocity.Y = -maxSpeedY;

            NPC.rotation = NPC.velocity.Y * 0.05f;
            if (NPC.rotation < -0.1f)
                NPC.rotation = -0.1f;
            if (NPC.rotation > 0.1f)
                NPC.rotation = 0.1f;
        }
        private void OutOfWaterBehavior()
        {
            NPC.damage = 0;
            if (NPC.wet)
            {
                NPC.noGravity = true;
                CurrentBehavior = _previousBehavior;
            }
        }
        #endregion

        #region Creature Detection and Hit Logic
        protected override bool PlayerSearchFilter(Player p) => Vector2.DistanceSquared(NPC.Center, p.Center) < 262144f || shouldAttack;
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
            NPC.noTileCollide = true;
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
            NPC.noTileCollide = true;
        }
        #endregion

        /*public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += (hasBeenHit || NPC.IsABestiaryIconDummy) ? 0.15f : 0f;
            NPC.frameCounter %= Main.npcFrameCount[Type];
            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }*/

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 halfSizeTexture = NPC.frame.Size() / 2f;
            Vector2 vector = NPC.Center - screenPos;
            Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Color.LightBlue);
            SpriteEffects sp = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.spriteBatch.Draw(GlowTexture.Value, vector, NPC.frame, color, NPC.rotation, halfSizeTexture, 1f, sp, 0f);
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.minion && !projectile.Calamity().overridesMinionDamagePrevention)
            {
                return hasBeenHit;
            }
            return null;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.6f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 2; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BlueCrystalShard, hit.HitDirection, -1f, 0, default, 1f);
                }
                if (!Main.dedServ)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("RayGore4").Type, 1f);
                }
            }
        }
    }
}
