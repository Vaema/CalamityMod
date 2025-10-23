using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Effects;
using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using CalamityMod.Systems;
using CalamityMod.Tiles.Ores;
using CalamityMod.World;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace CalamityMod.NPCs.Deconstructors
{
    public class ExcavatorHitbox : BaseWormHitboxNPC
    {
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
        public override void SetDefaults()
        {
            NPC.damage = 50;
            NPC.width = 44;
            NPC.height = 44;
            NPC.scale = 1;
            NPC.lifeMax = 10000;
            NPC.value = Item.buyPrice(1, 0, 0, 0);

            NPC.HitSound = ThanatosHead.ThanatosHitSoundClosed;
            NPC.DeathSound = CommonCalamitySounds.ExoDeathSound;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.Calamity().DR = 0.5f;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = false;
            base.SetDefaults();
        }
        public override void AI()
        {
            base.AI();
            var headNPC = Main.npc[(int)NPC.ai[0]];
            NPC.Calamity().DR = headNPC.Calamity().DR;
            NPC.width = 44;
            NPC.height = 44;
        }
    }
    public class Excavator : BaseWormNPC
    {
        public override string Texture => "CalamityMod/NPCs/Deconstructors/DeconstructorMK2Head";

        public override int WormHitboxNpcType => ModContent.NPCType<ExcavatorHitbox>();
        public override List<string> SegmentTextures => new()
        {
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2Body",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2BodyAlt1",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2BodyAlt2",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2Tail"
        };

        public override List<string?> GlowTextures => new()
        {
            null,
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2BodyGlow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2BodyAlt1Glow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2BodyAlt2Glow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK2TailGlow"
        };


        public static SoundStyle ChargeSound => new("CalamityMod/Sounds/Custom/DeconstructorCharge") { Volume = 2f };
        public override int SegmentCount => 15;
        public override List<float> SegmentTypePositionOffsets => new()
        {
            54, //Head
            40, 
            40, 
            40,
            40
        };
        public static HashSet<int> VulnerableDebuffs => [BuffID.Electrified, ModContent.BuffType<StaticDischarge>(), ModContent.BuffType<VermillionFlux>(), ModContent.BuffType<AuricRebuke>()];
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            foreach (var item in VulnerableDebuffs)
            {
                NPCID.Sets.SpecificDebuffImmunity[Type][item] = false;
            }
            NPCID.Sets.ImmuneToRegularBuffs[WormHitboxNpcType] = NPCID.Sets.ImmuneToRegularBuffs[Type];
            NPCID.Sets.SpecificDebuffImmunity[WormHitboxNpcType] = NPCID.Sets.SpecificDebuffImmunity[Type];
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {

            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 50;
            NPC.width = 38;
            NPC.height = 38;
            NPC.lifeMax = 4000;
            NPC.value = Item.buyPrice(1, 0, 0, 0);

            NPC.HitSound = ThanatosHead.ThanatosHitSoundClosed;
            NPC.DeathSound = SoundID.NPCDeath44;
            NPC.knockBackResist = 0f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.chaseable = false;
            NPC.Calamity().DR = 0.5f;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = false;

            for (var i = 0; i < SegmentCount - 1; i++)
            {
                Segments.Add(new BaseWormSegment(this, i % 3));
            }
            Segments.Add(new BaseWormSegment(this, 3));
        }

        #region AI Variables

        public enum AttackState
        {
            Idle,
            Mining,
            GettingItem,
            Fleeing,
            ChargingRaildash,
            GaussRaildash,
            PlasmaTorch,
            LaserBlast
        }
        public AttackState ActiveAttackState
        {
            get { return (AttackState)NPC.ai[1]; }
            set { NPC.ai[1] = (float)value; }
        }
        public float MainTimer
        {
            get { return NPC.ai[0]; }
            set { NPC.ai[0] = value; }
        }

        public float AttackSubstate
        {
            get { return NPC.ai[2]; }
            set { NPC.ai[2] = value; }
        }

        public float StateChangeCounter
        {
            get { return NPC.ai[3]; }
            set { NPC.ai[3] = value; }
        }
        public float VelocityRotation
        {

            get { return NPC.velocity.ToRotation(); }
            set { NPC.velocity = value.ToRotationVector2() * NPC.velocity.Length(); }
        }

        public Vector2 TargetVector = Vector2.Zero;
        public Vector2 SecondaryVector = Vector2.Zero;
        public float StoredValue = 0;
        void TurnTowards(Vector2 goal, float offset = 0, float maxSpeed = 1)
        {
            float goal2 = (goal - NPC.Center).ToRotation() + offset;
            maxSpeed *= (float)Math.PI / 180f;
            var dif = MathF.Atan2(MathF.Sin(goal2 - VelocityRotation), MathF.Cos(goal2 - VelocityRotation));
            if (dif < 0)
            {
                if (-dif > maxSpeed)
                    VelocityRotation -= maxSpeed;
                else
                    VelocityRotation += dif;
            }
            else
            {
                if (dif > maxSpeed)
                    VelocityRotation += maxSpeed;
                else
                    VelocityRotation += dif;
            }
        }
        #endregion

        public void SwitchAttackState(AttackState State, float Substate = 0, bool resetVector = true)
        {
            ActiveAttackState = State;
            AttackSubstate = Substate;
            MainTimer = 0;
            if (resetVector)
                TargetVector = Vector2.Zero;
        }

        public override void AI()
        {
            HandleAIStates();
            MainTimer++;
            UpdateSegments();
        }

        public static List<int> OreTypes => new()
        {
            TileID.Copper, TileID.Tin,
            TileID.Iron, TileID.Lead,
            TileID.Silver, TileID.Tungsten,
            TileID.Gold, TileID.Platinum,
            TileID.Demonite,TileID.Crimtane,
            ModContent.TileType<AerialiteOre>(),
            TileID.Cobalt, TileID.Palladium,
            TileID.Mythril,TileID.Orichalcum,
            TileID.Adamantite,TileID.Titanium,
            ModContent.TileType<CryonicOre>(),
            ModContent.TileType<PerennialOre>(),
            ModContent.TileType<HallowedOre>(),
            ModContent.TileType<AuricOre>(),
            ModContent.TileType<AstralOre>(),
            ModContent.TileType<UelibloomOre>()
        };
        public List<List<Point>> FindOreVeins()
        {
            List<List<Point>> oreVeins = new();
            var wormTile = NPC.Center.ToTileCoordinates();
            HashSet<Point> visited = new();

            for (int x = -30; x <= 30; x++)
            {
                int tileX = wormTile.X + x;
                if (tileX < 0 || tileX >= Main.maxTilesX)
                    continue;

                for (int y = -30; y <= 30; y++)
                {
                    int tileY = wormTile.Y + y;
                    if (tileY < 0 || tileY >= Main.maxTilesY)
                        continue;

                    Point start = new(tileX, tileY);
                    if (visited.Contains(start))
                        continue;

                    var tile = Main.tile[start];

                    //This uses a flood fill to check for the ore vein
                    if (tile.HasTile && OreTypes.Contains(tile.TileType))
                    {
                        List<Point> vein = new();
                        Queue<Point> queue = new();
                        queue.Enqueue(start);
                        visited.Add(start);
                        while (queue.Count > 0)
                        {
                            Point p = queue.Dequeue();
                            vein.Add(p);
                            foreach (var offset in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
                            {
                                Point neighbor = p + offset;
                                if (neighbor.X < 0 || neighbor.X >= Main.maxTilesX || neighbor.Y < 0 || neighbor.Y >= Main.maxTilesY)
                                    continue;
                                if (visited.Contains(neighbor))
                                    continue;

                                var neighborTile = Main.tile[neighbor];
                                if (neighborTile.HasTile && OreTypes.Contains(neighborTile.TileType))
                                {
                                    queue.Enqueue(neighbor);
                                    visited.Add(neighbor);
                                }
                            }
                        }

                        oreVeins.Add(vein);
                    }
                }
            }
            return oreVeins;
        }

        public static (Point, Point)? FindTargetFromVein(List<Point> vein)
        {
            HashSet<Point> veinSet = new(vein);
            List<(Point, Point)> outerPoints = [];

            foreach (var p in vein)
            {
                bool isOuter = false;
                foreach (var offset in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
                {
                    Point neighbor = p + offset;
                    if (!veinSet.Contains(neighbor))
                    {
                        isOuter = true;
                        if (neighbor.X >= 0 && neighbor.X < Main.maxTilesX && neighbor.Y >= 0 && neighbor.Y < Main.maxTilesY)
                        {
                            var tile = Main.tile[neighbor];
                            if (tile == null || !tile.HasTile || !tile.IsTileSolid())
                                outerPoints.Add((p, neighbor)); // Return the first found ore block adjacent to air, and the air block found.
                        }
                    }
                }
            }
            if (outerPoints.Count > 0)
                return outerPoints[Main.rand.Next(outerPoints.Count)];
            // If none are adjacent to air, return null
            return null;
        }

        private void LowerTargetToGround()
        {
            var pointToCheck = TargetVector.ToTileCoordinates();
            for (var i = 0; i < 50; i++)
            {
                if (pointToCheck.X < 0 || pointToCheck.X >= Main.maxTilesX || pointToCheck.Y < 0 || pointToCheck.Y >= Main.maxTilesY)
                    return;
                var targetTile = Main.tile[pointToCheck];
                if (targetTile == null || !targetTile.HasTile || !targetTile.IsTileSolidGround())
                    pointToCheck.Y += 1;
                else
                {
                    TargetVector = pointToCheck.ToWorldCoordinates() - new Vector2(0, 16);
                    return;
                }
            }
        }

        public void HandleAIStates()
        {
            NPC.CalamityTargeting(CalamityTargetingParameters.BossDefaults);
            if (!NPC.HasValidTarget)
                return;
            Player player = Main.player[NPC.target];
            SegmentMaxRotation = 0.65f;
            SegmentRigidity = 0.2f;
            if (NPC.life < NPC.lifeMax && ActiveAttackState == AttackState.Idle)
            {
                ActiveAttackState = AttackState.PlasmaTorch;
                GeneralParticleHandler.SpawnParticle(new EmoteExpressionParticle(NPC.Top, -Vector2.UnitY * 5, 2, ArsenalEffects.ArsenalLaserColor, 60, EmoteExpressionParticle.EmoteType.DoubleExclamation));
            }

            NPC.FindClosestPlayer(out float distanceToPlayer);
            bool noGravity = distanceToPlayer > 800 || NPC.wet || Collision.SolidCollision(NPC.position, NPC.width, NPC.height, true);
            var currentVelLength = NPC.velocity.Length();
            switch (ActiveAttackState)
            {
                case AttackState.Idle:
                    {
                        if (TargetVector == Vector2.Zero || MainTimer > 300 || NPC.Distance(TargetVector) < 32)
                        {
                            if (Main.rand.NextBool())
                            {
                                var veins = FindOreVeins();
                                while (veins.Count > 0)
                                {
                                    var targetVein = veins[Main.rand.Next(veins.Count)];
                                    var foundTarget = FindTargetFromVein(targetVein);
                                    if (foundTarget is not null)
                                    {
                                        TargetVector = foundTarget.Value.Item1.ToWorldCoordinates();
                                        SecondaryVector = foundTarget.Value.Item2.ToWorldCoordinates();
                                        if (NPC.Distance(TargetVector) > 160)
                                            GeneralParticleHandler.SpawnParticle(new EmoteExpressionParticle(NPC.Top, -Vector2.UnitY * 5, 2, ArsenalEffects.ArsenalGaussColor, 60, EmoteExpressionParticle.EmoteType.Exclamation));
                                        SwitchAttackState(AttackState.Mining, resetVector: false);
                                        return;
                                    }
                                    else
                                        veins.Remove(targetVein);
                                }
                            }
                            TargetVector = player.Center + Main.rand.NextVector2Circular(800, 800);
                            LowerTargetToGround();
                            MainTimer = 0;
                        }
                        if (AttackSubstate <= 0 && noGravity)
                        {
                            NPC.velocity += NPC.DirectionTo(TargetVector);
                            NPC.velocity *= 0.9f;
                        }
                        else
                        {
                            AttackSubstate--;
                            if (!noGravity)
                            {
                                AttackSubstate = 30;
                                NPC.velocity.Y += 1f;
                            }
                            else
                            {
                                if (NPC.velocity.Y > 8)
                                    NPC.velocity.Y *= 0.9f;
                                NPC.velocity.X *= 0.95f;
                            }
                        }
                        NPC.velocity = NPC.velocity.ClampMagnitude(0, 16);
                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                        break;
                    }
                case AttackState.Mining:
                    {
                        var dir = SecondaryVector.DirectionTo(TargetVector);
                        var offsetpos = SecondaryVector - dir * 16;
                        NPC.velocity += NPC.DirectionTo(offsetpos).SafeNormalize(Vector2.UnitY);
                        NPC.velocity *= 0.9f;
                        if (MainTimer > 600)
                            SwitchAttackState(AttackState.Idle);
                        if (NPC.Distance(offsetpos) < 4)
                        {

                            if (Main.tile[TargetVector.ToTileCoordinates()].TileType == ModContent.TileType<AuricOre>())
                            {
                                NPC.velocity = -NPC.DirectionTo(TargetVector) * 16;
                                NPC.Center = offsetpos + NPC.velocity;
                                NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
                                NPC.AddBuff(ModContent.BuffType<AuricRebuke>(), 600);
                                ActiveAttackState = AttackState.Fleeing;
                                AuricOre.Animate = true;
                                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/TeslaShoot1"), NPC.Center);
                                return;
                            }
                            SegmentRigidity = 0f;
                            NPC.velocity = Vector2.Zero;
                            NPC.rotation = offsetpos.DirectionTo(TargetVector).ToRotation() + MathHelper.PiOver2;

                            if (Main.netMode != NetmodeID.Server && !(TilePingerSystem.tileEffects["BurrowerPing"].Active))
                                TilePingerSystem.AddPing("BurrowerPing", NPC.Center, player);
                            Dust dust = Dust.NewDustPerfect(NPC.Center, ArsenalEffects.ArsenalPlasmaDust, dir.RotatedByRandom(0.5f) * 4, 0, default, 3);
                            dust.noGravity = true;
                            dust.fadeIn = 0.05f;
                            dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 1f,SoundLimitBehavior = SoundLimitBehavior.IgnoreNew}, NPC.Center);
                        }
                        else
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                        break;
                    }
                case AttackState.GettingItem:
                    {
                        ActiveAttackState = AttackState.Idle;
                        break;
                    }
                case AttackState.Fleeing:
                    {
                        bool shocked = false;
                        foreach (var item in VulnerableDebuffs)
                        {
                            if (NPC.HasBuff(item) || Main.npc.Any(x => x.active && x.type == WormHitboxNpcType && x.HasBuff(item)))
                            {
                                shocked = true;
                                break;
                            }
                        }
                        if (noGravity)
                        {
                            if (shocked)
                            {
                                SegmentRigidity = 0;
                                NPC.velocity *= 0.75f;
                                foreach (var item in Segments)
                                {
                                    if (!Collision.SolidCollision(item.Center - new Vector2(19, 17), 38, 38, true))
                                        item.Center.Y += 2f;
                                }
                            }
                            else
                            {
                                NPC.velocity += NPC.DirectionFrom(Main.player[NPC.FindClosestPlayer()].Center);
                                NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                            }
                        }
                        else
                        {
                            NPC.velocity.Y += shocked ? 0.5f : 1;
                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                        }
                        break;
                    }
                case AttackState.ChargingRaildash:
                    {
                        var telegraph = new BloomLineVFX(NPC.Center + NPC.Center.DirectionTo(player.Center) * 16f, NPC.Center.DirectionTo(player.Center) * 10000, 0.5f, ArsenalEffects.ArsenalGaussColor * (MainTimer / 150f), 2);
                        GeneralParticleHandler.SpawnParticle(telegraph);
                        Vector2 goalPos1 = player.Center - NPC.DirectionTo(player.Center) * 400;
                        if (NPC.Distance(goalPos1) > 0)
                            NPC.velocity = NPC.DirectionTo(goalPos1) * MathHelper.Min(NPC.Distance(goalPos1), MainTimer / 5f);
                        else
                            NPC.velocity = Vector2.Zero;
                        NPC.rotation = NPC.rotation.AngleLerp(NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, 0.5f);
                        if (MainTimer % 15 == 0)
                        {
                            Particle bolt2 = new CustomPulse(NPC.Center, Vector2.Zero, ArsenalEffects.ArsenalGaussColor, "CalamityMod/Particles/GlowSquareParticleBig", Vector2.One, MainTimer % 30 == 0 ? 0 : MathHelper.PiOver4, 0.9f, 0.4f, 30);
                            GeneralParticleHandler.SpawnParticle(bolt2);

                        }

                        Vector2 dustVel = (Vector2.UnitX).RotatedByRandom(100) * Main.rand.NextFloat(3f, 8.5f);
                        Dust dust = Dust.NewDustPerfect(NPC.Center + dustVel.SafeNormalize(Vector2.UnitX) * 90, Effects.ArsenalEffects.ArsenalGaussDust, -dustVel, 0, default, Main.rand.NextFloat(0.5f, 1f));
                        dust.noGravity = true;
                        dust.fadeIn = 0.05f;
                        dust.color = Effects.ArsenalEffects.ArsenalGaussColor;
                        if (MainTimer > 135)
                        {
                            ActiveAnimation = null;
                            AnimationFrame = 0;
                            NPC.velocity = Vector2.Zero;
                            TargetVector = NPC.DirectionTo(player.Center) * 70;
                            SwitchAttackState(AttackState.GaussRaildash, 0,false);
                        }
                    }
                    break;
                case AttackState.GaussRaildash:
                    {
                        if (MainTimer < 15)
                        {
                            var telegraph = new BloomLineVFX(NPC.Center + (NPC.rotation-MathHelper.PiOver2).ToRotationVector2() * 16f, (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 10000, 0.5f, ArsenalEffects.ArsenalGaussColor, 2);
                            GeneralParticleHandler.SpawnParticle(telegraph);

                            Vector2 dustVel = (Vector2.UnitX).RotatedByRandom(100) * Main.rand.NextFloat(3f, 8.5f);
                            Dust dust = Dust.NewDustPerfect(NPC.Center + dustVel.SafeNormalize(Vector2.UnitX) * 120, Effects.ArsenalEffects.ArsenalGaussDust, -dustVel, 0, default, Main.rand.NextFloat(0.5f, 1f));
                            dust.noGravity = true;
                            dust.fadeIn = 0.05f;
                            dust.color = Effects.ArsenalEffects.ArsenalGaussColor;
                        }
                        else if (MainTimer == 15 && TargetVector != Vector2.Zero)
                        {
                            NPC.velocity = TargetVector;
                            SoundEngine.PlaySound(CommonCalamitySounds.ExoPlasmaExplosionSound, player.Center);
                        }
                        else
                        {
                            Vector2 dustVel = (Vector2.UnitX).RotatedByRandom(100) * Main.rand.NextFloat(1f, 5f);
                            Dust dust = Dust.NewDustPerfect(NPC.Center + dustVel.SafeNormalize(Vector2.UnitX) * 32, ArsenalEffects.ArsenalGaussDust, -dustVel, 0, default, Main.rand.NextFloat(1f, 2f));
                            dust.noGravity = true;
                            dust.fadeIn = 0.05f;
                            dust.color = Effects.ArsenalEffects.ArsenalGaussColor;
                        }
                        if (MainTimer > 45)
                        {
                            SwitchAttackState(AttackState.LaserBlast, 0);
                            AttackSubstate = 0;
                            NPC.velocity = Vector2.Zero;
                        }
                        if (NPC.velocity != Vector2.Zero)
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                        break;
                    }
                case AttackState.PlasmaTorch:
                    {
                        var dist = NPC.Distance(player.Center);
                        if (NPC.Distance(player.Center) > 300)
                            AttackSubstate = 1;
                        if (AttackSubstate == 1)
                        {
                            TurnTowards(player.Center, 0, 7);
                            if (Vector2.Dot(NPC.DirectionTo(player.Center), NPC.velocity.SafeNormalize(Vector2.Zero)) > 0.95f)
                                AttackSubstate = 0;
                        }
                        NPC.velocity = VelocityRotation.ToRotationVector2() * (currentVelLength < (dist < 400 ? 10 : 15) ? currentVelLength + 0.2f : currentVelLength > (dist < 400 ? 13 : 18) ? currentVelLength - 1f : currentVelLength);
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                        Dust dust = Dust.NewDustPerfect(NPC.Center, ArsenalEffects.ArsenalPlasmaDust, NPC.velocity + NPC.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * 4, 0, default,3);
                        dust.noGravity = true;
                        dust.fadeIn = 0.05f;
                        dust.color = Effects.ArsenalEffects.ArsenalPlasmaColor;
                        if (MainTimer > 600)
                        {
                            NPC.velocity = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 20;
                            SwitchAttackState(AttackState.ChargingRaildash);
                            SoundEngine.PlaySound(ChargeSound);
                        }
                        break;
                    }
                case AttackState.LaserBlast:
                    {
                        if (TargetVector == Vector2.Zero)
                        {
                            TargetVector = player.Center;
                            StateChangeCounter = 0;
                        }
                        var adjustedTarget = TargetVector + new Vector2(0, 1200);
                        if (Math.Abs(NPC.Center.X - adjustedTarget.X) < 160 && NPC.Center.Y > adjustedTarget.Y - 64 &&  StateChangeCounter % 2 == 0 && MainTimer > 120)
                            StateChangeCounter++;
                        if (StateChangeCounter % 2 == 0)
                        {
                            if (NPC.Center.Y > adjustedTarget.Y)
                                NPC.velocity = NPC.DirectionTo(adjustedTarget) * 15;
                            else
                            {
                                NPC.velocity.Y += 1f * (1-(AttackSubstate/30f));
                            }
                        } else
                        {
                            if  (NPC.Distance(TargetVector) < 64)
                            {
                                TargetVector = player.Center;
                                StateChangeCounter++;
                                MainTimer = 0;
                                AttackSubstate = 30;
                                
                            }
                            else
                                NPC.velocity = NPC.DirectionTo(TargetVector) * 20;
                        }

                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                        if (AttackSubstate > 0)
                        {
                            AttackSubstate--;
                            if (AttackSubstate == 0 || AttackSubstate == 4 || AttackSubstate == 8 || AttackSubstate == 12)
                            {
                                int i = (3-(int)AttackSubstate / 4) *4;
                                TargetVector = player.Center; float projectileVelocity = 3f;
                                Vector2 velocityVector = Vector2.Normalize(player.Center - Segments[i].Center) * projectileVelocity;
                                int type = ModContent.ProjectileType<ExcavatorLaser>();
                                int damage = 50;
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), Segments[i].Center, velocityVector, type, damage, 0f, Main.myPlayer);
                                SoundEngine.PlaySound(CommonCalamitySounds.ExoLaserShootSound with {Volume = 0.5f, MaxInstances = 3});
                            }
                        }
                        if (StateChangeCounter > 10)
                        {
                            SwitchAttackState(AttackState.PlasmaTorch);
                            StateChangeCounter = 0;
                        }
                        break;
                    }
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<MysteriousCircuitry>(), 1, 4, 8);
            npcLoot.Add(ModContent.ItemType<DubiousPlating>(), 1, 4, 8);
            npcLoot.Add(ItemID.CopperOre, 2, 6, 12);
            npcLoot.Add(ItemID.TinOre, 2, 6, 12);
            npcLoot.Add(ItemID.IronOre, 4, 4, 8);
            npcLoot.Add(ItemID.LeadOre, 4, 4, 8);
            npcLoot.Add(ItemID.SilverOre, 6, 4, 8);
            npcLoot.Add(ItemID.TungstenOre, 6, 4, 8);
            npcLoot.Add(ItemID.GoldOre, 8, 4, 8);
            npcLoot.Add(ItemID.PlatinumOre, 8, 4, 8);
            npcLoot.Add(ItemID.DemoniteOre, 10, 4, 8);
            npcLoot.Add(ItemID.CrimtaneOre, 10, 4, 8);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().InAnyCalamityBiome || Main.npc.Any(x => x.active && x.type == Type))
            {
                return 0f;
            }
            return SpawnCondition.Cavern.Chance * 1f;
        }
    }

    public class ExcavatorLaser : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public override void SetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.hostile = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 6;
            Projectile.timeLeft = 180 * Projectile.MaxUpdates;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.FinalExtraUpdate())
            Lighting.AddLight(Projectile.Center, Effects.ArsenalEffects.ArsenalLaserColor.ToVector3() * 0.4f);
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time > 15 && targetDist < 1400)
            {
                if (Projectile.timeLeft % 3 == 0)
                {
                    Particle spark = new LineParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 8, 1.7f * Projectile.ai[0], Effects.ArsenalEffects.ArsenalLaserColor);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Projectile.timeLeft % 2 == 0)
                {
                    SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 3, 0.7f * Projectile.ai[0], Color.White);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ArsenalEffects.ArsenalPlasmaDust, Projectile.velocity, 0, default, 1);
                dust.noGravity = true;
                dust.fadeIn = 0.05f;
                dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Effects.ArsenalEffects.ArsenalLaserDust, (Projectile.velocity * 4).RotatedByRandom(0.1f) * Main.rand.NextFloat(0.3f, 0.8f), 0, default, Main.rand.NextFloat(0.7f, 1.3f));
                dust.noGravity = true;
                dust.color = Effects.ArsenalEffects.ArsenalLaserColor;
                dust.alpha = 100;
                dust.fadeIn = -3;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
            if (time < 1)
                return false;
            Texture2D pointTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/GlowBlade").Value;
            float fade = Utils.GetLerpValue(0, 15, Projectile.timeLeft, true);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition, null, Effects.ArsenalEffects.ArsenalLaserColor with { A = 0 } * fade * 0.4f, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.7f - i * 0.1f, 1 + i * 0.15f) * 0.018f * Projectile.ai[0], SpriteEffects.None);
                Main.EntitySpriteDraw(pointTexture, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.2f, Projectile.rotation, pointTexture.Size() * 0.5f, new Vector2(0.7f - i * 0.1f, 1 + i * 0.15f) * 0.013f * Projectile.ai[0], SpriteEffects.None);
            }
            return false;
        }
    }

}
