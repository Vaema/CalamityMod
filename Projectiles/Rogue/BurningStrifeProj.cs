using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class BurningStrifeProj : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.timeLeft = 720;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Projectile.ai[0]++;
        //Rotation code
        Projectile.rotation += Projectile.velocity.X * 0.05f * Projectile.direction;
        //Gravity
        Projectile.velocity.Y += 0.05f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;
        //Dust
        if (Projectile.ai[0] >= 25f)
        {
            Dust.NewDust(Projectile.Center, 1, 1, DustID.Shadowflame, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f, 0, default, 1.1f);
            Projectile.ai[0] = 0f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.7f;
        Projectile.velocity.X *= 0.9f;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.ShadowFlame, 180);
        OnHitEffect(target);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<Shadowflame>(), 180);
        OnHitEffect(target);
    }

    private void OnHitEffect(Entity target)
    {
        SoundEngine.PlaySound(SoundID.Item103, Projectile.Center);
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BurningStrifeExplosion>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
        if (Projectile.Calamity().stealthStrike)
        {
            for (int i = 0; i < 4; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Main.rand.NextVector2CircularEdge(10f, 10f), ModContent.ProjectileType<BurningTentacle>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
            }
        }
    }
}
