using CalamityMod.Particles;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.NPCs;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class TrackingDiskLaser : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float Time
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 100;
            Projectile.timeLeft = 600;
            Projectile.ArmorPenetration = 10;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            Time++;
            if (Time == 1)
            {
                for (int i = 0; i < 7; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 267);
                    dust.velocity = (Projectile.velocity * 3).RotatedByRandom(0.4f) * Main.rand.NextFloat(0.3f, 1f);
                    dust.scale = Main.rand.NextFloat(0.6f, 0.8f);
                    dust.noGravity = true;
                    dust.color = Color.Red;
                }
            }
            if (Time >= 15f)
            {
                if (Projectile.timeLeft % 11 == 0 && targetDist < 1400)
                {
                    Particle spark = new LineParticle(Projectile.Center - Projectile.velocity * 10, -Projectile.velocity * 0.01f, false, 4, 1.5f * Projectile.scale, Color.Red);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                if (Projectile.timeLeft % 2 == 0 && targetDist < 1400)
                {
                    Particle spark2 = new LineParticle(Projectile.Center - Projectile.velocity * 10, -Projectile.velocity * 0.01f, false, 4, 0.35f * Projectile.scale, Color.White);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Handles giving the NPC the laser burn effect
            CalamityGlobalNPC modNPC = target.Calamity();
            if (!modNPC.laserBurnMarked)
            {
                modNPC.laserBurnMarked = true;
                modNPC.laserBurnType = 1;
                modNPC.laserBurnTimer = CalamityGlobalNPC.laserBurnTime;
            }

            modNPC.laserBurnTimer -= modNPC.laserBurnStacks * 2;
            modNPC.laserBurnDamage += (int)(Projectile.damage * 0.2f);

            modNPC.laserBurnStacks++;

            if (Projectile.scale == 1)
            {
                Projectile.damage = 1;
                modifiers.HideCombatText();
            }
            else
                modifiers.SourceDamage *= 0.15f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 7; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 267);
                dust.velocity = (Projectile.velocity * 3 * Projectile.scale).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 1f);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f) * Projectile.scale;
                dust.noGravity = true;
                dust.color = Color.Red;
            }
        }
    }
}
