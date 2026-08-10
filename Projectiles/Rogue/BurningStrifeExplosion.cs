using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class BurningStrifeExplosion : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 130;
        Projectile.height = 130;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 40;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        Projectile.ai[1]++;
        if (Projectile.ai[1] % 3f == 0f)
        {
            for (var i = 0; i < 3; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, Main.rand.NextVector2Circular(8f, 8f), Scale: Main.rand.NextFloat(1f, 1.5f));
            }

            Vector2 spawnPos = new Vector2(Projectile.position.X + Main.rand.NextFloat(Projectile.width), Projectile.position.Y + Main.rand.NextFloat(Projectile.height));
            FlameParticle flame = new(spawnPos, 10, 0.45f, 0.05f, Color.Violet, Color.DarkViolet);
            GeneralParticleHandler.SpawnParticle(flame);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.ShadowFlame, 180);

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Shadowflame>(), 180);
}
