using CalamityMod.Items.Mounts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ExoTankHoverThrust : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 5;

        public override void SetDefaults()
        {
            Projectile.width = 186;
            Projectile.height = 74;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (Owner is null || Owner.dead || Owner.mount is null || !Owner.mount.Active)
                Projectile.Kill();

            if (Owner.mount._type != ModContent.MountType<ExoTank>())
                Projectile.Kill();

            var data = (ExoTank.ExoTankData)Owner.mount._mountSpecificData;
            if (!data.Hovering)
                Projectile.Kill();

            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 3 % Main.projFrames[Type];

            Projectile.Center = Owner.Bottom + Vector2.UnitY * (Projectile.height / 2);
            Projectile.spriteDirection = Projectile.direction = Owner.direction;
            Projectile.timeLeft = 2;

            // Left thruster
            Dust flame = Dust.NewDustPerfect(Projectile.Top - Vector2.UnitX * Main.rand.NextFloat(44f, 64f) * Owner.direction, DustID.BlueTorch);
            flame.noGravity = true;
            flame.noLight = true;
            flame.velocity = Owner.velocity + Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(6f)) * Main.rand.NextFloat(5f, 12f);
            flame.scale = Main.rand.NextFloat(0.5f, 1.5f);

            // Right thruster
            flame = Dust.NewDustPerfect(Projectile.Top + Vector2.UnitX * Main.rand.NextFloat(30f, 50f) * Owner.direction, DustID.BlueTorch);
            flame.noGravity = true;
            flame.noLight = true;
            flame.velocity = Owner.velocity + Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(6f)) * Main.rand.NextFloat(5f, 12f);
            flame.scale = Main.rand.NextFloat(0.5f, 1.5f);
        }

        public override bool ShouldUpdatePosition() => false;

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
