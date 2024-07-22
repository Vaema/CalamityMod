using System;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.Projectiles.Rogue.FinalDawnFlame;

namespace CalamityMod.Projectiles.Magic
{
    public class HeliumFlashBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private static float ExplosionRadius = 500.0f;
        private static float ParticleRadius = 210.0f;
        private Player Owner => Main.player[Projectile.owner];
        public int frameX = 0;
        public int frameY = 0;
        private const int horizontalFrames = 5;
        private const int verticalFrames = 4;
        public int time = 0;
        public int currentFrame = 1;
        private const int frameLength = 2;
        public bool damageFrame = false;


        public override void SetDefaults()
        {
            // Width and height don't actually do anything because the explosion uses custom collision
            Projectile.width = 250;
            Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter % frameLength == frameLength - 1)
            {
                currentFrame++;

                frameY++;
                if (frameY >= verticalFrames)
                {
                    frameX++;
                    frameY = 0;
                }
                if (frameX >= horizontalFrames)
                {
                    Projectile.Kill();
                }
            }
            if (currentFrame == 15)
                damageFrame = true;
            else
                damageFrame = false;
            if (currentFrame <= 8)
            {
                float rotation = Main.rand.NextBool() ? 1f : -1f;
                float orbScale = MathHelper.Clamp(Utils.GetLerpValue(15, 0, time), 0, 1);
                Particle orb = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.Red, Color.OrangeRed, 8f * orbScale, 8, rotation, 3);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.White, Color.AntiqueWhite, 7f * orbScale, 8, rotation, 3);
                GeneralParticleHandler.SpawnParticle(orb2);
            }
            if (currentFrame == 9)
            {
                float rotation = Main.rand.NextBool() ? 2.5f : -2.5f;
                Particle sparkle = new GenericSparkle(Projectile.Center, Vector2.Zero, Color.Coral, Color.Orange, 3f, 9, rotation, 2);
                GeneralParticleHandler.SpawnParticle(sparkle);
            }
            if (currentFrame == 15)
            {
                Owner.Calamity().GeneralScreenShakePower = 8f;
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/HeliumFlashExplodeNoMetal") { Volume = 1.2f }, Projectile.Center);
                Particle explosion = new PlasmaExplosion(Projectile.Center, Vector2.Zero, Color.OrangeRed, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0006f + 0.1f, 22);
                GeneralParticleHandler.SpawnParticle(explosion);
                Particle explosion2 = new PlasmaExplosion(Projectile.Center, Vector2.Zero, Color.DarkOrange, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0004f + 0.1f, Main.rand.Next(15, 21));
                GeneralParticleHandler.SpawnParticle(explosion2);
                Particle explosion3 = new DetailedExplosion(Projectile.Center, Vector2.Zero, Color.Red, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0030f + 0.1f, Main.rand.Next(18), false);
                GeneralParticleHandler.SpawnParticle(explosion3);
                Particle explosion4 = new PlasmaExplosion(Projectile.Center, Vector2.Zero, Color.Red, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0006f + 0.1f, 25);
                GeneralParticleHandler.SpawnParticle(explosion4);
                Particle explosion5 = new FlameExplosion(Projectile.Center, Vector2.Zero, Color.OrangeRed, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0015f + 0.1f, 25, 1f);
                GeneralParticleHandler.SpawnParticle(explosion5);
                Particle explosion6 = new FlameExplosion(Projectile.Center, Vector2.Zero, Color.DarkOrange, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0018f + 0.1f, 25, 1f);
                GeneralParticleHandler.SpawnParticle(explosion6);
                Particle explosion7 = new FlameExplosion(Projectile.Center, Vector2.Zero, Color.Red, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, ParticleRadius * 0.0021f + 0.1f, 25, 1f);
                GeneralParticleHandler.SpawnParticle(explosion7);
                Vector2 randVel = new Vector2(30, 30).RotatedByRandom(100) * Main.rand.NextFloat(0.8f, 1.6f);
                Particle smoke = new HeavySmokeParticle(Projectile.Center + randVel, randVel, new Color(57, 46, 115) * 0.9f, Main.rand.Next(25, 35 + 1), Main.rand.NextFloat(0.9f, 2.3f), 0.4f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
        public override bool? CanDamage() => damageFrame ? null : false;
    }
}


