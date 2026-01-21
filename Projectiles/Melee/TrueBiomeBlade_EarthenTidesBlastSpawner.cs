using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class EarthenTidesBlastSpawner : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 50;
        }

        public override void AI()
        {
            // Set how long the spawner will exist
            // 30-40-50-60 for 25%-50%-75%-100%
            if (Projectile.ai[1] == 0f)
            {
                Projectile.timeLeft = (int)Projectile.ai[0];
                Projectile.ai[1]++;
            }

            // Constantly stay on top of the owner
            Projectile.Center = Main.player[Projectile.owner].Center;

            // Spawn dust blasts
            if (Projectile.timeLeft % 6 == 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    float randomX = Main.rand.NextFloat(-200f, 200f);
                    float randomY = Main.rand.NextFloat(-200f, 200f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X + randomX, Projectile.Center.Y + randomY), Vector2.Zero, ModContent.ProjectileType<EarthenTidesBlast>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Main.rand.NextFloat(0.96f, 1.04f));
                }
            }
        }

        public override bool? CanDamage() => false;
    }
}
