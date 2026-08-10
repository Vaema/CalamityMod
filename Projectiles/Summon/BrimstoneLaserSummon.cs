using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon;

public class BrimstoneLaserSummon : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Summon";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionShot[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.alpha = 120;
        Projectile.timeLeft = 300;
        Projectile.extraUpdates = 2;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI()
    {
        CalamityUtils.HomeInOnNPC(Projectile, false, 300, 5.5f, 10f, true);

        if (Projectile.Calamity().HomingTarget < 0)
        {
            Projectile.velocity.Y += 0.025f;
        }

        CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(2, 2), 16f);

        if (Projectile.velocity.Y > 16f)
        {
            Projectile.velocity.Y = 16f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300);

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300);
}
