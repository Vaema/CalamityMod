using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
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
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.NPCs.Deconstructors
{
    public class DeconstructorHitbox : BaseWormHitboxNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<VermillionFlux>()] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<StaticDischarge>()] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<AuricRebuke>()] = false;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            NPC.damage = 50;
            NPC.width = 88;
            NPC.height = 88;
            NPC.lifeMax = 825000;
            NPC.value = Item.buyPrice(1, 0, 0, 0);

            NPC.HitSound = ThanatosHead.ThanatosHitSoundClosed;
            NPC.DeathSound = CommonCalamitySounds.ExoDeathSound;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.Calamity().DR = 0.75f;
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
            NPC.HitSound = headNPC.HitSound;
            NPC.width = 88;
            NPC.height = 88;
        }
    }
    public class Deconstructor : BaseWormNPC
    {
        public override string Texture => "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Head";

        public override int WormHitboxNpcType => ModContent.NPCType<DeconstructorHitbox>();
        public override List<string> SegmentTextures => new()
        {
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Body1",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Body2",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Tail"
        };

        public override List<string?> GlowTextures => new()
        {
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3HeadGlow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Body1Glow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3Body2Glow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK3TailGlow"
        };


        public static SoundStyle ChargeSound => new("CalamityMod/Sounds/Custom/DeconstructorCharge") { Volume = 2f };
        public override int SegmentCount => 20;

        public override List<float> SegmentTypePositionOffsets => new()
        {
            68, //Head
            52, //Body 
            52 //Tail
        };
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Electrified] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<VermillionFlux>()] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<StaticDischarge>()] = false;
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<AuricRebuke>()] = false;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {

            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.damage = 50;
            NPC.width = 88;
            NPC.height = 88;
            NPC.lifeMax = 825000;
            NPC.value = Item.buyPrice(1, 0, 0, 0);

            NPC.HitSound = ThanatosHead.ThanatosHitSoundClosed;
            NPC.DeathSound = CommonCalamitySounds.ExoDeathSound;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.Calamity().DR = 0.75f;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = false;
            NPC.Calamity().VulnerableToWater = false;

            for (var i = 0; i < SegmentCount - 1; i++)
            {
                Segments.Add(new BaseWormSegment(this, i % 2));
            }
            Segments.Add(new BaseWormSegment(this, 2));
        }

        #region AI Variables

        public enum AttackState
        {
            Passive,
            Hunting,
            Resting,
            SpawnAnimation,
            CutoffMovement,
            ArmorApproachPlayer,
            ChargingUpDash,
            ElectricDash,
            ThunderRain,
            CloudDashes,
            CoilDashes,
            LightingSpit,
            DeathAnimation
        }
        public AttackState ActiveAttackState
        {
            get { return (AttackState)NPC.ai[1]; }
            set { NPC.ai[1] = (float)value; }
        }

        public bool isArmored => !hasArmorExploded;

        public static float ArmorTotalHP = 100000;

        bool hasArmorExploded = false;

        public float ArmorDamageTaken
        {
            get { return NPC.Calamity().newAI[0]; }
            set { NPC.Calamity().newAI[0] = value; }
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

        public float ElectricStrength = 0;

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
            if (!hasArmorExploded && (ArmorDamageTaken >= ArmorTotalHP))
            {
                hasArmorExploded = true;
                NPC.HitSound = ThanatosHead.ThanatosHitSoundOpen;
                NPC.Calamity().DR = 0;

                if (!Main.dedServ)
                    Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SWArmorHead1").Type, NPC.scale);

                SoundEngine.PlaySound(StormWeaverHead.ArmorShedSound, NPC.Center);

                CalamityGlobalNPC global = NPC.Calamity();
                NPC.defense = 20;
                global.DR = 0.2f;
                global.unbreakableDR = false;
                NPC.chaseable = true;
                NPC.HitSound = SoundID.NPCHit13;
                NPC.frame = new Rectangle(0, 0, 62, 86);
                Projectile.NewProjectile(new EntitySource_Parent(Main.player[NPC.target]), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), (int)(NPC.lifeMax * 0.2f), 0f, Main.player[NPC.target].whoAmI, NPC.whoAmI);
                SwitchAttackState(AttackState.CoilDashes);
                ActiveAnimation = null;
                StateChangeCounter = 0;
            }
            HandleAIStates();
            MainTimer++;
            UpdateSegments();
        }
        public void HandleAIStates()
        {
            NPC.CalamityTargeting(CalamityTargetingParameters.BossDefaults);
            if (!NPC.HasValidTarget)
                return;
            Player player = Main.player[NPC.target];
            var currentVelLength = NPC.velocity.Length();
            switch (ActiveAttackState)
            {
                case AttackState.Passive:
                    if (MainTimer > 60)
                        SwitchAttackState(AttackState.Hunting);
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                    break;
                case AttackState.Hunting:
                    int[] npcsToHunt = { NPCID.Harpy, NPCID.WyvernHead, ModContent.NPCType<ShockstormShuttle>() };
                    if (NPC.life == NPC.lifeMax)
                    {
                        if (Main.npc.Any(x => x.active && x.type == NPCID.Harpy && x.Distance(NPC.Center) < 2000))
                        {
                            var targetNPC = Main.npc.First(x => x.active && npcsToHunt.Contains(x.type) && x.Distance(NPC.Center) < 1000);
                            NPC.velocity = NPC.DirectionTo(targetNPC.Center) * 10;
                            if (NPC.Distance(targetNPC.Center) < 64)
                            {
                                targetNPC.SimpleStrikeNPC(1000, 1);
                            }
                            MainTimer = 0;
                        }
                    }
                    if (MainTimer > 120)
                        SwitchAttackState(AttackState.CutoffMovement);
                    NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                    break;
                case AttackState.SpawnAnimation:
                    ActiveAttackState = AttackState.CutoffMovement;
                    break;
                case AttackState.CutoffMovement:
                    if (AttackSubstate == 0)
                    {
                        if (TargetVector == Vector2.Zero || (player.velocity.Length() > 5 && MainTimer < 120) || NPC.Distance(player.Center) > 800)
                            TargetVector = player.velocity.SafeNormalize(new Vector2(player.direction, 0)) * 700;
                        var goalPos = (player.Center + TargetVector);
                        if (NPC.Distance(goalPos) > 80)
                        {

                            var dogRotation = player.DirectionTo(NPC.Center).ToRotation();
                            var DOGDIR = -(NPC.velocity.X * player.DirectionTo(NPC.Center).Y - NPC.velocity.Y * player.DirectionTo(NPC.Center).X).DirectionalSign();

                            var goalpos = player.Center + new Vector2(700, 0).RotatedBy(dogRotation + 0.05f * DOGDIR);

                            var goalVel = NPC.DirectionTo(goalpos) * currentVelLength;


                            TurnTowards(goalpos, maxSpeed: 100);
                            NPC.velocity = VelocityRotation.ToRotationVector2() * (currentVelLength < 50 ? currentVelLength + 5f : currentVelLength > 55 ? currentVelLength - 5f : currentVelLength);

                            NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;
                        }
                        else
                        {
                            SwitchAttackState(ActiveAttackState, 1);
                            StateChangeCounter++;
                        }
                    }
                    else if (AttackSubstate == 1)
                    {
                        if (TargetVector == Vector2.Zero)
                            TargetVector = player.Center;
                        NPC.velocity *= 0.9f;
                        if (MainTimer % 20 == 19)
                        {
                            for (var i = 0; i < Segments.Count() - 1; i += 5 - (int)(MainTimer / 20))
                            {
                                if (i < 0)
                                    continue;
                                float projectileVelocity = 6f;
                                Vector2 velocityVector = Vector2.Normalize(TargetVector - Segments[i].Center) * projectileVelocity;
                                int type = ModContent.ProjectileType<ThanatosLaser>();
                                int damage = 50;//NPC.GetProjectileDamage(type);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), Segments[i].Center + velocityVector.SafeNormalize(Vector2.Zero) * 64, velocityVector, type, damage, 0f, Main.myPlayer,61f);   
                                Main.projectile[proj].timeLeft = 200;
                            }
                            SoundEngine.PlaySound(CommonCalamitySounds.ExoLaserShootSound);
                        }
                        if (MainTimer > 60)
                        {
                            if (StateChangeCounter > 2)
                            {
                                //NPC.velocity = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 20;
                                SwitchAttackState(AttackState.ArmorApproachPlayer, 0);
                                StateChangeCounter = 0;
                            }
                            SwitchAttackState(ActiveAttackState, 0);
                        }
                    }
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
                case AttackState.ArmorApproachPlayer:
                    var dist = NPC.Distance(player.Center);
                    if (NPC.Distance(player.Center) > 200)
                        TurnTowards(player.Center, 0, 5);
                    NPC.velocity = VelocityRotation.ToRotationVector2() * (currentVelLength < (dist < 400 ? 15 : 20) ? currentVelLength + 0.2f : currentVelLength > (dist < 400 ? 18 : 22) ? currentVelLength - 1f : currentVelLength);
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    if (MainTimer > 300)
                    {
                        NPC.velocity = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 20;
                        SwitchAttackState(AttackState.ChargingUpDash);
                        SoundEngine.PlaySound(ChargeSound);
                    }
                    break;
                case AttackState.ChargingUpDash:
                    var telegraph = new BloomLineVFX(NPC.Center, NPC.Center.DirectionTo(player.Center) * 10000, 1, Color.Red * (MainTimer / 180f), 2);
                    GeneralParticleHandler.SpawnParticle(telegraph);
                    Vector2 goalPos1 = player.Center - NPC.DirectionTo(player.Center) * 500;
                    if (NPC.Distance(goalPos1) > 0)
                        NPC.velocity = NPC.DirectionTo(goalPos1) * MathHelper.Min(NPC.Distance(goalPos1), MainTimer / 5f);
                    else
                        NPC.velocity = Vector2.Zero;
                    NPC.rotation = NPC.rotation.AngleLerp(NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2,0.5f);
                    ElectricStrength = MathF.Pow(MainTimer / 180, 0.25f);
                    if (MainTimer > 150)
                    {
                        ActiveAnimation = null;
                        AnimationFrame = 0;
                        NPC.velocity = NPC.DirectionTo(player.Center) * 70;
                        SoundEngine.PlaySound(CommonCalamitySounds.ExoPlasmaExplosionSound, player.Center);
                        SwitchAttackState(AttackState.ElectricDash, 0);
                    }
                    break;
                case AttackState.ElectricDash:
                    if (MainTimer > 90)
                    {
                        SwitchAttackState(AttackState.CutoffMovement, 0);
                    }
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
                case AttackState.ThunderRain:
                    if (AttackSubstate == 0)
                    {
                        if (TargetVector == Vector2.Zero || MainTimer == 0)
                            TargetVector = player.Center + new Vector2(player.velocity.Length() < 1 ? 0 : Main.rand.NextFloat(-500, 500));
                        telegraph = new BloomLineVFX(TargetVector - Vector2.UnitY * 2000, Vector2.UnitY * 4000, 1, Color.DeepSkyBlue * (MainTimer / 25f), 2);
                        GeneralParticleHandler.SpawnParticle(telegraph);
                        if (MainTimer > 30)
                        {
                            StateChangeCounter++;
                            SwitchAttackState(ActiveAttackState, 1, false);
                        }
                    }
                    if (AttackSubstate == 1)
                    {
                        if (ActiveAnimation is null)
                        {
                            ActiveAnimation = new();
                            ActiveAnimation.applyRotation = false;
                            ActiveAnimation.segmentRigidity = 1f;
                            ActiveAnimation.mirror = false;
                            ActiveAnimation.AnimationKeyframes = new()
                                        {
                                            {0, (new(),0) },
                                            {2, (new(),0) },
                                        };
                        }
                        if (MainTimer == 0)
                            NPC.Center = TargetVector - Vector2.UnitY * 2000;
                        NPC.velocity = Vector2.UnitY * 180;

                        if (StateChangeCounter < 7)
                        {
                            if (MainTimer > 5)
                            {
                                SwitchAttackState(ActiveAttackState, 0);
                            }
                        }
                        else if (MainTimer > 20)
                        {
                            StateChangeCounter = 0;
                            SwitchAttackState(AttackState.CutoffMovement);
                            ElectricStrength = 0;
                        }
                    }

                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
                case AttackState.CloudDashes:
                    if (AttackSubstate == 0)
                    {
                        if (TargetVector == Vector2.Zero || MainTimer == 0)
                        {
                            TargetVector = player.Center + new Vector2(Main.rand.NextFloat(-64, 64), Main.rand.NextFloat(-64, 64)) + player.velocity * 20;
                            SecondaryVector = player.DirectionTo(TargetVector) * (Main.rand.NextBool() ? 1 : -1);
                        }
                        telegraph = new BloomLineVFX(TargetVector - SecondaryVector * 2000, SecondaryVector * 4000, 1, Color.DeepSkyBlue * (MainTimer / 25f), 2);
                        GeneralParticleHandler.SpawnParticle(telegraph);
                        if (MainTimer > 30)
                        {
                            StateChangeCounter++;
                            SwitchAttackState(ActiveAttackState, 1, false);
                        }
                    }
                    if (AttackSubstate == 1)
                    {
                        if (ActiveAnimation is null)
                        {
                            ActiveAnimation = new();
                            ActiveAnimation.applyRotation = false;
                            ActiveAnimation.segmentRigidity = 1f;
                            ActiveAnimation.mirror = false;
                            ActiveAnimation.AnimationKeyframes = new()
                                        {
                                            {0, (new(),0) },
                                            {2, (new(),0) },
                                        };
                        }
                        if (MainTimer == 0)
                            NPC.Center = TargetVector - SecondaryVector * 2000;
                        NPC.velocity = SecondaryVector * 180;

                        if (StateChangeCounter < 7)
                        {
                            if (MainTimer > 1)
                            {
                                SwitchAttackState(ActiveAttackState, 0);
                            }
                        }
                        else if (MainTimer > 60)
                        {
                            StateChangeCounter = 0;
                            SwitchAttackState(AttackState.CoilDashes);
                        }
                    }
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
                case AttackState.CoilDashes:
                    switch (AttackSubstate)
                    {
                        case 0: //Coiling
                            SegmentFollowType = SegmentFollowLogic.Exact;
                            if (MainTimer == 1)
                            {
                                NPC.Center = player.Center + new Vector2(Main.rand.NextBool() ? -800 : 800, 400);
                                TargetVector = NPC.DirectionTo(player.Center - (Vector2.UnitY * 400));

                                ActiveAnimation = null;

                                NPC.velocity = TargetVector * 32;
                                NPC.netUpdate = true;
                            }
                            if (MainTimer % 35 >= 5)
                            {
                                NPC.velocity = NPC.velocity.RotatedBy(MathHelper.TwoPi / 30f);
                            }
                            else
                                NPC.velocity = TargetVector * 32;
                            ElectricStrength = MainTimer / 180;
                            if (MainTimer >= 180)
                                SwitchAttackState(ActiveAttackState, 1);
                            break;
                        case 1: //Repositioning Dash

                            SegmentFollowType = SegmentFollowLogic.Exact;
                            if (MainTimer == 1)
                            {
                                NPC.velocity = NPC.DirectionTo(player.Center).RotatedBy(Main.rand.NextBool() ? MathHelper.PiOver4 : -MathHelper.PiOver4) * 48;
                                NPC.netUpdate = true;
                            }
                            ElectricStrength = Math.Clamp(MainTimer / 30, 0, 1);
                            if (MainTimer > 30)
                            {
                                if (StateChangeCounter >= 9)
                                    SwitchAttackState(ActiveAttackState, 3);
                                else
                                {
                                    float dir = NPC.DirectionTo(player.Center).ToRotation();
                                    if (VelocityRotation.AngleBetween(dir) < 0.05f)
                                    {
                                        /*int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ElectricBomb>(), 50, 0f, ai0: 8, ai1: 30);
                                        Main.projectile[index].rotation = NPC.Center.DirectionTo(player.Center).ToRotation();*/
                                        SwitchAttackState(ActiveAttackState, 2);
                                        StateChangeCounter++;
                                    }
                                    else
                                    {
                                        VelocityRotation = VelocityRotation.AngleTowards(dir, MathHelper.Pi / 12f);
                                    }
                                }
                            }
                            break;
                        case 2: //Attack Dash
                            SegmentFollowType = SegmentFollowLogic.Exact;
                            if (MainTimer == 1)
                                NPC.velocity = NPC.DirectionTo(player.Center) * 48;
                            dist = NPC.position.Distance(player.Center);
                            if (MainTimer > 10 && dist > 700 && dist > NPC.oldPosition.Distance(player.Center))
                            {
                                if (StateChangeCounter >= 9)
                                    SwitchAttackState(ActiveAttackState, 3);
                                else
                                {
                                    ElectricStrength = 0;
                                    /*int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<AresGaussNukeProjectile>(), 50, 0f, ai0: 8, ai1: 30);
                                    Main.projectile[index].rotation = NPC.Center.DirectionTo(player.Center).ToRotation();*/
                                    SwitchAttackState(ActiveAttackState, 1);
                                    StateChangeCounter++;
                                }
                            }
                            break;
                        case 3: //Attack Switch

                            if (MainTimer == 1)
                                /*foreach (Projectile p in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<ElectricBomb>()))
                                    p.ModProjectile<ElectricBomb>().Explode = true;*/

                            if (MainTimer > 90)
                            {
                                StateChangeCounter = 0;
                                SwitchAttackState(AttackState.CloudDashes);
                            }
                            break;
                    }

                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    break;
                case AttackState.LightingSpit:
                    break;
                case AttackState.DeathAnimation:
                    break;
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ModContent.ItemType<MysteriousCircuitry>(), 1, 4, 8);
            npcLoot.Add(ModContent.ItemType<DubiousPlating>(), 1, 4, 8);
            npcLoot.AddIf(() => Main.zenithWorld, ModContent.ItemType<UnholyEssence>(), 1, 3, 6, ui: false);
            npcLoot.AddIf(() => Main.zenithWorld, ModContent.ItemType<SanctifiedSpark>(), 10, ui: false);
        }
    }
}
