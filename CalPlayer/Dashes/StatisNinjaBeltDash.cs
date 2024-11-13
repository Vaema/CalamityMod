using CalamityMod.Enums;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CalamityMod.CalPlayer.Dashes
{
    public class StatisNinjaBeltDash : PlayerDashEffect
    {
        public static new string ID => "Statis' Ninja Belt";
        public override DashCollisionType CollisionType => DashCollisionType.NoCollision;
        public override bool IsOmnidirectional => false;
        public int Time = 0;

        public override float CalculateDashSpeed(Player player) => 20f;

        public override void OnDashEffects(Player player)
        {
            Time = 0;
            for (int i = 0; i < 12; i++)
            {
                Vector2 smokePos = player.Center + new Vector2(Main.rand.Next(-player.width, player.width), Main.rand.Next(-player.height, player.height)) * 0.8f;
                Vector2 smokeVel = Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(1.25f, 1.75f);
                HeavySmokeParticle onDashSmoke = new(smokePos, smokeVel, Color.Gray, 30, 0.5f, 0.75f, 0.2f * Main.rand.NextFloatDirection());
                GeneralParticleHandler.SpawnParticle(onDashSmoke);
            }
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            Time++;
            if (Time % 3 == 0)
            {
                int colorType = Main.rand.Next(3);
                Color smokeColor = Color.White;
                switch (colorType)
                {
                    case 0:
                        smokeColor = Color.Gray;
                        break;
                    case 1:
                        smokeColor = new Color(28, 75, 163);
                        break;
                    case 2:
                        smokeColor = new Color(195, 0, 255);
                        break;
                }

                Vector2 smokePos = player.Center - (player.velocity * 1.25f) + Main.rand.NextVector2Circular(10, 20);
                float smokeScale = player.velocity.X * 0.045f;
                HeavySmokeParticle midDashSmoke = new(smokePos, player.velocity * 0.2f, smokeColor, 20, smokeScale, 0.75f, 0.2f * Main.rand.NextFloatDirection(), true);
                GeneralParticleHandler.SpawnParticle(midDashSmoke);
            }
        }
    }
}
