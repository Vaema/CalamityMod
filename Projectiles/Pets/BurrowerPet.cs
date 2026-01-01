using System;
using System.Collections.Generic;
using CalamityMod.Effects;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Deconstructors;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Systems;
using CalamityMod.Systems.Mechanic;
using CalamityMod.Tiles.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class BurrowerPet : BaseWormProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/NPCs/Deconstructors/DeconstructorMK1Head";
        public override List<string> SegmentTextures => new()
        {
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1Body",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1BodyAlt1",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1BodyAlt2",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1Tail"
        };

        public override List<string?> GlowTextures => new()
        {
            null,
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1BodyGlow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1BodyAlt1Glow",
            "CalamityMod/NPCs/Deconstructors/DeconstructorMK1BodyAlt2Glow"
        };
        public override int SegmentCount => 3;

        public override List<float> SegmentTypePositionOffsets => new()
        {
            32,
            32,
            32,
            32,
            32
        };
        public new string LocalizationCategory => "Projectiles.Pets";
        public Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.LightPet[Type] = true;
            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600; //100 tiles offscreen
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            for (var i = 0; i < SegmentCount - 1; i++)
            {
                Segments.Add(new BaseWormSegment(this, i % 3));
            }
            Segments.Add(new BaseWormSegment(this, 3));
        }

        public bool VerifyOwnerIsPresent()
        {
            // No logic should be run if the player is no longer active in the game.
            if (!Owner.active)
            {
                Projectile.Kill();
                return true;
            }

            if (Owner.dead)
                Owner.Calamity().burrowerPet = false;
            if (Owner.Calamity().burrowerPet)
                Projectile.timeLeft = 2;

            return false;
        }
        #region AI Variables

        public enum AttackState
        {
            Idle,
            Mining,
            Shocked
        }
        public AttackState ActiveAttackState
        {
            get { return (AttackState)Projectile.ai[1]; }
            set { Projectile.ai[1] = (float)value; }
        }
        public float MainTimer
        {
            get { return Projectile.ai[0]; }
            set { Projectile.ai[0] = value; }
        }

        public float AttackSubstate
        {
            get { return Projectile.ai[2]; }
            set { Projectile.ai[2] = value; }
        }

        public float StateChangeCounter
        {
            get { return Projectile.ai[3]; }
            set { Projectile.ai[3] = value; }
        }
        public float VelocityRotation
        {

            get { return Projectile.velocity.ToRotation(); }
            set { Projectile.velocity = value.ToRotationVector2() * Projectile.velocity.Length(); }
        }

        public Vector2 TargetVector = Vector2.Zero;
        public Vector2 SecondaryVector = Vector2.Zero;
        public float StoredValue = 0;
        #endregion

        public void SwitchAttackState(AttackState State, float Substate = 0, bool resetVector = true)
        {
            Projectile.netUpdate = true;
            ActiveAttackState = State;
            AttackSubstate = Substate;
            MainTimer = 0;
            if (resetVector)
                TargetVector = Vector2.Zero;
        }

        public override void AI()
        {
            if (VerifyOwnerIsPresent())
                return;

            HandleAIStates();
            MainTimer++;
            UpdateSegments();
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
            Player player = Main.player[Projectile.owner];
            SegmentMaxRotation = 0.65f;
            SegmentRigidity = 0.2f;
            var distanceToPlayer = Projectile.Distance(player.Center);
            bool onScreen = true;
            if (Main.netMode == NetmodeID.SinglePlayer) //In singleplayer we base the "off screen" check on the actual screen. In multiplayer, we base it off the maximum zoom size.
            {
                onScreen = Collision.CheckAABBvAABBCollision(Projectile.position, Projectile.Size, Main.screenPosition, Main.ScreenSize.ToVector2());
            }
            else
            {
                onScreen = Collision.CheckAABBvAABBCollision(Projectile.position, Projectile.Size, player.Center - new Vector2(990, 600), new Vector2(1980, 1200));
            }
            bool noGravity = (!onScreen || Projectile.wet || Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height, true));
            switch (ActiveAttackState)
            {
                case AttackState.Idle:
                    {
                        if (TargetVector == Vector2.Zero || MainTimer > 300 || (MainTimer > 120 && Projectile.Distance(TargetVector) < 64))
                        {
                            if (Main.rand.NextBool())
                            {
                                var veins = Burrower.FindOreVeins(Projectile.Center.ToTileCoordinates());
                                while (veins.Count > 0)
                                {
                                    var targetVein = veins[Main.rand.Next(veins.Count)];
                                    var foundTarget = Burrower.FindTargetFromVein(targetVein);
                                    if (foundTarget is not null)
                                    {
                                        TargetVector = foundTarget.Value.Item1.ToWorldCoordinates();
                                        SecondaryVector = foundTarget.Value.Item2.ToWorldCoordinates();
                                        if (Projectile.Distance(TargetVector) > 160)
                                            GeneralParticleHandler.SpawnParticle(new EmoteExpressionParticle(Projectile.Top, -Vector2.UnitY * 5, 2, ArsenalEffects.ArsenalGaussColor, 60, EmoteExpressionParticle.EmoteType.Exclamation));
                                        SwitchAttackState(AttackState.Mining, resetVector: false);
                                        return;
                                    }
                                    else
                                        veins.Remove(targetVein);
                                }
                            }
                            TargetVector = player.Center;
                            LowerTargetToGround();
                            MainTimer = 0;
                        }

                        var playerDistance = Projectile.Distance(Owner.Center);
                        float speed = 0.06f;
                        if (playerDistance > CalamityUtils.TilesToPixels(150))
                        {
                            Projectile.Center = Owner.Center - Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 200;
                            Projectile.velocity = Projectile.velocity.ClampMagnitude(1, 8);
                        }
                        else
                        {
                            if (playerDistance < 600 && !noGravity)
                                Projectile.velocity.Y += 0.5f;
                            if (playerDistance > 600)
                                speed = 0.5f;
                            if (playerDistance > 200f)
                                speed = 0.4f;
                            else if (playerDistance > 140f)
                                speed = 0.2f;
                            if (playerDistance > 100)
                                Projectile.velocity += Projectile.DirectionTo(Owner.Center) * speed;
                            else if (Projectile.velocity.Length() > 2)
                                Projectile.velocity *= 0.9f;

                            Projectile.velocity = Projectile.velocity.ClampMagnitude(0, 16);
                            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                        }
                        break;
                    }
                case AttackState.Mining:
                    {
                        Projectile.velocity += Projectile.DirectionTo(SecondaryVector).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity *= 0.9f;
                        if (MainTimer > 600)
                            SwitchAttackState(AttackState.Idle);
                        if (Projectile.Distance(SecondaryVector) < 4)
                        {
                            var dir = SecondaryVector.DirectionTo(TargetVector);

                            if (Main.tile[TargetVector.ToTileCoordinates()].TileType == ModContent.TileType<AuricOre>())
                            {
                                Projectile.velocity = -Projectile.DirectionTo(TargetVector) * 16;
                                Projectile.Center = SecondaryVector + Projectile.velocity;
                                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                                SwitchAttackState(AttackState.Shocked, 300); //Shock for 5 seconds
                                AuricOre.Animate = true;
                                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/TeslaShoot1"), Projectile.Center);
                                return;
                            }
                            SegmentRigidity = 0f;
                            Projectile.velocity = Vector2.Zero;
                            Projectile.rotation = SecondaryVector.DirectionTo(TargetVector).ToRotation() + MathHelper.PiOver2;

                            if (Main.netMode != NetmodeID.Server && !(BurrowerPingTileEffect.Instance.Active))
                                TilePingerSystem.AddPing(BurrowerPingTileEffect.Instance, Projectile.Center, player);
                            for (int i = 0; i < 1; i++)
                            {
                                int sparkLifetime = Main.rand.Next(10, 20);
                                float sparkScale = Main.rand.NextFloat(0.8f, 1f);
                                Color sparkColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.7f));
                                sparkColor = Color.Lerp(sparkColor, Color.Orange, Main.rand.NextFloat());

                                if (Main.rand.NextBool(10))
                                    sparkScale *= 2f;

                                Vector2 sparkVelocity = dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(6f, 16f);
                                SparkParticle spark = new SparkParticle((TargetVector + SecondaryVector) * 0.5f, -sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                                GeneralParticleHandler.SpawnParticle(spark);

                                if (MainTimer < 520)
                                    MainTimer = 520;
                            }
                            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.2f }, Projectile.Center);
                        }
                        else
                            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
                        break;
                    }
                case AttackState.Shocked:
                    {

                        if (AttackSubstate > 0)
                            AttackSubstate--;
                        else
                        {
                            SwitchAttackState(AttackState.Idle);
                            break;
                        }
                        if (noGravity)
                        {
                            SegmentRigidity = 0;
                            if (AttackSubstate < 295) //5 frames to get out of the auric ore that rejected it before it slows down from tiles
                                Projectile.velocity *= 0.55f;
                            foreach (var item in Segments)
                            {
                                if (!Collision.SolidCollision(item.Center - new Vector2(19, 17), 38, 38, true))
                                    item.Center.Y += 2f;
                            }
                        }
                        else
                        {
                            Projectile.velocity.Y += 0.5f;
                            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + MathHelper.PiOver2;
                        }
                        break;
                    }
            }
        }

    }
}
