using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class GolemInfernoBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (Projectile.ai[2] == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
                Projectile.ai[2] = 1f;
            }

            bool hasReachedX = (Projectile.velocity.X < 0f && Projectile.Center.X < Projectile.ai[0]) || (Projectile.velocity.X > 0f && Projectile.Center.X > Projectile.ai[0]);
            bool hasReachedY = (Projectile.velocity.Y < 0f && Projectile.Center.Y < Projectile.ai[1]) || (Projectile.velocity.Y > 0f && Projectile.Center.Y > Projectile.ai[1]);

            if (hasReachedX && hasReachedY)
                Projectile.Kill();

            // Add visual effects
            Dust.NewDustPerfect(Projectile.Center, DustID.InfernoFork, Vector2.Zero);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.OnFire, 360);
        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GolemInfernoBlast>(), Projectile.damage, 0f, Projectile.owner);
        }
    }
}
