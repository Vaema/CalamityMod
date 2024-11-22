using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Tiles.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class SeaDragonRocket : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public ref float time => ref Projectile.ai[0];
        public bool attacking => Projectile.ai[1] == 5; //  If the missile is launched at the enemy
        public ref float moveSpeed => ref Projectile.ai[2]; // Some speed variation applied to the missiles based on spawn order
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.5f);
            Player Owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Owner.ownedProjectileCounts[ModContent.ProjectileType<SeadragonHoldout>()] <= 0 && !attacking) // If no holdout exists and not attacking then die
                Projectile.Kill();
            if (attacking) // Fly at the enemy
            {
                NPC chosenTarget = Projectile.Center.ClosestNPCAt(600);
                CalamityUtils.HomeInOnSelectedNPC(Projectile, chosenTarget, true, 0.4f + moveSpeed, 16, 0.985f);
                if (chosenTarget != null)
                    Projectile.timeLeft++; // Don't die if you have a target to home in on
            }
            else // Swarm around the player until ready to be fired
            {
                Vector2 circle = Owner.Center + new Vector2(0, -195 + moveSpeed * 25).RotatedBy(time * 0.05f);
                Vector2 moveToEnemy = (circle - Projectile.Center).SafeNormalize(Vector2.UnitX);
                if (Projectile.velocity.Length() < 12)
                    Projectile.velocity = Projectile.velocity * 0.97f + moveToEnemy * moveSpeed;
                else
                    Projectile.velocity *= 0.9f;
            }
            if (Main.rand.NextBool(10))
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5, 5) - Projectile.velocity, 66);
                trailDust.scale = Main.rand.NextFloat(0.7f, 0.85f);
                trailDust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.5f);
                trailDust.color = Main.rand.NextBool() ? Color.AliceBlue : Color.SkyBlue;
                trailDust.noGravity = true;
                if (attacking)
                {
                    Particle trail = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(10, 10), -Projectile.velocity * Main.rand.NextFloat(0.2f, 0.8f), false, 15, Main.rand.NextFloat(0.6f, 0.8f), Main.rand.NextBool(3) ? Color.SeaGreen : Color.SkyBlue);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }
            if (Main.rand.NextBool(8) && !attacking)
            {
                Particle Star = new CritSpark(Projectile.Center, -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.5f), Color.SkyBlue, Main.rand.NextBool(3) ? Color.SeaGreen : Color.SkyBlue, Main.rand.NextFloat(0.4f, 0.7f), 30, 0.1f, 3f);
                GeneralParticleHandler.SpawnParticle(Star);
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            // Explode on kill if attacking, else just poof out
            if (attacking)
            {
                SoundStyle hitSound = new SoundStyle("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
                SoundEngine.PlaySound(hitSound with { Volume = 1.5f , Pitch = 0.7f , MaxInstances = -1 }, Projectile.Center);
                SoundStyle hitSound2 = new("CalamityMod/Sounds/NPCHit/AnahitaHit", 3);
                SoundEngine.PlaySound(hitSound2 with { Volume = 3f }, Projectile.Center);
                // Create Blast (If you want to know how to use this blast, check the projectile, it tells you exactly how to use it!)
                float blastSize = 80;
                float minMultiplier = 0.25f;
                int hitsToMinMult = 5;
                int debuff = ModContent.BuffType<CrushDepth>();
                int debuffTime = 120;
                Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                blast.localAI[0] = debuff;
                blast.localAI[1] = debuffTime;
                blast.timeLeft = 2;
                blast.DamageType = Projectile.DamageType;

                // Add visuals here
                for (int i = 0; i < 3; i++)
                {
                    Particle Star = new CritSpark(Projectile.Center, Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(4, 5), Color.SkyBlue, Main.rand.NextBool() ? Color.HotPink : Color.SeaGreen, Main.rand.NextFloat(0.6f, 0.9f), 30, 0.4f, 3f);
                    GeneralParticleHandler.SpawnParticle(Star);
                }
                Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, Color.SeaGreen, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.02f, 12, true);
                GeneralParticleHandler.SpawnParticle(blastRing);
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.HotPink, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0f, 0.25f, 12, true, 0.9f);
                GeneralParticleHandler.SpawnParticle(blastRing2);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(4, 6));
                    dust.noGravity = false;
                    dust.scale = 0.8f;
                    dust.color = Color.AliceBlue;
                    dust.noLightEmittence = true;
                }
            }
        }
        public override bool? CanDamage() => (attacking ? null : false); // Can't hit if not attacking
    }
}
