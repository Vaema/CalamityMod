using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.Dashes;

public class StatisNinjaBeltDash : PlayerDashEffect
{
    public static new string ID { get; private set; }
    public override DashCollisionType CollisionType => DashCollisionType.NoCollision;
    public override bool IsOmnidirectional => false;
    public int Time = 0;

    public override void Load()
    {
        ID = DashID;
    }

    public override float CalculateDashSpeed(Player player) => (48f);

    public override void OnDashEffects(Player player)
    {

        Time = 0;
        for (int i = 0; i < 15; i++)
        {
            Vector2 intenededVel = (MathHelper.TwoPi * i / 15f).ToRotationVector2() * 1.5f;
            Vector2 fxVel = new Vector2(intenededVel.X, intenededVel.Y * 2.3f).RotatedBy(player.velocity.ToRotation());
            Vector2 fxPlace = player.Center + fxVel.RotatedBy(player.velocity.ToRotation()) + player.velocity.SafeNormalize(Vector2.UnitX) * 60;

            Color dustColor = Main.rand.NextBool() ? Color.Purple : Color.DarkSlateBlue;

            int dustStyle = ModContent.DustType<SquashDust>();
            Dust dust2 = Dust.NewDustPerfect(fxPlace, dustStyle, (fxVel - player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 5)) * Main.rand.NextFloat(1.2f, 1.5f));
            dust2.scale = Main.rand.NextFloat(1.6f, 2.3f) * 1.8f;
            dust2.color = dustColor;
            dust2.noGravity = true;
            dust2.fadeIn = 1.5f;
        }
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        Time++;
        for (int i = 0; i < 2; i++)
        {
            HeavySmokeParticle midDashSmoke = new(player.Center + Main.rand.NextVector2Circular(15, 15), -player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5, 9), Main.rand.NextBool() ? Color.Purple : Color.DarkSlateBlue, 16, Main.rand.NextFloat(0.7f, 0.9f), 0.55f, 0.2f * Main.rand.NextFloatDirection(), true);
            GeneralParticleHandler.SpawnParticle(midDashSmoke);
        }

        int dustStyle = ModContent.DustType<SquashDust>();
        Dust dust2 = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(15, 15), dustStyle, player.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5, 9));
        dust2.scale = Main.rand.NextFloat(0.9f, 1.7f);
        dust2.color = Main.rand.NextBool() ? Color.Purple : Color.DarkSlateBlue;
        dust2.noGravity = true;
        dust2.fadeIn = 0.5f;

        player.velocity.X *= 0.93f;

        if (player.velocity.X > 140)
        {
            player.velocity.X = 140;
        }
        if (player.velocity.X < -140)
        {
            player.velocity.X = -140;
        }
        if (Time > 8)
            player.velocity.X *= 0.5f;
    }
}
