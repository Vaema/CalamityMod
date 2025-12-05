using System;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.Dashes
{
    public class StatisVoidSashDash : PlayerDashEffect
    {
        public static new string ID => "Statis' Void Sash";

        public override DashCollisionType CollisionType => DashCollisionType.NoCollision;

        public override bool IsOmnidirectional => false;
        public int Time = 0;
        public Vector2 aimVel;
        public bool strongVisuals = true;

        public override float CalculateDashSpeed(Player player) => (64f);

        public override void OnDashEffects(Player player)
        {

            strongVisuals = player.Calamity().voidSashVisuals;
            Time = 0;
            aimVel = player.velocity;

            if (strongVisuals)
            {
                SoundStyle sound = StatisVoidSash.VoidDash;
                SoundEngine.PlaySound(sound with { Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, player.Center);
                for (int i = 0; i < 40; i++)
                {
                    Vector2 intenededVel = (MathHelper.TwoPi * i / 40f).ToRotationVector2() * 3f;
                    Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2.3f).RotatedBy(player.velocity.ToRotation());
                    Vector2 fxPlace = player.Center + fxVel.RotatedBy(player.velocity.ToRotation()) + player.velocity.SafeNormalize(Vector2.UnitX) * 60;

                    SignusMetaball.SpawnParticle(fxPlace, fxVel - player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6, 9), 60f * Main.rand.NextFloat(0.7f, 1), 60, new Vector2(1.8f, 0.9f), 0.06f, 0);
                }
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 intenededVel = (MathHelper.TwoPi * i / 30f).ToRotationVector2() * 3f;
                Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2f).RotatedBy(player.velocity.ToRotation());
                Vector2 fxPlace = player.Center + fxVel.RotatedBy(player.velocity.ToRotation()) + player.velocity.SafeNormalize(Vector2.UnitX) * 60;

                Color dustColor = Main.rand.NextBool(3) ? Color.Indigo : Color.DarkOrchid;

                int dustStyle = ModContent.DustType<SquashDust>();
                Dust dust2 = Dust.NewDustPerfect(fxPlace, dustStyle, (fxVel - player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6, 9)) * Main.rand.NextFloat(1.2f, 1.5f));
                dust2.scale = Main.rand.NextFloat(1.6f, 2.3f) * 2.3f;
                dust2.color = dustColor;
                dust2.noGravity = true;
                dust2.fadeIn = 1.5f;
            }
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            Time++;
            aimVel = Vector2.Lerp(aimVel, player.velocity, 0.2f);

            float fxFade = Utils.GetLerpValue(5, 15, Math.Abs(aimVel.X), true);

            Vector2 trailPos = player.Center - (aimVel * 2) + Main.rand.NextVector2Circular(10, 20);
            float trailScale = 1 * fxFade;
            Color trailColor = Main.rand.NextBool(3) ? Color.Indigo : Color.DarkOrchid;

            int dustStyle = ModContent.DustType<SquashDust>();
            Dust dust2 = Dust.NewDustPerfect(trailPos, dustStyle, aimVel.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(9, 15));
            dust2.scale = Main.rand.NextFloat(0.9f, 1.7f);
            dust2.color = trailColor;
            dust2.noGravity = true;
            dust2.fadeIn = 0.5f;

            if (strongVisuals)
            {
                SignusMetaball.SpawnParticle(player.Center + Main.rand.NextVector2Circular(20, 20), -aimVel.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5, 15), 120f * Main.rand.NextFloat(0.7f, 1f), 60, new Vector2(0.8f, 1.2f), 0.08f, 0);

                int dir = MathF.Sign(aimVel.X);
                Vector2 safeVel = aimVel.SafeNormalize(Vector2.UnitX);
                float sparkscale2 = 0.35f * Math.Max(fxFade, 0.5f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 fxVelocity = safeVel.RotatedBy(dir * MathHelper.ToRadians(75) * -i) - safeVel * 3;
                    Vector2 fxPlace = player.Center + (safeVel * 35).RotatedBy(-0.2f * dir * i) * 1.5f;

                    SignusMetaball.SpawnParticle(fxPlace, fxVelocity * Main.rand.NextFloat(5, 8), 50f * Main.rand.NextFloat(0.7f, 1f), 8, new Vector2(0.8f, 1.2f), 0.18f, 0);
                }
            }
            player.velocity.X *= 0.95f;
            if (player.velocity.X > 140)
            {
                player.velocity.X = 140;
            }
            if (player.velocity.X < -140)
            {
                player.velocity.X = -140;
            }
            if (Time > 10)
                player.velocity.X *= 0.5f;
        }
    }
}
