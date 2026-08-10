using System;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee;

public class SwordsmithsPrideAstralEnergy : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Melee";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        Timer++;

        if (Timer >= 40f)
        {
            CalamityUtils.HomeInOnNPC(Projectile, true, 480f, 13f, 20f);
        }

        Projectile.ai[1] += 0.18f;
        float angle = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        float pulse = (float)Math.Sin(Projectile.ai[1]);
        float radius = 9f;
        Vector2 offset = angle.ToRotationVector2() * pulse * radius;
        Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<AstralOrange>(), Vector2.Zero, Scale: 0.75f);
        Dust.NewDustPerfect(Projectile.Center - offset, ModContent.DustType<AstralBlue>(), Vector2.Zero, Scale: 0.75f);

        Color partColor = Color.Lerp(Color.Orange, Color.Blue, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.5f));
        GlowOrbParticle obligatory = new(Projectile.Center, Vector2.Zero, false, 2, 0.8f, partColor);
        GeneralParticleHandler.SpawnParticle(obligatory);
    }
}
