using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class RubicoPrimeMag : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/Ranged/RubicoPrimeMag";
        public int Time = 0;
        public bool TouchedGrass = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = ProjAIStyleID.GroundProjectile;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 700;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.extraUpdates = 0;
            Time++;
            Player Owner = Main.player[Projectile.owner];
            if (!TouchedGrass)
            {
                Projectile.rotation += 0.5f * (float)Projectile.direction;
            }
            Projectile.velocity.Y -= 0.055f;
            Projectile.velocity.X *= 0.992f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.damage = 0;
            TouchedGrass = true;
            Projectile.velocity *= 0.98f;
            return false;
        }
    }
}
