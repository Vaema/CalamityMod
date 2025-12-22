using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class ShadeNimbusSpawner : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/NPCs/HiveMind/DankCreeper";

        public ref float EffectStrength => ref Projectile.ai[2]; // 1 Rotten Brain, 2 Amal Brain, 3 The Amalgam

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 70;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 35;
        }

        public override void AI() => Projectile.rotation = Projectile.velocity.X * 0.05f;

        // The spawner should not deal damage
        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            // Kill sound, dust, and gores of Dank Creeper
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

            for (int k = 0; k < 20; k++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, Projectile.velocity.X > 0f ? 1f : -1f, -1f);

            if (!Main.dedServ)
            {
                Vector2 goreVelocity = new Vector2(Projectile.velocity.X, Projectile.velocity.Y * 0.4f);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, goreVelocity, Mod.Find<ModGore>("DankCreeperGore").Type);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, goreVelocity, Mod.Find<ModGore>("DankCreeperGore2").Type);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, goreVelocity, Mod.Find<ModGore>("DankCreeperGore3").Type);
            }

            // Spawn shade rain clouds out to the sides
            // Number of clouds and their damage scale based on what accessory in the upgrade path triggered it (damage is set when the spawner is created)
            if (Main.myPlayer == Projectile.owner)
            {
                int cloudAmt = EffectStrength == 3f ? 5 : EffectStrength == 2f ? 3 : 2;
                for (int c = -(cloudAmt - 1) / 2; c <= (cloudAmt - 1) / 2; c++)
                {
                    Vector2 cloudVelocity = c == 0 ? Vector2.Zero : Vector2.UnitX.RotatedByRandom(MathHelper.Pi / 72f) * Main.rand.NextFloat(3f, 9.5f);
                    cloudVelocity *= c < 0f ? -1f : 1f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, cloudVelocity, ModContent.ProjectileType<ShadeNimbus>(), Projectile.damage, 0f, Main.myPlayer);
                }
            }
        }
    }
}
