using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.DataStructures;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SepticSkewerHarpoon : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public ref float time => ref Projectile.ai[0];

        public Vector2[] OldVelocities = new Vector2[20];
        public bool canDamage = true;
        public bool returning = false;
        public float CenterX;
        public float CenterY;
        public bool setPosition = true;
        public int returnTime = 75;

        public NPC chosenTarget;
        public bool stuckInTarget = false;
        public bool canStick = true;
        public Vector2 placementCenter;
        float placementDistance;
        Vector2 placementVelocity;
        public Vector2 storedVelocity;
        public bool collideWithTiles = true;
        public bool hasHitTile = false;
        public Color bColor = Color.Chartreuse;

        public bool ripped = false;
        public bool pullingTarget = false;
        public bool spawnPullBlood = true;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }
        public Vector2 DrawStartPosition
        {
            get
            {
                if (Projectile.owner < 0 || Projectile.owner >= Main.player.Length)
                    return Vector2.Zero;
                return Main.player[Projectile.owner].Center;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 900;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            bool pullCheckValid = ((chosenTarget != null && chosenTarget.life < Projectile.damage * 15f && Projectile.ai[2] != 5 && chosenTarget.CanBeMoved(true)) || Main.zenithWorld);

            if (Projectile.ai[2] == 5)
            {
                ripped = true;
                pullingTarget = false;
            }
            if (Projectile.ai[1] > 0)
                Projectile.extraUpdates = (int)(MathHelper.Clamp(Projectile.ai[1], 1, 18));

            if (!stuckInTarget && (!hasHitTile && !returning))
                storedVelocity = Projectile.velocity;

            if (Main.zenithWorld)
            {
                returnTime = 90;
                Projectile.extraUpdates = 7;
            }

            if (time >= returnTime * (pullCheckValid ? 0.9f : stuckInTarget ? 5 : 1) || ripped)
            {
                stuckInTarget = false;
                returning = true;
                if (pullCheckValid && chosenTarget != null)
                {
                    pullingTarget = true;
                    canDamage = false;
                    chosenTarget.damage = 0;
                    chosenTarget.Center = Projectile.Center;
                    chosenTarget.velocity = Projectile.velocity;
                }
                else
                {
                    canDamage = true;
                }
            }

            if (returning)
            {
                int startTime = 1000;
                int endTime = startTime + (int)((pullingTarget ? 45 : ripped ? 25 : 85));
                if (setPosition)
                {
                    SoundStyle pull3 = new("CalamityMod/Sounds/Item/ChainPull");
                    SoundEngine.PlaySound(pull3 with { Pitch = (Main.rand.NextFloat(0, 0.05f)), Volume = (Projectile.numHits > 0 && ripped ? 0.6f : 0.4f) }, Owner.Center);

                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;
                    canStick = false;
                    time = 1000;
                    CenterX = Projectile.Center.X;
                    CenterY = Projectile.Center.Y;
                    setPosition = false;
                    if (Projectile.numHits > 0 && !pullingTarget)
                    {
                        float ripIntensity = ripped ? 2 : 1;
                        if (ripped)
                        {
                            SoundStyle pull = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveIchorShoot");
                            SoundStyle pull2 = new("CalamityMod/Sounds/Item/FinalDawnSlash");
                            SoundEngine.PlaySound(pull with { Pitch = Main.rand.NextFloat(0, -0.2f), Volume = 0.7f }, Projectile.Center);
                            SoundEngine.PlaySound(pull2 with { Pitch = Main.rand.NextFloat(0.3f, 0.4f), Volume = 0.9f }, Projectile.Center);
                        }
                        else
                        {
                            SoundStyle pull = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveShoot3");
                            SoundStyle pull2 = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit2");
                            SoundEngine.PlaySound(pull with { Pitch = -0.2f, Volume = 0.6f }, Projectile.Center);
                            SoundEngine.PlaySound(pull2 with { Pitch = -0.6f, Volume = 0.5f }, Projectile.Center);
                        }

                        for (int i = 0; i < 7 * ripIntensity; i++)
                        {
                            Vector2 vel = ((Projectile.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * -18).RotatedByRandom(0.2f * ripIntensity) * Main.rand.NextFloat(0.2f, 0.6f) * ripIntensity;
                            if (i % 3 == 0)
                            {
                                Particle spark = new AltLineParticle(Projectile.Center, vel, true, (int)(18 * ripIntensity), Main.rand.NextFloat(0.55f, 0.8f) * ripIntensity, Color.DarkRed * 0.8f);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }
                            else
                            {
                                Particle spark = new GlowOrbParticle(Projectile.Center, vel, true, (int)(18 * ripIntensity), Main.rand.NextFloat(0.55f, 0.8f) * ripIntensity, Color.DarkRed * 0.8f, false, false, false);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }

                            Dust dust = Dust.NewDustPerfect(Projectile.Center, 5, vel * 3, 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                            dust.noGravity = true;
                        }

                        if (!pullingTarget)
                        {
                            for (int i = 0; i < 4 + (ripped ? 2 : 0); i++)
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, ((Projectile.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * -18).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 1.2f), ModContent.ProjectileType<SepticSkewerBacteria>(), Projectile.damage / 9, Projectile.knockBack, Projectile.owner, 0);
                        }
                    }
                }

                storedVelocity += (Projectile.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * 0.5f;
                Projectile.Center = new Vector2(MathHelper.Lerp(CenterX, Owner.Center.X, Utils.GetLerpValue(startTime, endTime, time, true)), MathHelper.Lerp(CenterY, Owner.Center.Y, Utils.GetLerpValue(startTime, endTime, time, true)));
                if (time >= endTime)
                    Projectile.Kill();

                if (time >= endTime - 2 && pullingTarget)
                {
                    if (spawnPullBlood && chosenTarget != null && chosenTarget.life > 0)
                    {
                        SoundStyle die = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
                        SoundEngine.PlaySound(die with { Pitch = Main.rand.NextFloat(0, 0.1f), Volume = 0.85f }, Projectile.Center);
                        for (int i = 0; i < 40; i++)
                        {
                            Vector2 vel = (Vector2.One * -28).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 0.9f);
                            if (i % 3 == 0)
                            {
                                Particle spark = new AltLineParticle(Projectile.Center, vel, true, (int)(35), Main.rand.NextFloat(0.55f, 1.3f), Color.DarkRed * 0.8f);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }
                            else
                            {
                                Particle spark = new GlowOrbParticle(Projectile.Center, vel, true, (int)(35), Main.rand.NextFloat(0.55f, 1.3f), Color.DarkRed * 0.8f, false, false, false);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }

                            for (int r = 0; r < 2; r++)
                            {
                                Vector2 vel2 = (Vector2.One * -28).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.8f);
                                Dust dust = Dust.NewDustPerfect(Projectile.Center, 5, vel2, 100, default, Main.rand.NextFloat(0.9f, 1.7f));
                                dust.noGravity = false;
                            }
                        }

                        NPC closestTarget = null;
                        float distance = 2000;
                        for (int index = 0; index < Main.npc.Length; index++)
                        {
                            if (Main.npc[index].CanBeChasedBy(null, false))
                            {
                                float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                                bool canHit = true;

                                if (Vector2.Distance(Projectile.Center, Main.npc[index].Center) < distance && canHit && Main.npc[index] != chosenTarget)
                                {
                                    distance = Vector2.Distance(Projectile.Center, Main.npc[index].Center);
                                    closestTarget = Main.npc[index];
                                }
                            }
                            if (Main.zenithWorld && Main.npc[index] != null && Main.rand.NextBool(15) && index < 80 && Main.npc[index].realLife == -1 && Owner.ownedProjectileCounts[Projectile.type] < 80)
                            {
                                closestTarget = Main.npc[index];
                                Projectile harpoon = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.Center, ((closestTarget.Center - Owner.Center + closestTarget.velocity * 1.5f).SafeNormalize(Vector2.UnitX) * 18), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.ai[1] + 1);
                            }
                        }
                        if (closestTarget != null)
                        {
                            Projectile harpoon = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.Center, ((closestTarget.Center - Owner.Center + closestTarget.velocity * 1.5f).SafeNormalize(Vector2.UnitX) * 18), Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, 0, Projectile.ai[1] + 1);
                        }

                        int heal = 25;
                        Owner.statLife += heal;
                        Owner.HealEffect(heal);
                        if (Owner.statLife > Owner.statLifeMax2)
                            Owner.statLife = Owner.statLifeMax2;

                        spawnPullBlood = false;
                    }
                    canDamage = true;
                }
            }

            if ((time <= returnTime * 0.6f && !stuckInTarget || returning && Main.rand.NextBool(5)))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9, 9), Main.rand.NextBool(7) ? 28 : 215, storedVelocity * Main.rand.NextFloat(0.05f, 0.15f), 0, default, Main.rand.NextFloat(0.5f, 0.9f));
                dust.noGravity = true;
            }

            if (stuckInTarget)
            {
                placementCenter = chosenTarget.Center + placementVelocity * placementDistance + storedVelocity * 2;

                Projectile.Center = placementCenter;

                if (chosenTarget.life <= 0 || chosenTarget == null)
                {
                    returning = true;
                    stuckInTarget = false;
                }
            }
            else if (time >= returnTime * 0.4f && !returning)
            {
                Projectile.velocity *= 0.95f;
            }

            if (collideWithTiles && Collision.SolidCollision(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30, 4, 4) && !Main.zenithWorld && Projectile.ai[1] < 1)
            {
                hasHitTile = true;
                Projectile.velocity *= -0.5f;
                Vector2 sparkVelocity = Projectile.velocity * 3;
                for (int i = 0; i < 6; i++)
                {
                    float sparkScale1 = Main.rand.NextFloat(0.3f, 0.8f);
                    Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.5f, 0.7f);
                    Particle spark1 = new LineParticle(Projectile.Center, sparkvelocity1, true, 40, sparkScale1, Main.rand.NextBool() ? bColor : Color.Green);
                    GeneralParticleHandler.SpawnParticle(spark1);
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2) with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
                }

                time = returnTime * 0.5f;
                collideWithTiles = false;
                canStick = false;
            }

            if (!hasHitTile || returning)
            {
                Projectile.rotation = storedVelocity.ToRotation() + MathHelper.ToRadians(90f);
            }
            AdjustOldVelocityArray();
            time++;
        }
        public void AdjustOldVelocityArray()
        {
            for (int i = OldVelocities.Length - 1; i > 0; i--)
                OldVelocities[i] = OldVelocities[i - 1];

            OldVelocities[0] = storedVelocity;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            bool pullCheckValid = ((chosenTarget != null && chosenTarget.life < Projectile.damage * 15f && Projectile.ai[2] != 5 && chosenTarget.CanBeMoved(true)) || Main.zenithWorld);

            if (!stuckInTarget && canStick)
            {
                for (int i = 0; i <= 8; i++)
                {
                    Vector2 sparkVelocity = Projectile.velocity * 0.5f;

                    float sparkScale1 = Main.rand.NextFloat(0.3f, 0.8f);
                    Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.45f) * Main.rand.NextFloat(0.5f, 0.7f);
                    Particle spark1 = new LineParticle(Projectile.Center, sparkvelocity1, false, 40, sparkScale1, Main.rand.NextBool() ? bColor : Color.Green);
                    GeneralParticleHandler.SpawnParticle(spark1);

                    float sparkScale2 = Main.rand.NextFloat(0.4f, 1f);
                    Vector2 sparkvelocity2 = sparkVelocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 1.6f);
                    Particle spark2 = new LineParticle(Projectile.Center, sparkvelocity2, false, 40, sparkScale2, Main.rand.NextBool() ? bColor : Color.Green);
                    GeneralParticleHandler.SpawnParticle(spark2);

                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 5, sparkvelocity2, 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                    dust.noGravity = true;
                }

                time = 1;
                collideWithTiles = false;
                canDamage = false;
                placementDistance = -Vector2.Distance(target.Center, Projectile.Center);
                placementVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                placementCenter = placementVelocity * (placementDistance * 0.01f);
                chosenTarget = target;
                stuckInTarget = true;
                canStick = false;
                storedVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                SoundStyle sound = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit2");
                SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);

                pullCheckValid = ((chosenTarget != null && chosenTarget.life < Projectile.damage * 15f && Projectile.ai[2] != 5 && chosenTarget.CanBeMoved(true)) || Main.zenithWorld);
                if (pullCheckValid)
                {
                    SoundStyle sound5 = new("CalamityMod/Sounds/Item/HeliumFlashCoreImpact");
                    SoundEngine.PlaySound(sound5 with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.3f, -0.4f) }, Projectile.Center);
                }
            }

            bool hitTarget = chosenTarget != null && target == chosenTarget;
            modifiers.SourceDamage *= hitTarget ? pullingTarget ? 20 : (ripped ? 2 : Projectile.numHits < 1 ? 0.01f : 1) : 0.2f;

            pullCheckValid = ((chosenTarget != null && chosenTarget.life < Projectile.damage * 15f && Projectile.ai[2] != 5 && chosenTarget.CanBeMoved(true)) || Main.zenithWorld);
            if (!pullCheckValid)
                target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
        }
        public override bool? CanDamage() => canDamage ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, !spawnPullBlood ? 150 : 25, targetHitbox);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D chain = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/SepticChain").Value;
            Vector2 end = Projectile.Center - storedVelocity.SafeNormalize(Vector2.UnitX) * 35;

            List<Vector2> controlPoints = new List<Vector2>
            {
                DrawStartPosition
            };
            for (int i = 0; i < OldVelocities.Length; i++)
            {
                float swayResponsiveness = Utils.GetLerpValue(0f, 6f, i, true) * Utils.GetLerpValue(OldVelocities.Length, OldVelocities.Length - 6f, i, true);
                Vector2 swayTotalOffset = OldVelocities[i] * swayResponsiveness;
                controlPoints.Add(Vector2.Lerp(DrawStartPosition, end, i / (float)OldVelocities.Length) + swayTotalOffset);
            }
            controlPoints.Add(end);

            int chainPointCount = (int)(Vector2.Distance(controlPoints.First(), controlPoints.Last()) / 7f);
            if (chainPointCount < 12)
                chainPointCount = 12;
            BezierCurve bezierCurve = new BezierCurve(controlPoints.ToArray());
            List<Vector2> chainPoints = bezierCurve.GetPoints(chainPointCount);

            for (int i = 0; i < chainPoints.Count; i++)
            {
                Vector2 positionAtPoint = chainPoints[i];
                if (Vector2.Distance(positionAtPoint, Projectile.Center) < 10f)
                    continue;
                float angleAtPoint = i == chainPoints.Count - 1 ? (end - chainPoints[i]).ToRotation() : (chainPoints[i + 1] - chainPoints[i]).ToRotation();
                angleAtPoint += MathHelper.PiOver2;
                Main.EntitySpriteDraw(chain,
                                 positionAtPoint - Main.screenPosition,
                                 null,
                                 Lighting.GetColor(positionAtPoint.ToTileCoordinates()),
                                 angleAtPoint,
                                 chain.Size() / 2f,
                                 0.85f,
                                 SpriteEffects.None,
                                 0);
            }
            return true;
        }
    }
}
