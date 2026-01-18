using System;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.Dashes
{
    public class V8000EngineDash : PlayerDashEffect
    {
        public static new string ID { get; private set; }

        public override DashCollisionType CollisionType => DashCollisionType.ShieldSlam;

        public override bool IsOmnidirectional => false;
        public int Time = 0;

        public override void Load()
        {
            ID = DashID;
        }

        public override float CalculateDashSpeed(Player player) => 16f;

        public override void DashStartupEffects(Player player)
        {
            player.velocity *= 0.9f;
        }

        public override void OnDashEffects(Player player)
        {
            Time = 0;
            for (int m = 0; m < 3; m++)
            {
                PointParticle spark = new PointParticle(player.Center - player.velocity, -player.velocity * (0.08f * m), false, 25, 4f - (0.5f * m), (Main.rand.NextBool() ? Color.DodgerBlue : Color.LightSkyBlue) * 0.46f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            Time++; // For VFX

            if (Time % 2 == 0)
            {
                Vector2 dustVel = -player.velocity.RotatedBy(0.05f + MathHelper.Clamp(Time * 0.03f, 0, 0.55f)) * 0.75f;
                Vector2 dustVel2 = -player.velocity.RotatedBy(-0.05f - MathHelper.Clamp(Time * 0.03f, 0, 0.55f)) * 0.75f;

                PointParticle spark = new PointParticle(player.Center + new Vector2(0, -15 * player.direction) + dustVel, dustVel, false, 8, 1.4f, (Main.rand.NextBool() ? Color.DodgerBlue : Color.LightSkyBlue) * 0.66f);
                GeneralParticleHandler.SpawnParticle(spark);
                PointParticle spark2 = new PointParticle(player.Center + new Vector2(0, 15 * player.direction) + dustVel2, dustVel2, false, 8, 1.4f, (Main.rand.NextBool() ? Color.DodgerBlue : Color.LightSkyBlue) * 0.66f);
                GeneralParticleHandler.SpawnParticle(spark2);
                PointParticle spark3 = new PointParticle(player.Center + new Vector2(0, 45 * player.direction) + dustVel2, dustVel2, false, 8, 1.4f, (Main.rand.NextBool() ? Color.DodgerBlue : Color.LightSkyBlue) * 0.66f);
                GeneralParticleHandler.SpawnParticle(spark2);
            }

            dashSpeed = 14f;
        }

        public override void OnHitEffects(Player player, NPC npc, IEntitySource source, ref DashHitContext hitContext)
        {
            SoundStyle hit = new("CalamityMod/Sounds/Item/DoomsdayDeviceImpact");
            SoundEngine.PlaySound(hit with { Pitch = 0f, Volume = 0.4f }, player.Center);
            for (int i = 0; i <= 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, Main.rand.NextBool() ? 278 : 132, player.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(0.5f, 1f) + new Vector2(0, -3f));
                if (dust.type == 278)
                {
                    dust.scale = 1.2f;
                    dust.color = Main.rand.NextBool() ? Color.DodgerBlue : Color.LightSkyBlue;
                }
                else
                {
                    dust.scale = 0.9f;
                }
                dust.noGravity = false;
                dust.shader = GameShaders.Armor.GetSecondaryShader(player.cShield, player);
            }

            // Define hit context variables.
            int hitDirection = player.direction;
            if (player.velocity.X != 0f)
                hitDirection = Math.Sign(player.velocity.X);
            hitContext.HitDirection = hitDirection;
            hitContext.PlayerImmunityFrames = V8000Engine.ShieldSlamIFrames;

            // Define damage parameters.
            hitContext.damageClass = DamageClass.Melee;
            hitContext.BaseDamage = V8000Engine.ShieldSlamDamage;
            hitContext.BaseKnockback = V8000Engine.ShieldSlamKnockback;
        }
    }
}
