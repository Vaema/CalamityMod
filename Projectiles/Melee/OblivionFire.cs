using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee;

public class OblivionFire : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Melee";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
    public ref float Time => ref Projectile.ai[0];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.MaxUpdates = 4;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.noEnchantmentVisuals = true;
    }

    public override void AI()
    {
        Time++;
        if (Time >= 100f)
            CalamityUtils.HomeInOnNPC(Projectile, false, 300, 2.5f, 20f, true);
        CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(2, 2), 16f);
    }

    public override bool? CanDamage() => Time >= 45f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
}
