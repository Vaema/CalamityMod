using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class PrimroseKeepsakeDisplay : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 0, 1)
            .WithOffset(-22f, 0f).WithSpriteDirection(-1).WhenNotSelected(0, 0);
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1;
        }

        public override void AI()
        {
            Projectile.Kill();
        }
    }
}
