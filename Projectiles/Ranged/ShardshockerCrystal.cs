using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod.Tiles.Abyss;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.Actions.Sprites;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Melee;
using System;
using System.Data;
using CalamityMod.Items.Ammo;

namespace CalamityMod.Projectiles.Ranged
{
    public class ShardshockerCrystal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/Aquashard";
        public static readonly SoundStyle Collision = new("CalamityMod/Sounds/Item/ArcNovaDiffuserChargeImpact") { Volume = 0.5f };
        public int time = 0;

        public override void SetDefaults()
        {
            Projectile.width = 33;
            Projectile.height = 33;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
        }

        //The crystal can only damage enemies if it's not hanging in the air
        public override bool? CanDamage() => time <= 30 ? null : false;
        public override void AI()
        {
            //Set the projectile as traveling and only update rotation while traveling
            if (time < 30)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                Projectile.ai[1] = 1f;
            }

            // Make it lose velocity as it travels
            Projectile.velocity *= 0.9f;
            #region Traveling Visuals
            //Firing effects on frame one
            if (time == 1)
            {
                Vector2 smokeVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 5;
                int smokeAmount = Main.rand.Next(8, 12 + 1);
                for (int i = 0; i < smokeAmount; i++)
                {
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.2f, 2f), Color.White, Main.rand.Next(40, 60 + 1), Main.rand.NextFloat(0.2f, 0.4f), 0.3f, Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextBool(), required: true);
                    GeneralParticleHandler.SpawnParticle(smoke);

                }
                for (int i = 0; i < 5; i++)
                {
                    Particle spark = new GlowSparkParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.4f), false, 12, 0.009f, Color.DarkTurquoise, new Vector2(1.5f, 0.7f), true, false, 1.3f);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            //Only spawn dust trails while traveling
            if (time >= 2 && time <= 30)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 135 : 279, -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.05f, 0.2f) - new Vector2(0, 1));
                dust.scale = Main.rand.NextFloat(0.6f, 0.8f);
                dust.noGravity = false;
            }
            #endregion
            //After traveling for 30 frames, come to a stop in the air
            if (time >= 30)
            {
                //Set AI as stopped so traveling crystals can collide with it
                Projectile.ai[1] = 0f;
                Projectile.velocity = Vector2.Zero;
                Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(8f, 10f));
                Dust spark = Dust.NewDustPerfect(Projectile.Center, 278, velocity);
                spark.noGravity = true;
                spark.color = Main.rand.NextBool() ? Color.Aquamarine : Color.LightSkyBlue;
                if (Main.rand.NextBool(13))
                {
                    // Create a lightning bolt-like particle in the direction of the shot and 3 random hue-shifted ones by the side
                    Particle bolt = new CrackParticle(Projectile.Center, velocity * 0.5f, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, Vector2.One, 0, 0, Main.rand.NextFloat(0.4f, 0.5f), 12);
                    GeneralParticleHandler.SpawnParticle(bolt);

                    for (int i = 1; i < 4; i++)
                    {
                        Particle bolt2 = new CrackParticle(Projectile.Center, velocity.RotatedBy(MathHelper.PiOver2 * i + MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f))) * 0.5f, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, Vector2.One, 0, 0, Main.rand.NextFloat(0.2f, 0.3f), 12);
                        GeneralParticleHandler.SpawnParticle(bolt2);
                    }
                }
                //Create a lightning AoE every second
                if (time % 60 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item93, Projectile.Center);
                    // Create Blast
                    float blastSize = 120;
                    float minMultiplier = 0.25f;
                    int hitsToMinMult = 8;
                    Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage), 0f, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                    blast.DamageType = DamageClass.Ranged;
                    Particle blastRing3 = new CustomPulse(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.MediumTurquoise : Color.SkyBlue, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.12f, 30);
                    GeneralParticleHandler.SpawnParticle(blastRing3);
                }
                //Set up colliding behavior
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    // Only allow other large crystals to collide
                    if (proj.type != Type)
                        continue;

                    // Check if the colliding crystal is traveling
                    if (proj.ai[1] > 0f && Projectile.Hitbox.Intersects(proj.Hitbox))
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/LightningAura"), Projectile.Center);
                        //Set on kill effects and kill the stopped crystal
                        Projectile.ai[1] = 2f;
                        Projectile.Kill();
                        //Kill the traveling crystal
                        proj.timeLeft = 0;
                    }
                }
            }
            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Create a smaller AoE on direct hits
            float blastSize = 130;
            float minMultiplier = 0.25f;
            int hitsToMinMult = 4;
            Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.8f), 0f, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
            blast.DamageType = DamageClass.Ranged;

            target.AddBuff(ModContent.BuffType<GalvanicCorrosion>(), 60);

            #region Visuals and Sound
            SoundEngine.PlaySound(Collision, Projectile.Center);

            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.MediumAquamarine : Color.DarkSeaGreen, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.1f, 22, true);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumAquamarine, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-12, 12), 0f, 0.18f, 22, true);
            GeneralParticleHandler.SpawnParticle(blastRing2);
            Particle blastRing3 = new CustomPulse(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.12f, 22);
            GeneralParticleHandler.SpawnParticle(blastRing3);

            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2() * 0.5f; ;
                Particle cross = new SparkParticle(Projectile.Center + velocity * 15f, velocity, false, 15, 1.5f, Color.MediumAquamarine, true);
                GeneralParticleHandler.SpawnParticle(cross);
            }
            for (int k = 0; k < 3; k++)
            {
                Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkSlateBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.5f, 0.25f, 25);
                GeneralParticleHandler.SpawnParticle(outerGlow);
                Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumAquamarine, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.4f, 0.15f, 25);
                GeneralParticleHandler.SpawnParticle(innerGlow);
            }

            for (int i = 1; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(9f, 11f));
                Dust spark = Dust.NewDustPerfect(Projectile.Center, 278, velocity);
                spark.color = Main.rand.NextBool() ? Color.Aquamarine : Color.SkyBlue;
            }

            for (int i = 1; i < 6; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(8f, 10f));
                Particle bolt2 = new CrackParticle(Projectile.Center, velocity.RotatedBy(MathHelper.PiOver2 * i + MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f))) * 0.5f, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, Vector2.One, 0, 0, Main.rand.NextFloat(0.8f, 1.1f), 17);
                GeneralParticleHandler.SpawnParticle(bolt2);
            }
            #endregion
        }

        public override void OnKill(int timeLeft)
        {
            //These effects only occur if another crystal collides with a stopped one
            if (Projectile.ai[1] == 2f)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/LightningAura"), Projectile.Center);
                Player Owner = Main.player[Projectile.owner];
                if (Owner.Calamity().GeneralScreenShakePower < 1.2f)
                    Owner.Calamity().GeneralScreenShakePower = 1.2f;
                int projAmt = 15;
                for (int i = 0; i < projAmt; i++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(50f, 35f, 50f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AquashardSplit>(), (int)(Projectile.damage * 0.27f), 0f, Projectile.owner);
                }
                float blastSize = 185;
                float minMultiplier = 0.25f;
                int hitsToMinMult = 8;
                int debuff1 = BuffID.Electrified;
                int debuffTime = 240;
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 2f), 0f, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.localAI[0] = debuff1;
                blast.localAI[1] = debuffTime;
                blast.DamageType = DamageClass.Ranged;
                for (int i = 1; i < 9; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * (Main.rand.NextFloat(8f, 10f));
                    Particle bolt2 = new CrackParticle(Projectile.Center, velocity.RotatedBy(MathHelper.PiOver2 * i + MathHelper.ToRadians(Main.rand.NextFloat(-40f, 40f))) * 0.5f, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, Vector2.One, 0, 0, Main.rand.NextFloat(0.9f, 1.5f), 20);
                    GeneralParticleHandler.SpawnParticle(bolt2);
                }
                Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumAquamarine, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(-12, 12), 0f, 0.25f, 25, true);
                GeneralParticleHandler.SpawnParticle(blastRing);
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.MediumAquamarine : Color.DarkSeaGreen, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.13f, 25, true);
                GeneralParticleHandler.SpawnParticle(blastRing2);
                Particle blastRing3 = new CustomPulse(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.DarkTurquoise : Color.SkyBlue, "CalamityMod/Particles/PlasmaExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.16f, 25, true);
                GeneralParticleHandler.SpawnParticle(blastRing3);
                float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2() * 0.5f; ;
                    Particle cross = new SparkParticle(Projectile.Center + velocity * 15f, velocity, false, 15, 2.5f, Color.MediumAquamarine, true);
                    GeneralParticleHandler.SpawnParticle(cross);
                }
                for (int k = 0; k < 3; k++)
                {
                    Particle outerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkSlateBlue, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.5f, 0.25f, 25);
                    GeneralParticleHandler.SpawnParticle(outerGlow);
                    Particle innerGlow = new CustomPulse(Projectile.Center, Vector2.Zero, Color.MediumAquamarine, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 0.4f, 0.15f, 25);
                    GeneralParticleHandler.SpawnParticle(innerGlow);
                }
            }
            //These effects always occur
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 0.8f }, Projectile.Center);
            for (int i = 0; i < (int)(15); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), 0, default, Main.rand.NextFloat(1f, 1.3f));
                dust.noGravity = !Main.rand.NextBool();
                dust.color = Main.rand.NextBool(5) ? Color.DeepSkyBlue : Color.DarkTurquoise;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            //Don't draw the projectile for the first two frames, to prevent it from coming out of the player
            if (time < 2)
            {
                return false;
            }
            return base.PreDraw(ref lightColor);
        }
    }
}
