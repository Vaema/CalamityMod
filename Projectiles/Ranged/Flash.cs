using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class Flash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0.5f / 255f);
            Color dustColor = Projectile.ai[1] == 1f ? CalamityUtils.ColorSwap(LeonidProgenitor.blueColor, LeonidProgenitor.purpleColor, 1f) : default;
            float randRot = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + randRot) * 3f;
                Dust flashDust = Dust.NewDustPerfect(Projectile.Center, DustID.PortalBoltTrail, dustVel, 100, dustColor, 1.5f);
                flashDust.noGravity = true;
            }
        }
    }
}
