using System;
using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class PulsePistolShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private Color mainColor = Color.Orchid;
        private bool doDamage = false;
        private NPC closestTarget = null;
        private NPC lastTarget = null;
        private float distance;
        private int timesItCanHit = 1;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 1240;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // If it's hit targeted enemies enough, kill it
            if (timesItCanHit <= 0)
            {
                if (Projectile.ai[1] < 5)
                {
                    Projectile uwa = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5).RotatedBy(0.3f) * 0.5f, Projectile.type, Projectile.damage / 4, Projectile.knockBack / 2, Projectile.owner, 0, 5);
                    Projectile uwaAgain = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5).RotatedBy(-0.3f) * 0.5f, Projectile.type, Projectile.damage / 4, Projectile.knockBack / 2, Projectile.owner, 0, 5);
                    uwa.penetrate = 1;
                    uwaAgain.penetrate = 1;
                }
                Projectile.Kill();
                return;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0f, 0.5f);
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();

            float createDustVar = 10f;

            if (Projectile.localAI[0] == 0)
            {
                if (Projectile.ai[1] == 1)
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Volume = 1.7f, Pitch = 0.3f }, Projectile.Center);
            }

            if (targetDist < 1400f)
            {
                GlowOrbParticle spark = new GlowOrbParticle(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), false, 5, 1.7f - Projectile.ai[1] * 0.18f, Main.rand.NextBool(3) ? Color.DarkViolet : mainColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
            if (targetDist < 1400f && Main.rand.NextBool())
            {
                GlowOrbParticle spark = new GlowOrbParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2f, 2f), -Projectile.velocity * Main.rand.NextFloat(-0.01f, 0.01f), false, 5, 1.7f - Projectile.ai[1] * 0.18f, Main.rand.NextBool(3) ? Color.DarkViolet : mainColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            if (Projectile.localAI[0] > 90)
            {
                // Velocity must look like it has stoped, but can't actually be zero otherwise homing code doesn't work
                if (Projectile.localAI[0] == 91)
                    Projectile.velocity *= 0.001f;

                // When they begin the homing after a hit of spawning, do a few visuals
                if (Projectile.localAI[0] == 120)
                {
                    distance = 3000;

                    SoundStyle fire = new("CalamityMod/Sounds/Item/PulseSound");
                    SoundEngine.PlaySound(fire with { Volume = 0.35f, Pitch = 0.3f, MaxInstances = -1 }, Projectile.Center);


                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, mainColor, new Vector2(1f, 1f), Main.rand.NextFloat(12f, 25f), 0f, 0.5f, 15);
                    GeneralParticleHandler.SpawnParticle(pulse);

                    for (int k = 0; k < 6; k++)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, 66);
                        dust.scale = Main.rand.NextFloat(0.6f, 1.1f);
                        dust.velocity = new Vector2(6, 6).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f);
                        dust.noGravity = true;
                        dust.color = Main.rand.NextBool(3) ? Color.DarkViolet : mainColor;
                        dust.noLight = true;
                    }
                }
                // Homing code
                if (Projectile.localAI[0] > 120)
                {
                    // Do damage if it's near its target otherwise don't, this is to prevent excessive hits as orbs rail through worm bosses and such
                    if (closestTarget is not null)
                        doDamage = Vector2.Distance(Projectile.Center, closestTarget.Center) < 10;
                    else
                        doDamage = false;

                    // Tracking code, originally was going to try only tracking the closest target not including the last target you hit, but I couldn't make it work
                    // Eventually I settled on how it works now and it seems to home consistently so I'm happy enough there
                    float projectileSpeed = 9.5f;
                    if (closestTarget is not null && closestTarget.active)
                    {
                        float targetDirectionRotation = Projectile.SafeDirectionTo(closestTarget.Center).ToRotation();
                        float turningRate = 10f + Projectile.localAI[0] * 0.00008f;
                        Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetDirectionRotation, turningRate).ToRotationVector2() * projectileSpeed;
                    }
                    else
                    {
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * projectileSpeed;
                        Projectile.velocity *= 0.999f;
                    }
                    if (closestTarget is not null && Vector2.Distance(Projectile.Center, closestTarget.Center) < 10)
                    {
                        closestTarget = null;
                        distance = 3000;
                    }

                    // Add extra updates as it hits more times, this smoothy increases the speed without destroying velocity based visual effects
                    Projectile.extraUpdates = 5 + (int)(Projectile.numHits * 0.3f);
                    {
                        // Actual homing movement
                        for (int index = 0; index < Main.npc.Length; index++)
                        {
                            if (Main.npc[index].CanBeChasedBy(null, false) || Main.npc[index] == lastTarget)
                            {
                                float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                                if (Vector2.Distance(Projectile.Center, Main.npc[index].Center) < (distance + extraDistance))
                                {
                                    closestTarget = Main.npc[index];
                                    distance = Vector2.Distance(Projectile.Center, Main.npc[index].Center);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Minor dust trail for when they first spawn
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5), 66);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
                    dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.5f);
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool(3) ? Color.DarkViolet : mainColor;
                    dust.noLight = true;
                }
                Projectile.velocity *= 0.98f;
            }

            // Visuals for the shot as it exits the tip of the rifle
            if (Projectile.localAI[0] == createDustVar)
                PulseBurst(4f, 5f);

            Projectile.localAI[0]++;
        }

        public override bool? CanHitNPC(NPC target) => doDamage ? null : false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool onKill = target.life <= 0;
            
            // Set some values to get ready foir it to home again for its next hit
            lastTarget = target;
            distance = 3000;
            Projectile.localAI[0] = 60;
            Projectile.velocity *= MathHelper.Clamp(1.5f - Projectile.numHits * 0.5f, 1f, 1.5f);

            for (int i = 0; i <= 2; i++)
            {
                SquishyLightParticle energy = new SquishyLightParticle(Projectile.Center, (Projectile.velocity * 2).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.1f, 0.4f), Main.rand.NextFloat(0.1f, 0.25f), Main.rand.NextBool(3) ? Color.DarkViolet : mainColor, Main.rand.Next(20, 30 + 1), 0.25f, 2f);
                GeneralParticleHandler.SpawnParticle(energy);
            }

            if (target == closestTarget)
                timesItCanHit--;

            if (onKill)
            {
                timesItCanHit += 2;
                Projectile.timeLeft += 90;
            }
        }
        private void PulseBurst(float speed1, float speed2)
        {
            for (int i = 0; i <= 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 66);
                dust.scale = Main.rand.NextFloat(0.4f, 1.4f);
                dust.velocity = (Projectile.velocity * 4).RotateRandom(0.6f) * Main.rand.NextFloat(0.2f, 0.9f);
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.DarkViolet : mainColor;
                dust.noLight = true;
            }
            for (int i = 0; i <= 8; i++)
            {
                SquishyLightParticle energy = new SquishyLightParticle(Projectile.Center, (Projectile.velocity * 4).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.1f, 0.4f), Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextBool(3) ? Color.DarkViolet : mainColor, Main.rand.Next(30, 40 + 1), 0.25f, 2f);
                GeneralParticleHandler.SpawnParticle(energy);
            }
        }
    }
}
