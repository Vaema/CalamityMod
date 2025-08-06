using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.Dashes
{
    public class AsgardianAegisDash : PlayerDashEffect
    {
        public static new string ID => "Asgardian Aegis";

        public override DashCollisionType CollisionType => DashCollisionType.ShieldSlam;
        public override bool IsOmnidirectional => false;
        public int Time = 0;
        public bool PostHit = false;

        public override int dashStartup => 12;

        public override float CalculateDashSpeed(Player player) => 23.3f;

        public override void OnDashEffects(Player player)
        {
            Time = 0;
            PostHit = false;
        }

        public override void DashStartupEffects(Player player)
        {
            player.velocity *= 0.9f;
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            Time += 2;
            float radiusFactor = MathHelper.Lerp(0f, 1f, Utils.GetLerpValue(2f, 2.5f, Time, true));
            if (DashTimeAdjustedForStartup <= 18)
            {
                for (int d = 0; d < 2; d++)
                {
                    Dust hFlameDust = Dust.NewDustPerfect(player.Center + new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-15f, 15f)) - (player.velocity * 1.2f), Main.rand.NextBool(8) ? 180 : 295, -player.velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.1f, 0.8f), 0, default, Main.rand.NextFloat(1.8f, 2.8f));
                    hFlameDust.shader = GameShaders.Armor.GetSecondaryShader(player.cShield, player);
                    hFlameDust.noGravity = hFlameDust.type == 180 ? false : true;
                    hFlameDust.fadeIn = 0.5f;
                    if (hFlameDust.type == 180)
                    {
                        hFlameDust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                        hFlameDust.velocity += new Vector2(0, -2.5f) * Main.rand.NextFloat(0.8f, 1.2f);
                    }

                    Dust dust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(6, 6) - player.velocity * 2, DustID.FrostStaff);
                    dust.velocity = -player.velocity * Main.rand.NextFloat(0.6f, 1.4f);
                    dust.scale = Main.rand.NextFloat(0.9f, 1.4f);
                    dust.noGravity = true;
                    if (d < 1)
                    {
                        Particle spark = new CustomSpark(player.Center + new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-15f, 15f)) - (player.velocity * 1.2f), -player.velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.1f, 0.8f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 17, Main.rand.NextFloat(1.15f, 1.25f), Main.rand.NextBool() ? Color.Fuchsia : Color.Cyan, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.4f, 0.5f));
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
            }

            // Dash at a faster speed than the default value.
            dashSpeed = 16f;
        }

        public override void OnHitEffects(Player player, NPC npc, IEntitySource source, ref DashHitContext hitContext)
        {
            if (DashTimeAdjustedForStartup > 18)
                return;
            if (!PostHit)
            {
                player.Calamity().GeneralScreenShakePower = 5f;
                PostHit = true;
            }

            Particle pulse = new DirectionalPulseRing(npc.Center, Vector2.Zero, Color.Aqua, new Vector2(2f, 2f), 0, 0.1f, 0.85f, 36);
            GeneralParticleHandler.SpawnParticle(pulse);

            Particle explosion2 = new DetailedExplosion(npc.Center, Vector2.Zero, Color.Magenta, Vector2.One, Main.rand.NextFloat(-5, 5), 0f, 0.65f, 26);
            GeneralParticleHandler.SpawnParticle(explosion2);

            // Define hit context variables.
            int hitDirection = player.direction;
            if (player.velocity.X != 0f)
                hitDirection = Math.Sign(player.velocity.X);
            hitContext.HitDirection = hitDirection;
            hitContext.PlayerImmunityFrames = AsgardianAegis.ShieldSlamIFrames;

            // Define damage parameters.
            hitContext.damageClass = DamageClass.Melee;
            hitContext.BaseDamage = AsgardianAegis.ShieldSlamDamage;
            hitContext.BaseKnockback = AsgardianAegis.ShieldSlamKnockback;

            // On-hit Cosmic Dash Explosion
            int explosionDamage = (int)player.GetBestClassDamage().ApplyTo(AsgardianAegis.RamExplosionDamage);
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<CosmicDashExplosion>(), explosionDamage, AsgardianAegis.RamExplosionKnockback, Main.myPlayer, 3f, 0f);
            npc.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);
        }
    }
}
