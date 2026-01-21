using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class DragonsBreathFlames : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public int Time = 0;
        public float beamWidth = 1.1f;
        public bool beamsize = false;
        public float bbbBONUSbeamSizeWOAHH = 50;
        public bool postHit = false;
        public int beamWeldBloomReduction = 0;
        public ref int audioCooldown => ref Main.player[Projectile.owner].Calamity().DragonsBreathAudioCooldown;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 6;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Vector3 Light = Projectile.ai[0] == 1 ? new Vector3(0.255f, 0.080f, 0.080f) : new Vector3(0.255f, 0.060f, 0.000f);
            Lighting.AddLight(Projectile.Center, Light * (Projectile.ai[0] == 1 ? 5 : 3));
            if (Time == 0)
            {
                bbbBONUSbeamSizeWOAHH += Projectile.ai[2];
                beamWidth = Projectile.ai[1];
                if (Projectile.ai[0] == 1)
                    Projectile.penetrate = 1;
            }
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center); //used for some drawing prevention for when it's offscreen
            float sine = MathHelper.Clamp(Math.Abs((float)Math.Sin((Time + Projectile.ai[1]) * 0.575f / MathHelper.Pi)), 0.6f + Time * 0.0015f, 1);
            beamWidth = sine;
            //Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 22.5f;

            if (Projectile.ai[0] == 0) //Fireball Mode
            {
                if (Time == 0)
                {
                    for (int i = 0; i <= 7; i++)
                    {
                        int DustType1 = 259;
                        Vector2 dustyVelocity = Projectile.velocity * 4f;
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? DustType1 : 174);
                        dust.scale = dust.type == DustType1 ? Main.rand.NextFloat(0.9f, 1.9f) : Main.rand.NextFloat(0.8f, 1.7f);
                        dust.velocity = dustyVelocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.3f, 0.8f);
                        dust.noGravity = true;
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? DustType1 : 174);
                        dust2.scale = dust.type == DustType1 ? Main.rand.NextFloat(0.85f, 1.8f) : Main.rand.NextFloat(0.75f, 1.6f);
                        dust2.velocity = dustyVelocity.RotatedByRandom(0.15f) * Main.rand.NextFloat(0.8f, 2.1f);
                        dust2.noGravity = true;
                    }
                    for (int i = 0; i <= 5; i++)
                    {
                        Vector2 smokeVel = Projectile.velocity * 4.2f;
                        float rotationAngle = Main.rand.NextFloat(-0.15f, 0.15f);
                        smokeVel = smokeVel.RotatedBy(rotationAngle) * Main.rand.NextFloat(0.45f, 1.3f);

                        float smokeScale = Main.rand.NextFloat(0.4f, 1.2f);

                        SmallSmokeParticle smoke = new SmallSmokeParticle(Projectile.Center, smokeVel, Color.DimGray, Main.rand.NextBool() ? Color.SlateGray : Color.Black, smokeScale, 100);
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }
                if (Time % 9 == 0 && Time > 14f && targetDist < 1400f)
                {
                    SparkParticle spark = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(1 + (Time * 0.15f), 1 + (Time * 0.15f)), -Projectile.velocity * 0.5f, false, 15, Main.rand.NextFloat(0.4f, 0.7f), Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (targetDist < 1400f)
                {
                    //ModContent.GetInstance<DragonsBreathFlameMetaball2>().SpawnParticle(Projectile.Center, Time * 0.9f);
                    //ModContent.GetInstance<DragonsBreathFlameMetaball>().SpawnParticle(Projectile.Center + Projectile.velocity, Time * 0.85f);

                    Particle beam3 = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/SmallBloom", false, 10, 0.1f + Time * 0.0015f, Color.OrangeRed * 0.5f, new Vector2(1f, 1), true, false, 0, false, false, 0.5f);
                    GeneralParticleHandler.SpawnParticle(beam3);
                    Particle beam33 = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/SmallBloom", false, 10, 0.04f + Time * 0.0015f, Color.Lerp(Color.Orange, Color.White, 0.25f) * 0.5f, new Vector2(1f, 1), true, false, 0, false, false, 0.5f);
                    GeneralParticleHandler.SpawnParticle(beam33);
                }
                if (Time < 120)
                    Projectile.velocity *= 1.01f;
            }
            if (Projectile.ai[0] == 1) //Welding Mode
            {
                if (bbbBONUSbeamSizeWOAHH > 0)
                    bbbBONUSbeamSizeWOAHH -= 0.65f;

                Projectile.extraUpdates = 20;

                if (Time > 0)
                {
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), Main.rand.NextBool(3) ? 162 : 259);
                    dust2.scale = Main.rand.NextFloat(0.3f, 0.5f);
                    dust2.noGravity = true;
                    if (Main.rand.NextBool(6))
                    {
                        dust2.velocity = new Vector2(0, Main.rand.NextFloat(-1, -13));
                    }
                    if (targetDist < 1400f)
                    {
                        //ModContent.GetInstance<DragonsBreathBeamMetaball2>().SpawnParticle(Projectile.Center, beamWidth * 46 + bbbBONUSbeamSizeWOAHH);
                        //ModContent.GetInstance<DragonsBreathBeamMetaball>().SpawnParticle(Projectile.Center, beamWidth * 45 + bbbBONUSbeamSizeWOAHH);

                        Particle beam3 = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/SmallBloom", false, 8, (0.2f + bbbBONUSbeamSizeWOAHH * 0.0015f) * beamWidth, Color.OrangeRed, new Vector2(1f, 1), true, false, 0, false, false, 0.6f);
                        GeneralParticleHandler.SpawnParticle(beam3);
                        Particle beam33 = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), "CalamityMod/Particles/SmallBloom", false, 8, (0.1f + bbbBONUSbeamSizeWOAHH * 0.0015f) * beamWidth, Color.Lerp(Color.Orange, Color.White, 0.3f), new Vector2(1f, 1), true, false, 0, false, false, 0.6f);
                        GeneralParticleHandler.SpawnParticle(beam33);
                    }
                }
                if (Time == 0)
                {
                    for (int i = 0; i <= 3; i++)
                    {
                        Vector2 sparkVelocity = Projectile.velocity * 3;

                        float sparkScale1 = Main.rand.NextFloat(0.006f, 0.008f) * 2;
                        Vector2 sparkvelocity1 = sparkVelocity.RotatedByRandom(0.85f) * Main.rand.NextFloat(0.4f, 0.95f);
                        Particle spark1 = new GlowSparkParticle(Projectile.Center, sparkvelocity1, false, 8, sparkScale1, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed, new Vector2(2, 1), true, false, 1.3f);
                        GeneralParticleHandler.SpawnParticle(spark1);

                        float sparkScale2 = Main.rand.NextFloat(0.01f, 0.017f) * 2;
                        Vector2 sparkvelocity2 = sparkVelocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.1f, 3.1f);
                        Particle spark2 = new GlowSparkParticle(Projectile.Center, sparkvelocity2, false, 8, sparkScale2, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed, new Vector2(2, 1), true, false, 1.3f);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                }
            }
            Time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 1)
            {
                if (beamWeldBloomReduction < 3)
                {
                    if (audioCooldown == 0)
                    {
                        SoundEngine.PlaySound(DragonsBreath.WeldingBurn, Projectile.Center);
                        audioCooldown = 12;
                    }
                    //float OrbSize = Main.rand.NextFloat(0.5f, 0.8f) - beamWeldBloomReduction * 0.14f;
                    //Particle orb = new GenericBloom(target.Center, Vector2.Zero, Color.OrangeRed, OrbSize + 0.6f, 8, true);
                    //GeneralParticleHandler.SpawnParticle(orb);
                    //Particle orb2 = new GenericBloom(target.Center, Vector2.Zero, Color.White, OrbSize + 0.2f, 8, true);
                    //GeneralParticleHandler.SpawnParticle(orb2);

                    Particle blast = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/SmallBloom", false, 7, Main.rand.NextFloat(0.6f, 0.7f), Color.OrangeRed, Vector2.One, true, false);
                    GeneralParticleHandler.SpawnParticle(blast);
                    Particle blast2 = new CustomSpark(Projectile.Center, Vector2.Zero, "CalamityMod/Particles/SmallBloom", false, 7, Main.rand.NextFloat(0.4f, 0.5f), Color.OrangeRed, Vector2.One, true, true);
                    GeneralParticleHandler.SpawnParticle(blast2);

                    if (!postHit)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 SpawnPosition = target.Center + Main.rand.NextVector2Circular(target.width, target.height) * 0.04f;
                            Vector2 splatterDirection = (Projectile.Center - SpawnPosition).SafeNormalize(Vector2.UnitY);
                            int sparkLifetime = Main.rand.Next(22, 36);
                            float sparkScale = Main.rand.NextFloat(0.8f, 1.3f);
                            Color sparkColor = Main.rand.NextBool(4) ? Color.OrangeRed : Color.Orange;
                            Vector2 sparkVelocity = splatterDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(12f, 25f);
                            SparkParticle spark = new SparkParticle(target.Center, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        for (int i = 0; i <= 3; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, DustID.FireworksRGB, Projectile.velocity.RotatedByRandom(100) * Main.rand.NextFloat(2.5f, 8.5f), 0, default, Main.rand.NextFloat(0.5f, 1.1f));
                            dust.noGravity = true;
                            dust.color = Main.rand.NextBool() ? Color.Orange : Color.OrangeRed;
                        }
                        for (int i = 0; i <= 3; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, Main.rand.NextBool(3) ? 293 : 174, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.5f, 3.5f), 0, default, Main.rand.NextFloat(1.6f, 2.3f));
                            dust.noGravity = true;
                            Dust dust2 = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, Main.rand.NextBool(3) ? 293 : 174, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(0.8f, 5.8f), 0, default, Main.rand.NextFloat(1.6f, 2.3f));
                            dust2.noGravity = true;
                        }
                        postHit = true;
                    }
                    beamWeldBloomReduction++;
                }

                float sine = MathHelper.Clamp(Math.Abs((float)Math.Sin((Time + Projectile.ai[1]) * 0.575f / MathHelper.Pi)), 0.2f + Time * 0.0022f, 1);
                Particle blastRing = new CustomPulse(target.Center, Vector2.Zero, Color.Lerp(Color.Orange, Color.OrangeRed, sine) * 0.7f, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(0.2f, 1.2f), 2.5f * Main.rand.NextFloat(0.9f, 1.2f), 15, true);
                GeneralParticleHandler.SpawnParticle(blastRing);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DragonsBreathBurst>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                target.AddBuff(ModContent.BuffType<Dragonfire>(), 420);
            }
            else if (!postHit)
            {
                for (int i = 0; i <= 5; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, Main.rand.NextBool(3) ? 293 : 174, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.5f, 3.5f), 0, default, Main.rand.NextFloat(1.6f, 2.3f));
                    dust.noGravity = true;
                    Dust dust2 = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, Main.rand.NextBool(3) ? 293 : 174, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(0.8f, 5.8f), 0, default, Main.rand.NextFloat(1.6f, 2.3f));
                    dust2.noGravity = true;
                }
                for (int i = 0; i <= 2; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, DustID.FireworksRGB, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.5f, 3.5f), 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.Orange : Color.OrangeRed;
                }
                postHit = true;
            }

            //Doze - Flamethrowers in vanilla are long debuff infliction tools (20 seconds of their debuff).
            //I am applying this as the base for Cal flamethrowers, with shorter times being the exception instead of the rule
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 1200);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Player Owner = Main.player[Projectile.owner];
            if (Time <= 1)
            {
                float _ = float.NaN;
                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Owner.Center, Projectile.width, ref _);
            }
            else
                return CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.ai[1] == 1 ? 4 : Projectile.width + Time * 0.1f, targetHitbox);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.numHits > 0)
                Projectile.damage = (int)(Projectile.damage * 0.80f); // 20% damage nerf for every enemy hit
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }
    }
}
