using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon;

public class StormjawSpark : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Summon";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    // Clone of Typeless/GenericElectricSpark but specfically benefits from whips
    public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;

    public override void SetDefaults()
    {
        Projectile.width = 6;
        Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 60;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<StaticDischarge>(), 120);

    public override void AI()
    {
        if (Projectile.velocity.X != Projectile.velocity.X)
            Projectile.velocity.X *= -0.1f;
        if (Projectile.velocity.Y != Projectile.velocity.Y && Projectile.velocity.Y > 1f)
            Projectile.velocity.Y *= -0.5f;

        Projectile.ai[0]++;
        if (Projectile.ai[0] > 5f)
        {
            Projectile.ai[0] = 5f;
            if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
            {
                Projectile.velocity.X *= 0.97f;
                if (MathF.Abs(Projectile.velocity.X) < 0.01f)
                {
                    Projectile.velocity.X = 0f;
                    Projectile.netUpdate = true;
                }
            }
            Projectile.velocity.Y += 0.2f;
        }
        Dust spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedWhiteBluePurple, Alpha: 100);
        spark.position.X -= 2f;
        spark.position.Y += 2f;
        spark.scale += Main.rand.NextFloat(0f, 0.5f);
        spark.noGravity = true;
        spark.velocity.Y -= 2f;
        if (Main.rand.NextBool())
        {
            spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedWhiteBluePurple, Alpha: 100);
            spark.position.X -= 2f;
            spark.position.Y += 2f;
            spark.scale += Main.rand.NextFloat(0.3f, 0.8f);
            spark.noGravity = true;
            spark.velocity *= 0.1f;
        }

        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;
}
