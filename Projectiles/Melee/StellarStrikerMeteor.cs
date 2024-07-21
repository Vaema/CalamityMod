using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class StellarStrikerMeteor : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public ref float time => ref Projectile.ai[0];
        public Color mainColor = Color.Turquoise;
        public int fallTime = 60;
        public bool spawnMet = true;
        public int direction = 1;
        public float wavePower = 7;
        public NPC chosenTarget;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 800;
            Projectile.penetrate = 1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            if (time % 5 == 0 && Projectile.extraUpdates < 12)
                Projectile.extraUpdates ++;
            if (time == 0)
            {
                chosenTarget = Owner.Calamity().mouseWorld.ClosestNPCAt(2000);
                if (chosenTarget != null)
                    Projectile.velocity = (chosenTarget.Center - Projectile.Center + chosenTarget.velocity * 8).SafeNormalize(Vector2.UnitX) * 3;
                else
                    Projectile.velocity = (Owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 3;
            }
            if (Projectile.numHits < 1)
            {
                if (chosenTarget == null || chosenTarget.life <= 0)
                    chosenTarget = Owner.Calamity().mouseWorld.ClosestNPCAt(700);
                if (chosenTarget != null)
                {
                    Vector2 moveTotarget = (chosenTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    if (Projectile.velocity.Length() < 5)
                        Projectile.velocity += moveTotarget * 0.15f;
                    else
                        Projectile.velocity *= 0.8f;
                }
            }

            if (targetDist < 1400f)
            {
                if (time % 11 == 0)
                {
                    GlowSparkParticle orb = new(Projectile.Center + Main.rand.NextVector2Circular(20, 20), -Projectile.velocity * 2, false, 11, 0.03f, mainColor * Main.rand.NextFloat(0.7f, 1), new Vector2(1f, 1f), true, false, 0.6f);
                    GeneralParticleHandler.SpawnParticle(orb);
                }
                if (time % 4 == 0)
                {
                    GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(0.2f, 1.5f), Main.rand.NextBool(4) ? Color.PaleTurquoise : Color.Turquoise, 6, Main.rand.NextFloat(0.3f, 0.7f), 0.65f, 0, true));
                } 
            }
            if (time % 2 == 0)
            {
                // Spawn in a helix-style pattern
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * wavePower;

                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset * direction, 267, -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.3f));
                dust.scale = Main.rand.NextFloat(0.85f, 0.95f);
                dust.noGravity = true;
                dust.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            time++;

            if (time == 20 && spawnMet && Projectile.ai[2] > 0)
            {
                spawnMet = false;
                Vector2 spawnSpot = Owner.Center + new Vector2(Main.rand.NextFloat(-550, 550), Main.rand.NextFloat(-750, -950));
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnSpot, Vector2.Zero, ModContent.ProjectileType<StellarStrikerMeteor>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0, Projectile.ai[2] - 1);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White, 1, tex);
            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.2f;
            int hitsToMinMult = 4;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }

        public override void OnKill(int timeLeft)
        {
            Player Owner = Main.player[Projectile.owner];
            SoundEngine.PlaySound(SoundID.Item89, Projectile.position);

            if (Projectile.ai[2] > 0 && spawnMet)
            {
                Vector2 spawnSpot = Owner.Center + new Vector2(Main.rand.NextFloat(-550, 550), Main.rand.NextFloat(-750, -950));
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnSpot, Vector2.Zero, ModContent.ProjectileType<StellarStrikerMeteor>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0, 0, Projectile.ai[2] - 1);
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.damage = (int)(Projectile.damage * 0.3f);
                Projectile.ExpandHitboxBy((int)(228f * Projectile.scale));
                Projectile.penetrate = -1;
                Projectile.Damage();
            }
            for (int i = 0; i < 6; i++)
            {
                Particle spark3 = new SparkParticle(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(3.5f, 14), false, 20, Main.rand.NextFloat(0.3f, 0.8f), Main.rand.NextBool(5) ? Color.PaleTurquoise : Color.Turquoise);
                GeneralParticleHandler.SpawnParticle(spark3);
            }
            for (int i = 0; i < 18; i++)
            {
                Dust c = Dust.NewDustPerfect(Projectile.Center, 267);
                c.velocity = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 10f + Owner.velocity * 0.5f;
                c.scale = Main.rand.NextFloat(0.8f, 0.9f);
                c.noGravity = true;
                c.color = Main.rand.NextBool(3) ? Color.PaleTurquoise : Color.Turquoise;
            }
            Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, mainColor * 0.7f, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 2f, 1f, 25, true);
            GeneralParticleHandler.SpawnParticle(blastRing);
            Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White * 0.7f, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(-10, 10), 1f, 0.3f, 25, true);
            GeneralParticleHandler.SpawnParticle(blastRing2);
        }
    }
}
