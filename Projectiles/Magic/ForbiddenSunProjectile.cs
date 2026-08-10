using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class ForbiddenSunProjectile : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "CalamityMod/Projectiles/Melee/VolcanicFireball";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 26;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.frameCounter++;
        Projectile.frame = Projectile.frameCounter / 4 % Main.projFrames[Type];
        Lighting.AddLight(Projectile.Center, 0.25f, 0.2f, 0.01f);
        if (Projectile.wet && !Projectile.lavaWet)
        {
            Projectile.Kill();
        }
        if (Projectile.localAI[0] == 0f)
        {
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            Projectile.localAI[0] += 1f;
        }
        if (Main.rand.NextBool(4))
        {
            Dust fire = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, Main.rand.NextBool(3) ? 16 : 174);
            fire.noGravity = true;
            fire.velocity *= 0f;
        }
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        if (Projectile.owner == Main.myPlayer)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ForbiddenSunburst>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.OnFire3, 300);
    }
}
