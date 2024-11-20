using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Typeless;
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
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), -Projectile.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.1f, 0.5f));
                dust.noGravity = false;
                dust.scale = 0.9f;
                dust.color = Color.DodgerBlue;
                dust.noLightEmittence = true;
            }
            time++;
        }

        public override void OnKill(int timeLeft)
        {
            // Explode on kill if attacking, else just poof out
            if (attacking)
            {
                SoundEngine.PlaySound(SoundID.Item110, Projectile.Center);

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
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(9, 12));
                    dust.noGravity = true;
                    dust.scale = 1.8f;
                    dust.color = Color.Cyan;
                    dust.noLightEmittence = true;
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
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
