using CalamityMod.CalPlayer;
using CalamityMod.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Pets
{
    public class BabyGhostBell : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Pets";
        private bool underwater = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            Main.projPet[Type] = true;
            ProjectileID.Sets.LightPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
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
                modPlayer.babyGhostBell = false;
            }
            if (modPlayer.babyGhostBell)
            {
                Projectile.timeLeft = 2;
            }
            underwater = Collision.DrownCollision(player.position, player.width, player.height, player.gravDir);
            if (underwater)
            {
                EnhancedDarknessSystem.lights.Add(new() { center = Projectile.Center, rotation = 0, scale = 3, texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle") });
                Lighting.AddLight(Projectile.Center, 0.3f, 0.9f, 1.5f);
            }
            else
            {
                Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.5f);
            }
            Projectile.FloatingPetAI(false, 0.05f, true);
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }
        }
    }
}
