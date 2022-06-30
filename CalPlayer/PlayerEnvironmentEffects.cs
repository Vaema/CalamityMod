using CalamityMod.Projectiles.Environment;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer
{
    public partial class CalamityPlayer : ModPlayer
    {
        public void DoEnvironmentalEffects()
        {
            CreateLilyglowInFloralParadise();
        }

        public void CreateLilyglowInFloralParadise()
        {
            // Don't do anything if the player is not in the floral paradise of if this code is called by anyone other than the player.
            if (!ZoneFloralParadise || Main.myPlayer != Player.whoAmI)
                return;

            if (!Main.rand.NextBool(18))
                return;

            for (int tries = 0; tries < 50; tries++)
            {
                Vector2 potentialSpawnPosition = Player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(300f, 800f);
                if (Collision.SolidCollision(potentialSpawnPosition, 1, 1))
                    continue;

                Vector2 spawnVelocity = -Vector2.UnitY.RotatedByRandom(0.82f) * Main.rand.NextFloat(0.5f, 1.35f);
                Projectile.NewProjectile(new EntitySource_WorldEvent(), potentialSpawnPosition, spawnVelocity, ModContent.ProjectileType<Lilyglow>(), 0, 0f, Player.whoAmI);
                break;
            }
        }
    }
}
