using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class RegulusEnergy : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public ref float Timer => ref Projectile.ai[0];
    public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 360;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Timer++;
        if (Timer % 2 == 0)
        {
            int dustType = Timer % 4 == 0 ? ModContent.DustType<AstralBlue>() : ModContent.DustType<AstralOrange>();
            Dust ambientDust = Dust.NewDustDirect(Projectile.position, Projectile.width - 28, Projectile.height - 28, dustType, 0f, 0f, 100, default, 1.5f);
            ambientDust.noGravity = true;
            ambientDust.velocity *= 0.1f;
            ambientDust.velocity += Projectile.velocity * 0.5f;
        }

        if (Timer < 60)
        {
            Projectile.velocity.X *= 0.98f;
            Projectile.velocity.Y *= 0.98f;
        }
        else
        {
            CalamityUtils.HomeInOnNPC(Projectile, true, 500f, 12f, 20f);
        }
    }

    public override bool? CanDamage() => Timer >= 60 ? null : false;

    public override void OnKill(int timeLeft)
    {
        int dustAmt = 18;
        for (int j = 0; j < dustAmt; j++)
        {
            float dustRotation = MathHelper.Pi / dustAmt * j;
            Vector2 dustVel = -Vector2.UnitY.RotatedBy(dustRotation) * (float)(Math.Sin(dustRotation) + 5f) * 2f;

            Dust deathDust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<AstralOrange>(), dustVel, 50, default, 0.8f);
            deathDust.noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 60);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 60);
}
