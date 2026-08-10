using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using CalamityMod.Buffs.DamageOverTime;
using System;

namespace CalamityMod.Projectiles.Ranged;

public class SpectralFlare : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Ranged";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 6;
        Projectile.scale = 1.15f;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.netImportant = true;
        Projectile.aiStyle = ProjAIStyleID.Flare;
        Projectile.alpha = 255;
        Projectile.penetrate = 3;
        Projectile.MaxUpdates = 2;
        Projectile.timeLeft = 600;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30 * Projectile.MaxUpdates;
        DrawOriginOffsetY = -10;
    }

    public override bool PreAI()
    {
        if (Projectile.alpha > 0)
        {
            Projectile.alpha -= 80;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
        }

        float xVel = Projectile.ai[0];
        float yVel = Projectile.ai[1];
        if (xVel == 0f && yVel == 0f)
            xVel = 1f;
        float dist = MathF.Sqrt(xVel * xVel + yVel * yVel);
        dist = 4f / dist;
        xVel *= dist;
        yVel *= dist;

        if (Projectile.alpha < 70)
        {
            Dust dust = Dust.NewDustDirect(Projectile.position - Vector2.UnitY * 2f, 7, 7, DustID.SpectreStaff, Projectile.velocity.X, Projectile.velocity.Y, Scale: 1.15f);
            dust.noGravity = true;
            dust.velocity *= 0.3f;
            dust.position.X -= xVel;
            dust.position.Y -= yVel;
            dust.velocity.X -= xVel;
            dust.velocity.Y -= yVel;
        }

        if (Projectile.localAI[0] == 0f)
        {
            Projectile.ai[0] = Projectile.velocity.X;
            Projectile.ai[1] = Projectile.velocity.Y;

            if (Projectile.localAI[1] == 1f)
            {
                Projectile.velocity.Y += 0.09f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;
            }
        }
        else
        {
            if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.localAI[0] = 0f;
                Projectile.localAI[1] = 1f;
            }

            Projectile.damage = 0;
        }
        Projectile.rotation = MathF.Atan2(Projectile.ai[1], Projectile.ai[0]) + MathHelper.PiOver2;

        return false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Nightwither>(), Main.rand.NextBool(3) ? 600 : 300);
}
