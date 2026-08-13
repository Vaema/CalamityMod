using CalamityMod.Events;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss;

public class RavagerFlame : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.coldDamage = true;
        Projectile.alpha = 255;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        Particle beam3 = new CustomSpark(Projectile.Center, new Vector2(1).RotatedByRandom(6.3f), "CalamityMod/Particles/BloomCircle", false, 10, 0.25f, Color.SkyBlue, new Vector2(1), true, false, 0, false, false, 0f);
        GeneralParticleHandler.SpawnParticle(beam3);
        for (int i = 0; i < 2; i++)
        {
            int icyFlame = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 1.5f);
            Main.dust[icyFlame].noGravity = true;
            Main.dust[icyFlame].velocity *= 0f;
        }
        if (Projectile.ai[1] == 0f)
        {
            Projectile.ai[1] = 1f;
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (info.Damage <= 0)
            return;

        target.AddBuff(BuffID.Frostburn, 180);
    }
}
