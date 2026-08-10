using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless;

// Modified clone of Atlas Munitions Laser to circumvent the minion attack type tag
public class ExoTankLaser : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "CalamityMod/Projectiles/Summon/AtlasMunitionsLaserOverdrive";

    public override void SetStaticDefaults() => Main.projFrames[Type] = 4;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.MaxUpdates = 3;
        Projectile.timeLeft = 240;
        Projectile.Opacity = 0f;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * Projectile.Opacity * 0.5f);

        Projectile.Opacity = Utils.GetLerpValue(240f, 235f, Projectile.timeLeft, true);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.frameCounter++;
        Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

        if (Projectile.velocity.Length() < 18f)
            Projectile.velocity *= 1.03f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.rand.NextBool())
        {
            int dustCount = 16;
            bool sideways = Main.rand.NextBool(6);
            float generalAngularOffset = Main.rand.NextFloatDirection() * MathHelper.Pi / 24f;
            if (sideways)
                dustCount += 36;

            for (int i = 0; i < dustCount; i++)
            {
                float offsetAngle = MathHelper.TwoPi * i / dustCount;

                float unitOffsetX = MathF.Pow(MathF.Cos(offsetAngle), 3f);
                float unitOffsetY = MathF.Pow(MathF.Sin(offsetAngle), 3f);

                Vector2 puffDustVelocity = new Vector2(unitOffsetX, unitOffsetY) * 5f;
                if (sideways)
                    puffDustVelocity = puffDustVelocity.RotatedBy(MathHelper.PiOver4) * 1.7f;
                else
                    puffDustVelocity = puffDustVelocity.RotatedBy(generalAngularOffset);

                Dust magic = Dust.NewDustPerfect(target.Center, DustID.TheDestroyer, puffDustVelocity);
                magic.scale = sideways ? 1.8f : 1f;
                magic.fadeIn = 0.5f;
                magic.noGravity = true;
            }
        }
    }
}
