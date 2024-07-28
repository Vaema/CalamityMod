using CalamityMod.NPCs.SunkenSea;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class SharkoonExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = Projectile.height = Sharkoon.ExplosionRadius * 2;
            Projectile.timeLeft = 2;
        }
    }
}
