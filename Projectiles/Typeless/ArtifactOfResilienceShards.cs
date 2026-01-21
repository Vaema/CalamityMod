using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    [PierceResistException]
    public class ArtifactOfResilienceShards : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public bool orbiting => Projectile.ai[1] == 0;
        public Player Owner => Main.player[Projectile.owner];

        public Vector2 goalPosition;
        public bool behind = false;
        public int relicType = 1;
        public float orbitSine = 0;

        public int burstTimer = 0;
        public float speedMult = 1;
        public float placementMult = 1;
        public float orbitRot = 0;
        public bool isAttacking => (burstTimer == 0 && !orbiting && Projectile.ai[1] != -1);
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 4;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 230;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }
        public override void AI()
        {
            float sine = (float)Math.Sin(time * 0.03f * speedMult / MathHelper.Pi);
            float sine2 = (float)Math.Sin(time * (0.03f * 0.5f * speedMult) / MathHelper.Pi);
            float sineNumberThreeSurelyWeNeedAThirdSineYouWillNotRegretAThirdSine = (float)Math.Sin((Main.GlobalTimeWrappedHourly + Owner.Calamity().rOfResilienceOrbitOffset) * (4.5f) / MathHelper.Pi);

            orbitSine = MathHelper.Lerp(Math.Abs(sine2), 0.1f, 1 - Math.Abs(sine2));
            float shardNumMult = Utils.GetLerpValue(-10, 30, Owner.ownedProjectileCounts[ModContent.ProjectileType<ArtifactOfResilienceShards>()], true) * (Owner.Calamity().profanedSoulRelicBuff ? 2 : 1);

            float displace = new Vector2(25, 0).RotatedBy(sineNumberThreeSurelyWeNeedAThirdSineYouWillNotRegretAThirdSine * 0.5f).ToRotation();
            orbitRot = Utils.AngleLerp(orbitRot, displace, 0.01f);
            goalPosition = Owner.Center + (new Vector2(250 * sine * shardNumMult * placementMult, (125 * orbitSine - 45) * shardNumMult * placementMult)).RotatedBy(orbitRot);

            Lighting.AddLight(Projectile.Center, Color.Lerp(Color.White, Color.Sienna, 0.5f).ToVector3() * 0.8f);

            if (Owner.Calamity().rOfResilienceCooldown > 0 && Projectile.ai[1] == 0)
            {
                burstTimer = 120;
                Projectile.ai[1] = 1;
                Projectile.netUpdate = true;
            }
            if (Owner.Calamity().rOfResilienceEffect == 0 && Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = -1;
                Projectile.timeLeft = 95;
                Projectile.velocity = Utils.DirectionTo(Projectile.Center, goalPosition) * Main.rand.NextFloat(1f, 3f);
            }

            if (time == 0)
            {
                relicType = Main.rand.Next(1, 6 + 1);
                speedMult = (Owner.Calamity().profanedSoulRelicBuff ? Main.rand.NextFloat(0.6f, 1.5f) : Main.rand.NextFloat(0.8f, 1.2f));
                placementMult = Main.rand.NextFloat(0.75f, 1.15f);
            }

            if (orbiting)
            {
                Projectile.scale = 0.8f + orbitSine * 0.5f;
                Projectile.rotation += 0.02f * sine;

                Projectile.timeLeft++;

                Projectile.Center = goalPosition;
                Projectile.velocity = Vector2.Zero;

                if (Projectile.scale < 0.9f)
                {
                    behind = true;
                    Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.15f, 0.057f);
                }
                else
                {
                    behind = false;
                    Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.7f, 0.027f);
                }
            }
            else
            {
                Projectile.Opacity = Utils.GetLerpValue(0, 90, Projectile.timeLeft, true);
                if (Projectile.ai[1] == -1)
                {
                    Projectile.velocity *= 0.98f;
                }
                else
                {
                    if (burstTimer > 0)
                    {
                        Projectile.extraUpdates = 1;
                        Projectile.velocity *= 0.9f;
                        if (Utils.Distance(Projectile.Center, Owner.Center) < 15)
                        {
                            behind = true;
                            Projectile.Center = Owner.Center;
                        }
                        else
                        {
                            Projectile.Center += Utils.DirectionTo(Projectile.Center, Owner.Center) * 40f * Utils.GetLerpValue(90, 0, burstTimer);
                            if (time % 8 == 0)
                            {
                                Particle spark = new CustomSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(3, 8), "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard" + Main.rand.Next(1, 6 + 1).ToString(), true, Main.rand.Next(20, 32 + 1), Main.rand.NextFloat(0.65f, 1.1f), Color.White * Main.rand.NextFloat(0.4f, 0.9f), Vector2.One, false, false, Main.rand.NextFloat(-5, 5), false, false);
                                GeneralParticleHandler.SpawnParticle(spark);
                            }
                        }
                        burstTimer--;
                        if (burstTimer == 0)
                        {
                            Projectile.Opacity = 1;

                            int projNum = Owner.ownedProjectileCounts[ModContent.ProjectileType<ArtifactOfResilienceShards>()] - 4;

                            Projectile.velocity = ((MathHelper.TwoPi * Projectile.ai[2] / projNum).ToRotationVector2()) * 15 * speedMult;
                            Owner.SetScreenshake(5f);

                            if (Projectile.ai[2] == 1)
                            {
                                SoundStyle boom = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianShieldDeactivate");
                                SoundEngine.PlaySound(boom with { Volume = 0.7f, Pitch = 0.1f }, Owner.Center);
                                SoundStyle boom2 = new("CalamityMod/Sounds/Item/MagicRockSound");
                                SoundEngine.PlaySound(boom2 with { Volume = 0.7f, Pitch = 0f }, Owner.Center);
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                Particle spark = new CustomSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(6, 28), "CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard" + Main.rand.Next(1, 6 + 1).ToString(), Main.rand.NextBool(3), Main.rand.Next(25, 55 + 1), Main.rand.NextFloat(1.2f, 1.7f), Color.White, new Vector2(0.9f, 1.1f), false, false, 0, false, false);
                                GeneralParticleHandler.SpawnParticle(spark);

                                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(5, 38));
                                dust.noGravity = Main.rand.NextBool();
                                dust.scale = Main.rand.NextFloat(1.35f, 2.8f);
                                dust.color = Main.rand.NextBool() ? Color.OrangeRed : Color.Sienna;
                            }
                        }
                    }
                    else
                    {
                        Projectile.velocity *= 0.97f;
                    }
                }
            }
            if (isAttacking && Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), -Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 8f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.55f, 1.1f) * Projectile.Opacity;
                dust.color = Main.rand.NextBool() ? Color.Orange : Color.Goldenrod;
            }

            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            bool crit = Main.rand.Next(0, 100 + 1) < Owner.GetTotalCritChance(Owner.GetBestClass());
            if (crit)
                modifiers.SetCrit();

            modifiers.SourceDamage *= (isAttacking ? 1 : 0.45f);
            if (isAttacking)
                target.AddBuff(ModContent.BuffType<ProfanedWeakness>(), 520);
        }
        public override void OnKill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = relicType switch
            {
                1 => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard1").Value,
                2 => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard2").Value,
                3 => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard3").Value,
                4 => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard4").Value,
                5 => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard5").Value,
                _ => ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ArtifactOfResilienceShard6").Value
            };
            if (isAttacking)
                Projectile.DrawProjectileWithBackglow(Color.Goldenrod with { A = 0 } * Projectile.Opacity, Color.White * Projectile.Opacity, 3 * Projectile.scale, tex);
            else
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (behind)
                behindProjectiles.Add(index);
            else
                overPlayers.Add(index);
        }
        public override bool? CanDamage() => (Projectile.ai[1] == -1 || burstTimer > 0) ? false : null;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(burstTimer);
            writer.Write7BitEncodedInt(Owner.Calamity().rOfResilienceCooldown);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            burstTimer = reader.Read7BitEncodedInt();
            Owner.Calamity().rOfResilienceCooldown = reader.Read7BitEncodedInt();
        }
    }
}
