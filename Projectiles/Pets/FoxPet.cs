using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Pets
{
    public class FoxPet : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 11;
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 7, 6)
            .WithOffset(-35f, 0f).WithSpriteDirection(1).WhenNotSelected(0, 0);
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.aiStyle = ProjAIStyleID.Pet;
            AIType = ProjectileID.Puppy;
            Projectile.penetrate = -1;
            Projectile.timeLeft *= 5;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            CalamityPlayer modPlayer = player.Calamity();
            if (player.dead)
            {
                modPlayer.fox = false;
            }
            if (modPlayer.fox)
            {
                Projectile.timeLeft = 2;
            }
            Projectile.spriteDirection = Projectile.direction;
        }
    }
}
