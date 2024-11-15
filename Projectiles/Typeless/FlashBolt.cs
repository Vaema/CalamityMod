using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class FlashBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public int time = 0;
        public float colorValue = 0;
        public float sizeMult = 1;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.penetrate = 1; // Survives through its first hit by "cheating" and incrementing its own pierce counter
            Projectile.extraUpdates = 75;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            // Visibility/Sound toggle on acc visibility
            bool visible = Owner.Calamity().arcFlashRingVisual;
            colorValue = MathHelper.Lerp(colorValue, 50, 0.025f);
            Color usedColor = Color.Lerp(Color.Cyan, Color.Orchid, Utils.GetLerpValue(0, 50, colorValue));

            if (time == 0)
            {
                colorValue += Main.rand.Next(0, 20);
                sizeMult = 1;
                if (visible)
                {
                    SoundStyle fire = new("CalamityMod/Sounds/Item/ArcFlash");
                    SoundEngine.PlaySound(fire with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);
                }
            }

            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (targetDist < 1400f)
            {
                Vector2 pos = Projectile.Center - Projectile.velocity * 15;
                if (Projectile.timeLeft % 4 == 0)
                {
                    Particle spark2 = new BoltParticle(pos, -Projectile.velocity * 0.05f, false, 30, 0.6f * sizeMult, usedColor * (visible ? 1 : 0.25f), new Vector2(1.8f, 0.8f), true, true, false, 0.3f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (Main.rand.NextBool(35))
                {
                    Particle spark2 = new BoltParticle(pos, Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.3f, 1.9f), false, 23, Main.rand.NextFloat(0.2f, 0.25f) * sizeMult, usedColor * (visible ? 1 : 0.3f), new Vector2(1.8f, 0.8f), true, true, false, 0.3f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (Main.rand.NextBool(10) && visible)
                {
                    Particle spark2 = new CustomSpark(pos, Projectile.velocity * Main.rand.NextFloat(-0.4f, 0.4f), "CalamityMod/Particles/DrainLineBloom", false, 80, Main.rand.NextFloat(1.2f, 1.3f) * sizeMult, usedColor * (visible ? 1 : 0.3f), new Vector2(1, 4), true, true);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                if (time % 5 == 0 && visible)
                {
                    Dust dust = Dust.NewDustPerfect(pos, 278, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), 0, default, Main.rand.NextFloat(0.45f, 0.6f));
                    dust.noGravity = true;
                    dust.color = usedColor;
                }
            }
            if (Projectile.numHits > 0)
            {
                sizeMult *= 0.97f;
            }
            time++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Stops the projectile from deleting itself, while maintaining the masquerade 
            Projectile.penetrate++;
            
            //colorValue = Main.rand.Next(0, 10);
            target.AddBuff(BuffID.Electrified, 180);
            Projectile.timeLeft = 25;
            Player Owner = Main.player[Projectile.owner];
            if (Owner.Calamity().arcFlashRingVisual && Projectile.numHits == 0)
            {
                Vector2 pos = target.Center;
                for (int i = 0; i < 10; i++)
                {
                    Particle spark2 = new BoltParticle(pos, new Vector2(8, 8).RotatedByRandom(100) * Main.rand.NextFloat(0.3f, 1.9f), true, 13, Main.rand.NextFloat(0.2f, 0.3f) * sizeMult, Main.rand.NextBool(5) ? Color.Cyan : Color.Orchid, new Vector2(1.8f, 0.8f), true, true, false, 0.8f);
                    GeneralParticleHandler.SpawnParticle(spark2);
                    Dust dust = Dust.NewDustPerfect(pos, 278, new Vector2(14, 14).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), 0, default, Main.rand.NextFloat(0.8f, 0.9f));
                    dust.noGravity = false;
                    dust.color = Main.rand.NextBool(5) ? Color.Cyan : Color.Orchid;
                }
                Particle pulse2 = new CustomPulse(pos, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/HighResFoggyCircleHardEdge", new Vector2(1, 1), 0, 0f, 0.0715f, 10);
                GeneralParticleHandler.SpawnParticle(pulse2);
                Particle orb = new CustomPulse(pos, Vector2.Zero, Color.Orchid, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.78f, 0.5f, 14);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(pos, Vector2.Zero, Color.White, "CalamityMod/Particles/LargeBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.325f, 0.2f, 14);
                GeneralParticleHandler.SpawnParticle(orb2);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        // Theres some leftover code here for an explosion on hit, in case we want that, if we are sure we dont, feel free to remove it
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.numHits > 0 ? false : CalamityUtils.CircularHitboxCollision(Projectile.Center, 60 * sizeMult * (Projectile.numHits > 1 ? 3 : 1), targetHitbox);
        public override bool? CanCutTiles() => false;
    }
}
