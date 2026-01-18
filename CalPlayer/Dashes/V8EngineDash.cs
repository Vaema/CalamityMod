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
    public class V8EngineDash : PlayerDashEffect
    {
        public static new string ID { get; private set; }

        public override DashCollisionType CollisionType => DashCollisionType.ShieldSlam;

        public override bool IsOmnidirectional => false;
        public int Time = 0;

        public override void Load()
        {
            ID = DashID;
        }

        public override float CalculateDashSpeed(Player player) => 6.5f;

        public override void OnDashEffects(Player player)
        {
            Time = 0;
            for (int m = 0; m < 3; m++)
            {
                PointParticle spark = new PointParticle(player.Center - player.velocity, -player.velocity * (0.08f * m), false, 25, 4f - (0.5f * m), (Main.rand.NextBool() ? Color.Firebrick : Color.OrangeRed) * 0.4f);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            Time++; // For VFX

            player.velocity.X *= 0.925f;

            if (Time % 2 == 0)
            {
                Vector2 dustVel = -player.velocity.RotatedBy(0.05f + MathHelper.Clamp(Time * 0.03f, 0, 0.55f)) * 0.75f;
                Vector2 dustVel2 = -player.velocity.RotatedBy(-0.05f - MathHelper.Clamp(Time * 0.03f, 0, 0.55f)) * 0.75f;

                PointParticle spark = new PointParticle(player.Center + new Vector2(0, -15 * player.direction) + dustVel, dustVel, false, 8, 1.4f, (Main.rand.NextBool() ? Color.Firebrick : Color.OrangeRed) * 0.66f);
                GeneralParticleHandler.SpawnParticle(spark);
                PointParticle spark2 = new PointParticle(player.Center + new Vector2(0, 15 * player.direction) + dustVel2, dustVel2, false, 8, 1.4f, (Main.rand.NextBool() ? Color.Firebrick : Color.OrangeRed) * 0.66f);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
        }
    }
}
