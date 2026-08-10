using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class TerraSigilSmallRock : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2; 
    }

    public override void SetDefaults()
    {
        Projectile.width = 26;
        Projectile.height = 32;
        Projectile.friendly = false; // VFX
        Projectile.damage = 0; // This is VFX
        Projectile.hostile = false; 
        Projectile.tileCollide = true; 
        Projectile.timeLeft = 100;
        Projectile.scale *= 0.6f;
    }

    public override void AI()
    {
        // Set the frame to be randomly chosen on spawn
        if (Projectile.ai[0] == 0)
        {
            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
            Projectile.ai[0] = 1;
        }

        // Gravity!
        Projectile.velocity.Y = Projectile.velocity.Y + 0.45f;

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }
}
